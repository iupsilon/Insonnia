using Insonnia.Properties;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Insonnia
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Blocca doppio avvio
            bool createdNew;
            using (var mutex = new Mutex(true, "Insonnia_SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(L.Msg_AlreadyRunning,
                        "Insonnia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                bool autoStart = args != null && args.Length > 0 &&
                                 string.Equals(args[0], "-s", StringComparison.OrdinalIgnoreCase);
                Application.Run(new MainForm(autoStart));
            }
        }
    }
}
