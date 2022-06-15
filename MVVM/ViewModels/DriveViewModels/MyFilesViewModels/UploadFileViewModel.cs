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
    internal class UploadFileViewModel : ViewModelBase
    {
        public readonly Folder _currentFolder;
        public readonly ObservableCollection<File> _viewFiles;

        public string folderPath
        {
            get { return _currentFolder.GetPathIncludeFolder(); }
        }

        private string _fileName;
        public string fileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged();
            }
        }

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

        public ICommand UploadFile { get; }
        public ICommand Cancel { get; }

        public UploadFileViewModel(Folder f, ClientSocket clientSocket, ObservableCollection<File> viewFiles,
            NavigationStore myFilesNavigationStore, MainMyFilesViewModel returnToViewModel, SecurityManager security)
        {
            _currentFolder = f;
            _viewFiles = viewFiles;

            UploadFile = new UploadFileCommand(clientSocket, security);
            Cancel = new NavigationCommand(myFilesNavigationStore, new Func<object, ViewModelBase>((o) => returnToViewModel));
        }
    }
}
