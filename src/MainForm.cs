using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Insonnia.Properties;

namespace Insonnia
{
    public partial class MainForm : Form
    {
        // ── costanti ────────────────────────────────────────────────────
        private const string RunRegKey  = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName    = "Insonnia";

        private const double Threshold75 = 0.75;
        private const double Threshold90 = 0.90;
        private const double Threshold95 = 0.95;
        private const int    UrgentSecs  = 60;
        private const int    UrgentEvery = 15;

        // ── stato ────────────────────────────────────────────────────────
        private bool      _keepAwake       = false;   // sessione armata (Avvia premuto)
        private DateTime? _insonniaStarted = null;
        private TimeSpan? _maxDuration     = null;

        // ── sospensione per inattività ─────────────────────────────────────
        // _idleThreshold null = funzione disattivata per questa sessione.
        // _idleSuspended  true = sessione armata ma lock rilasciato perché inattivo.
        // Il lock è tenuto sse: _keepAwake && !_idleSuspended.
        private TimeSpan? _idleThreshold = null;
        private bool      _idleSuspended = false;

        private System.Windows.Forms.Timer _uiTimer;
        private int _tickCount = 0;

        private readonly HashSet<string> _notifiedKeys = new HashSet<string>();
        private int  _lastUrgentNotifySec = int.MinValue;
        private bool _forceClose  = false;

        // ── durata combo ─────────────────────────────────────────────────
        private static string[] DurationLabels
        {
            get
            {
                return new string[] {
                    L.Duration_Infinite,
                    L.Duration_30min,
                    L.Duration_1h,
                    L.Duration_2h,
                    L.Duration_4h,
                    L.Duration_Custom
                };
            }
        }
        private static readonly TimeSpan?[] DurationValues = {
            null,
            TimeSpan.FromMinutes(30),
            TimeSpan.FromHours(1),
            TimeSpan.FromHours(2),
            TimeSpan.FromHours(4),
            null   // placeholder: il valore viene letto da nudCustomMinutes
        };
        // indice della voce "Personalizzato..." nel combo
        private const int CustomDurationIndex = 5;

        // indice selezionato dal menu tray (-1 = segue combo form)
        private int _trayDurationIndex = -1;
        // ultimo valore custom usato (in minuti), per mostrarlo nel menu tray
        private int _lastCustomMinutes = 45;

        // ── soglie inattività ──────────────────────────────────────────────
        // Preset offerti dal menu tray; il menu aggiorna direttamente i controlli del form.
        private static readonly int[] IdlePresets = { 10, 20, 30, 60 };
        // ultimo valore inattività usato (in minuti), per la voce "Personalizzato" del menu
        private int _lastIdleMinutes = 20;

