using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NivDrive.Security
{
    internal class SecurityManager
    {
        public byte[] key;
        public byte[] iv;
        public RSACryptoServiceProvider rsa;

        public SecurityManager()
        {
            this.key = null;
            this.iv = null;
            rsa = new RSACryptoServiceProvider(2048);
        }

        public byte[] AESEncrypt(byte[] data)
        {
            byte[] encrypted;

            using (Aes Aes = Aes.Create())
            {
                Aes.Mode = CipherMode.CFB;
                Aes.Padding = PaddingMode.None;

                ICryptoTransform encryptor = Aes.CreateEncryptor(key, iv);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (BinaryWriter bw = new BinaryWriter(cs, Encoding.UTF8))
                        {
                            bw.Write(data);
                        }
                    }
                    encrypted = ms.ToArray();
                }
            }
            return encrypted;
        }

        public byte[] AESDecrypt(byte[] data)
        {
            byte[] decrypted;

            using (Aes Aes = Aes.Create())
            {
                Aes.Mode = CipherMode.CFB;
                Aes.Padding = PaddingMode.None;

                ICryptoTransform encryptor = Aes.CreateDecryptor(key, iv);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Read))
                    {
                        using (BinaryReader br = new BinaryReader(cs, Encoding.UTF8))
                        {
                            decrypted = br.ReadBytes(data.Length);
                        }
                    }
                }
            }
            return decrypted;
        }

        public byte[] RSAEncrypt(byte[] data)
        {
            return rsa.Encrypt(data, true);
        }

        public byte[] RSADecrypt(byte[] data)
        {
            return rsa.Decrypt(data, true);
        }
    }
}
