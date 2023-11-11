using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

            if (Debugger.IsAttached == false) {
                var current_proc_name = Process.GetCurrentProcess().ProcessName;
                var current_proc_hwnd = Process.GetCurrentProcess().MainWindowHandle;
                var processes = Process.GetProcesses();
                for (int i = 0; i < processes.Count(); i++) {
                    if (processes[i].ProcessName == current_proc_name) {
                        if (processes[i].MainWindowHandle != current_proc_hwnd) {
                            ShowWindow(processes[i].MainWindowHandle, SW_SHOW);
                            SetForegroundWindow(processes[i].MainWindowHandle);
                            Application.Current.Shutdown();
                        }
                    }
                }
            }
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
