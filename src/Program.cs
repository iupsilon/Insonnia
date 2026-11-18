using Insonnia.Properties;
using System;
using System.Threading;
using System.Windows.Forms;
using Insonnia.Forms;
using Insonnia.UI;

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

                bool autoStart = HasArg(args, "-s");
                // -christmas: tema natalizio anche fuori stagione (anteprima)
                Theme.ForceFestive = HasArg(args, "-christmas");
                Application.Run(new MainForm(autoStart));
            }
        }

        private static bool HasArg(string[] args, string name)
        {
            if (args == null) return false;
            foreach (string arg in args)
                if (string.Equals(arg, name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
