using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Insonnia.MonitorSwitch
{
    /// <summary>
    /// Rileva collegamento/scollegamento di un dispositivo USB (VID/PID) tramite WM_DEVICECHANGE:
    /// nessun polling, notifica immediata. Usa una finestra "message-only" registrata con
    /// RegisterDeviceNotification sulla classe GUID_DEVINTERFACE_USB_DEVICE (solo il dispositivo
    /// padre, non le singole interfacce di un composito).
    /// Va creato, avviato e liberato sul thread UI: gli eventi arrivano sul thread UI.
    /// Gli eventi non sono filtrati nel tempo: il debounce è compito del chiamante.
    /// </summary>
    internal sealed class UsbDeviceWatcher : NativeWindow, IDisposable
    {
        private const int WM_DEVICECHANGE             = 0x0219;
        private const int DBT_DEVICEARRIVAL           = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE    = 0x8004;
        private const int DBT_DEVTYP_DEVICEINTERFACE  = 5;
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        private const int DbccNameOffset              = 28;   // offset di dbcc_name in DEV_BROADCAST_DEVICEINTERFACE

        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);
        private static readonly Guid   GUID_DEVINTERFACE_USB_DEVICE = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");

        private static readonly Regex VidPidLong  = new Regex(@"VID_([0-9A-F]{4})&PID_([0-9A-F]{4})", RegexOptions.IgnoreCase);
        private static readonly Regex VidPidShort = new Regex(@"\b([0-9A-F]{4})\s*:\s*([0-9A-F]{4})\b", RegexOptions.IgnoreCase);

        private readonly string _usbId;          // normalizzato: VID_xxxx&PID_yyyy
        private IntPtr _notification = IntPtr.Zero;

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        /// <param name="usbId">ID già normalizzato con <see cref="NormalizeUsbId"/>.</param>
        public UsbDeviceWatcher(string usbId)
        {
            _usbId = usbId;
        }

        public void Start()
        {
            if (Handle != IntPtr.Zero) return;

            CreateParams cp = new CreateParams();
            cp.Parent = HWND_MESSAGE;
            CreateHandle(cp);

            DEV_BROADCAST_DEVICEINTERFACE filter = new DEV_BROADCAST_DEVICEINTERFACE();
            filter.dbcc_size       = Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE));
            filter.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            filter.dbcc_classguid  = GUID_DEVINTERFACE_USB_DEVICE;

            IntPtr buffer = Marshal.AllocHGlobal(filter.dbcc_size);
            try
            {
                Marshal.StructureToPtr(filter, buffer, false);
                _notification = RegisterDeviceNotification(Handle, buffer, DEVICE_NOTIFY_WINDOW_HANDLE);
                if (_notification == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    DestroyHandle();
                    throw new Win32Exception(error, "RegisterDeviceNotification");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public void Dispose()
        {
            if (_notification != IntPtr.Zero)
            {
                UnregisterDeviceNotification(_notification);
                _notification = IntPtr.Zero;
            }
            if (Handle != IntPtr.Zero)
                DestroyHandle();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE && m.LParam != IntPtr.Zero)
            {
                int evt = (int)m.WParam.ToInt64();
                if (evt == DBT_DEVICEARRIVAL || evt == DBT_DEVICEREMOVECOMPLETE)
                {
                    try
                    {
                        OnDeviceChange(evt == DBT_DEVICEARRIVAL, m.LParam);
                    }
                    catch (Exception)
                    {
                        // un evento malformato non deve interrompere il message loop
                    }
                }
            }
            base.WndProc(ref m);
        }

        private void OnDeviceChange(bool arrived, IntPtr lParam)
        {
            int devType = Marshal.ReadInt32(lParam, 4);
            if (devType != DBT_DEVTYP_DEVICEINTERFACE) return;

            // es. \\?\USB#VID_046D&PID_C52B#5&1a2b3c&0&2#{a5dcbf10-6530-11d2-901f-00c04fb951ed}
            string path = Marshal.PtrToStringUni(IntPtr.Add(lParam, DbccNameOffset));
            if (path == null || path.IndexOf(_usbId, StringComparison.OrdinalIgnoreCase) < 0) return;

            EventHandler handler = arrived ? Connected : Disconnected;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        // ── ID USB ───────────────────────────────────────────────────────
        /// <summary>
        /// Normalizza un ID USB in "VID_xxxx&amp;PID_yyyy". Accetta "046D:C52B", "VID_046D&amp;PID_C52B"
        /// o un DeviceID completo. Restituisce null se non riconosciuto.
        /// </summary>
        public static string NormalizeUsbId(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            Match m = VidPidLong.Match(text);
            if (!m.Success) m = VidPidShort.Match(text);
            if (!m.Success) return null;

            return string.Format("VID_{0}&PID_{1}",
                m.Groups[1].Value.ToUpperInvariant(), m.Groups[2].Value.ToUpperInvariant());
        }

        /// <summary>"VID_046D&amp;PID_C52B" → "046D:C52B".</summary>
        public static string ToShortId(string normalizedId)
        {
            return normalizedId.Substring(4, 4) + ":" + normalizedId.Substring(13, 4);
        }

        /// <summary>
        /// Dispositivi USB presenti, "VID:PID" → nomi (per la finestra di configurazione).
        /// Usa WMI: lento, da chiamare fuori dal thread UI.
        /// </summary>
        public static SortedDictionary<string, string> ListPresentDevices()
        {
            Dictionary<string, List<string>> names = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                @"SELECT DeviceID, Name FROM Win32_PnPEntity WHERE DeviceID LIKE 'USB\\VID%'"))
            using (ManagementObjectCollection results = searcher.Get())
            {
                foreach (ManagementBaseObject mo in results)
                {
                    using (mo)
                    {
                        string id = NormalizeUsbId(mo["DeviceID"] as string);
                        if (id == null) continue;

                        string       key = ToShortId(id);
                        List<string> list;
                        if (!names.TryGetValue(key, out list))
                        {
                            list = new List<string>();
                            names[key] = list;
                        }
                        string name = mo["Name"] as string;
                        if (!string.IsNullOrEmpty(name) && !list.Contains(name))
                            list.Add(name);
                    }
                }
            }

            SortedDictionary<string, string> result = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, List<string>> pair in names)
                result[pair.Key] = string.Join(", ", pair.Value.ToArray());
            return result;
        }

        // ── P/Invoke ─────────────────────────────────────────────────────
        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int   dbcc_size;
            public int   dbcc_devicetype;
            public int   dbcc_reserved;
            public Guid  dbcc_classguid;
            public short dbcc_name;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);
    }
}