        // ────────────────────────────────────────────────────────────────
        public MainForm(bool autoStart)
        {
            InitializeComponent();

            for (int i = 0; i < DurationLabels.Length; i++)
                cmbDuration.Items.Add(DurationLabels[i]);

            // ── Carica impostazioni persistite ──────────────────────────
            _lastCustomMinutes     = Settings.Default.CustomDurationMinutes;
            nudCustomMinutes.Value = Math.Max(nudCustomMinutes.Minimum,
                                    Math.Min(nudCustomMinutes.Maximum,
                                    (decimal)_lastCustomMinutes));
            int savedIdx = Settings.Default.SelectedDurationIndex;
            cmbDuration.SelectedIndex = (savedIdx >= 0 && savedIdx < cmbDuration.Items.Count)
                ? savedIdx : 0;

            // Sospensione per inattività (letta prima dell'eventuale autostart)
            _lastIdleMinutes        = Settings.Default.IdleThresholdMinutes;
            nudIdleMinutes.Value    = Math.Max(nudIdleMinutes.Minimum,
                                     Math.Min(nudIdleMinutes.Maximum,
                                     (decimal)_lastIdleMinutes));
            chkIdleSuspend.Checked  = Settings.Default.IdleSuspendEnabled;

            _uiTimer          = new System.Windows.Forms.Timer();
            _uiTimer.Interval = 1000;
            _uiTimer.Tick    += UiTimer_Tick;

            UpdateUI();
            UpdateStatusDisplay(TimeSpan.Zero);
            UpdateStartWithWindowsMenu();
            RebuildDurationMenu();
            RebuildIdleMenu();
            SetTrayIcon(InsonniaLevel.GetCurrent(TimeSpan.Zero).Icon);

            // Versione assembly
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = string.Format("v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);

            if (autoStart)
            {
                Minimize();
                StartInsonnia();
            }
        }

        // ── timer UI ─────────────────────────────────────────────────────
        private void UiTimer_Tick(object sender, EventArgs e)
        {
            if (!_insonniaStarted.HasValue) return;

            TimeSpan elapsed = DateTime.Now - _insonniaStarted.Value;

            _tickCount++;

            // ── Macchina a stati inattività ──────────────────────────────
            if (_idleThreshold.HasValue)
            {
                uint idleMs = Win32Interop.GetIdleTime();
                if (!_idleSuspended && idleMs >= _idleThreshold.Value.TotalMilliseconds)
                    EnterIdleSuspend();
                else if (_idleSuspended && idleMs < _idleThreshold.Value.TotalMilliseconds)
                    ResumeFromIdle();
            }

            // Rinnova il lock ogni 60 tick, ma solo se lo stiamo davvero tenendo
            if (!_idleSuspended && _tickCount % 60 == 0)
                Win32Interop.SetThreadExecutionState(
                    EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED);

            InsonniaLevel level = InsonniaLevel.GetCurrent(elapsed);
            bool timerMode = _maxDuration.HasValue;
            if (_idleSuspended)
            {
                SetTrayIconDynamic(TrayIconRenderer.IdleSuspended(level.Source));
            }
            else if (timerMode)
            {
                double ratio  = elapsed.TotalSeconds / _maxDuration.Value.TotalSeconds;
                bool   urgent = (_maxDuration.Value - elapsed).TotalSeconds <= UrgentSecs;
                SetTrayIconDynamic(TrayIconRenderer.Timer(level.Source, ratio, urgent, _tickCount % 2 == 0));
            }
            else
            {
                SetTrayIconDynamic(TrayIconRenderer.Active(level.Source));
            }

            UpdateStatusDisplay(elapsed);
            UpdateTrayTooltip(elapsed);

            if (timerMode)
            {
                // La scadenza ferma la sessione in entrambi gli stati (anche da sospeso:
                // al risveglio del PC il tempo trascorso può aver superato la durata)
                if (elapsed >= _maxDuration.Value)
                {
                    StopInsonnia(L.Balloon_Expired);
                    return;
                }
                // Notifiche progressive e lampeggio solo quando il lock è attivo
                if (!_idleSuspended)
                    HandleTimerNotifications(elapsed);
            }
        }

        // ── start / stop ─────────────────────────────────────────────────
        private void StartInsonnia()
        {
            // Se c'è una selezione dal menu tray, quella ha la precedenza
            int idx = (_trayDurationIndex >= 0) ? _trayDurationIndex : cmbDuration.SelectedIndex;

            // Voce "Personalizzato...": leggi i minuti dal NumericUpDown
            if (idx == CustomDurationIndex)
                _maxDuration = TimeSpan.FromMinutes((double)nudCustomMinutes.Value);
            else
                _maxDuration = (idx >= 0 && idx < DurationValues.Length) ? DurationValues[idx] : null;

            // Allinea il combo form alla selezione tray
            if (_trayDurationIndex >= 0 && _trayDurationIndex != CustomDurationIndex)
                cmbDuration.SelectedIndex = _trayDurationIndex;

            // ── Soglia di inattività (letta dai controlli del form) ──
            _idleThreshold = chkIdleSuspend.Checked
                ? TimeSpan.FromMinutes((double)nudIdleMinutes.Value)
                : (TimeSpan?)null;
            _idleSuspended = false;

            _insonniaStarted     = DateTime.Now;
            _tickCount           = 0;
            _notifiedKeys.Clear();
            _lastUrgentNotifySec = int.MinValue;

            Win32Interop.SetThreadExecutionState(
                EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED);

            _keepAwake = true;
            _uiTimer.Start();

            string durationText = _maxDuration.HasValue
                ? L.Balloon_StartFor(FormatDuration(_maxDuration.Value))
                : L.Balloon_StartIndefinite;
            ShowBalloon(L.Balloon_StartedTitle,
                L.Balloon_StartedText(durationText),
                ToolTipIcon.Info, 2000);

            UpdateUI();
            UpdateStatusDisplay(TimeSpan.Zero);
        }

        private void StopInsonnia(string balloonMessage)
        {
            _uiTimer.Stop();
            _insonniaStarted = null;
            _maxDuration     = null;
            _trayDurationIndex = -1;
            _idleThreshold   = null;
            _idleSuspended   = false;

            Win32Interop.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);

            _keepAwake = false;

            SetTrayIcon(InsonniaLevel.GetCurrent(TimeSpan.Zero).Icon);

            ShowBalloon(L.Balloon_StoppedTitle, balloonMessage, ToolTipIcon.Info, 2000);
            notifyIcon.Text = L.Tray_Paused;

            UpdateUI();
            UpdateStatusDisplay(TimeSpan.Zero);
        }

