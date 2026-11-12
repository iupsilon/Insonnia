using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Insonnia.Properties;
using Insonnia.MonitorSwitch;
using Insonnia.UI;

namespace Insonnia.Forms
{
    /// <summary>
    /// Configurazione del cambio ingresso monitor. Elenca i monitor rilevati (seriale EDID, istanza,
    /// ingresso attuale) e i dispositivi USB presenti, così da ricavare i valori corretti.
    /// Le impostazioni sono salvate solo con "Salva"; "Prova" invia subito il comando senza salvare.
    /// Rilevamenti e comandi DDC/CI girano fuori dal thread UI.
    /// </summary>
    internal partial class MonitorSwitchForm : Form
    {
        private static readonly Regex HexValue = new Regex(@"0x([0-9A-F]{1,2})\b", RegexOptions.IgnoreCase);

        /// <summary>Voce dei combo ingresso: valore VCP 0x60 + etichetta.</summary>
        private sealed class InputItem
        {
            public readonly int    Value;
            private readonly string _text;

            public InputItem(int value, string text)
            {
                Value = value;
                _text = text;
            }

            public override string ToString() { return _text; }
        }

        private List<MonitorInfo> _monitors = new List<MonitorInfo>();
        private bool _populating = false;   // evita che la selezione automatica sovrascriva l'ID

        /// <summary>True se l'utente ha salvato la configurazione.</summary>
        public bool Saved { get; private set; }

        public MonitorSwitchForm()
        {
            InitializeComponent();
            Icon = TrayIconRenderer.Active;

            Settings s = Settings.Default;
            chkEnabled.Checked = s.MonitorSwitchEnabled;
            cmbUsb.Text        = s.MonitorSwitchUsbId;
            txtMonitorId.Text  = s.MonitorSwitchMonitorId;
            FillInputCombo(cmbConnect,    s.MonitorSwitchInputOnConnect);
            FillInputCombo(cmbDisconnect, s.MonitorSwitchInputOnDisconnect);

            ApplyTheme();
            Theme.Changed += Theme_Changed;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Theme.ApplyWindowChrome(this);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Theme.Apply(this);   // ora i controlli nativi hanno l'handle (tema scuro di combo e campi)
            cmbUsb.SelectionLength        = 0;   // nessun testo preselezionato nei combo editabili
            cmbConnect.SelectionLength    = 0;
            cmbDisconnect.SelectionLength = 0;
            RefreshMonitors();
            RefreshUsbDevices();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Theme.Changed -= Theme_Changed;
            base.OnFormClosed(e);
        }

        // ── tema ──────────────────────────────────────────────────────────
        private void ApplyTheme()
        {
            Theme t = Theme.Current;
            BackColor = t.Background;
            ForeColor = t.Text;
            Theme.Apply(this);
            lblHeaderSub.ForeColor = t.TextMuted;
            lblStatus.ForeColor    = t.TextMuted;
            Theme.ApplyWindowChrome(this);
            Invalidate(true);
        }

        private void Theme_Changed(object sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke((MethodInvoker)ApplyTheme);
            else                ApplyTheme();
        }

        // Intestazioni dell'elenco disegnate a tema (quelle native restano chiare in modalità scura)
        private void lvMonitors_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            Theme t = Theme.Current;
            using (SolidBrush bg = new SolidBrush(t.Surface))
                e.Graphics.FillRectangle(bg, e.Bounds);
            using (Pen line = new Pen(t.Border))
                e.Graphics.DrawLine(line, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

            Rectangle text = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, Theme.UiFont(9F, FontStyle.Bold), text, t.TextMuted,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        private void lvMonitors_DrawItem(object sender, DrawListViewItemEventArgs e)     { e.DrawDefault = true; }
        private void lvMonitors_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) { e.DrawDefault = true; }

        /// <summary>Testo localizzato per l'esito di un comando (finestra e balloon del tray).</summary>
        internal static string DescribeOutcome(MonitorSwitchOutcome outcome, string monitorId, int input, string detail)
        {
            switch (outcome)
            {
                case MonitorSwitchOutcome.Switched:        return L.MS_Outcome_Switched(MonitorControl.InputName(input));
                case MonitorSwitchOutcome.AlreadyActive:   return L.MS_Outcome_AlreadyActive(MonitorControl.InputName(input));
                case MonitorSwitchOutcome.NotConfigured:   return L.MS_Outcome_NotConfigured;
                case MonitorSwitchOutcome.MonitorNotFound: return L.MS_Outcome_NotFound(monitorId);
                case MonitorSwitchOutcome.Ambiguous:       return L.MS_Outcome_Ambiguous(monitorId);
                default:                                   return L.MS_Outcome_DdcError(detail);
            }
        }

