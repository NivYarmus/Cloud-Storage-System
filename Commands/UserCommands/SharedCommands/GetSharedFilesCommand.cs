using NivDrive.MVVM.Models;
using NivDrive.MVVM.ViewModels.DriveViewModels;
using NivDrive.MVVM.ViewModels.DriveViewModels.SharedFilesViewModels;
using NivDrive.Network;
using NivDrive.Protocol;
using NivDrive.Protocol.Types;
using NivDrive.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace NivDrive.Commands.UserCommands.SharedCommands
{
    internal class GetSharedFilesCommand : UserCommandBase
    {
        public GetSharedFilesCommand(ClientSocket clientSocket, SecurityManager security) : base(clientSocket, security) { }

        public override void Execute(object? parameter)
        {
            MainSharedFilesViewModel vm = (MainSharedFilesViewModel)parameter;

            Dictionary<string, object> command = ClientTypes.GetCommand("Get_Shared_Files");
            string data = ClientTypes.Serialize(command);

            byte[] bytestream = System.Text.Encoding.UTF8.GetBytes(data);
            bytestream = Assembler.build_message(bytestream, _security);

            _clientSocket.send(bytestream);

            byte[] length_bytestream = _clientSocket.receive(8);
            int length = Assembler.bytes_to_int(length_bytestream);
            JsonElement root = Assembler.load_message(_clientSocket.receive(length), _security);

            if (root.GetProperty("opcode").GetInt32() == 2)
            {
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Get shared files", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            byte[] files_bytestream = _clientSocket.receive(root.GetProperty("template").GetProperty("extended_length").GetInt32());
            files_bytestream = _security.AESDecrypt(files_bytestream);
            string jsonfiles = System.Text.Encoding.UTF8.GetString(files_bytestream);
            JsonDocument doc = JsonDocument.Parse(jsonfiles);
            root = doc.RootElement;

            JsonElement[] files = JsonSerializer.Deserialize<JsonElement[]>(root);
            foreach (JsonElement elem in files)
            {
                int fileID = elem.GetProperty("file").GetInt32();
                string fileExtension = elem.GetProperty("extension").ToString();
                string fileName = elem.GetProperty("name").ToString();
                long size = elem.GetProperty("size").GetInt64();
                string username = elem.GetProperty("username").ToString();
                long modify_time = elem.GetProperty("modify_time").GetInt64();
                long share_time = elem.GetProperty("share_time").GetInt64();
                vm.files.Add(new SharedFile(fileID, username, fileName, fileExtension, size, modify_time, share_time));
            }
        }
    }
}
