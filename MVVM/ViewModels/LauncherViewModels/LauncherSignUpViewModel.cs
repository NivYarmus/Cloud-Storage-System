using NivDrive.Commands;
using NivDrive.Network;
using NivDrive.Security;
using System.Windows;
using System.Windows.Input;

namespace NivDrive.MVVM.ViewModels.LauncherViewModels
{
    internal class LauncherSignUpViewModel : ViewModelBase
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

        public ICommand SignUp { get; }

        public LauncherSignUpViewModel(ClientSocket clientSocket, SecurityManager security)
        {
            SignUp = new SignUpCommand(clientSocket, security);
        }
    }
}
