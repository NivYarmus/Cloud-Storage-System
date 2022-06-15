using NivDrive.Commands;
using NivDrive.Network;
using NivDrive.Security;
using NivDrive.Stores;
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.LauncherViewModels
{
    internal class LauncherLogInViewModel : ViewModelBase
    {
        private string _username;
        public string username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public ICommand LogIn { get; }

        public LauncherLogInViewModel(NavigationStore mainNavigationStore, ClientSocket clientSocket, SecurityManager security)
        {
            LogIn = new LogInCommand(mainNavigationStore, clientSocket, security);
        }
    }
}
