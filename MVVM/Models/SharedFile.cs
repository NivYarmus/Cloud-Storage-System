using System;

namespace NivDrive.MVVM.Models
{
    internal class SharedFile
    {
        public int Id { get; }
        public string Username { get; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Size { private get; set; }
        public long ModifyTime { get; set; }
        public long ShareTime { get; }

        public SharedFile(int id, string username, string name, string extension, long size,
            long modifyTime, long shareTime)
        {
            this.Id = id;
            this.Username = username;
            this.Name = name;
            this.Extension = extension;
            this.Size = size;
            this.ModifyTime = modifyTime;
            this.ShareTime = shareTime;
        }

        public string GetSizeToString()
        {
            if (Size < Math.Pow(2, 10))
                return $"{Size} Bytes";
            else if (Size < Math.Pow(2, 20))
            {
                return $"{Size / Math.Pow(2, 10):0.00} KB";
            }
            else if (Size < Math.Pow(2, 40))
            {
                return $"{Size / Math.Pow(2, 20):0.00} MB";
            }
            return $"{Size / Math.Pow(2, 40):0.00} GB";
        }
    }
}
