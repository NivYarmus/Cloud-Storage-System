using NivDrive.MVVM.ViewModels.LauncherViewModels;
using NivDrive.Network;
using NivDrive.Protocol;
using NivDrive.Protocol.Types;
using NivDrive.Security;
using NivDrive.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NivDrive.Commands.UserCommands.GeneralCommands
{
    internal class DisconnectCommand : UserCommandBase
    {
        private NavigationStore _mainNavigationStore;

        public DisconnectCommand(NavigationStore mainNavigationStore, ClientSocket clientSocket, SecurityManager security) : base(clientSocket, security)
        {
            _mainNavigationStore = mainNavigationStore;
        }

        public override void Execute(object? parameter)
        {
            Dictionary<string, object> command = ClientTypes.GetCommand("Disconnect");

            string data = ClientTypes.Serialize(command);

            byte[] bytestream = System.Text.Encoding.UTF8.GetBytes(data);
            bytestream = Assembler.build_message(bytestream, _security);

            _clientSocket.send(bytestream);

            _mainNavigationStore.ViewModel = new LauncherViewModel(_mainNavigationStore, _clientSocket, _security);
        }
    }
}
