using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Free_Gamma
{
    public partial class App : Application
    {

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;


        // Prodcast a message to all process's window handles :
        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

            return handles;
        }
        // End Prodcast a message to all process's window handles





        public App()
        {
            this.Startup += Application_Startup;
            this.DispatcherUnhandledException += Application_DispatcherUnhandledException;

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }




        private void Application_Startup(object sender, StartupEventArgs e)
        {
            fastJSON.JSON.Parameters.AllowNonQuotedKeys = true;
            fastJSON.JSON.Parameters.InlineCircularReferences = true;
            fastJSON.JSON.Parameters.EnableAnonymousTypes = true;
            fastJSON.JSON.Parameters.UseEscapedUnicode = false;

            bool found = false;
            var cp = Process.GetCurrentProcess();
            var p = Process.GetProcessesByName(cp.ProcessName);
            for (int i = 0; i < p.Length; i++) {
                if (p[i].MainModule.ModuleName == cp.MainModule.ModuleName & p[i].Id != cp.Id) {

                    if (Debugger.IsAttached == true) {
                        // Another instance is running and may case confusion. Close it first.
                        Debugger.Break();
                    }

                    StringMessage.SendString(p[i].MainWindowHandle.ToInt32(), "Restore");

                    // This is needed in case the app is minimized to tray:
                    foreach (IntPtr hwnd in EnumerateProcessWindowHandles(p[i].Id)) {
                        StringMessage.SendString(hwnd.ToInt32(), "Restore");
                    }

                    SetForegroundWindow(p[i].MainWindowHandle);
                    found = true;
                    break;
                }
            }
            if (found) Environment.Exit(0);


        }



        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string t = e.Exception.Message;
            if (e.Exception.InnerException != null)
                t += Environment.NewLine + e.Exception.InnerException.Message;

            MessageBox.Show(t, "Unhandled Error", MessageBoxButton.OK, MessageBoxImage.Error);
            System.IO.File.AppendAllText(Environment.CurrentDirectory + @"\Errors.log", string.Format("d MMM yyyy  h:m tt", DateTime.Now) + Environment.NewLine + t + Environment.NewLine + Environment.NewLine);
            System.Environment.Exit(0);
        }




    }
}
