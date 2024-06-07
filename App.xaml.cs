using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Free_Gamma
{
    public partial class App : Application
    {

        static Mutex mutex = new Mutex(true, "{C58C5013-A8A5-484C-A565-ED5ABA17270C}");

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
                if (mutex.WaitOne(TimeSpan.Zero, true))
                    mutex.ReleaseMutex();
                else
                {
                    var cp = Process.GetCurrentProcess();
                    var p = Process.GetProcessesByName(cp.ProcessName);
                    for (int i = 0; i < p.Length; i++)
                    {
                        if (p[i] != cp & p[i].MainModule.FileName == cp.MainModule.FileName)
                        {
                            ShowWindow(p[i].MainWindowHandle, SW_SHOW);
                            SetForegroundWindow(p[i].MainWindowHandle);
                            break;
                        }
                    }
                    Environment.Exit(0);
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
