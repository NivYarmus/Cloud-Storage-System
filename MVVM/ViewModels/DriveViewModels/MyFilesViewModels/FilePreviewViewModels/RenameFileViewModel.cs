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
    internal class RenameFileViewModel : ViewModelBase
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

        private string _newFileName;
        public string newFileName
        {
            get { return _newFileName; }
            set
            {
                _newFileName = value;
                OnPropertyChanged();
            }
        }

        public ICommand RenameFile { get; }
        public ICommand Cancel { get; }

        public RenameFileViewModel(Folder currentFolder, File f,
            ClientSocket clientSocket,
            NavigationStore filePreviewNavigationStore, MainFilePreviewViewModel returnToViewModel,
            SecurityManager security)
        {
            _currentFolder = currentFolder;
            _currentFile = f;

            RenameFile = new RenameFileCommand(clientSocket, security);
            Cancel = new NavigationCommand(filePreviewNavigationStore, new Func<object, ViewModelBase>((o) => returnToViewModel));
        }
    }
}
