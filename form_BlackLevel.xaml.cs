
using System.Windows;
using System.Windows.Media;

namespace Free_Gamma
{
    public partial class form_BlackLevel: Window
    {


        public form_BlackLevel()
        {
            this.InitializeComponent();

            double r = SystemParameters.PrimaryScreenHeight / 768;
            if (r > 1) pnl_1.LayoutTransform = new ScaleTransform(r, r);
        }


    }
}