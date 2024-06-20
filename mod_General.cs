using System;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Free_Gamma
{
    public static class mod_General
    {


        internal static class Box
        {
            public static object Get(object Owner, string memberPath)
            {
                string[] n = memberPath.Split('.');
                object result = Owner;
                for (int i = 0; i < n.Length; i++) {
                    var p = result.GetType().GetProperty(n[i].Trim());
                    if (!(p is null)) { result = p.GetValue(result); }
                    else {
                        var f = result.GetType().GetField(n[i].Trim());
                        if (!(f is null)) { result = f.GetValue(result); }
                        else { throw new Exception("No such member '" + n[i].Trim() + "'"); }
                    }
                }
                return result;
            }


            public static void Set(object Owner, string memberName, object value)
            {
                var p = Owner.GetType().GetProperty(memberName);
                if (!(p is null)) { p.SetValue(Owner, value); }
                else {
                    var f = Owner.GetType().GetField(memberName);
                    if (!(f is null)) { f.SetValue(Owner, value); }
                    else { throw new Exception("No property or field with the given name."); }
                }
            }

        }





        public static bool ShowWindowOnce<WindowType>() where WindowType : Window, new()
        {
            var ws = Application.Current.Windows;
            for (int i = 0; i < ws.Count; i++) {
                if (ws[i].GetType().Name == typeof(WindowType).Name) {
                    if (ws[i].WindowState == WindowState.Minimized) ws[i].WindowState = WindowState.Normal;
                    ws[i].Activate();
                    return false;
                }
            }
            var w = new WindowType();
            w.Show();
            return true;
        }





        // ===== DoEvents for WPF ==============
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
        public static void DoEvents(ushort Cycles = 1)
        {
            for (int i = 0; i < Cycles; i++) {
                var frame = new System.Windows.Threading.DispatcherFrame();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new System.Windows.Threading.DispatcherOperationCallback(ExitFrame), frame);
                System.Windows.Threading.Dispatcher.PushFrame(frame);
            }
        }
        public static object ExitFrame(object f)
        {
            ((System.Windows.Threading.DispatcherFrame)f).Continue = false;
            return null;
        }
        // ================================





        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }






    }



    public class DpiDecorator : Decorator
    {
        public DpiDecorator()
        {
            Loaded += DpiDecorator_Loaded;
        }

        private void DpiDecorator_Loaded(object sender, RoutedEventArgs e)
        {
            var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            var dpiTransform = new ScaleTransform(1d / m.M11, 1d / m.M22);
            if (dpiTransform.CanFreeze)
                dpiTransform.Freeze();
            LayoutTransform = dpiTransform;
        }
    }


}