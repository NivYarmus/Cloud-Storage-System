using NivDrive.Commands.NavigationCommands;
using NivDrive.Commands.UserCommands.SharedCommands;
using NivDrive.MVVM.Models;
using NivDrive.Network;
using NivDrive.Security;
using NivDrive.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.DriveViewModels.SharedFilesViewModels.SharedFilePreviewViewModels
{
    internal class MainSharedFilePreviewViewModel : ViewModelBase
    {
        public readonly ObservableCollection<SharedFile> _files;
        public readonly SharedFile _sharedFile;

        public string username
        {
            get { return _sharedFile.Username; }
        }

        public string fileName
        {
            get { return _sharedFile.Name; }
        }

        public string fileExtension
        {
            get { return _sharedFile.Extension; }
        }

        public string fileSize
        {
            get { return _sharedFile.GetSizeToString(); }
        }

        public string fileModifyTime
        {
            get { return DateTimeOffset.UnixEpoch.AddSeconds(_sharedFile.ModifyTime).ToLocalTime().ToString(); }
        }

        public string fileShareTime
        {
            get { return DateTimeOffset.UnixEpoch.AddSeconds(_sharedFile.ShareTime).ToLocalTime().ToString(); }
        }

        public ICommand DownloadFile { get; }
        public ICommand RemoveFile { get; }
        public ICommand RenameFile { get; }
        public ICommand UpdateFile { get; }
        public ICommand Cancel { get; }

        public MainSharedFilePreviewViewModel(ObservableCollection<SharedFile>  files, SharedFile sharedFile, ClientSocket clientSocket,
            NavigationStore sharedFilesNavigationStore, MainSharedFilesViewModel returnToViewModel, 
            NavigationStore sharedFilePreviewNavigationStore, SecurityManager security)
        {
            _files = files;
            _sharedFile = sharedFile;

            DownloadFile = new DownloadSharedFileCommand(clientSocket, security);
            RemoveFile = new RemoveSharedFileCommand(clientSocket, security);
            RenameFile = new NavigationCommand(sharedFilePreviewNavigationStore, new Func<object, ViewModelBase>((o) => new RenameSharedFileViewModel(_sharedFile, clientSocket, sharedFilePreviewNavigationStore, this, security)));
            UpdateFile = new NavigationCommand(sharedFilePreviewNavigationStore, new Func<object, ViewModelBase>((o) => new UpdateSharedFileViewModel(_sharedFile, clientSocket, sharedFilePreviewNavigationStore, this, security)));
            Cancel = new NavigationCommand(sharedFilesNavigationStore, new Func<object, ViewModelBase>((o) => returnToViewModel));
        }
    }
}
