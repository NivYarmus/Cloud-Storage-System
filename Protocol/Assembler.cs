using NivDrive.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NivDrive.Protocol
{
    internal class Assembler
    {
        public static byte[] build_message(byte[] data, SecurityManager security)
        {
            data = security.AESEncrypt(data);
            byte[] length = Assembler.int_to_bytes((UInt64)data.Length);
            return length.Concat(data).ToArray();
        }

        public static JsonElement load_message(byte[] data, SecurityManager security)
        {
            string json = Encoding.UTF8.GetString(security.AESDecrypt(data));
            JsonDocument doc = JsonDocument.Parse(json);
            return doc.RootElement;
        }

        public static byte[] int_to_bytes(UInt64 number)
        {
            byte[] length = BitConverter.GetBytes(number);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(length);
            return length;
        }

        public static int bytes_to_int(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);
            return BitConverter.ToInt32(data);
        }
    }
}
