using NivDrive.Commands;
using NivDrive.Commands.UserCommands;
using NivDrive.MVVM.Models;
using NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels;
using NivDrive.Network;
using NivDrive.Security;
using NivDrive.Stores;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.DriveViewModels
{
    internal class MyFilesViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        public ViewModelBase ViewModel => _navigationStore.ViewModel;

        public MyFilesViewModel(Folder currentFolder, ClientSocket clientSocket, SecurityManager security)
        {
            _navigationStore = new NavigationStore(null);

            _navigationStore.ViewModel = new MainMyFilesViewModel(currentFolder, clientSocket, _navigationStore, security);

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(ViewModel));
        }
    }
}
