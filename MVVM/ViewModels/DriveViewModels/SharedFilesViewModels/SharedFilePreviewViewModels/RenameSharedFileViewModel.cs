using NivDrive.Commands.NavigationCommands;
using NivDrive.Commands.UserCommands.SharedCommands;
using NivDrive.MVVM.Models;
using NivDrive.Network;
using NivDrive.Security;
using NivDrive.Stores;
using System;
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.DriveViewModels.SharedFilesViewModels.SharedFilePreviewViewModels
{
    internal class RenameSharedFileViewModel : ViewModelBase
    {
        public readonly SharedFile _sharedFile;

        public string fileName
        {
            get { return _sharedFile.Name; }
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

        public RenameSharedFileViewModel(SharedFile f,
            ClientSocket clientSocket,
            NavigationStore sharedFilesPreviewNavigationStore, MainSharedFilePreviewViewModel returnToViewModel,
            SecurityManager security)
        {
            _sharedFile = f;

            RenameFile = new RenameSharedFileCommand(clientSocket, security);
            Cancel = new NavigationCommand(sharedFilesPreviewNavigationStore, new Func<object, ViewModelBase>((o) => returnToViewModel));
        }
    }
}
