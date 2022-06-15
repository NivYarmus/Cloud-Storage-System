using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NivDrive.MVVM.Views.DriveViews
{
    /// <summary>
    /// Interaction logic for DriveView.xaml
    /// </summary>
    public partial class DriveView : System.Windows.Controls.UserControl
    {
        public DriveView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to exit the app?",
                "Exit", MessageBoxButtons.OKCancel);
            if (dialogResult == DialogResult.OK)
            {
                ((dynamic)this.DataContext).Quit.Execute(null);
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to disconnect the client?",
                "Disconnect", MessageBoxButtons.OKCancel);
            if (dialogResult == DialogResult.OK)
                ((dynamic)this.DataContext).Disconnect.Execute(null);
        }
    }
}
