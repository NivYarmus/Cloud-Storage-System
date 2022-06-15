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

namespace NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels
{
    internal class FilePreviewViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        public ViewModelBase ViewModel => _navigationStore.ViewModel;

        public FilePreviewViewModel(Folder currentFolder, ObservableCollection<File> files,
            File f, ClientSocket clientSocket,
            NavigationStore myFilesNavigationStore, MainMyFilesViewModel returnToViewModel,
            SecurityManager security)
        {
            _navigationStore = new NavigationStore(null);

            _navigationStore.ViewModel = new MainFilePreviewViewModel(currentFolder, files, f, clientSocket, myFilesNavigationStore, returnToViewModel, _navigationStore, security);

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(ViewModel));
        }
    }
}
