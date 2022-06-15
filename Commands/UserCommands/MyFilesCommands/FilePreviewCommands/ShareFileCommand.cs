using NivDrive.MVVM.ViewModels.DriveViewModels;
using NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels.FilePreviewViewModels;
using NivDrive.Network;
using NivDrive.Protocol;
using NivDrive.Protocol.Types;
using NivDrive.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace NivDrive.Commands.UserCommands
{
    internal class ShareFileCommand : UserCommandBase
    {
        public ShareFileCommand(ClientSocket clientSocket, SecurityManager security) : base(clientSocket, security) { }

        public override void Execute(object? parameter)
        {
            ShareFileViewModel vm = parameter as ShareFileViewModel;

            Dictionary<string, object> command = ClientTypes.GetCommand("Share_File");
            Dictionary<string, object> template = (Dictionary<string, object>)command["template"];

            long UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            template["file"] = vm._currentFile.Id;
            template["user"] = vm.ShareUsername;
            template["share_time"] = UnixTime;

            string data = ClientTypes.Serialize(command);

            byte[] bytestream = System.Text.Encoding.UTF8.GetBytes(data);
            bytestream = Assembler.build_message(bytestream, _security);

            _clientSocket.send(bytestream);

            byte[] length_bytestream = _clientSocket.receive(8);
            int length = Assembler.bytes_to_int(length_bytestream);
            JsonElement root = Assembler.load_message(_clientSocket.receive(length), _security);

            if (root.GetProperty("opcode").GetInt32() == 1)
            {
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Share File", MessageBoxButton.OK, MessageBoxImage.Information);
                vm.Cancel.Execute(null);
            }
            else
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Share file", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
