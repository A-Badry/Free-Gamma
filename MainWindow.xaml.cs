using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using fastJSON;
using Microsoft.VisualBasic;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Media.Imaging;

namespace Free_Gamma
{


    public partial class MainWindow : Window
    {

        private System.Windows.Threading.DispatcherTimer tmr_Adjust;
        private bool TestMode;
        private TaskbarIcon NotifyIcon = new TaskbarIcon();


        public class SSettings
        {
            public string SelectedProfile;
            public List<Profile> Profiles = new List<Profile>();

            public SSettings()
            {
                this.Profiles.Add(new Profile { Name = "- Default" });
            }
        }


        public class Profile : IComparable
        {
            public string Name;
            public int Type = 0; // 0 = Mixed channels, 1 = Separated channels
            public List<Point> GrayPoints = new List<Point>();
            public List<Point> RedPoints = new List<Point>();
            public List<Point> GreenPoints = new List<Point>();
            public List<Point> BluePoints = new List<Point>();
            public int MasterBrightness = 100;
            public int BrightnessRed = 100;
            public int BrightnessGreen = 100;
            public int BrightnessBlue = 100;

            public Profile()
            {
                var p = ctrl_RampGraph.GetDefaultPoints();
                GrayPoints.AddRange(p.ToList());
                RedPoints.AddRange(p.ToList());
                GreenPoints.AddRange(p.ToList());
                BluePoints.AddRange(p.ToList());
            }

            public int CompareTo(object obj)
            {
                // Comparer function for sort.
                return Name.CompareTo(mod_General.Box.Get(obj, "Name"));
            }
        }

        private int ScreenHDC;
        private mod_API.RAMP InitRamp;
        private mod_API.RAMP g_ramp = new mod_API.RAMP();
        private SSettings Settings = new SSettings();
        private System.Windows.Threading.DispatcherTimer tmr_BlackLevelTest;

        private bool UpdatingView;
        private bool ApplyingToScreen;

        private bool HasLoaded;







