using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace SingleInstanceWPFApp
{

    // Class to maintain single instance app functionality for WPF apps.

    internal class SingleInstanceApp
    {


        // StartWatcher function credit: https://stackoverflow.com/a/52720547/1470776 with modifications.

        public static void StartWatcher(string UniqueAppID)
        {
            if (string.IsNullOrEmpty(UniqueAppID)) {
                throw new ArgumentNullException("UniqueAppID can't be null or empty.");
            }
            string UniqueEventName = UniqueAppID;
            EventWaitHandle eventWaitHandle;

            try {
                eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);
                eventWaitHandle.Set();

                if (Debugger.IsAttached) {
                    throw new Exception("Another instance is running, close it first!");
                }
                Environment.Exit(0);
            }
            catch (WaitHandleCannotBeOpenedException) {
                eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }

            new Task(() =>
            {
                while (eventWaitHandle.WaitOne()) {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                      {
                          if (!Application.Current.MainWindow.Equals(null)) {
                              var mw = Application.Current.MainWindow;
                              var mwH = new WindowInteropHelper(mw).Handle;

                              if (IsWindowVisible(mwH ) == false & mw.IsVisible == true) ShowWindow(mwH, 9); // The window was hidden using WinAPI functions.
                              else if (mw.IsVisible == false) mw.Visibility = Visibility.Visible; // The window was hidden using WPF functions.

                              if (mw.WindowState == WindowState.Minimized) {
                                  mw.WindowState = WindowState.Normal;
                              }

                              SetForegroundWindowInternal(new WindowInteropHelper(mw).Handle);
                          }
                      }));
                }
            }, TaskCreationOptions.LongRunning)
            .Start();
        }





        // Function to force activate a window
        // credit: https://www.codeproject.com/Tips/76427/How-to-bring-window-to-top-with-SetForegroundWindo

        private static void SetForegroundWindowInternal(IntPtr hWnd)
        {
            if (!IsWindow(hWnd)) return;

            byte[] keyState = new byte[256];
            // To unlock SetForegroundWindow, we need to imitate Alt pressing
            if (GetKeyboardState(keyState)) {
                if ((keyState[(int)VirtualKeyStates.VK_MENU] & 0x80) == 0) {
                    keybd_event((byte)VirtualKeyStates.VK_MENU, 0, KEYEVENTF_EXTENDEDKEY, 0);
                }
            }

            SetForegroundWindow(hWnd);

            if (GetKeyboardState(keyState)) {
                if ((keyState[(int)VirtualKeyStates.VK_MENU] & 0x80) == 0) {
                    keybd_event((byte)VirtualKeyStates.VK_MENU, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private enum VirtualKeyStates : int
        {
            VK_MENU = 0x12
        }

        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 2;






    }
}
