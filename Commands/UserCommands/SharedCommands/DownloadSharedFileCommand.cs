using NivDrive.MVVM.ViewModels;
using NivDrive.MVVM.ViewModels.DriveViewModels;
using NivDrive.MVVM.ViewModels.DriveViewModels.SharedFilesViewModels.SharedFilePreviewViewModels;
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
using System.Windows.Forms;

namespace NivDrive.Commands.UserCommands.SharedCommands
{
    internal class DownloadSharedFileCommand : UserCommandBase
    {
        public DownloadSharedFileCommand(ClientSocket clientSocket, SecurityManager security) : base(clientSocket, security) { }

        public override void Execute(object? parameter)
        {
            MainSharedFilePreviewViewModel vm = parameter as MainSharedFilePreviewViewModel;

            Dictionary<string, object> command = ClientTypes.GetCommand("Download_File");
            Dictionary<string, object> template = (Dictionary<string, object>)command["template"];

            template["file"] = vm._sharedFile.Id;

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
                ExecuteOnSecendThread(newSocket, vm.fileName, vm.fileExtension);
            });
            thread.Start();
        }

        private void ExecuteOnSecendThread(ClientSocket clientSocket, string fileName, string fileExtention)
        {
            byte[] length_bytestream = clientSocket.receive(8);
            int length = Assembler.bytes_to_int(length_bytestream);
            JsonElement root = Assembler.load_message(clientSocket.receive(length), _security);

            if (root.GetProperty("opcode").GetInt32() == 2)
            {
                MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString(), "Download File", MessageBoxButtons.OK);
                return;
            }

            string path = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE") + @"\Downloads", fileName);
            path = TryCreateSaveFile(path, fileExtention);


            MessageBox.Show($"Saving file to {path}.", "Download File", MessageBoxButtons.OK);
            using (FileStream fs = File.Create(path))
            {
            }
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    long received_bytes = 0;
                    long total_length = root.GetProperty("template").GetProperty("extended_length").GetInt32();
                    while (received_bytes < total_length)
                    {
                        byte[] data = clientSocket.receive((int)Math.Min(1024, total_length - received_bytes));
                        received_bytes += data.Length;
                        data = _security.AESDecrypt(data);
                        bw.Write(data, 0, data.Length);
                    }
                }
            }
            clientSocket.disconnect();

            DialogResult dialogResult = MessageBox.Show(root.GetProperty("template").GetProperty("description").ToString() + "\nWould you like to view the file?",
                "Download File", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                DialogResult viewDialogResult = MessageBox.Show("Would you like to run the file?", "Download File", MessageBoxButtons.YesNo);
                if (viewDialogResult == DialogResult.Yes)
                {
                    System.Diagnostics.Process p = new System.Diagnostics.Process
                    {
                        StartInfo =
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            FileName = "cmd.exe",
                            Arguments = $"/C \"{path}\""
                        }
                    };
                    p.Start();
                }
                else
                {
                    System.Diagnostics.Process p = new System.Diagnostics.Process
                    {
                        StartInfo =
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            FileName = "cmd.exe",
                            Arguments = $"/CNotepad \"{path}\""
                        }
                    };
                    p.Start();
                }
            }
        }

        private string TryCreateSaveFile(string path, string extension)
        {
            if (!File.Exists(path + extension))
                return path + extension;

            int cnt = 1;
            string newPath = $"{path} ({cnt}){extension}";
            while (true)
            {
                if (!File.Exists(newPath))
                    return newPath;
                cnt++;
                newPath = $"{path} ({cnt}){extension}";
            }
        }
    }
}