        public MainWindow()
        {
            InitializeComponent();

            var ico = new BitmapImage(new Uri("pack://application:,,,/Free Gamma;component/FreeGamma.ico", UriKind.Absolute));
            NotifyIcon.IconSource = ico;
            NotifyIcon.TrayToolTip = (UIElement) Application .Current .FindResource ("TrayToolTip");
            NotifyIcon.Visibility = Visibility.Hidden;
            NotifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;

            TitleBar.MinimizeToTrayClick += TitleBar_MinimizeToTrayClick;

            tmr_Adjust = new System.Windows.Threading.DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
            tmr_BlackLevelTest = new System.Windows.Threading.DispatcherTimer();
            mnu_Profiles_Refresh = new MenuItem() { Header = "Refresh" };
            mnu_Profiles_Tests = new MenuItem() { Header = "Tests" };
            mnu_Profiles_Rename = new MenuItem() { Header = "Rename" };
            mnu_Profiles_Duplicate = new MenuItem() { Header = "Duplicate" };
            mnu_Profiles_NewProfile = new MenuItem() { Header = "New Profile" };
            mnu_Profiles_Delete = new MenuItem() { Header = "Delete" };
            mnu_Profiles_Settings = new MenuItem() { Header = "Settings" };
            mnu_Profiles_About = new MenuItem() { Header = "About" };

            mnu_Graph_CopyFromGray = new MenuItem() { Header = "Copy from mixed-mode" };
            mnu_Graph_CopyFrom = new MenuItem() { Header = "Copy from .." };
            mnu_Graph_CopyFromRed = new MenuItem() { Header = "Red" };
            mnu_Graph_CopyFromGreen = new MenuItem() { Header = "Green" };
            mnu_Graph_CopyFromBlue = new MenuItem() { Header = "Blue" };

            mnu_Tests_CalibImage = new MenuItem() { Header = "View calibration image" };
            mnu_Tests_BlackLevel = new MenuItem() { Header = "Black level" };
            mnu_Tests_DefaultRamp = new MenuItem() { Header = "Default ramp" };

            this.cmbo_Mode.ItemsSource = new[] { "Mode: Mixed Channels", "Mode: Discreet Channels" };
            this.cmbo_Channel.ItemsSource = new[] { "Red", "Green", "Blue" };

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            this.IsVisibleChanged += MainWindow_IsVisibleChanged;
            tmr_Adjust.Tick += tmr_Adjust_Tick;
            cmbo_Profile.SelectionChanged += cmbo_Profile_SelectionChanged;
            cmbo_Mode.SelectionChanged += cmbo_Mode_SelectionChanged;
            cmbo_Channel.SelectionChanged += cmbo_Channel_SelectionChanged;
            lbl_Save.MouseDown += lbl_Save_MouseDown;
            lbl_CancelSave.MouseDown += lbl_CancelSave_MouseDown;
            lbl_SaveAs.MouseDown += lbl_SaveAs_MouseDown;
            slider_master_brightness.ValueChanged += slider_brightness_ValueChanged;
            slider_master_brightness_red.ValueChanged += slider_brightness_ValueChanged;
            slider_master_brightness_green.ValueChanged += slider_brightness_ValueChanged;
            slider_master_brightness_blue.ValueChanged += slider_brightness_ValueChanged;
            Graph.PointChanged += Graph_PointChanged;
            Graph.MouseRightButtonUp += Graph_MouseRightButtonUp;
            btn_Profiles.Click += btn_Profiles_Click;
            btn_Profiles.PreviewKeyDown += btn_Profiles_KeyDown;
            mnu_Profiles_Refresh.Click += mnu_Profiles_Refresh_Click;
            mnu_Profiles_Rename.Click += mnu_Profiles_Rename_Click;
            mnu_Profiles_NewProfile.Click += mnu_Profiles_NewProfile_Click;
            mnu_Profiles_Duplicate.Click += mnu_Profiles_Duplicate_Click;
            mnu_Profiles_Delete.Click += mnu_Profiles_Delete_Click;
            mnu_Profiles_Settings.Click += mnu_Profiles_Settings_Click;
            mnu_Tests_CalibImage.Click += mnu_Tests_CalibImage_Click;
            mnu_Tests_BlackLevel.Click += mnu_Tests_BlackLevel_Click;
            mnu_Tests_DefaultRamp.Click += mnu_Tests_DefaultRamp_Click;
            tmr_BlackLevelTest.Tick += tmr_Tests_Tick;
            btn_CalibrationImage.Click += btn_CalibrationImage_Click;
            mnu_Graph_CopyFromGray.Click += mnu_Graph_CopyFromGray_Click;
            mnu_Graph_CopyFromRed.Click += mnu_Graph_CopyFromRed_Click;
            mnu_Graph_CopyFromGreen.Click += mnu_Graph_CopyFromGreen_Click;
            mnu_Graph_CopyFromBlue.Click += mnu_Graph_CopyFromBlue_Click;
            mnu_Profiles_About.Click += mnu_Profiles_About_Click;


            double r = SystemParameters.PrimaryScreenHeight / 1080;
            if (r > 1) pnl_1.LayoutTransform = new ScaleTransform(r, r);

            var cl = Environment.GetCommandLineArgs();
            if (cl.Length > 1)
            {
                if (cl.Contains("/h"))
                {
                    MainWindow_Loaded(null, null);
                    this.Visibility = Visibility.Hidden;
                    NotifyIcon.Visibility = Visibility.Visible;
                }
            }

        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (HasLoaded) return;

            InitialieMenues();
            this.pnl_Save.Visibility = Visibility.Hidden;
            ScreenHDC = mod_API.GetDC(new IntPtr(0)).ToInt32();
            mod_API.GetDeviceGammaRamp(ScreenHDC, ref InitRamp);

            g_ramp.Red = new ushort[256];
            g_ramp.Green = new ushort[256];
            g_ramp.Blue = new ushort[256];

            LoadSettings();

            if (Settings.Profiles.Count == 2 && Settings.Profiles[1].Name == "Unnamed Profile" & string.IsNullOrEmpty(Settings.SelectedProfile))
            {
                Settings.SelectedProfile = Settings.Profiles[1].Name;
                SaveSettings();
            }
            else if (string.IsNullOrEmpty(Settings.SelectedProfile))
            {
                Settings.SelectedProfile = Settings.Profiles[0].Name;
                SaveSettings();
            }

            FillComboProfiles();

            for (int i = 0; i < this.cmbo_Profile.Items.Count; i++)
            {
                if ((string)this.cmbo_Profile.Items[i] == Settings.SelectedProfile)
                {
                    this.cmbo_Profile.SelectedIndex = i;
                    break;
                }
            }

            tmr_Adjust.Start();
            HasLoaded = true;
        }




        private void tmr_Adjust_Tick(object sender, EventArgs e)
        {
            if (!TestMode) mnu_Profiles_Refresh_Click(null, null);
        }




        // Read setting files that also contains the user profiles.
        public void LoadSettings()
        {
            Profile Profile;
            if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\Settings"))
            {
                Settings = JSON.ToObject<SSettings>(System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Settings"));
                Settings.Profiles.Insert(0, new Profile { Name = "- Default" });
                Profile = Settings.Profiles.Where(n => n.Name == Settings.SelectedProfile).ElementAtOrDefault(0);
            }
            else
            {
                Profile = new Profile() { Name = "Unnamed Profile" };
                Settings.Profiles.Add(Profile);
            }
        }


        public void FillComboProfiles()
        {
            this.cmbo_Profile.Items.Clear();
            for (int i = 0; i < Settings.Profiles.Count; i++)
                this.cmbo_Profile.Items.Add(Settings.Profiles[i].Name);
        }





        private void cmbo_Profile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Graph.IsEnabled = true;
            this.pnl_Sliders.IsEnabled = true;
            this.cmbo_Mode.IsEnabled = true;
            if (this.cmbo_Profile.SelectedIndex == -1) return;
            else if (this.cmbo_Profile.SelectedIndex == 0)
            {
                this.Graph.IsEnabled = false;
                this.pnl_Sliders.IsEnabled = false;
                this.cmbo_Mode.IsEnabled = false;
            }
            SelectAndViewProfile((string)this.cmbo_Profile.SelectedItem);
        }


