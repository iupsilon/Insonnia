using System;
using System.Runtime.InteropServices;

namespace Insonnia
{
    [FlagsAttribute]
    internal enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,

        ES_SYSTEM_REQUIRED = 0x00000001
        // Legacy flag, should not be used.
        // ES_USER_PRESENT = 0x00000004
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    internal static class Win32Interop
    {
        // https://stackoverflow.com/questions/49045701/prevent-screen-from-sleeping-with-c-sharp
        // Define other methods and classes here
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public extern static bool DestroyIcon(IntPtr handle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        /// <summary>Millisecondi trascorsi dall'ultimo input tastiera/mouse. 0 se fallisce.</summary>
        public static uint GetIdleTime()
        {
            LASTINPUTINFO lii = new LASTINPUTINFO();
            lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
            if (!GetLastInputInfo(ref lii)) return 0;
            // unchecked: la sottrazione resta corretta anche al wrap a 32 bit di TickCount (~49.7 giorni)
            return unchecked((uint)Environment.TickCount - lii.dwTime);
        }
    }
}
