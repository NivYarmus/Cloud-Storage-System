using NivDrive.Commands.NavigationCommands;
using NivDrive.Commands.UserCommands.GeneralCommands;
using NivDrive.Network;
using NivDrive.Security;
using NivDrive.Stores;
using System;
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.LauncherViewModels
{
    internal class LauncherViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        public ViewModelBase ViewModel => _navigationStore.ViewModel;

        public ICommand goToHomeCommand { get; }
        public ICommand goToSignUpCommand { get; }
        public ICommand goToLogInCommand { get; }
        
        public ICommand Quit { get; }

        public LauncherViewModel(NavigationStore mainNavigationStore, ClientSocket clientSocket, SecurityManager security)
        {
            _navigationStore = new NavigationStore(new LauncherHomeViewModel());

            goToHomeCommand = new NavigationCommand(_navigationStore, new Func<object, ViewModelBase>((o) => new LauncherHomeViewModel()));
            goToSignUpCommand = new NavigationCommand(_navigationStore, new Func<object, ViewModelBase>((o) => new LauncherSignUpViewModel(clientSocket, security)));
            goToLogInCommand = new NavigationCommand(_navigationStore, new Func<object, ViewModelBase>((o) => new LauncherLogInViewModel(mainNavigationStore, clientSocket, security)));
            
            Quit = new QuitCommand(clientSocket, security);

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(ViewModel));
        }
    }
}