        public void SelectAndViewProfile(string Name)
        {
            if (string.IsNullOrEmpty(Name)) return;
            UpdatingView = true;
            this.pnl_Save.Visibility = Visibility.Hidden;
            Settings.SelectedProfile = Name;
            var Profile = GetSelectedProfile();

            this.cmbo_Mode.SelectedIndex = -1;
            switch (Profile.Type)
            {
                case 0:
                    {
                        this.cmbo_Mode.SelectedIndex = 0;
                        for (int i = 0; i < this.Graph.PointsCount; i++)
                        {
                            var p = new Point(Profile.GrayPoints[i].X, Profile.GrayPoints[i].Y);
                            this.Graph.set_Point(i, p);
                        }
                        break;
                    }

                default:
                    {
                        this.cmbo_Mode.SelectedIndex = 1;
                        this.cmbo_Channel.SelectedIndex = 0;
                        break;
                    }
                    // Color ramp to graph will be filled at 'cmbo_Channel_SelectionChanged'
            }

            this.slider_master_brightness.Value = (double)Profile.MasterBrightness;
            this.slider_master_brightness_red.Value = (double)Profile.BrightnessRed;
            this.slider_master_brightness_green.Value = (double)Profile.BrightnessGreen;
            this.slider_master_brightness_blue.Value = (double)Profile.BrightnessBlue;

            ApplyToScreen();
            if (cmbo_Profile.SelectedIndex != 0) this.cmbo_Mode.IsEnabled = true;
            this.cmbo_Channel.IsEnabled = true;
            UpdatingView = false;
        }





        private void cmbo_Mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.cmbo_Channel.SelectedIndex = -1;
            switch (this.cmbo_Mode.SelectedIndex)
            {
                case 0:
                    {
                        this.cmbo_Channel.Visibility = Visibility.Collapsed;
                        if (!UpdatingView)
                        {
                            GetSelectedProfile().Type = 0;
                            SelectAndViewProfile((string)this.cmbo_Profile.SelectedItem);
                        }

                        break;
                    }
                case 1:
                    {
                        this.cmbo_Channel.Visibility = Visibility.Visible;
                        this.cmbo_Channel.SelectedIndex = 0;
                        if (!UpdatingView) GetSelectedProfile().Type = 1;
                        break;
                    }
            }
        }




