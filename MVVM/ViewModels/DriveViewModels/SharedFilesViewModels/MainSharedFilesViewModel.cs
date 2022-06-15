using NivDrive.Commands;
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
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.DriveViewModels.SharedFilesViewModels
{
    internal class MainSharedFilesViewModel : ViewModelBase
    {
        public ObservableCollection<SharedFile> files { get; }

        private ICommand GetSharedFiles;

        public ICommand OpenPreviewFileWindow { get; }

        public ICommand RefreshCommand { get; }

        public MainSharedFilesViewModel(ClientSocket clientSocket, NavigationStore sharedFilesNavigationStore, SecurityManager security)
        {
            files = new ObservableCollection<SharedFile>();

            GetSharedFiles = new GetSharedFilesCommand(clientSocket, security);

            RefreshCommand = new DefaultCommand(Refresh);

            OpenPreviewFileWindow = new NavigationCommand(sharedFilesNavigationStore, new Func<object, ViewModelBase>((o) => new SharedFilePreviewViewModel(files, (SharedFile)o, clientSocket, sharedFilesNavigationStore, this, security)));

            GetSharedFiles.Execute(this);
        }

        private void Refresh(object? parameter)
        {
            files.Clear();

            GetSharedFiles.Execute(this);
        }
    }
}
