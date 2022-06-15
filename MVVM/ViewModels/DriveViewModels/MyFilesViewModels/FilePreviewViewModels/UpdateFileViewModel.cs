using NivDrive.Commands.NavigationCommands;
using NivDrive.Commands.UserCommands.MyFilesCommands.FilePreviewCommands;
using NivDrive.MVVM.Models;
using NivDrive.Network;
using NivDrive.Security;
using NivDrive.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels.FilePreviewViewModels
{
    internal class UpdateFileViewModel : ViewModelBase
    {
        public readonly File _currentFile;

        private string _filePath;
        public string filePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        public ICommand UpdateFile { get; }
        public ICommand Cancel { get; }

        public UpdateFileViewModel(File f, ClientSocket clientSocket,
            NavigationStore myFilesNavigationStore, MainFilePreviewViewModel returnToViewModel, SecurityManager security)
        {
            _currentFile = f;

            UpdateFile = new UpdateFileCommand(clientSocket, security);
            Cancel = new NavigationCommand(myFilesNavigationStore, new Func<object, ViewModelBase>((o) => returnToViewModel));
        }
    }
}
