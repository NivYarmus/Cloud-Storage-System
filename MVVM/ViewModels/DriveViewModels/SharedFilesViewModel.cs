using NivDrive.Commands.UserCommands.SharedCommands;
using NivDrive.MVVM.Models;
using NivDrive.MVVM.ViewModels.DriveViewModels.SharedFilesViewModels;
using NivDrive.Network;
using NivDrive.Security;
using NivDrive.Stores;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.DriveViewModels
{
    internal class SharedFilesViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        public ViewModelBase ViewModel => _navigationStore.ViewModel;

        public SharedFilesViewModel(ClientSocket clientSocket, SecurityManager security)
        {
            _navigationStore = new NavigationStore(null);

            _navigationStore.ViewModel = new MainSharedFilesViewModel(clientSocket, _navigationStore, security);

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(ViewModel));
        }
    }
}
