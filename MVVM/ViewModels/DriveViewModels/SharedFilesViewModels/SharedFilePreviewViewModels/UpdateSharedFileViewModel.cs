using NivDrive.Commands.NavigationCommands;
using NivDrive.Commands.UserCommands.SharedCommands;
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

namespace NivDrive.MVVM.ViewModels.DriveViewModels.SharedFilesViewModels.SharedFilePreviewViewModels
{
    internal class UpdateSharedFileViewModel : ViewModelBase
    {
        public readonly SharedFile _sharedFile;

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

        public UpdateSharedFileViewModel(SharedFile f,
            ClientSocket clientSocket,
            NavigationStore sharedFilesPreviewNavigationStore, MainSharedFilePreviewViewModel returnToViewModel,
            SecurityManager security)
        {
            _sharedFile = f;

            UpdateFile = new UpdateSharedFileCommand(clientSocket, security);
            Cancel = new NavigationCommand(sharedFilesPreviewNavigationStore, new Func<object, ViewModelBase>((o) => returnToViewModel));
        }
    }
}
