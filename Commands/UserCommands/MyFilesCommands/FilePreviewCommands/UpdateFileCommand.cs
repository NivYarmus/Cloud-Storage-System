using NivDrive.MVVM.ViewModels;
using NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels;
using NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels.FilePreviewViewModels;
using NivDrive.Network;
using NivDrive.Protocol;
using NivDrive.Protocol.Types;
using NivDrive.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NivDrive.Commands.UserCommands.MyFilesCommands.FilePreviewCommands
{
    internal class UpdateFileCommand : UserCommandBase
    {
        public UpdateFileCommand(ClientSocket clientsocket, SecurityManager security) : base(clientsocket, security) { }

        public override void Execute(object? parameter)
        {
            UpdateFileViewModel vm = (UpdateFileViewModel)parameter;
            if (!File.Exists(vm.filePath))
            {
                MessageBox.Show("File does not exist.", "Upload File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show("Attemping file upload.", "Upload File", MessageBoxButton.OK, MessageBoxImage.Information);

            Dictionary<string, object> command = ClientTypes.GetCommand("Update_File");
            Dictionary<string, object> template = (Dictionary<string, object>)command["template"];

            FileInfo fi = new FileInfo(vm.filePath);
            long UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            template["file"] = vm._currentFile.Id;
            template["extension"] = fi.Extension;
            template["extended_length"] = fi.Length;
            template["modify_time"] = UnixTime;

            string data = ClientTypes.Serialize(command);

            byte[] bytestream = System.Text.Encoding.UTF8.GetBytes(data);
            bytestream = Assembler.build_message(bytestream, _security);

            _clientSocket.send(bytestream);

            byte[] length = _clientSocket.receive(5);
            length = _security.AESDecrypt(length);
            int port = Assembler.bytes_to_int(length);

            ClientSocket newSocket = new ClientSocket();
            newSocket.connect(MainViewModel.IP, port);

            Thread thread = new Thread(delegate ()
            {
                ExecuteOnSecendThread(vm, newSocket, vm.filePath, fi, UnixTime);
            });
            thread.Start();
        }

        private void ExecuteOnSecendThread(UpdateFileViewModel vm, ClientSocket clientSocket, string filepath, FileInfo fi, long UnixTime)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] data = new byte[1024];
                    while (true)
                    {
                        int read = br.Read(data, 0, data.Length);
                        if (read == 0)
                            break;
                        data = _security.AESEncrypt(data);
                        clientSocket.send(data);
                    }
                }
            }

            byte[] length_bytestream = clientSocket.receive(8);
            int length = Assembler.bytes_to_int(length_bytestream);
            JsonElement root = Assembler.load_message(clientSocket.receive(length), _security);
            clientSocket.disconnect();

            if (root.GetProperty("opcode").GetInt32() == 1)
            {
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Update File", MessageBoxButton.OK, MessageBoxImage.Information);
                vm._currentFile.Extension = fi.Extension;
                vm._currentFile.Size = fi.Length;
                vm._currentFile.ModifyTime = UnixTime;
                vm.Cancel.Execute(null);
            }
            else
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Update File", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
