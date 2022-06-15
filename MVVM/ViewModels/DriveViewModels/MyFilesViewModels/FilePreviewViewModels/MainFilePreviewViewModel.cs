using NivDrive.Commands;
using NivDrive.Commands.NavigationCommands;
using NivDrive.Commands.UserCommands.MyFilesCommands;
using NivDrive.MVVM.Models;
using NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels.FilePreviewViewModels;
using NivDrive.Network;
using NivDrive.Security;
using NivDrive.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels.FilePreviewViewModels
{
    internal class MainFilePreviewViewModel : ViewModelBase
    {
        public readonly Folder _currentFolder;
        public readonly File _currentFile;
        public readonly ObservableCollection<File> _files;

        public string folderPath
        {
            get { return _currentFolder.GetPathIncludeFolder(); }
        }

        public string fileName
        {
            get { return _currentFile.Name; }
        }

        public string fileExtension
        {
            get { return _currentFile.Extension; }
        }

        public string fileUploadTime
        {
            get { return DateTimeOffset.UnixEpoch.AddSeconds(_currentFile.UploadTime).ToLocalTime().ToString(); }
        }

        public string fileSize
        {
            get { return _currentFile.GetSizeToString(); }
        }

        public string fileModifyTime
        {
            get { return DateTimeOffset.UnixEpoch.AddSeconds(_currentFile.ModifyTime).ToLocalTime().ToString(); }
        }

        public ICommand DownloadFile { get; }
        public ICommand Cancel { get; }
        public ICommand DeleteFile { get; }
        public ICommand RenameFile { get; }
        public ICommand UpdateFile { get; }
        public ICommand ShareFile { get; }

        public MainFilePreviewViewModel(Folder currentFolder, ObservableCollection<File> files,
            File f, ClientSocket clientSocket,
            NavigationStore myFilesNavigationStore, MainMyFilesViewModel returnToViewModel,
            NavigationStore filePreviewNavigationStore,
            SecurityManager security)
        {
            _currentFolder = currentFolder;
            _currentFile = f;
            _files = files;

            DownloadFile = new DownloadFileCommand(clientSocket, security);
            DeleteFile = new DeleteFileCommand(clientSocket, security);
            Cancel = new NavigationCommand(myFilesNavigationStore, new Func<object, ViewModelBase>((o) => returnToViewModel));
            RenameFile = new NavigationCommand(filePreviewNavigationStore, new Func<object, ViewModelBase>((o) => new RenameFileViewModel(currentFolder, f, clientSocket, filePreviewNavigationStore, this, security)));
            ShareFile = new NavigationCommand(filePreviewNavigationStore, new Func<object, ViewModelBase>((o) => new ShareFileViewModel(currentFolder, f, clientSocket, filePreviewNavigationStore, this, security)));
            UpdateFile = new NavigationCommand(filePreviewNavigationStore, new Func<object, ViewModelBase>((o) => new UpdateFileViewModel(f, clientSocket, filePreviewNavigationStore, this, security)));
        }
    }
}
