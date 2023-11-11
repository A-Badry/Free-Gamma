using System;
using System.Runtime.InteropServices;

namespace Free_Gamma
{

    static class mod_API
    {


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Red;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Green;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public ushort[] Blue;
        }

        [DllImport("gdi32.dll")]
        public static extern bool SetDeviceGammaRamp(int hdc, ref RAMP ramp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("gdi32.dll")]
        public static extern bool GetDeviceGammaRamp(int hdc, ref RAMP lpv);
        // <DllImport("user32.dll")>
        // Public Function GetKeyboardState(KeyStates As Byte()) As Boolean
        // End Function



    }
}