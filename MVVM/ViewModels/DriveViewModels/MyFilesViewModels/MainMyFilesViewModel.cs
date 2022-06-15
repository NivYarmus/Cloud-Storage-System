using NivDrive.Commands;
using NivDrive.Commands.NavigationCommands;
using NivDrive.Commands.UserCommands;
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

namespace NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels
{
    internal class MainMyFilesViewModel : ViewModelBase
    {
        private readonly ClientSocket _clientSocket;
        public Folder _currentFolder { get; private set; }

        public ObservableCollection<Folder> folders { get; }
        public ObservableCollection<File> files { get; }

        private ICommand GetFolders;
        private ICommand GetFiles;

        public ICommand NavigateFoldersCommand { get; }
        public ICommand OpenAddFolderWindow { get; }
        public ICommand OpenUploadFileWindow { get; }
        public ICommand OpenPreviewFileWindow { get; }

        public ICommand RefreshCommand { get; }

        public MainMyFilesViewModel(Folder currentFolder, ClientSocket clientSocket, NavigationStore myFilesNavigationStore, SecurityManager security)
        {
            _clientSocket = clientSocket;
            _currentFolder = currentFolder;

            folders = new ObservableCollection<Folder>();
            files = new ObservableCollection<File>();

            GetFolders = new GetFoldersCommand(clientSocket, security);
            GetFiles = new GetFilesCommand(clientSocket, security);

            RefreshCommand = new DefaultCommand(Refresh);

            NavigateFoldersCommand = new DefaultCommand(ChangeFolder);
            OpenAddFolderWindow = new NavigationCommand(myFilesNavigationStore, new Func<object, ViewModelBase>((o) => new AddFolderViewModel(_currentFolder, _clientSocket, folders, myFilesNavigationStore, this, security)));
            OpenUploadFileWindow = new NavigationCommand(myFilesNavigationStore, new Func<object, ViewModelBase>((o) => new UploadFileViewModel(_currentFolder, _clientSocket, files, myFilesNavigationStore, this, security)));
            OpenPreviewFileWindow = new NavigationCommand(myFilesNavigationStore, new Func<object, ViewModelBase>((o) => new FilePreviewViewModel(_currentFolder, files, (File)o, _clientSocket, myFilesNavigationStore, this, security)));

            GetFolders.Execute(this);
            GetFiles.Execute(this);
        }
        private void Refresh(object? parameter)
        {
            folders.Clear();
            files.Clear();

            GetFolders.Execute(this);
            GetFiles.Execute(this);
        }

        private void ChangeFolder(object? parameter)
        {
            folders.Clear();
            files.Clear();

            Folder pressedOnFolder = parameter as Folder;
            _currentFolder = pressedOnFolder;

            GetFolders.Execute(this);
            GetFiles.Execute(this);
        }
    }
}
