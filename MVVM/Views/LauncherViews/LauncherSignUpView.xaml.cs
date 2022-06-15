using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NivDrive.MVVM.Views.LauncherViews
{
    /// <summary>
    /// Interaction logic for LauncherSignUpView.xaml
    /// </summary>
    public partial class LauncherSignUpView : UserControl
    {
        public LauncherSignUpView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((dynamic)this.DataContext).password = ((PasswordBox)sender).Password;
        }
    }
}
