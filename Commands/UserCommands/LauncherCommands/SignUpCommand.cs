using NivDrive.MVVM.ViewModels.LauncherViewModels;
using NivDrive.Protocol.Types;
using NivDrive.Commands.UserCommands;
using NivDrive.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text.Json;
using NivDrive.Protocol;
using NivDrive.Security;
using System.Diagnostics;

namespace NivDrive.Commands
{
    internal class SignUpCommand : UserCommandBase
    {

        public SignUpCommand(ClientSocket clientSocket, SecurityManager security) : base(clientSocket, security) { }

        public override void Execute(object? parameter)
        {
            LauncherSignUpViewModel vm = (LauncherSignUpViewModel)parameter;

            string username = vm.username;
            string password = vm.password;

            Dictionary<string, object> command = ClientTypes.GetCommand("Sign_Up");
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
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Sign Up", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Sign Up", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