        // ── monitor ──────────────────────────────────────────────────────
        private void RefreshMonitors()
        {
            btnMonitorsRefresh.Enabled = false;
            SetStatus(L.MS_Detecting, false);

            ThreadPool.QueueUserWorkItem(delegate
            {
                List<MonitorInfo> monitors;
                try
                {
                    monitors = MonitorControl.GetMonitors();
                }
                catch (Exception)
                {
                    monitors = new List<MonitorInfo>();
                }

                PostToUi(delegate
                {
                    ShowMonitors(monitors);
                    btnMonitorsRefresh.Enabled = true;
                    SetStatus(L.MS_Detected(monitors.Count), false);
                });
            });
        }

        private void ShowMonitors(List<MonitorInfo> monitors)
        {
            _monitors   = monitors;
            _populating = true;
            lvMonitors.BeginUpdate();
            try
            {
                lvMonitors.Items.Clear();
                foreach (MonitorInfo m in monitors)
                {
                    ListViewItem item = new ListViewItem(m.Index.ToString());
                    item.SubItems.Add(m.DisplayName);
                    item.SubItems.Add(string.IsNullOrEmpty(m.SerialNumber) ? "-" : m.SerialNumber);
                    item.SubItems.Add(m.CurrentInputText ?? L.MS_DdcUnavailable);
                    item.SubItems.Add(m.InstanceId ?? "-");
                    item.SubItems.Add(m.GdiDevice ?? "-");
                    item.Tag = m;
                    if (m.DdcError != null)
                    {
                        item.ForeColor   = Theme.Current.TextMuted;
                        item.ToolTipText = m.DdcError;
                    }
                    lvMonitors.Items.Add(item);

                    // evidenzia il monitor attualmente configurato
                    if (m.Matches(txtMonitorId.Text))
                        item.Selected = true;
                }
            }
            finally
            {
                lvMonitors.EndUpdate();
                _populating = false;
            }
        }

        private void lvMonitors_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_populating || lvMonitors.SelectedItems.Count == 0) return;

