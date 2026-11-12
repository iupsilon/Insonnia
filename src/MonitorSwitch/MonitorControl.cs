using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Insonnia.MonitorSwitch
{
    /// <summary>Esito di un comando di cambio ingresso.</summary>
    internal enum MonitorSwitchOutcome
    {
        Switched,         // comando inviato
        AlreadyActive,    // il monitor era già sull'ingresso richiesto: nessun comando
        NotConfigured,    // ID monitor o ingresso mancanti
        MonitorNotFound,  // nessun monitor corrisponde all'ID (es. mostra l'altro PC e non è più enumerato)
        Ambiguous,        // l'ID corrisponde a più monitor: per sicurezza non si tocca nulla
        DdcError          // DDC/CI non disponibile o comando rifiutato
    }

    /// <summary>
    /// Monitor fisico rilevato, con l'identità letta dall'EDID e l'ingresso attuale (VCP 0x60).
    /// </summary>
    internal sealed class MonitorInfo
    {
        public int    Index;
        public string GdiDevice;          // es. \\.\DISPLAY1 — cambia tra riavvii/docking: solo informativo
        public string InstanceId;         // es. DISPLAY\DELA1B2\5&2a3b&0&UID4352 — stabile per porta/percorso
        public string FriendlyName;       // nome modello da EDID, es. "DELL U2722D"
        public string Manufacturer;       // codice PnP produttore da EDID, es. "DEL"
        public string ProductCode;        // codice prodotto da EDID
        public string SerialNumber;       // seriale da EDID: distingue due monitor identici
        public string Description;        // descrizione restituita da dxva2
        public int    CurrentInput = -1;  // valore VCP 0x60 letto, -1 = non disponibile
        public string DdcError;           // motivo per cui il DDC/CI non risponde (null se ok)

        /// <summary>Seriale EDID utilizzabile come identificativo (alcuni monitor riportano "0" o nulla).</summary>
        public bool HasValidSerial
        {
            get { return !string.IsNullOrEmpty(SerialNumber) && SerialNumber.Trim('0', ' ').Length > 0; }
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(FriendlyName)) return FriendlyName;
                if (!string.IsNullOrEmpty(Description))  return Description;
                return GdiDevice ?? "?";
            }
        }

        /// <summary>ID consigliato per la configurazione: il seriale EDID se valido, altrimenti l'istanza.</summary>
        public string SuggestedId
        {
            get { return HasValidSerial ? SerialNumber : InstanceId; }
        }

        public string CurrentInputText
        {
            get
            {
                if (CurrentInput < 0) return null;
                // Alcuni monitor valorizzano anche il byte alto: il codice MCCS è nel byte basso.
                string text = MonitorControl.InputName(CurrentInput & 0xFF);
                if (CurrentInput > 0xFF)
                    text += string.Format(" [raw 0x{0:X}]", CurrentInput);
                return text;
            }
        }

        /// <summary>True se <paramref name="id"/> identifica questo monitor (seriale EDID o istanza dispositivo).</summary>
        public bool Matches(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            id = id.Trim();
            if (HasValidSerial && string.Equals(SerialNumber, id, StringComparison.OrdinalIgnoreCase))
                return true;
            return !string.IsNullOrEmpty(InstanceId) &&
                   string.Equals(InstanceId, id, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return string.Format("#{0}  {1}  seriale={2}  ingresso={3}  istanza={4}  schermo={5}",
                Index,
                DisplayName,
                string.IsNullOrEmpty(SerialNumber) ? "-" : SerialNumber,
                CurrentInputText ?? ("n/d (" + (DdcError ?? "?") + ")"),
                InstanceId ?? "-",
                GdiDevice ?? "-");
        }
    }

    /// <summary>
    /// Comandi DDC/CI verso i monitor (dxva2.dll), in particolare il codice VCP 0x60 (Input Source).
    /// Identifica in modo affidabile anche due monitor identici:
    /// HMONITOR → monitor collegati a quell'uscita (EnumDisplayDevices, nome interfaccia)
    /// → istanza PnP → seriale EDID (WMI, classe WmiMonitorID).
    /// Le operazioni sono sincrone e lente (centinaia di ms): vanno chiamate fuori dal thread UI.
    /// Un lock interno serializza gli accessi al bus DDC (watcher USB, menu tray, finestra di configurazione).
    /// </summary>
    internal static class MonitorControl
    {
        public  const byte VcpInputSource = 0x60;
        private const int  Attempts       = 3;    // tentativi per ogni lettura/scrittura VCP
        private const int  RetryDelayMs   = 200;  // pausa tra i tentativi: il DDC/CI è lento e a volte instabile

        private static readonly object _ddcLock = new object();

        /// <summary>Valori MCCS standard dell'ingresso, nell'ordine proposto in configurazione.</summary>
        public static readonly int[] KnownInputs = { 0x0F, 0x10, 0x11, 0x12, 0x1B, 0x03, 0x04, 0x01, 0x02 };

        /// <summary>Etichetta leggibile di un valore VCP 0x60, es. "DisplayPort 1 (0x0F)".</summary>
        public static string InputName(int value)
        {
            string name;
            switch (value)
            {
                case 0x01: name = "VGA 1";         break;
                case 0x02: name = "VGA 2";         break;
                case 0x03: name = "DVI 1";         break;
                case 0x04: name = "DVI 2";         break;
                case 0x0F: name = "DisplayPort 1"; break;
                case 0x10: name = "DisplayPort 2"; break;
                case 0x11: name = "HDMI 1";        break;
                case 0x12: name = "HDMI 2";        break;
                case 0x1B: name = "USB-C";         break;
                default:   name = null;            break;
            }
            return name != null
                ? string.Format("{0} (0x{1:X2})", name, value)
                : string.Format("0x{0:X2}", value);
        }

        // ── API pubblica ─────────────────────────────────────────────────
        /// <summary>Elenca i monitor rilevati con identità EDID e ingresso attuale.</summary>
        public static List<MonitorInfo> GetMonitors()
        {
            lock (_ddcLock)
            {
                List<PhysicalEntry> entries = OpenAll();
                try
                {
                    List<MonitorInfo> result = new List<MonitorInfo>();
                    foreach (PhysicalEntry entry in entries)
                    {
                        if (entry.HasHandle)
                        {
                            uint current, max;
                            int  error;
                            if (TryGetVcp(entry.Handle, VcpInputSource, out current, out max, out error))
                            {
                                // Difesa: un valore fuori scala non è un codice d'ingresso MCCS
                                if (current <= 0xFFFF)
                                    entry.Info.CurrentInput = (int)current;
                                else
                                    entry.Info.DdcError = string.Format("risposta VCP non valida (0x{0:X})", current);
                            }
                            else
                                entry.Info.DdcError = "GetVCPFeature " + FormatWin32Error(error);
                        }
                        result.Add(entry.Info);
                    }
                    return result;
                }
                finally
                {
                    CloseAll(entries);
                }
            }
        }

        /// <summary>
        /// Imposta l'ingresso <paramref name="input"/> (VCP 0x60) SOLO sul monitor identificato da
        /// <paramref name="monitorId"/>. Nessun ripiego: se l'ID non corrisponde a esattamente un monitor
        /// non viene inviato alcun comando. <paramref name="detail"/> descrive l'esito (es. l'errore DDC/CI).
        /// </summary>
        public static MonitorSwitchOutcome SetInput(string monitorId, int input, out string detail)
        {
            if (string.IsNullOrWhiteSpace(monitorId) || input <= 0 || input > 0xFF)
            {
                detail = "ID monitor o ingresso non configurati";
                return MonitorSwitchOutcome.NotConfigured;
            }

            lock (_ddcLock)
            {
                List<PhysicalEntry> entries = OpenAll();
                try
                {
                    List<PhysicalEntry> matches = new List<PhysicalEntry>();
                    foreach (PhysicalEntry entry in entries)
                        if (entry.Info.Matches(monitorId)) matches.Add(entry);

                    if (matches.Count == 0)
                    {
                        detail = "nessun monitor corrisponde; rilevati: " + Summarize(entries);
                        return MonitorSwitchOutcome.MonitorNotFound;
                    }
                    if (matches.Count > 1)
                    {
                        detail = string.Format("l'ID corrisponde a {0} monitor, nessun comando inviato", matches.Count);
                        return MonitorSwitchOutcome.Ambiguous;
                    }

                    PhysicalEntry target = matches[0];
                    if (!target.HasHandle)
                    {
                        detail = "handle DDC/CI non disponibile: " + (target.Info.DdcError ?? "?");
                        return MonitorSwitchOutcome.DdcError;
                    }

                    // Ingresso già attivo: nessun comando (evita sfarfallii e comandi doppi tra i due PC).
                    // Se la lettura non riesce il comando viene inviato comunque.
                    uint current, max;
                    int  error;
                    if (TryGetVcp(target.Handle, VcpInputSource, out current, out max, out error) &&
                        (current & 0xFF) == (uint)input)
                    {
                        detail = target.Info.DisplayName;
                        return MonitorSwitchOutcome.AlreadyActive;
                    }

                    for (int attempt = 1; attempt <= Attempts; attempt++)
                    {
                        if (SetVCPFeature(target.Handle, VcpInputSource, (uint)input))
                        {
                            detail = string.Format("{0}, tentativo {1}", target.Info.DisplayName, attempt);
                            return MonitorSwitchOutcome.Switched;
                        }
                        error = Marshal.GetLastWin32Error();
                        if (attempt < Attempts) Thread.Sleep(RetryDelayMs);
                    }

                    detail = "SetVCPFeature " + FormatWin32Error(error);
                    return MonitorSwitchOutcome.DdcError;
                }
                finally
                {
                    CloseAll(entries);
                }
            }
        }

        // ── enumerazione ─────────────────────────────────────────────────
        private sealed class PhysicalEntry
        {
            public MonitorInfo Info;
            public IntPtr      Handle;
            // dxva2 numera gli handle fisici da 0: IntPtr.Zero è un handle VALIDO, serve un flag a parte
            public bool        HasHandle;
        }

        private sealed class EdidInfo
        {
            public string FriendlyName;
            public string Manufacturer;
            public string ProductCode;
            public string SerialNumber;
        }

        /// <summary>Apre gli handle fisici di tutti i monitor. Vanno sempre chiusi con <see cref="CloseAll"/>.</summary>
        private static List<PhysicalEntry> OpenAll()
        {
            List<PhysicalEntry> entries = new List<PhysicalEntry>();
            Dictionary<string, EdidInfo> edid = ReadEdidInfo();

            List<IntPtr> hMonitors = new List<IntPtr>();
            MonitorEnumProc callback = delegate(IntPtr hMonitor, IntPtr hdc, IntPtr rect, IntPtr data)
            {
                hMonitors.Add(hMonitor);
                return true;
            };
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            GC.KeepAlive(callback);

            foreach (IntPtr hMonitor in hMonitors)
            {
                string       gdiDevice = GetGdiDeviceName(hMonitor);
                List<string> attached  = GetAttachedMonitorInstances(gdiDevice);

                uint               count;
                PHYSICAL_MONITOR[] physical = null;
                string             error    = null;
                if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out count))
                {
                    error = "GetNumberOfPhysicalMonitorsFromHMONITOR " + FormatWin32Error(Marshal.GetLastWin32Error());
                }
                else if (count > 0)
                {
                    physical = new PHYSICAL_MONITOR[count];
                    if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, count, physical))
                    {
                        error    = "GetPhysicalMonitorsFromHMONITOR " + FormatWin32Error(Marshal.GetLastWin32Error());
                        physical = null;
                    }
                }

                if (physical == null || physical.Length == 0)
                {
                    // Nessun handle fisico: il monitor compare in elenco ma non è comandabile.
                    MonitorInfo info = CreateInfo(entries.Count + 1, gdiDevice,
                        attached.Count == 1 ? attached[0] : null, null, edid);
                    info.DdcError = error ?? "nessun monitor fisico";
                    entries.Add(new PhysicalEntry { Info = info, Handle = IntPtr.Zero, HasHandle = false });
                    continue;
                }

                // L'ordine dei monitor fisici coincide con quello dei monitor collegati all'uscita.
                // Se i conteggi non tornano l'identità resta ignota e il monitor non è selezionabile
                // per ID: meglio nessun comando che un comando al monitor sbagliato.
                bool mapped = attached.Count == physical.Length;

                for (int i = 0; i < physical.Length; i++)
                {
                    MonitorInfo info = CreateInfo(entries.Count + 1, gdiDevice,
                        mapped ? attached[i] : null, physical[i].szPhysicalMonitorDescription, edid);
                    entries.Add(new PhysicalEntry { Info = info, Handle = physical[i].hPhysicalMonitor, HasHandle = true });
                }
            }
            return entries;
        }

        private static void CloseAll(List<PhysicalEntry> entries)
        {
            foreach (PhysicalEntry entry in entries)
            {
                if (!entry.HasHandle) continue;
                DestroyPhysicalMonitor(entry.Handle);
                entry.HasHandle = false;
            }
        }

        private static MonitorInfo CreateInfo(int index, string gdiDevice, string instanceId,
                                              string description, Dictionary<string, EdidInfo> edid)
        {
            MonitorInfo info = new MonitorInfo();
            info.Index       = index;
            info.GdiDevice   = gdiDevice;
            info.InstanceId  = instanceId;
            info.Description = description;

            EdidInfo data;
            if (instanceId != null && edid.TryGetValue(instanceId, out data))
            {
                info.FriendlyName = data.FriendlyName;
                info.Manufacturer = data.Manufacturer;
                info.ProductCode  = data.ProductCode;
                info.SerialNumber = data.SerialNumber;
            }
            return info;
        }

        private static string GetGdiDeviceName(IntPtr hMonitor)
        {
            MONITORINFOEX mi = new MONITORINFOEX();
            mi.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
            return GetMonitorInfo(hMonitor, ref mi) ? mi.szDevice : null;
        }

        /// <summary>
        /// Istanze PnP dei monitor attivi collegati all'uscita GDI indicata, nell'ordine di enumerazione.
        /// Un elemento può essere null se l'istanza non è ricavabile (l'indice resta allineato).
        /// </summary>
        private static List<string> GetAttachedMonitorInstances(string gdiDevice)
        {
            List<string> result = new List<string>();
            if (gdiDevice == null) return result;

            DISPLAY_DEVICE dd = new DISPLAY_DEVICE();
            dd.cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE));
            for (uint i = 0; EnumDisplayDevices(gdiDevice, i, ref dd, EDD_GET_DEVICE_INTERFACE_NAME); i++)
            {
                if ((dd.StateFlags & DISPLAY_DEVICE_ACTIVE) != 0)
                    result.Add(InterfaceToInstanceId(dd.DeviceID));
                dd = new DISPLAY_DEVICE();
                dd.cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE));
            }
            return result;
        }

        /// <summary>
        /// "\\?\DISPLAY#DELA1B2#5&amp;2a3b&amp;0&amp;UID4352#{e6f07b5f-...}" → "DISPLAY\DELA1B2\5&amp;2a3b&amp;0&amp;UID4352"
        /// </summary>
        private static string InterfaceToInstanceId(string interfacePath)
        {
            if (string.IsNullOrEmpty(interfacePath) ||
                !interfacePath.StartsWith(@"\\?\", StringComparison.Ordinal))
                return null;

            string s    = interfacePath.Substring(4);
            int    guid = s.LastIndexOf("#{", StringComparison.Ordinal);
            if (guid > 0) s = s.Substring(0, guid);
            return s.Replace('#', '\\');
        }

        /// <summary>Dati EDID di tutti i monitor noti a Windows, indicizzati per istanza PnP.</summary>
        private static Dictionary<string, EdidInfo> ReadEdidInfo()
        {
            Dictionary<string, EdidInfo> map = new Dictionary<string, EdidInfo>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\wmi",
                    "SELECT InstanceName, ManufacturerName, ProductCodeID, SerialNumberID, UserFriendlyName FROM WmiMonitorID"))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementBaseObject mo in results)
                    {
                        using (mo)
                        {
                            string instance = mo["InstanceName"] as string;
                            if (string.IsNullOrEmpty(instance)) continue;

                            // InstanceName = "<istanza PnP>_0": rimuove il suffisso numerico
                            int underscore = instance.LastIndexOf('_');
                            if (underscore > 0 && IsDigits(instance.Substring(underscore + 1)))
                                instance = instance.Substring(0, underscore);

                            EdidInfo data = new EdidInfo();
                            data.FriendlyName = DecodeWmiString(mo["UserFriendlyName"]);
                            data.Manufacturer = DecodeWmiString(mo["ManufacturerName"]);
                            data.ProductCode  = DecodeWmiString(mo["ProductCodeID"]);
                            data.SerialNumber = DecodeWmiString(mo["SerialNumberID"]);
                            map[instance] = data;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // WMI non disponibile: i monitor restano identificabili solo per istanza dispositivo
            }
            return map;
        }

        private static string DecodeWmiString(object value)
        {
            ushort[] chars = value as ushort[];
            if (chars == null) return null;

            StringBuilder sb = new StringBuilder(chars.Length);
            foreach (ushort c in chars)
            {
                if (c == 0) break;
                sb.Append((char)c);
            }
            string text = sb.ToString().Trim();
            return text.Length > 0 ? text : null;
        }

        private static bool IsDigits(string s)
        {
            if (s.Length == 0) return false;
            foreach (char c in s)
                if (c < '0' || c > '9') return false;
            return true;
        }

        private static string Summarize(List<PhysicalEntry> entries)
        {
            if (entries.Count == 0) return "nessuno";
            List<string> parts = new List<string>();
            foreach (PhysicalEntry entry in entries)
                parts.Add(string.Format("{0} [{1}]", entry.Info.DisplayName,
                    entry.Info.HasValidSerial ? entry.Info.SerialNumber : (entry.Info.InstanceId ?? "?")));
            return string.Join(", ", parts.ToArray());
        }

        // ── VCP ──────────────────────────────────────────────────────────
        private static bool TryGetVcp(IntPtr hPhysical, byte code, out uint current, out uint max, out int lastError)
        {
            current   = 0;
            max       = 0;
            lastError = 0;
            for (int attempt = 1; attempt <= Attempts; attempt++)
            {
                uint codeType;
                if (GetVCPFeatureAndVCPFeatureReply(hPhysical, code, out codeType, out current, out max))
                    return true;
                lastError = Marshal.GetLastWin32Error();
                if (attempt < Attempts) Thread.Sleep(RetryDelayMs);
            }
            return false;
        }

        private static string FormatWin32Error(int error)
        {
            return string.Format("0x{0:X8} {1}", error, new Win32Exception(error).Message);
        }

        // ── P/Invoke ─────────────────────────────────────────────────────
        private const uint EDD_GET_DEVICE_INTERFACE_NAME = 0x00000001;
        private const uint DISPLAY_DEVICE_ACTIVE         = 0x00000001;

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MONITORINFOEX
        {
            public int  cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DISPLAY_DEVICE
        {
            public int    cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public uint   StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        private struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize,
                                                                   [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyPhysicalMonitor(IntPtr hMonitor);

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetVCPFeatureAndVCPFeatureReply(IntPtr hMonitor, byte bVCPCode, out uint pvct,
                                                                   out uint pdwCurrentValue, out uint pdwMaximumValue);

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetVCPFeature(IntPtr hMonitor, byte bVCPCode, uint dwNewValue);
    }
}
