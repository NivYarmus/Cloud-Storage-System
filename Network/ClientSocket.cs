using System.Diagnostics;
using System.Net.Sockets;

namespace NivDrive.Network
{
    internal class ClientSocket
    {
        private readonly TcpClient _client;
        private NetworkStream _stream;

        public ClientSocket()
        {
            _client = new TcpClient();
            _stream = null;
        }

        public void connect(string ip, int port)
        {
            if (!_client.Connected)
            {
                _client.Connect(ip, port);
                _stream = _client.GetStream();
            }
        }

        public void disconnect()
        {
            _client.Close();
        }

        public void send(byte[] data)
        {
            _stream.Write(data, 0, data.Length);
        }

        public byte[] receive(int length)
        {
            return receiveOnLoop(length);
        }

        private byte[] receiveOnLoop(int length)
        {
            int received = 0;
            byte[] data = new byte[length];
            while (received < length)
                received += _stream.Read(data, received, length - received);
            return data;
        }
    }
}
