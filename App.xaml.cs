using SingleInstanceWPFApp;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Free_Gamma
{
    public partial class App : Application
    {

       


        public App()
        {
            SingleInstanceWPFApp.SingleInstanceApp.StartWatcher("2DE97815-F265-4ED8-B27F-CFA131ADDA03");

            this.Startup += Application_Startup;
            this.DispatcherUnhandledException += Application_DispatcherUnhandledException;
        }




        private void Application_Startup(object sender, StartupEventArgs e)
        {
            fastJSON.JSON.Parameters.AllowNonQuotedKeys = true;
            fastJSON.JSON.Parameters.InlineCircularReferences = true;
            fastJSON.JSON.Parameters.EnableAnonymousTypes = true;
            fastJSON.JSON.Parameters.UseEscapedUnicode = false;
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