        private void StopInsonnia()
        {
            StopInsonnia(L.Balloon_StoppedText);
        }

        // ── sospensione per inattività ─────────────────────────────────────
        /// <summary>Inattivo oltre soglia: rilascia il lock, il PC può dormire (sessione ancora armata).</summary>
        private void EnterIdleSuspend()
        {
            _idleSuspended = true;
            Win32Interop.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            InsonniaLevel level = InsonniaLevel.GetCurrent(DateTime.Now - _insonniaStarted.Value);
            SetTrayIconDynamic(TrayIconRenderer.IdleSuspended(level.Source));
            ShowBalloon(L.Balloon_IdleSuspendTitle, L.Balloon_IdleSuspendText, ToolTipIcon.Info, 2500);
        }

        /// <summary>Attività rilevata: riacquisisce il lock e riprende il keep-awake.</summary>
        private void ResumeFromIdle()
        {
            _idleSuspended = false;
            Win32Interop.SetThreadExecutionState(
                EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
            ShowBalloon(L.Balloon_IdleResumeTitle, L.Balloon_IdleResumeText, ToolTipIcon.Info, 2000);
        }

        // ── notifiche timer progressive ───────────────────────────────────
        private void HandleTimerNotifications(TimeSpan elapsed)
        {
            if (!_maxDuration.HasValue) return;

            double totalSecs     = _maxDuration.Value.TotalSeconds;
            double elapsedSecs   = elapsed.TotalSeconds;
            double ratio         = elapsedSecs / totalSecs;
            double remainingSecs = totalSecs - elapsedSecs;
            // La scadenza è gestita centralmente in UiTimer_Tick.

            TryNotify("75pct",
                ratio >= Threshold75 && ratio < Threshold90,
                L.Balloon_75Title,
                L.Balloon_75Text(FormatDuration(TimeSpan.FromSeconds(remainingSecs))),
                ToolTipIcon.Info, 2500);

            TryNotify("90pct",
                ratio >= Threshold90 && ratio < Threshold95,
                L.Balloon_90Title,
                L.Balloon_90Text(FormatDuration(TimeSpan.FromSeconds(remainingSecs))),
                ToolTipIcon.Warning, 2500);

            TryNotify("95pct",
                ratio >= Threshold95 && remainingSecs > UrgentSecs,
                L.Balloon_95Title,
                L.Balloon_95Text(FormatDuration(TimeSpan.FromSeconds(remainingSecs))),
                ToolTipIcon.Warning, 2500);

            if (remainingSecs <= UrgentSecs && remainingSecs > 0)
            {
                // Il lampeggio è ora reso dall'anello pulsante (vedi UiTimer_Tick + TrayIconRenderer)
                int secInt = (int)remainingSecs;
                bool firstUrgent    = _lastUrgentNotifySec == int.MinValue;
                bool intervalPassed = (_lastUrgentNotifySec - secInt) >= UrgentEvery;
                if (secInt != _lastUrgentNotifySec && (firstUrgent || intervalPassed))
                {
                    _lastUrgentNotifySec = secInt;
                    ShowBalloon(L.Balloon_UrgentTitle,
                        L.Balloon_UrgentText(secInt),
                        ToolTipIcon.Warning, 2000);
                }
            }
        }

        private void TryNotify(string key, bool condition, string title, string text,
            ToolTipIcon icon, int duration)
        {
            if (condition && !_notifiedKeys.Contains(key))
            {
                _notifiedKeys.Add(key);
                ShowBalloon(title, text, icon, duration);
            }
        }

        // ── helpers UI ────────────────────────────────────────────────────
        private void UpdateUI()
        {
            btnStart.Visible            = !_keepAwake;
            btnStop.Visible             = _keepAwake;
            cmbDuration.Enabled         = !_keepAwake;
            nudCustomMinutes.Enabled    = !_keepAwake;
            progressTimer.Visible       = false;

            // Configurazione inattività modificabile solo a sessione ferma (come la durata)
            chkIdleSuspend.Enabled = !_keepAwake;
            nudIdleMinutes.Enabled = !_keepAwake && chkIdleSuspend.Checked;

            // Aggiorna voce tray start/stop
            startStopMenuItem.Text = _keepAwake ? L.Menu_Stop : L.Menu_Start;
            // Durata e inattività selezionabili solo se non attivo
            durationMenuItem.Enabled = !_keepAwake;
            idleMenuItem.Enabled     = !_keepAwake;
        }

        private void UpdateStatusDisplay(TimeSpan elapsed)
        {
            if (!_keepAwake)
            {
                lblStatus.Text      = L.UI_StatusPaused;
                lblStatus.ForeColor = Color.Gray;
                progressTimer.Visible = false;
                return;
            }

            if (_idleSuspended)
            {
                lblStatus.Text        = L.UI_StatusIdleSuspended;
                lblStatus.ForeColor   = Color.SlateGray;
                progressTimer.Visible = false;
                return;
            }

            InsonniaLevel level = InsonniaLevel.GetCurrent(elapsed);

            if (_maxDuration.HasValue)
            {
                TimeSpan remaining = _maxDuration.Value - elapsed;
                if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;

                double ratio = elapsed.TotalSeconds / _maxDuration.Value.TotalSeconds;
                bool urgent  = remaining.TotalSeconds <= UrgentSecs;

                lblStatus.Text = L.UI_StatusTimer(level.Name, FormatTime(remaining));
                lblStatus.ForeColor = urgent ? Color.Crimson : Color.FromArgb(0, 120, 60);

                progressTimer.Visible = true;
                progressTimer.Maximum = (int)_maxDuration.Value.TotalSeconds;
                progressTimer.Value   = Math.Min((int)elapsed.TotalSeconds, progressTimer.Maximum);

                Color barColor;
                if (urgent)             barColor = Color.Crimson;
                else if (ratio >= 0.75) barColor = Color.Orange;
                else                    barColor = Color.FromArgb(0, 153, 76);
                SetProgressBarColor(progressTimer, barColor);
            }
            else
            {
                lblStatus.Text        = L.UI_StatusActive(level.Name, FormatTime(elapsed));
                lblStatus.ForeColor   = Color.FromArgb(0, 102, 204);
                progressTimer.Visible = false;
            }
        }

        private void UpdateTrayTooltip(TimeSpan elapsed)
        {
            string tip;
            if (_idleSuspended)
            {
                tip = L.Tray_IdleSuspended;
            }
            else if (_maxDuration.HasValue)
            {
                TimeSpan remaining = _maxDuration.Value - elapsed;
                if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
                tip = L.Tray_ActiveRemaining(FormatTime(remaining));
            }
            else
            {
                tip = L.Tray_ActiveElapsed(FormatTime(elapsed));
            }
            notifyIcon.Text = tip.Length > 63 ? tip.Substring(0, 63) : tip;
        }

        // Icona dinamica corrente (anello/lunetta): va liberata, a differenza di quelle statiche.
        private Icon _dynamicTrayIcon;

        /// <summary>Imposta un'icona statica condivisa (non va liberata) e scarta l'eventuale dinamica.</summary>
        private void SetTrayIcon(Icon icon)
        {
            this.Icon = notifyIcon.Icon = icon;
            DisposeDynamicIcon();
        }

        /// <summary>Imposta un'icona generata a runtime, liberando la precedente per non perdere handle GDI.</summary>
        private void SetTrayIconDynamic(Icon dyn)
        {
            Icon previous = _dynamicTrayIcon;
            this.Icon = notifyIcon.Icon = dyn;
            _dynamicTrayIcon = dyn;
            if (previous != null) previous.Dispose();
        }

        private void DisposeDynamicIcon()
        {
            if (_dynamicTrayIcon != null)
            {
                _dynamicTrayIcon.Dispose();
                _dynamicTrayIcon = null;
            }
        }

        private void ShowBalloon(string title, string text, ToolTipIcon icon, int ms)
        {
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText  = text;
            notifyIcon.BalloonTipIcon  = icon;
            notifyIcon.ShowBalloonTip(ms);
        }

        // ── colore ProgressBar via SendMessage ───────────────────────────
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wp, IntPtr lp);
        private const uint PBM_SETBARCOLOR = 0x409;

