using NivDrive.MVVM.Models;
using NivDrive.MVVM.ViewModels.DriveViewModels.SharedFilesViewModels.SharedFilePreviewViewModels;
using NivDrive.Network;
using NivDrive.Security;
using NivDrive.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NivDrive.MVVM.ViewModels.DriveViewModels.SharedFilesViewModels
{
    internal class SharedFilePreviewViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        public ViewModelBase ViewModel => _navigationStore.ViewModel;

        public SharedFilePreviewViewModel(ObservableCollection<SharedFile> files, SharedFile sharedFile, ClientSocket clientSocket,
            NavigationStore sharedFilesNavigationStore, MainSharedFilesViewModel returnToViewModel, SecurityManager security)
        {
            _navigationStore = new NavigationStore(null);

            _navigationStore.ViewModel = new MainSharedFilePreviewViewModel(files, sharedFile, clientSocket, sharedFilesNavigationStore, returnToViewModel, _navigationStore, security);

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(ViewModel));
        }
    }
}
