using NivDrive.MVVM.ViewModels.DriveViewModels.SharedFilesViewModels.SharedFilePreviewViewModels;
using NivDrive.Network;
using NivDrive.Protocol;
using NivDrive.Protocol.Types;
using NivDrive.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NivDrive.Commands.UserCommands.SharedCommands
{
    internal class RemoveSharedFileCommand : UserCommandBase
    {
        public RemoveSharedFileCommand(ClientSocket clientSocket, SecurityManager security) : base(clientSocket, security) { }

        public override void Execute(object? parameter)
        {
            MainSharedFilePreviewViewModel vm = (MainSharedFilePreviewViewModel)parameter;

            Dictionary<string, object> command = ClientTypes.GetCommand("Remove_Shared_File");
            Dictionary<string, object> template = (Dictionary<string, object>)command["template"];

            template["file"] = vm._sharedFile.Id;

            string data = ClientTypes.Serialize(command);

            byte[] bytestream = System.Text.Encoding.UTF8.GetBytes(data);
            bytestream = Assembler.build_message(bytestream, _security);

            _clientSocket.send(bytestream);

            byte[] length_bytestream = _clientSocket.receive(8);
            int length = Assembler.bytes_to_int(length_bytestream);
            JsonElement root = Assembler.load_message(_clientSocket.receive(length), _security);

            if (root.GetProperty("opcode").GetInt32() == 1)
            {
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Remove File", MessageBoxButton.OK, MessageBoxImage.Information);
                vm._files.Remove(vm._sharedFile);
                vm.Cancel.Execute(null);
            }
            else
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Remove File", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
