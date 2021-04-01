using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Insonnia
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource CancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
        }

      
        
        public async Task StartInsonnia()
        {
            CancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() =>
            {
                do
                {
                    // Prevent Idle-to-Sleep (monitor not affected) (see note above)
                    SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
                    CancellationTokenSource.Token.WaitHandle.WaitOne(30000);
                    // Attendo la cancellazione    
                } while (!CancellationTokenSource.Token.IsCancellationRequested);

            });

        }

        public void StopInsonnia()
        {

            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);

        }
        // https://stackoverflow.com/questions/49045701/prevent-screen-from-sleeping-with-c-sharp
        // Define other methods and classes here
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,

            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            
            notifyIcon.ShowBalloonTip(5000,"Insonnia","Started",ToolTipIcon.Info);

            btnStart.Visible = false;
            btnStop.Visible = true;

            await StartInsonnia();

            notifyIcon.ShowBalloonTip(5000, "Insonnia", "Stopped", ToolTipIcon.Info);
            btnStart.Visible = true;
            btnStop.Visible = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            CancellationTokenSource.Cancel();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;

        }
    }
}
