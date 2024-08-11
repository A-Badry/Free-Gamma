using fastJSON;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Free_Gamma
{
    public partial class ctrl_TitleBar : StackPanel
    {
        Window ParentWindow;
        SolidColorBrush HoverBackground = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200));
        SolidColorBrush CloseBackground = new SolidColorBrush(Color.FromArgb(255, 242, 112, 112));

        public event EventHandler MinimizeToTrayClick;



        public ctrl_TitleBar()
        {
            InitializeComponent();
            this.Background = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255));
            this.MouseLeftButtonDown += pnl_TitleBar_MouseLeftButtonDown;


            Title = Application.Current.MainWindow.Title;

            try  // Attempt to set the Icon automatically.
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                string n = asm.GetName().Name;
                string icon_name = ""; UnmanagedMemoryStream m = null;
                var resourceManager = new ResourceManager(asm.GetName().Name + ".g", asm);
                var resources = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
                foreach (DictionaryEntry a in resources) {
                    string t = (string)a.Key;
                    if (t.Contains("/") == false & t.EndsWith(".ico")) {
                        icon_name = t; m = (UnmanagedMemoryStream)a.Value; break;
                    }
                }
                if (icon_name != "") {
                    BitmapImage b_img = new BitmapImage();
                    b_img.BeginInit();
                    b_img.StreamSource = m;
                    b_img.EndInit();
                    Icon = b_img;
                }
            }
            catch { }


        }






        protected override void OnRender(DrawingContext dc)
        {
            if (ParentWindow == null) {
                ParentWindow = Window.GetWindow(this);
                ParentWindow.StateChanged += ParentWindow_StateChanged;
                ParentWindow.Deactivated += ParentWindow_Deactivated;
                ParentWindow.Activated += ParentWindow_Activated;
                Window.ResizeModeProperty.OverrideMetadata(ParentWindow.GetType(), new FrameworkPropertyMetadata(Window.ResizeModeProperty.DefaultMetadata.DefaultValue, new PropertyChangedCallback(PW_RMCanged)));
                ReviewControls();
            }
        }




        private void ParentWindow_Activated(object sender, EventArgs e)
        {
            pnl_Close.Opacity = 1;
            pnl_Max.Opacity = 1;
            pnl_Min.Opacity = 1;
            pnl_Tray.Opacity = 1;
            txt_AppTitle.Opacity = 1;
            mod_General.DoEvents();
            // When activating the main window by a continuous left click, the above code may
            // experience a 1 second delay, thats why the DoEvents().
        }

        private void ParentWindow_Deactivated(object sender, EventArgs e)
        {
            pnl_Close.Opacity = 0.6;
            pnl_Max.Opacity = 0.6;
            pnl_Min.Opacity = 0.6;
            pnl_Tray.Opacity = 0.6;
            txt_AppTitle.Opacity = 0.6;
        }





        void PW_RMCanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ReviewControls();
        }



        void ReviewControls()
        {
            switch (ParentWindow.ResizeMode) {
                case ResizeMode.CanResize:
                case ResizeMode.CanResizeWithGrip:
                    pnl_Max.Visibility = Visibility.Visible;
                    pnl_Min.Visibility = Visibility.Visible;
                    break;
                case ResizeMode.CanMinimize:
                    pnl_Max.Visibility = Visibility.Collapsed;
                    pnl_Min.Visibility = Visibility.Visible;
                    break;
                case ResizeMode.NoResize:
                    pnl_Max.Visibility = Visibility.Collapsed;
                    pnl_Min.Visibility = Visibility.Collapsed;
                    break;
            }
        }





        private void ParentWindow_StateChanged(object sender, EventArgs e)
        {
            switch (ParentWindow.WindowState) {
                case WindowState.Normal:
                case WindowState.Minimized: {
                        pnl_1.Margin = new Thickness(0);
                        break;
                    }
                case WindowState.Maximized: {
                        pnl_1.Margin = new Thickness(5, 7, 5, 0);
                        break;
                    }
            }
        }









        private void pnl_TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == sender) { ParentWindow.DragMove(); }
        }






        private void pnl_Close_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ParentWindow.Close();
        }

        private void pnl_Max_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ParentWindow.WindowState == WindowState.Maximized) ParentWindow.WindowState = WindowState.Normal;
            else if (ParentWindow.WindowState == WindowState.Normal) ParentWindow.WindowState = WindowState.Maximized;
        }

        private void pnl_Min_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ParentWindow.WindowState = WindowState.Minimized;
        }

        private void pnl_Tray_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //ParentWindow.IsEnabled = false ;
            //ParentWindow.Top = SystemParameters.PrimaryScreenHeight + 10;
            //ParentWindow.ShowInTaskbar = false;

            if (MinimizeToTrayClick != null) MinimizeToTrayClick.Invoke(this, EventArgs.Empty);
        }



        private void pnl_Close_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Background = CloseBackground.Clone();
        }

        private void pnl_Command_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Background = HoverBackground.Clone();
        }

        private void pnl_Command_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Background = Brushes.Transparent;
            var tooltip = (ToolTip)((Grid)sender).ToolTip;
            if (tooltip != null) { tooltip.IsOpen = false; };
        }






        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(System.String), typeof(ctrl_TitleBar), new PropertyMetadata(new PropertyChangedCallback(OnTitleChanged)));
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ctrl_TitleBar)d).txt_AppTitle.Text = (string)e.NewValue;
        }




        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(ctrl_TitleBar), new PropertyMetadata(new PropertyChangedCallback(OnIconChanged)));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ctrl_TitleBar)d).img_Icon.Source = (ImageSource)e.NewValue;
        }




        public bool CanHideToTray
        {
            get
            {
                if (pnl_Tray.Visibility == Visibility.Visible) return true; else return false;
            }
            set
            {
                if (value) { pnl_Tray.Visibility = Visibility.Visible; }
                else pnl_Tray.Visibility = Visibility.Collapsed;
            }
        }






        DispatcherTimer LMouseUpTimer = new DispatcherTimer() ;
        bool LMouseUpTimer_Subscribed = false;
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]  public static extern int GetDoubleClickTime();

        private void img_Icon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!LMouseUpTimer_Subscribed) {
                LMouseUpTimer.Interval  = new TimeSpan(0, 0, 0, 0, GetDoubleClickTime());
                LMouseUpTimer.Tick += LMouseUpTimer_Tick; LMouseUpTimer_Subscribed = true; 
            }
          
            if (LMouseUpTimer.IsEnabled) {
                LMouseUpTimer.Stop();
                Application.Current.Shutdown();
            }
            else {
                LMouseUpTimer.Start();
            }
        }

        void LMouseUpTimer_Tick(object s, EventArgs e)
        {
            LMouseUpTimer.Stop();  
        }






    }
}
