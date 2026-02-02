using Insonnia.Properties;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Insonnia
{
    public partial class Form1 : Form
    {
        private System.Threading.Timer _timer;
        private bool _keepAwake = false;
        private DateTime? _insonniaStarted = null;

        public Form1(bool autoStart)
        {
            InitializeComponent();
            
            _timer = new System.Threading.Timer(
                TimerCB,
                null,
                Timeout.Infinite,
                Timeout.Infinite);

            UpdateUI();
            UpdateLevel();

            if (autoStart)
            {
                Minimize();
                StartInsonnia();
            }
        }

        private void TimerCB(object state)
        {
            Win32Interop.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | 
                EXECUTION_STATE.ES_DISPLAY_REQUIRED | 
                EXECUTION_STATE.ES_AWAYMODE_REQUIRED);

            UpdateLevel();
        }

        private void StartInsonnia()
        {
            notifyIcon.ShowBalloonTip(2000, "Insonnia", "Started", ToolTipIcon.Info);

            _insonniaStarted = DateTime.Now;
            _timer.Change(0, 5000);

            _keepAwake = true;
            UpdateUI();
            UpdateLevel();
        }

        private void StopInsonnia()
        {
            notifyIcon.ShowBalloonTip(2000, "Insonnia", "Stopped", ToolTipIcon.Info);

            _insonniaStarted = null;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            _keepAwake = false;
            UpdateUI();
            UpdateLevel();
        }

        private void Minimize()
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
        }

        private void UpdateUI()
        {
            btnStart.Visible = !_keepAwake;
            btnStop.Visible = _keepAwake;

            keepAwakeToolStripMenuItem.Checked = _keepAwake;
        }

        private void SetIcons(Icon icon)
        {
            if (this.InvokeRequired)
                this.BeginInvoke((MethodInvoker)(() => SetIcons(icon)));
            else
                this.Icon = notifyIcon.Icon = icon;
        }

        private void UpdateLevel()
        {
            TimeSpan timeAwake = _insonniaStarted.HasValue ? DateTime.Now - _insonniaStarted.Value : TimeSpan.Zero;

            foreach (InsonniaLevel level in InsonniaLevel.Levels)
            {
                if (timeAwake <= level.TimeKeptAwake)
                {
                    SetIcons(level.Icon);
                    break;
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void btnStart_Click(object sender, EventArgs e) => StartInsonnia();

        private void btnStop_Click(object sender, EventArgs e) => StopInsonnia();

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void keepAwakeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_keepAwake) StopInsonnia();
            else StartInsonnia();
        }
    }
}
