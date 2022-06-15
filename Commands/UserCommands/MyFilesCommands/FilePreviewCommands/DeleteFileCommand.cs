using NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels;
using NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels.FilePreviewViewModels;
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

namespace NivDrive.Commands.UserCommands.MyFilesCommands
{
    internal class DeleteFileCommand : UserCommandBase
    {
        public DeleteFileCommand(ClientSocket clientsocket, SecurityManager security) : base(clientsocket, security) { }

        public override void Execute(object? parameter)
        {
            MainFilePreviewViewModel vm = (MainFilePreviewViewModel)parameter;

            Dictionary<string, object> command = ClientTypes.GetCommand("Delete_File");
            Dictionary<string, object> template = (Dictionary<string, object>)command["template"];

            template["file"] = vm._currentFile.Id;

            string data = ClientTypes.Serialize(command);

            byte[] bytestream = System.Text.Encoding.UTF8.GetBytes(data);
            bytestream = Assembler.build_message(bytestream, _security);

            _clientSocket.send(bytestream);

            byte[] length_bytestream = _clientSocket.receive(8);
            int length = Assembler.bytes_to_int(length_bytestream);
            JsonElement root = Assembler.load_message(_clientSocket.receive(length), _security);

            if (root.GetProperty("opcode").GetInt32() == 1)
            {
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Delete File", MessageBoxButton.OK, MessageBoxImage.Information);
                vm._files.Remove(vm._currentFolder.RemoveAndGetFile(vm._currentFile));
                vm.Cancel.Execute(null);
            }
            else
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Delete File", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