            MonitorInfo m  = (MonitorInfo)lvMonitors.SelectedItems[0].Tag;
            string      id = m.SuggestedId;
            if (!string.IsNullOrEmpty(id))
                txtMonitorId.Text = id;
        }

        private void btnMonitorsRefresh_Click(object sender, EventArgs e) { RefreshMonitors(); }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (MonitorInfo m in _monitors)
                sb.AppendLine(m.ToString());
            if (sb.Length == 0) return;

            try
            {
                Clipboard.SetText(sb.ToString());
                SetStatus(L.MS_Copied, false);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        // ── dispositivi USB ──────────────────────────────────────────────
        private void RefreshUsbDevices()
        {
            btnUsbRefresh.Enabled = false;

            ThreadPool.QueueUserWorkItem(delegate
            {
                SortedDictionary<string, string> devices;
                try
                {
                    devices = UsbDeviceWatcher.ListPresentDevices();
                }
                catch (Exception)
                {
                    devices = new SortedDictionary<string, string>();
                }

                PostToUi(delegate
                {
                    string text = cmbUsb.Text;   // conserva quanto scritto/salvato
                    cmbUsb.BeginUpdate();
                    cmbUsb.Items.Clear();
                    foreach (KeyValuePair<string, string> d in devices)
                        cmbUsb.Items.Add(string.IsNullOrEmpty(d.Value) ? d.Key : d.Key + "  -  " + d.Value);
                    cmbUsb.EndUpdate();
                    cmbUsb.Text = text;
                    btnUsbRefresh.Enabled = true;
                });
            });
        }

        private void btnUsbRefresh_Click(object sender, EventArgs e) { RefreshUsbDevices(); }

        // ── ingressi ─────────────────────────────────────────────────────
        private static void FillInputCombo(ComboBox combo, int value)
        {
            combo.Items.Clear();
            combo.Items.Add(new InputItem(0, L.MS_NoAction));
            foreach (int input in MonitorControl.KnownInputs)
                combo.Items.Add(new InputItem(input, MonitorControl.InputName(input)));

            foreach (object item in combo.Items)
            {
                if (((InputItem)item).Value == value)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            combo.Text = MonitorControl.InputName(value);   // valore non standard
        }

        /// <summary>
        /// Legge il valore di un combo ingresso: voce dell'elenco, "0x1B" o numero decimale 0-255.
        /// 0 = nessuna azione.
        /// </summary>
        private static bool TryParseInput(ComboBox combo, out int value)
        {
            string    text = combo.Text.Trim();
            InputItem item = combo.SelectedItem as InputItem;
            if (item != null && text == item.ToString())
            {
                value = item.Value;
                return true;
            }
            if (text.Length == 0 || text == L.MS_NoAction)
            {
                value = 0;
                return true;
            }

            Match m = HexValue.Match(text);
            if (m.Success)
            {
                value = Convert.ToInt32(m.Groups[1].Value, 16);
                return true;
            }
            if (int.TryParse(text, out value) && value >= 0 && value <= 0xFF)
                return true;

            value = 0;
            return false;
        }

        private void btnTestConnect_Click(object sender, EventArgs e)    { TestInput(cmbConnect); }
        private void btnTestDisconnect_Click(object sender, EventArgs e) { TestInput(cmbDisconnect); }

        private void TestInput(ComboBox combo)
        {
            int input;
            if (!TryParseInput(combo, out input) || input == 0)
            {
                SetStatus(L.MS_InvalidInput, true);
                return;
            }
            string monitorId = txtMonitorId.Text.Trim();
            if (monitorId.Length == 0)
            {
                SetStatus(L.MS_MissingMonitor, true);
                return;
            }

            SetTestButtonsEnabled(false);
            SetStatus(L.MS_Sending, false);

            ThreadPool.QueueUserWorkItem(delegate
            {
                MonitorSwitchOutcome outcome;
                string               detail;
                try
                {
                    outcome = MonitorControl.SetInput(monitorId, input, out detail);
                }
                catch (Exception ex)
                {
                    outcome = MonitorSwitchOutcome.DdcError;
                    detail  = ex.Message;
                }

                PostToUi(delegate
                {
                    bool ok = outcome == MonitorSwitchOutcome.Switched || outcome == MonitorSwitchOutcome.AlreadyActive;
                    SetStatus(DescribeOutcome(outcome, monitorId, input, detail), !ok);
                    SetTestButtonsEnabled(true);
                });
            });
        }

        private void SetTestButtonsEnabled(bool enabled)
        {
            btnTestConnect.Enabled    = enabled;
            btnTestDisconnect.Enabled = enabled;
        }

        // ── salva / annulla ──────────────────────────────────────────────
        private void btnSave_Click(object sender, EventArgs e)
        {
            string usbText = cmbUsb.Text.Trim();
            string usbId   = UsbDeviceWatcher.NormalizeUsbId(usbText);
            if (usbId == null && (usbText.Length > 0 || chkEnabled.Checked))
            {
                ShowValidationError(L.MS_InvalidUsb, cmbUsb);
                return;
            }

            string monitorId = txtMonitorId.Text.Trim();
            if (monitorId.Length == 0 && chkEnabled.Checked)
            {
                ShowValidationError(L.MS_MissingMonitor, txtMonitorId);
                return;
            }

            int onConnect, onDisconnect;
            if (!TryParseInput(cmbConnect, out onConnect))
            {
                ShowValidationError(L.MS_InvalidInput, cmbConnect);
                return;
            }
            if (!TryParseInput(cmbDisconnect, out onDisconnect))
            {
                ShowValidationError(L.MS_InvalidInput, cmbDisconnect);
                return;
            }

            Settings s = Settings.Default;
            s.MonitorSwitchEnabled           = chkEnabled.Checked;
            s.MonitorSwitchUsbId             = usbId != null ? UsbDeviceWatcher.ToShortId(usbId) : "";
            s.MonitorSwitchMonitorId         = monitorId;
            s.MonitorSwitchInputOnConnect    = onConnect;
            s.MonitorSwitchInputOnDisconnect = onDisconnect;
            s.Save();

            Saved = true;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) { Close(); }

        // ── helpers ──────────────────────────────────────────────────────
        private void ShowValidationError(string message, Control focus)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            focus.Focus();
        }

        private void SetStatus(string text, bool error)
        {
            lblStatus.Text      = text;
            lblStatus.ForeColor = error ? Theme.Current.Critical : Theme.Current.TextMuted;
        }

        /// <summary>Esegue <paramref name="action"/> sul thread UI, se la finestra è ancora aperta.</summary>
        private void PostToUi(MethodInvoker action)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (!IsDisposed) action();
                });
            }
            catch (InvalidOperationException)
            {
                // finestra chiusa nel frattempo
            }
        }
    }
}
