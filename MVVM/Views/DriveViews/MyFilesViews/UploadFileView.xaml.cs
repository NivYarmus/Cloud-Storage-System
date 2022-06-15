using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace NivDrive.MVVM.Views.DriveViews.MyFilesViews
{
    /// <summary>
    /// Interaction logic for UploadFileView.xaml
    /// </summary>
    public partial class UploadFileView : UserControl
    {
        public UploadFileView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() is not null)
                UploadFilePath.Text = openFileDialog.FileName;
        }
    }
}
