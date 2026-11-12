using System;
using System.Drawing;
using Insonnia.Interop;

namespace Insonnia.UI
{
    internal static class Extensions
    {
        public static Icon ToIcon(this Bitmap bitmap)
        {
            IntPtr hIcon = bitmap.GetHicon();

            try
            { return (Icon)Icon.FromHandle(hIcon).Clone(); }
            finally
            { Win32Interop.DestroyIcon(hIcon); } // cleanup unmanaged pointer
        }
    }
}
