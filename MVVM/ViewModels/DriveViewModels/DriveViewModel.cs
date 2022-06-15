using NivDrive.Commands;
using NivDrive.Commands.NavigationCommands;
using NivDrive.Commands.UserCommands.GeneralCommands;
using NivDrive.MVVM.Models;
using NivDrive.Network;
using NivDrive.Security;
using NivDrive.Stores;
using System;
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.DriveViewModels
{
    internal class DriveViewModel : ViewModelBase
    {
        private readonly ClientSocket _clientSocket;
        private readonly NavigationStore _navigationStore;
        public ViewModelBase ViewModel => _navigationStore.ViewModel;

        private User user;

        public string username
        {
            get { return user.Name; }
        }

        public ICommand goToMyFilesCommand { get; }
        public ICommand goToSharedFilesCommand { get; }

        public ICommand Disconnect { get; }
        public ICommand Quit { get; }

        public DriveViewModel(NavigationStore mainNavigationStore, string username, int folder_id, ClientSocket clientSocket, SecurityManager security)
        {
            _clientSocket = clientSocket;
            user = new User(folder_id, username);

            _navigationStore = new NavigationStore(new MyFilesViewModel(user.Main, clientSocket, security));

            goToMyFilesCommand = new NavigationCommand(_navigationStore, new Func<object, ViewModelBase>((o) => new MyFilesViewModel(user.Main, clientSocket, security)));
            goToSharedFilesCommand = new NavigationCommand(_navigationStore, new Func<object, ViewModelBase>((o) => new SharedFilesViewModel(clientSocket, security)));

            Disconnect = new DisconnectCommand(mainNavigationStore, clientSocket, security);
            Quit = new QuitCommand(clientSocket, security);

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(ViewModel));
        }
    }
}