        private void cmbo_Channel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Profile = Settings.Profiles.Where(n => (n.Name ?? "") == (Settings.SelectedProfile ?? "")).ElementAtOrDefault(0);
            for (int i = 0; i < this.Graph.PointsCount; i++)
            {
                switch (this.cmbo_Channel.SelectedIndex)
                {
                    case 0:
                        {
                            var p = new Point(Profile.RedPoints[i].X, Profile.RedPoints[i].Y);
                            this.Graph.set_Point(i, p);
                            break;
                        }
                    case 1:
                        {
                            var p = new Point(Profile.GreenPoints[i].X, Profile.GreenPoints[i].Y);
                            this.Graph.set_Point(i, p);
                            break;
                        }
                    case 2:
                        {
                            var p = new Point(Profile.BluePoints[i].X, Profile.BluePoints[i].Y);
                            this.Graph.set_Point(i, p);
                            break;
                        }
                }
            }
            if (!UpdatingView) ApplyToScreen();
        }













        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // SaveSelectedProfile()  ' to be removed later
            SaveSettings();
        }








        public void SaveSelectedProfile()
        {
            var p = Settings.Profiles.Where(n => (n.Name ?? "") == (Settings.SelectedProfile ?? "")).ElementAtOrDefault(0);
            p.Type = this.cmbo_Mode.SelectedIndex;
            if (p.Type == 0)
            {
                p.GrayPoints.Clear();
                for (int i = 0; i < this.Graph.PointsCount; i++)
                    p.GrayPoints.Add(this.Graph.get_Point(i));
            }
            else
            {
                switch (this.cmbo_Channel.SelectedIndex)
                {
                    case 0:
                        {
                            p.RedPoints.Clear();
                            break;
                        }
                    case 1:
                        {
                            p.GreenPoints.Clear();
                            break;
                        }
                    case 2:
                        {
                            p.BluePoints.Clear();
                            break;
                        }
                }
                for (int i = 0; i < this.Graph.PointsCount; i++)
                {
                    switch (this.cmbo_Channel.SelectedIndex)
                    {
                        case 0:
                            {
                                p.RedPoints.Add(this.Graph.get_Point(i));
                                break;
                            }
                        case 1:
                            {
                                p.GreenPoints.Add(this.Graph.get_Point(i));
                                break;
                            }
                        case 2:
                            {
                                p.BluePoints.Add(this.Graph.get_Point(i));
                                break;
                            }
                    }
                }
            }
            p.MasterBrightness = (int)Math.Round(this.slider_master_brightness.Value);
            p.BrightnessRed = (int)Math.Round(this.slider_master_brightness_red.Value);
            p.BrightnessGreen = (int)Math.Round(this.slider_master_brightness_green.Value);
            p.BrightnessBlue = (int)Math.Round(this.slider_master_brightness_blue.Value);
            int k = Settings.Profiles.IndexOf(Settings.Profiles.Where(n => (n.Name ?? "") == (Settings.SelectedProfile ?? "")).ElementAtOrDefault(0));
            Settings.Profiles[k] = p;
            SaveSettings();
            if (cmbo_Profile.SelectedIndex != 0) this.cmbo_Mode.IsEnabled = true;
            this.cmbo_Channel.IsEnabled = true;
        }



        public void SaveSettings()
        {
            Settings.Profiles.RemoveAt(0);
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Settings", JSON.ToNiceJSON(Settings));
            Settings.Profiles.Insert(0, new Profile { Name = "- Default" });
        }








        private void lbl_Save_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SaveSelectedProfile();
            this.pnl_Save.Visibility = Visibility.Hidden;
        }




        private void lbl_CancelSave_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int i = this.cmbo_Channel.SelectedIndex;
            SelectAndViewProfile((string)this.cmbo_Profile.SelectedItem);
            this.pnl_Save.Visibility = Visibility.Hidden;
            this.cmbo_Channel.SelectedIndex = i;
        }




        private void lbl_SaveAs_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string n = Interaction.InputBox("Profile Name");
            if (string.IsNullOrEmpty(n))
                return;

            for (int i = 0; i < Settings.Profiles.Count; i++)
            {
                if (n == Settings.Profiles[i].Name)
                {
                    Interaction.MsgBox("This Name is used", MsgBoxStyle.Exclamation);
                    return;
                }
            }

            var old_p = Settings.Profiles.Where(pp => (pp.Name ?? "") == (Settings.SelectedProfile ?? "")).ElementAtOrDefault(0);
            var p = new Profile();
            p.Name = n;
            p.Type = old_p.Type;
            p.MasterBrightness = (int)Math.Round(this.slider_master_brightness.Value);
            p.BrightnessRed = (int)Math.Round(this.slider_master_brightness_red.Value);
            p.BrightnessGreen = (int)Math.Round(this.slider_master_brightness_green.Value);
            p.BrightnessBlue = (int)Math.Round(this.slider_master_brightness_blue.Value);

            switch (p.Type)
            {
                case 0:
                    {
                        p.GrayPoints = this.Graph.Points;
                        p.RedPoints = old_p.RedPoints.ToList();
                        p.GreenPoints = old_p.GreenPoints.ToList();
                        p.BluePoints = old_p.BluePoints.ToList();
                        break;
                    }
                case 1:
                    {
                        p.GrayPoints = old_p.GrayPoints.ToList();
                        switch (this.cmbo_Channel.SelectedIndex)
                        {
                            case 0:
                                {
                                    p.RedPoints = this.Graph.Points;
                                    p.GreenPoints = old_p.GreenPoints.ToList();
                                    p.BluePoints = old_p.BluePoints.ToList();
                                    break;
                                }
                            case 1:
                                {
                                    p.RedPoints = old_p.RedPoints.ToList();
                                    p.GreenPoints = this.Graph.Points;
                                    p.BluePoints = old_p.BluePoints.ToList();
                                    break;
                                }
                            case 2:
                                {
                                    p.RedPoints = old_p.RedPoints.ToList();
                                    p.GreenPoints = old_p.GreenPoints.ToList();
                                    p.BluePoints = this.Graph.Points;
                                    break;
                                }
                        }

                        break;
                    }
            }

            Settings.Profiles.Add(p);
            Settings.Profiles.Sort();
            FillComboProfiles();
            for (int i = 0; i < this.cmbo_Profile.Items.Count; i++)
            {
                if ((string)this.cmbo_Profile.Items[i] == n)
                {
                    this.cmbo_Profile.SelectedIndex = i;
                    break;
                }
            }
            SaveSettings();
        }










        private void slider_brightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (g_ramp.Red is null)
                return;
            if (UpdatingView)
                return;

            object s = null;
            if (object.ReferenceEquals(sender, this.slider_master_brightness))
            {
                Settings.Profiles.Where(n => (n.Name ?? "") == (Settings.SelectedProfile ?? "")).ElementAtOrDefault(0).MasterBrightness = (int)Math.Round(this.slider_master_brightness.Value);
                ApplyToScreen();
            }
            else
            {
                ApplyToScreen(true);
            }
        }





        private void Graph_PointChanged(object sender, EventArgs e)
        {
            ApplyToScreen(true);
        }






        private void ApplyToScreen(bool ByUser = false)
        {
            if (g_ramp.Red is null) return;
            ApplyingToScreen = true;

            if (this.cmbo_Mode.SelectedIndex == 0)
            {
                for (int i = 0; i <= 255; i++)
                {
                    g_ramp.Red[i] = (ushort)this.Graph.get_y(i);
                    g_ramp.Green[i] = (ushort)this.Graph.get_y(i);
                    g_ramp.Blue[i] = (ushort)this.Graph.get_y(i);
                }
            }
            else
            {
                var p = GetSelectedProfile();
                switch (this.cmbo_Channel.SelectedIndex)
                {
                    case 0:
                        {
                            for (int i = 0; i <= 255; i++)
                            {
                                g_ramp.Red[i] = (ushort)this.Graph.get_y(i);
                                g_ramp.Green[i] = (ushort)this.Graph.get_yy(p.GreenPoints, i);
                                g_ramp.Blue[i] = (ushort)this.Graph.get_yy(p.BluePoints, i);
                            }

                            break;
                        }
                    case 1:
                        {
                            for (int i = 0; i <= 255; i++)
                            {
                                g_ramp.Red[i] = (ushort)this.Graph.get_yy(p.RedPoints, i);
                                g_ramp.Green[i] = (ushort)this.Graph.get_y(i);
                                g_ramp.Blue[i] = (ushort)this.Graph.get_yy(p.BluePoints, i);
                            }

                            break;
                        }
                    case 2:
                        {
                            for (int i = 0; i <= 255; i++)
                            {
                                g_ramp.Red[i] = (ushort)this.Graph.get_yy(p.RedPoints, i);
                                g_ramp.Green[i] = (ushort)this.Graph.get_yy(p.GreenPoints, i);
                                g_ramp.Blue[i] = (ushort)this.Graph.get_y(i);
                            }

                            break;
                        }
                }
            }


            this.txt_brightness_value.Text = (int)Math.Round(this.slider_master_brightness.Value) + "%";
            this.txt_red_value.Text = (int)Math.Round(this.slider_master_brightness_red.Value) + "%";
            this.txt_green_value.Text = (int)Math.Round(this.slider_master_brightness_green.Value) + "%";
            this.txt_blue_value.Text = (int)Math.Round(this.slider_master_brightness_blue.Value) + "%";

            mod_API.RAMP b_ramp;
            b_ramp.Red = new ushort[256];
            b_ramp.Green = new ushort[256];
            b_ramp.Blue = new ushort[256];
            for (int i = 0; i <= 255; i++)
            {
                b_ramp.Red[i] = (ushort)Math.Round((double)g_ramp.Red[i] * this.slider_master_brightness_red.Value / 100d * this.slider_master_brightness.Value / 100d);
                b_ramp.Green[i] = (ushort)Math.Round((double)g_ramp.Green[i] * this.slider_master_brightness_green.Value / 100d * this.slider_master_brightness.Value / 100d);
                b_ramp.Blue[i] = (ushort)Math.Round((double)g_ramp.Blue[i] * this.slider_master_brightness_blue.Value / 100d * this.slider_master_brightness.Value / 100d);
            }
            bool retVal = mod_API.SetDeviceGammaRamp(ScreenHDC, ref b_ramp);

            if (ByUser)
            {
                this.pnl_Save.Visibility = Visibility.Visible;
                this.cmbo_Mode.IsEnabled = false;
                this.cmbo_Channel.IsEnabled = false;
            }
        }



        private void SetGrapgValueToRamp(ref mod_API.RAMP ramp, ref int x, ref int y)
        {
            switch (this.cmbo_Mode.SelectedIndex)
            {
                case 0:
                    {
                        g_ramp.Red[x] = (ushort)y;
                        g_ramp.Green[x] = (ushort)y;
                        g_ramp.Blue[x] = (ushort)y;
                        break;
                    }
                case 1:
                    {
                        switch (this.cmbo_Channel.SelectedIndex)
                        {
                            case 0:
                                {
                                    g_ramp.Red[x] = (ushort)y;
                                    break;
                                }
                            case 1:
                                {
                                    g_ramp.Green[x] = (ushort)y;
                                    break;
                                }
                            case 2:
                                {
                                    g_ramp.Blue[x] = (ushort)y;
                                    break;
                                }
                        }

                        break;
                    }
            }
        }







        // lllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll
        // lllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll
        // lllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll


        private ContextMenu mnu_Profiles = new ContextMenu();
        private MenuItem mnu_Profiles_Refresh;
        private MenuItem mnu_Profiles_Tests;
        private MenuItem mnu_Profiles_Rename;
        private MenuItem mnu_Profiles_Duplicate;
        private MenuItem mnu_Profiles_NewProfile;
        private MenuItem mnu_Profiles_Delete;
        private MenuItem mnu_Profiles_Settings;
        private MenuItem mnu_Profiles_About;

        private ContextMenu mnu_Graph = new ContextMenu();
        private MenuItem mnu_Graph_CopyFromGray;
        private MenuItem mnu_Graph_CopyFrom;
        private MenuItem mnu_Graph_CopyFromRed;
        private MenuItem mnu_Graph_CopyFromGreen;
        private MenuItem mnu_Graph_CopyFromBlue;
        private MenuItem mnu_Tests_CalibImage;
        private MenuItem mnu_Tests_BlackLevel;
        private MenuItem mnu_Tests_DefaultRamp;


        public void InitialieMenues()
        {
            TextOptions.SetTextFormattingMode(mnu_Profiles, TextFormattingMode.Display);
            TextOptions.SetTextFormattingMode(mnu_Graph, TextFormattingMode.Display);

            mnu_Profiles.Items.Add(mnu_Profiles_Rename);
            mnu_Profiles.Items.Add(mnu_Profiles_Duplicate);
            mnu_Profiles.Items.Add(mnu_Profiles_NewProfile);
            mnu_Profiles.Items.Add(new Separator());
            mnu_Profiles.Items.Add(mnu_Profiles_Refresh);
            mnu_Profiles.Items.Add(mnu_Profiles_Tests);
            mnu_Profiles.Items.Add(new Separator());
            mnu_Profiles.Items.Add(mnu_Profiles_Delete);
            mnu_Profiles.Items.Add(new Separator());
            mnu_Profiles.Items.Add(mnu_Profiles_Settings);
            mnu_Profiles.Items.Add(new Separator());
            mnu_Profiles.Items.Add(mnu_Profiles_About);

            mnu_Graph.Items.Add(mnu_Graph_CopyFromGray);
            mnu_Graph.Items.Add(mnu_Graph_CopyFrom);
            mnu_Graph_CopyFrom.Items.Add(mnu_Graph_CopyFromRed);
            mnu_Graph_CopyFrom.Items.Add(mnu_Graph_CopyFromGreen);
            mnu_Graph_CopyFrom.Items.Add(mnu_Graph_CopyFromBlue);

            mnu_Profiles_Tests.Items.Add(mnu_Tests_CalibImage);
            mnu_Profiles_Tests.Items.Add(mnu_Tests_BlackLevel);
            mnu_Profiles_Tests.Items.Add(mnu_Tests_DefaultRamp);
        }


        private void btn_Profiles_Click(object sender, RoutedEventArgs e)
        {
            mnu_Profiles_Rename.IsEnabled = true;
            mnu_Profiles_Delete.IsEnabled = true;
            mnu_Profiles_Duplicate.IsEnabled = true;
            if (this.cmbo_Profile.Items.Count == 0 | this.cmbo_Profile.SelectedIndex == 0)
            {
                mnu_Profiles_Rename.IsEnabled = false;
                mnu_Profiles_Delete.IsEnabled = false;
                mnu_Profiles_Duplicate.IsEnabled = false;
            }
            if (this.cmbo_Profile.Items.Count == 1)
                mnu_Profiles_Delete.IsEnabled = false;

            mnu_Profiles.PlacementTarget = this.btn_Profiles;
            mnu_Profiles.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            mnu_Profiles.IsOpen = true;
        }



        private void mnu_Profiles_Refresh_Click(object sender, RoutedEventArgs e)
        {
            ApplyToScreen();
        }




        private void mnu_Profiles_Rename_Click(object sender, RoutedEventArgs e)
        {
            string new_name = Interaction.InputBox("Profile Name", DefaultResponse: this.cmbo_Profile.Text);
            if (string.IsNullOrEmpty(new_name))
                return;
            int i;
            for (i = 0; i < Settings.Profiles.Count; i++)
            {
                if (new_name == Settings.Profiles[i].Name & Settings.Profiles[i].Name != this.cmbo_Profile.Text)
                {
                    Interaction.MsgBox("This name is used.", MsgBoxStyle.Exclamation);
                    return;
                }
            }

            Settings.Profiles.Where(n => n.Name == Settings.SelectedProfile).ElementAtOrDefault(0).Name = new_name;
            Settings.Profiles.Sort();
            SaveSettings();
            FillComboProfiles();
            this.cmbo_Profile.SelectedItem = new_name;
        }



        private void mnu_Profiles_NewProfile_Click(object sender, RoutedEventArgs e)
        {
            string n = Interaction.InputBox("Profile Name");
            if (string.IsNullOrEmpty(n))
                return;

            for (int i = 0; i < Settings.Profiles.Count; i++)
            {
                if (n == Settings.Profiles[i].Name)
                {
                    Interaction.MsgBox("This name is used", MsgBoxStyle.Exclamation);
                    return;
                }
            }

            var p = new Profile() { Name = n };
            Settings.Profiles.Add(p);
            Settings.Profiles.Sort();
            SaveSettings();
            FillComboProfiles();
            for (int i = 0; i < this.cmbo_Profile.Items.Count; i++)
            {
                if ((string)this.cmbo_Profile.Items[i] == n)
                {
                    this.cmbo_Profile.SelectedIndex = i;
                    break;
                }
            }
        }




        private void mnu_Profiles_Duplicate_Click(object sender, RoutedEventArgs e)
        {
            var p = JSON.ToObject<Profile>(JSON.ToJSON(Settings.Profiles.Where(n => n.Name == Settings.SelectedProfile).ElementAtOrDefault(0)));
            p.Name = "Copy of " + p.Name;

            int x = 0;

            for (int i = 0; i < Settings.Profiles.Count; i++)
            {
                if (Settings.Profiles[i].Name.StartsWith(p.Name))
                {
                    x += 1;
                }
            }
            if (x > 0)
                p.Name = p.Name + " - " + x;

            Settings.Profiles.Add(p);
            Settings.Profiles.Sort();
            SaveSettings();
            FillComboProfiles();
            for (int i = 0; i < this.cmbo_Profile.Items.Count; i++)
            {
                if ((string)this.cmbo_Profile.Items[i] == p.Name)
                {
                    this.cmbo_Profile.SelectedIndex = i;
                    break;
                }
            }
        }



        private void mnu_Profiles_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Interaction.MsgBox("Delete profile: " + this.cmbo_Profile.Text, MsgBoxStyle.Exclamation | MsgBoxStyle.YesNo) != MsgBoxResult.Yes)
                return;

            Settings.Profiles.Remove(Settings.Profiles.Where(n => n.Name == Settings.SelectedProfile).ElementAtOrDefault(0));
            int i = this.cmbo_Profile.SelectedIndex;
            FillComboProfiles();
            if (i > 0)
                i -= 1;
            this.cmbo_Profile.SelectedIndex = i;
            SaveSettings();
        }




        private void mnu_Tests_CalibImage_Click(object sender, RoutedEventArgs e)
        {
            // Dim f As New form_CalibImage
            // f.Show()

            int found = -1;
            for (int i = 0; i < Application.Current.Windows.Count; i++)
            {
                if (Application.Current.Windows[i].Title == "Calibration Image")
                {
                    found = i;
                    break;
                }
            }
            if (found == -1)
            {
                var f = new form_CalibImage();
                f.Show();
            }
            else
            {
                if (Application.Current.Windows[found].WindowState == WindowState.Minimized) Application.Current.Windows[found].WindowState = WindowState.Normal;
                Application.Current.Windows[found].Activate();
            }
        }



        private void mnu_Tests_BlackLevel_Click(object sender, RoutedEventArgs e)
        {
            // tmr_BlackLevelTest.Tag = sender
            // tmr_BlackLevelTest.Interval = New TimeSpan(0, 0, 0, 0, 50)
            // MsgBox("Screen will go black (pixels will turn black with screen backlighting is on)." & vbCrLf & vbCrLf & "The brighter the screen, the worse black level it has." & vbCrLf & vbCrLf & "Press any key to return.", MsgBoxStyle.Information, "Black level test")
            // TestMode = True
            // tmr_BlackLevelTest.Start()

            int found = -1;
            for (int i = 0; i < Application.Current.Windows.Count; i++)
            {
                if (Application.Current.Windows[i].Title == "Black Level Test")
                {
                    found = i;
                    break;
                }
            }
            if (found == -1)
            {
                var f = new form_BlackLevel();
                f.WindowState = WindowState.Maximized;
                f.Show();
            }
            else
            {
                if (Application.Current.Windows[found].WindowState == WindowState.Minimized)
                    Application.Current.Windows[found].WindowState = WindowState.Normal;
                Application.Current.Windows[found].Activate();
            }
        }




        private void mnu_Tests_DefaultRamp_Click(object sender, RoutedEventArgs e)
        {
            mod_API.RAMP b_ramp;
            b_ramp.Red = new ushort[256];
            b_ramp.Green = new ushort[256];
            b_ramp.Blue = new ushort[256];
            for (int i = 0; i <= 255; i++)
            {
                b_ramp.Red[i] = (ushort)(255 * i);
                b_ramp.Green[i] = (ushort)(255 * i);
                b_ramp.Blue[i] = (ushort)(255 * i);
            }
            TestMode = true;
            bool retVal = mod_API.SetDeviceGammaRamp(ScreenHDC, ref b_ramp);
            Interaction.MsgBox("Press OK to exit.");
            this.slider_brightness_ValueChanged(this.slider_master_brightness, (RoutedPropertyChangedEventArgs<double>)null);
            TestMode = false;
        }





        // lllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll
        // lllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll
        // lllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll


        private bool BlackLevelTest;

        private void tmr_Tests_Tick(object sender, EventArgs e)
        {
            if (ReferenceEquals(tmr_BlackLevelTest.Tag, mnu_Tests_BlackLevel))
            {
                if (!BlackLevelTest)
                {
                    mod_API.RAMP b_ramp;
                    b_ramp.Red = new ushort[256];
                    b_ramp.Green = new ushort[256];
                    b_ramp.Blue = new ushort[256];
                    for (int i = 0; i <= 255; i++)
                    {
                        b_ramp.Red[i] = 0;
                        b_ramp.Green[i] = 0;
                        b_ramp.Blue[i] = 0;
                    }
                    bool retVal = mod_API.SetDeviceGammaRamp(ScreenHDC, ref b_ramp);
                    BlackLevelTest = true;
                }
                else if (IsKeyDown())
                {
                    tmr_BlackLevelTest.Stop();
                    BlackLevelTest = false;
                    TestMode = false;
                    ApplyToScreen();
                }
            }
        }





        private bool IsKeyDown()
        {
            var all = Enum.GetValues(typeof(Key)).Cast<Key>();
            for (int i = 0; i < all.Count(); i++)
            {
                Key k = (Key)all.ElementAt(i);
                if (k != Key.None)
                {
                    if (Keyboard.IsKeyDown(k)) return true;
                }
            }
            return false;
        }



        private void btn_Profiles_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter | e.Key == Key.Space) & BlackLevelTest)
            {
                e.Handled = true;
            }
        }




        private void btn_CalibrationImage_Click(object sender, RoutedEventArgs e)
        {
            // Dim f As New form_CalibImage
            // f.Show()

            mod_General.ShowWindowOnce<form_CalibImage>();
        }



        private void Graph_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            mnu_Graph.Items.Clear();
            mnu_Graph_CopyFrom.Items.Clear();
            if (this.cmbo_Mode.SelectedIndex == 1)
            {
                mnu_Graph.Items.Add(mnu_Graph_CopyFromGray);
                mnu_Graph.Items.Add(mnu_Graph_CopyFrom);
                if (this.cmbo_Channel.SelectedIndex != 0)
                    mnu_Graph_CopyFrom.Items.Add(mnu_Graph_CopyFromRed);
                if (this.cmbo_Channel.SelectedIndex != 1)
                    mnu_Graph_CopyFrom.Items.Add(mnu_Graph_CopyFromGreen);
                if (this.cmbo_Channel.SelectedIndex != 2)
                    mnu_Graph_CopyFrom.Items.Add(mnu_Graph_CopyFromBlue);
            }

            if (mnu_Graph.Items.Count == 0)
                return;
            mnu_Graph.IsOpen = true;
        }



        private void mnu_Graph_CopyFromGray_Click(object sender, RoutedEventArgs e)
        {
            var p = Settings.Profiles.Where(n => (n.Name ?? "") == (Settings.SelectedProfile ?? "")).ElementAtOrDefault(0);
            for (int i = 0; i < p.GrayPoints.Count; i++)
            {
                switch (this.cmbo_Channel.SelectedIndex)
                {
                    case 0:
                        {
                            p.RedPoints[i] = new Point(p.GrayPoints[i].X, p.GrayPoints[i].Y);
                            break;
                        }
                    case 1:
                        {
                            p.GreenPoints[i] = new Point(p.GrayPoints[i].X, p.GrayPoints[i].Y);
                            break;
                        }
                    case 2:
                        {
                            p.BluePoints[i] = new Point(p.GrayPoints[i].X, p.GrayPoints[i].Y);
                            break;
                        }
                }
                this.Graph.set_Point(i, p.GrayPoints[i]);
            }
            ApplyToScreen();
        }

        private void mnu_Graph_CopyFromRed_Click(object sender, RoutedEventArgs e)
        {
            var p = Settings.Profiles.Where(n => (n.Name ?? "") == (Settings.SelectedProfile ?? "")).ElementAtOrDefault(0);
            for (int i = 0; i < p.RedPoints.Count; i++)
            {
                switch (this.cmbo_Channel.SelectedIndex)
                {
                    case 1:
                        {
                            p.GreenPoints[i] = new Point(p.RedPoints[i].X, p.RedPoints[i].Y);
                            break;
                        }
                    case 2:
                        {
                            p.BluePoints[i] = new Point(p.RedPoints[i].X, p.RedPoints[i].Y);
                            break;
                        }
                }
                this.Graph.set_Point(i, p.RedPoints[i]);
            }
            ApplyToScreen();
        }

        private void mnu_Graph_CopyFromGreen_Click(object sender, RoutedEventArgs e)
        {
            var p = Settings.Profiles.Where(n => (n.Name ?? "") == (Settings.SelectedProfile ?? "")).ElementAtOrDefault(0);
            for (int i = 0; i < p.GreenPoints.Count; i++)
            {
                switch (this.cmbo_Channel.SelectedIndex)
                {
                    case 0:
                        {
                            p.RedPoints[i] = new Point(p.GreenPoints[i].X, p.GreenPoints[i].Y);
                            break;
                        }
                    case 2:
                        {
                            p.BluePoints[i] = new Point(p.GreenPoints[i].X, p.GreenPoints[i].Y);
                            break;
                        }
                }
                this.Graph.set_Point(i, p.GreenPoints[i]);
            }
            ApplyToScreen();
        }

        private void mnu_Graph_CopyFromBlue_Click(object sender, RoutedEventArgs e)
        {
            var p = Settings.Profiles.Where(n => (n.Name ?? "") == (Settings.SelectedProfile ?? "")).ElementAtOrDefault(0);
            for (int i = 0; i < p.BluePoints.Count; i++)
            {
                switch (this.cmbo_Channel.SelectedIndex)
                {
                    case 0:
                        {
                            p.RedPoints[i] = new Point(p.BluePoints[i].X, p.BluePoints[i].Y);
                            break;
                        }
                    case 1:
                        {
                            p.GreenPoints[i] = new Point(p.BluePoints[i].X, p.BluePoints[i].Y);
                            break;
                        }
                }
                this.Graph.set_Point(i, p.BluePoints[i]);
            }
            ApplyToScreen();
        }




        public Profile GetSelectedProfile()
        {
            return Settings.Profiles.Where(p => (p.Name ?? "") == (Settings.SelectedProfile ?? "")).ElementAtOrDefault(0);
        }





        private void mnu_Profiles_Settings_Click(object sender, RoutedEventArgs e)
        {
            var f = new form_Settings();
            f.ShowDialog();
            f.Close();
        }





        private async void mnu_Profiles_About_Click(object sender, RoutedEventArgs e)
        {
            mnu_Profiles.IsOpen = false;

            var f = new ctrl_About();
            var a = new System.Windows.Controls.Primitives.Popup() { Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse, PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade, StaysOpen = false, AllowsTransparency = true };
            a.Child = f;
            a.IsOpen = true;

        }





        private void TitleBar_MinimizeToTrayClick(object sender, EventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Visible;
            NotifyIcon.TrayToolTip.Visibility = Visibility.Visible;
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            NotifyIcon.TrayToolTip.Visibility = Visibility.Collapsed;
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)  NotifyIcon.Visibility = Visibility.Hidden;
        }





    }
}