using System;
using System.Runtime.InteropServices;
using System.Text;

namespace  Free_Gamma
{
    internal class StringMessage
    {
        // Class to facilitate sending and receiving string messages between windows.

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        public const int WM_COPYDATA = 0x4A;

        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }



        public static int SendString(int hWnd, string message)
        {
            byte[] sarr = Encoding.Default.GetBytes(message);
            int len = sarr.Length;
            COPYDATASTRUCT cds;
            cds.dwData = (IntPtr)100;
            cds.lpData = message;
            cds.cbData = len ;
            return SendMessage(hWnd, WM_COPYDATA, 0, ref cds);
        }




        public static string ReceiveString(IntPtr lParam)
        {
            var struc = Marshal.PtrToStructure<COPYDATASTRUCT>(lParam);
            return  struc.lpData;
        }




    }
}
