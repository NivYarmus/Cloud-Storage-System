using NivDrive.MVVM.ViewModels.LauncherViewModels;
using NivDrive.Stores;
using NivDrive.Network;
using NivDrive.Security;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using NivDrive.Protocol;
using NivDrive.Protocol.Types;

namespace NivDrive.MVVM.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public static readonly string IP = Stores.IniFile.Read("ServerIP", "Network");
        public static readonly int PORT = int.Parse(Stores.IniFile.Read("Port", "Network"));

        private readonly NavigationStore _navigationStore;
        private readonly ClientSocket _clientSocket;
        private readonly SecurityManager _security;
        public ViewModelBase ViewModel => _navigationStore.ViewModel;

        public MainViewModel()
        {
            _navigationStore = new NavigationStore(null);
            _clientSocket = new ClientSocket();
            _security = new SecurityManager();

            _clientSocket.connect(IP, PORT);
            KeyExchange();

            _navigationStore.ViewModel = new LauncherViewModel(_navigationStore, _clientSocket, _security);

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(ViewModel));
        }

        private void KeyExchange()
        {
            var buffer = new StringBuilder();
            buffer.AppendLine(Convert.ToBase64String(_security.rsa.ExportRSAPublicKey(), Base64FormattingOptions.InsertLineBreaks));

            byte[] bytestream = Encoding.UTF8.GetBytes(buffer.ToString());
            byte[] bytestream_length = BitConverter.GetBytes((UInt64)bytestream.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytestream_length);

            _clientSocket.send(bytestream_length.Concat(bytestream).ToArray());

            byte[] length = _clientSocket.receive(8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(length);
            string data = Encoding.UTF8.GetString(_security.RSADecrypt(_clientSocket.receive(BitConverter.ToInt32(length))));

            JsonDocument doc = JsonDocument.Parse(data);
            JsonElement root = doc.RootElement;

            _security.key = Convert.FromBase64String(root.GetProperty("key").ToString());
            _security.iv = Convert.FromBase64String(root.GetProperty("iv").ToString());
        }
    }
}
