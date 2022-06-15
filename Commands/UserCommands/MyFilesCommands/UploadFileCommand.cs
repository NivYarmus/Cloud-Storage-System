using NivDrive.Network;
using NivDrive.Commands.UserCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NivDrive.MVVM.ViewModels.DriveViewModels;
using System.Windows;
using System.IO;
using NivDrive.Protocol.Types;
using System.Threading;
using System.Text.Json;
using System.Windows.Threading;
using NivDrive.MVVM.ViewModels;
using NivDrive.MVVM.ViewModels.DriveViewModels.MyFilesViewModels;
using NivDrive.Security;
using NivDrive.Protocol;

namespace NivDrive.Commands
{
    internal class UploadFileCommand : UserCommandBase
    {
        public UploadFileCommand(ClientSocket clientsocket, SecurityManager security) : base(clientsocket, security) { }

        public override void Execute(object? parameter)
        {
            UploadFileViewModel vm = (UploadFileViewModel)parameter;
            if (!File.Exists(vm.filePath))
            {
                MessageBox.Show("File does not exist.", "Upload File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show("Attemping file upload.", "Upload File", MessageBoxButton.OK, MessageBoxImage.Information);

            Dictionary<string, object> command = ClientTypes.GetCommand("Upload_File");
            Dictionary<string, object> template = (Dictionary<string, object>)command["template"];

            FileInfo fi = new FileInfo(vm.filePath);
            long UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            template["folder"] = vm._currentFolder.Id;
            template["name"] = vm.fileName;
            template["extended_length"] = fi.Length;
            template["extension"] = fi.Extension;
            template["upload_time"] = UnixTime;

            string data = ClientTypes.Serialize(command);

            byte[] bytestream = System.Text.Encoding.UTF8.GetBytes(data);
            bytestream = Assembler.build_message(bytestream, _security);

            _clientSocket.send(bytestream);

            byte[] length = _clientSocket.receive(5);
            length = _security.AESDecrypt(length);
            int port = Assembler.bytes_to_int(length);

            ClientSocket newSocket = new ClientSocket();
            newSocket.connect(MainViewModel.IP, port);

            Thread thread = new Thread(delegate()
            {
                ExecuteOnSecendThread(vm, newSocket, vm.filePath, fi, UnixTime);
            });
            thread.Start();
        }

        private void ExecuteOnSecendThread(UploadFileViewModel vm, ClientSocket clientSocket, string filepath, FileInfo fi, long UnixTime)
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

            if (root.GetProperty("opcode").GetInt32() == 1)
            {
                byte[] folder_id_bytestream = clientSocket.receive(root.GetProperty("template").GetProperty("extended_length").GetInt32());
                folder_id_bytestream = _security.AESDecrypt(folder_id_bytestream);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(folder_id_bytestream);
                int folder_id = BitConverter.ToInt32(folder_id_bytestream);
                clientSocket.disconnect();

                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Upload File", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate ()
                {
                    vm._viewFiles.Add(vm._currentFolder.AddAndGetFile(folder_id, vm.fileName, fi.Extension, UnixTime, fi.Length));
                }));
                vm.Cancel.Execute(null);
            }
            else
            {
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Upload File", MessageBoxButton.OK, MessageBoxImage.Error);
                clientSocket.disconnect();
            }
        }
    }
}
