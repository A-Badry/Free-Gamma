using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Free_Gamma
{
    public partial class form_Settings : Window
    {

        private bool skip_chk_StartWithWindows = false;


        public form_Settings()
        {
            InitializeComponent();

            this.Loaded += form_Settings_Loaded;
            this.Closed += form_Settings_Closed;
            chk_StartWithWindows.Checked += chk_StartWithWindows_Checked;
            chk_StartWithWindows.Unchecked += chk_StartWithWindows_Unchecked;
            btn_Close.Click += btn_Close_Click;
        }

       

        private void form_Settings_Loaded(object sender, RoutedEventArgs e)
        {
            var k = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            var n = k.GetValueNames(); k.Close();
            skip_chk_StartWithWindows = true;
            if (n.ToList().Contains("FreeGamma"))  chk_StartWithWindows.IsChecked = true; 
            else chk_StartWithWindows.IsChecked = false ;
            skip_chk_StartWithWindows = false;

        }





        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }










        private void chk_StartWithWindows_Unchecked(object sender, RoutedEventArgs e)
        {
            if (skip_chk_StartWithWindows) return;
            if (mod_General .IsAdministrator ()==false)
            {
                MessageBox.Show("You need to start the app as administrator to change this setting.", "Notice", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                skip_chk_StartWithWindows = true;
                chk_StartWithWindows .IsChecked = true;
                skip_chk_StartWithWindows = false;
                return;
            }

            bool SwW_Found = false;
            var k = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var n = k.GetValueNames();
            if (n.ToList().Contains("FreeGamma")) SwW_Found = true;
            else SwW_Found = false;

            var p = (char)34 + Process.GetCurrentProcess().MainModule.FileName + (char)34;
            if (SwW_Found) k.DeleteValue("FreeGamma");
            k.Close();

        }




        private void chk_StartWithWindows_Checked(object sender, RoutedEventArgs e)
        {
            if (skip_chk_StartWithWindows) return;
            if (mod_General.IsAdministrator() == false)
            {
                MessageBox.Show("You need to start the app as administrator to change this setting.", "Notice", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                skip_chk_StartWithWindows = true;
                chk_StartWithWindows.IsChecked = false ;
                skip_chk_StartWithWindows = false;
                return;
            }

            bool SwW_Found = false;
            var k = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var n = k.GetValueNames();
            if (n.ToList().Contains("FreeGamma")) SwW_Found = true;
            else SwW_Found = false;

            var p = (char)34 + Process.GetCurrentProcess().MainModule.FileName + (char)34;
            if (SwW_Found==false ) k.SetValue("FreeGamma", p, RegistryValueKind.String); 
            k.Close();
        }






        private void form_Settings_Closed(object sender, System.EventArgs e)
        { 



        }






    }
}
