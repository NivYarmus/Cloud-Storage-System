using NivDrive.Commands;
using NivDrive.Commands.NavigationCommands;
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

namespace NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels
{
    internal class AddFolderViewModel : ViewModelBase
    {
        public readonly Folder _currentFolder;
        public readonly ObservableCollection<Folder> _viewFolders;

        public string folderPath
        {
            get { return _currentFolder.GetPathIncludeFolder(); }
        }

        private string _folderName;
        public string folderName
        {
            get { return _folderName; }
            set
            {
                _folderName = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddFolder { get; }
        public ICommand Cancel { get; }

        public AddFolderViewModel(Folder f, ClientSocket clientSocket, ObservableCollection<Folder> viewFolders,
            NavigationStore myFilesNavigationStore, MainMyFilesViewModel returnToViewModel,
            SecurityManager security)
        {
            _currentFolder = f;
            _viewFolders = viewFolders;

            AddFolder = new CreateFolderCommand(clientSocket, security);
            Cancel = new NavigationCommand(myFilesNavigationStore, new Func<object, ViewModelBase>((o) => returnToViewModel));
        }
    }
}
