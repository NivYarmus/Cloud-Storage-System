using NivDrive.MVVM.ViewModels.DriveViewModels;
using NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels;
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
    internal class GetFoldersCommand : UserCommandBase
    {
        public GetFoldersCommand(ClientSocket clientSocket, SecurityManager security) : base(clientSocket, security) { }

        public override void Execute(object? parameter)
        {
            MainMyFilesViewModel vm = (MainMyFilesViewModel)parameter;

            Dictionary<string, object> command = ClientTypes.GetCommand("Get_Folders");
            Dictionary<string, object> template = (Dictionary<string, object>)command["template"];

            template["folder"] = vm._currentFolder.Id;
            string data = ClientTypes.Serialize(command);

            byte[] bytestream = System.Text.Encoding.UTF8.GetBytes(data);
            bytestream = Assembler.build_message(bytestream, _security);

            _clientSocket.send(bytestream);

            byte[] length_bytestream = _clientSocket.receive(8);
            int length = Assembler.bytes_to_int(length_bytestream);
            JsonElement root = Assembler.load_message(_clientSocket.receive(length), _security);

            if (root.GetProperty("opcode").GetInt32() == 2)
            {
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Get Folders", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            byte[] folders_bytestream = _clientSocket.receive(root.GetProperty("template").GetProperty("extended_length").GetInt32());
            folders_bytestream = _security.AESDecrypt(folders_bytestream);
            string jsonfolders = System.Text.Encoding.UTF8.GetString(folders_bytestream);
            JsonDocument doc = JsonDocument.Parse(jsonfolders);
            root = doc.RootElement;

            JsonElement[] folders = JsonSerializer.Deserialize<JsonElement[]>(root);
            
            if (vm._currentFolder.HeadFolder is not null)
                vm.folders.Add(vm._currentFolder.HeadFolder);
            foreach (JsonElement elem in folders)
                vm.folders.Add(vm._currentFolder.AddAndGetFolder(elem.GetProperty("folder").GetInt32(), elem.GetProperty("name").ToString()));
        }
    }
}
