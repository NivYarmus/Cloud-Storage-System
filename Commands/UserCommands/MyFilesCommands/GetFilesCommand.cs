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
    internal class GetFilesCommand : UserCommandBase
    {
        public GetFilesCommand(ClientSocket clientSocket, SecurityManager security) : base(clientSocket, security) { }

        public override void Execute(object? parameter)
        {
            MainMyFilesViewModel vm = (MainMyFilesViewModel)parameter;

            Dictionary<string, object> command = ClientTypes.GetCommand("Get_Files");
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
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Get Files", MessageBoxButton.OK, MessageBoxImage.Error);
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
                long upload_time = elem.GetProperty("upload_time").GetInt64();
                long size = elem.GetProperty("size").GetInt64();
                long modify_time = elem.GetProperty("modify_time").GetInt64();
                vm.files.Add(vm._currentFolder.AddAndGetFile(fileID, fileName, fileExtension, upload_time, size, modify_time));
            }
        }
    }
}
