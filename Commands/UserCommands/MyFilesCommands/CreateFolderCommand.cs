using NivDrive.Network;
using NivDrive.Commands.UserCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using NivDrive.MVVM.ViewModels.DriveViewModels;
using NivDrive.Protocol.Types;
using System.Windows;
using System.Text.Json;
using NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels;
using NivDrive.Security;
using NivDrive.Protocol;

namespace NivDrive.Commands
{
    internal class CreateFolderCommand : UserCommandBase
    {
        public CreateFolderCommand(ClientSocket clientsocket, SecurityManager security) : base(clientsocket, security) { }

        public override void Execute(object? parameter)
        {
            AddFolderViewModel vm = (AddFolderViewModel)parameter;

            Dictionary<string, object> command = ClientTypes.GetCommand("Create_Folder");
            Dictionary<string, object> template = (Dictionary<string, object>)command["template"];

            template["folder"] = vm._currentFolder.Id;
            template["name"] = vm.folderName;

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

                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Create Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                vm._viewFolders.Add(vm._currentFolder.AddAndGetFolder(folder_id, vm.folderName));
                vm.Cancel.Execute(null);
            }
            else
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Create Folder", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