        private static void SetProgressBarColor(ProgressBar pb, Color color)
        {
            SendMessage(pb.Handle, PBM_SETBARCOLOR, IntPtr.Zero,
                (IntPtr)System.Drawing.ColorTranslator.ToWin32(color));
        }

        // ── avvia con Windows ────────────────────────────────────────────
        private void UpdateStartWithWindowsMenu()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegKey, false))
            {
                startWithWindowsMenuItem.Checked = key != null && key.GetValue(AppName) != null;
            }
        }

        /// <summary>Ricostruisce il sottomenu "Durata" del tray icon.</summary>
        private void RebuildDurationMenu()
        {
            durationMenuItem.DropDownItems.Clear();

            // Voci predefinite (indici 0-4, escluso Custom)
            string[] labels = DurationLabels;
            for (int i = 0; i < labels.Length - 1; i++) // esclude "Personalizzato..."
            {
                int capturedIndex = i;
                System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem(labels[i]);
                item.Checked = (capturedIndex == cmbDuration.SelectedIndex && capturedIndex != CustomDurationIndex);
                item.Click  += delegate(object s, EventArgs e2)
                {
                    cmbDuration.SelectedIndex = capturedIndex;
                    nudCustomMinutes.Visible  = false;
                    lblMinutes.Visible        = false;
                    _trayDurationIndex        = capturedIndex;
                    RebuildDurationMenu();
                };
                durationMenuItem.DropDownItems.Add(item);
            }

            // Separatore
            durationMenuItem.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());

            // Voce "Personalizzato: X min" — mostra l'ultimo valore custom
            System.Windows.Forms.ToolStripMenuItem customItem =
                new System.Windows.Forms.ToolStripMenuItem(L.Menu_CustomMinutes(_lastCustomMinutes));
            customItem.Checked = (cmbDuration.SelectedIndex == CustomDurationIndex);
            customItem.Click  += delegate(object s, EventArgs e2)
            {
                cmbDuration.SelectedIndex = CustomDurationIndex;
                nudCustomMinutes.Visible  = true;
                lblMinutes.Visible        = true;
                _trayDurationIndex        = CustomDurationIndex;
                RebuildDurationMenu();
            };
            durationMenuItem.DropDownItems.Add(customItem);
        }

        /// <summary>Ricostruisce il sottomenu "Sospensione inattività" del tray icon.</summary>
        private void RebuildIdleMenu()
        {
            idleMenuItem.DropDownItems.Clear();

            bool enabled = chkIdleSuspend.Checked;
            int  current = (int)nudIdleMinutes.Value;

            // Voce "Disattivato"
            System.Windows.Forms.ToolStripMenuItem offItem =
                new System.Windows.Forms.ToolStripMenuItem(L.Menu_IdleOff);
            offItem.Checked = !enabled;
            offItem.Click  += delegate(object s, EventArgs e2)
            {
                chkIdleSuspend.Checked = false;   // i *_Changed persistono e ricostruiscono
            };
            idleMenuItem.DropDownItems.Add(offItem);

            // Preset 10/20/30/60 min
            bool isPreset = false;
            foreach (int minutes in IdlePresets)
            {
                int captured = minutes;
                if (enabled && current == captured) isPreset = true;
                System.Windows.Forms.ToolStripMenuItem item =
                    new System.Windows.Forms.ToolStripMenuItem(L.Menu_IdleMinutes(captured));
                item.Checked = enabled && current == captured;
                item.Click  += delegate(object s, EventArgs e2)
                {
                    nudIdleMinutes.Value   = captured;   // ValueChanged persiste e ricostruisce
                    chkIdleSuspend.Checked = true;
                };
                idleMenuItem.DropDownItems.Add(item);
            }

            // Separatore
            idleMenuItem.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());

            // Voce "Personalizzato: X min" — mostra l'ultimo valore custom
            System.Windows.Forms.ToolStripMenuItem customItem =
                new System.Windows.Forms.ToolStripMenuItem(L.Menu_IdleCustomMinutes(_lastIdleMinutes));
            customItem.Checked = enabled && !isPreset;
            customItem.Click  += delegate(object s, EventArgs e2)
            {
                nudIdleMinutes.Value   = _lastIdleMinutes;
                chkIdleSuspend.Checked = true;
            };
            idleMenuItem.DropDownItems.Add(customItem);
        }

        // ── formattazione tempi ───────────────────────────────────────────
        private static string FormatTime(TimeSpan t)
        {
            if (t.TotalHours >= 1)
                return string.Format("{0}:{1:D2}:{2:D2}", (int)t.TotalHours, t.Minutes, t.Seconds);
            return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        private static string FormatDuration(TimeSpan t)
        {
            if (t.TotalHours >= 1)
                return string.Format("{0}h {1:D2}m", (int)t.TotalHours, t.Minutes);
            if (t.TotalMinutes >= 1)
                return string.Format("{0} min", (int)t.TotalMinutes);
            return string.Format("{0} s", (int)t.TotalSeconds);
        }

        // ── event handlers ────────────────────────────────────────────────
        private void btnStart_Click(object sender, EventArgs e) { StartInsonnia(); }
        private void btnStop_Click(object sender, EventArgs e)  { StopInsonnia(); }

        private void startStopMenuItem_Click(object sender, EventArgs e)
        {
            if (_keepAwake) StopInsonnia();
            else StartInsonnia();
        }

        private void cmbDuration_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool custom = cmbDuration.SelectedIndex == CustomDurationIndex;
            nudCustomMinutes.Visible = custom;
            lblMinutes.Visible       = custom;
            if (custom)
                _lastCustomMinutes = (int)nudCustomMinutes.Value;
            _trayDurationIndex = -1;
            Settings.Default.SelectedDurationIndex = cmbDuration.SelectedIndex;
            Settings.Default.Save();
            RebuildDurationMenu();
        }

        private void nudCustomMinutes_ValueChanged(object sender, EventArgs e)
        {
            _lastCustomMinutes = (int)nudCustomMinutes.Value;
            Settings.Default.CustomDurationMinutes = _lastCustomMinutes;
            Settings.Default.Save();
            RebuildDurationMenu();
        }

        private void chkIdleSuspend_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.IdleSuspendEnabled = chkIdleSuspend.Checked;
            Settings.Default.Save();
            UpdateUI();           // riallinea l'abilitazione di nudIdleMinutes
            RebuildIdleMenu();
        }

        private void nudIdleMinutes_ValueChanged(object sender, EventArgs e)
        {
            _lastIdleMinutes = (int)nudIdleMinutes.Value;
            Settings.Default.IdleThresholdMinutes = _lastIdleMinutes;
            Settings.Default.Save();
            RebuildIdleMenu();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Hide();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                Hide();
        }

        private void MainForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (!_forceClose && e.CloseReason == System.Windows.Forms.CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
                WindowState = FormWindowState.Normal;
                Activate();
            }
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _forceClose = true;
            Close();
        }

        private void startWithWindowsMenuItem_Click(object sender, EventArgs e)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegKey, true))
            {
                if (key == null) return;
                if (startWithWindowsMenuItem.Checked)
                {
                    key.DeleteValue(AppName, false);
                    startWithWindowsMenuItem.Checked = false;
                }
                else
                {
                    key.SetValue(AppName, string.Format("\"{0}\" -s", Application.ExecutablePath));
                    startWithWindowsMenuItem.Checked = true;
                }
            }
        }

        private void Minimize()
        {
            WindowState   = FormWindowState.Minimized;
            ShowInTaskbar = false;
        }
    }
}
