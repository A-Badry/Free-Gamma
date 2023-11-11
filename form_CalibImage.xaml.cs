using System.Windows;

namespace Free_Gamma
{
    public partial class form_CalibImage : Window
    {


        public form_CalibImage()
        {
            this.InitializeComponent();
            this.Loaded += form_CalibImage_Loaded;
        }


        private void form_CalibImage_Loaded(object sender, RoutedEventArgs e)
        {
            double m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            if (m != 1d) {
                this.Width = this.Width / m;
                this.Height = this.Height / m;
            }
        }



    }
}
