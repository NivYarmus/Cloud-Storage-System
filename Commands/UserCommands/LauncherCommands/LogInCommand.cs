using NivDrive.MVVM.ViewModels.LauncherViewModels;
using NivDrive.Protocol.Types;
using NivDrive.Network;
using NivDrive.Commands.UserCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text.Json;
using NivDrive.MVVM.ViewModels.DriveViewModels;
using NivDrive.Stores;
using NivDrive.Security;
using NivDrive.Protocol;

namespace NivDrive.Commands
{
    internal class LogInCommand : UserCommandBase
    {
        private NavigationStore _mainNavigationStore;

        public LogInCommand(NavigationStore mainNavigationStore, ClientSocket clientSocket, SecurityManager security) : base(clientSocket, security)
        {
            _mainNavigationStore = mainNavigationStore;
        }

        public override void Execute(object? parameter)
        {
            LauncherLogInViewModel vm = (LauncherLogInViewModel)parameter;

            string username = vm.username;
            string password = vm.password;

            Dictionary<string, object> command = ClientTypes.GetCommand("Log_In");
            Dictionary<string, object> template = (Dictionary<string, object>)command["template"];

            template["username"] = username;
            template["password"] = password;

            string data = ClientTypes.Serialize(command);

            byte[] bytestream = System.Text.Encoding.UTF8.GetBytes(data);
            bytestream = Assembler.build_message(bytestream, _security);

            _clientSocket.send(bytestream);

            byte[] length_bytestream = _clientSocket.receive(8);
            int length = Assembler.bytes_to_int(length_bytestream);
            JsonElement root = Assembler.load_message(_clientSocket.receive(length), _security);

            if (root.GetProperty("opcode").GetInt32() == 1)
            {
                byte[] folder_id_bytestream = _clientSocket.receive(root.GetProperty("template").GetProperty("extended_length").GetInt32());
                folder_id_bytestream = _security.AESDecrypt(folder_id_bytestream);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(folder_id_bytestream);

                int folder_id = BitConverter.ToInt32(folder_id_bytestream);

                _mainNavigationStore.ViewModel = new DriveViewModel(_mainNavigationStore ,username, folder_id, _clientSocket, _security);
            }      
            else
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Log In", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
