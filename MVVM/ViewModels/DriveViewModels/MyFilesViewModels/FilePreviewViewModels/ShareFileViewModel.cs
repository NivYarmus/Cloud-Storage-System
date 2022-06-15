using NivDrive.Commands.NavigationCommands;
using NivDrive.Commands.UserCommands;
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
    internal class ShareFileViewModel : ViewModelBase
    {
        public readonly Folder _currentFolder;
        public readonly File _currentFile;

        public string folderPath
        {
            get { return _currentFolder.GetPathIncludeFolder(); }
        }

        public string fileName
        {
            get { return _currentFile.Name; }
        }

        private string _shareUsername;
        public string ShareUsername
        {
            get { return _shareUsername; }
            set
            {
                _shareUsername = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShareFile { get; }
        public ICommand Cancel { get; }

        public ShareFileViewModel(Folder currentFolder, File f,
            ClientSocket clientSocket,
            NavigationStore filePreviewNavigationStore, MainFilePreviewViewModel returnToViewModel,
            SecurityManager security)
        {
            _currentFolder = currentFolder;
            _currentFile = f;

            ShareFile = new ShareFileCommand(clientSocket, security);
            Cancel = new NavigationCommand(filePreviewNavigationStore, new Func<object, ViewModelBase>((o) => returnToViewModel));
        }
    }
}
