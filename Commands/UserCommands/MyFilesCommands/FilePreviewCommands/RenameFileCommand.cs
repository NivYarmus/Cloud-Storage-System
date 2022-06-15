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

namespace NivDrive.Commands.UserCommands.MyFilesCommands.FilePreviewCommands
{
    internal class RenameFileCommand : UserCommandBase
    {
        public RenameFileCommand(ClientSocket clientsocket, SecurityManager security) : base(clientsocket, security) { }

        public override void Execute(object? parameter)
        {
            RenameFileViewModel vm = (RenameFileViewModel)parameter;

            Dictionary<string, object> command = ClientTypes.GetCommand("Rename_File");
            Dictionary<string, object> template = (Dictionary<string, object>)command["template"];

            long UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            template["file"] = vm._currentFile.Id;
            template["name"] = vm.newFileName;
            template["modify_time"] = UnixTime;

            string data = ClientTypes.Serialize(command);

            byte[] bytestream = System.Text.Encoding.UTF8.GetBytes(data);
            bytestream = Assembler.build_message(bytestream, _security);

            _clientSocket.send(bytestream);

            byte[] length_bytestream = _clientSocket.receive(8);
            int length = Assembler.bytes_to_int(length_bytestream);
            JsonElement root = Assembler.load_message(_clientSocket.receive(length), _security);

            if (root.GetProperty("opcode").GetInt32() == 1)
            {
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Rename File", MessageBoxButton.OK, MessageBoxImage.Information);
                vm._currentFile.Name = vm.newFileName;
                vm._currentFile.ModifyTime = UnixTime;
                vm.Cancel.Execute(null);
            }
            else
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Rename File", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
