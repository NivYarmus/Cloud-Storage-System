using System;

namespace NivDrive.MVVM.Models
{
    internal class File
    {
        public int Id { get; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public long UploadTime { get; }
        public long Size { private get; set; }
        public long ModifyTime { get; set; }

        public File(int id, string name, string extension, long upload_time, long size)
        {
            this.Id = id;
            this.Name = name;
            this.Extension = extension;
            this.UploadTime = upload_time;
            this.Size = size;
            this.ModifyTime = upload_time;
        }

        public File(int id, string name, string extension, long upload_time, long size, long modify_time) : this(id, name, extension, upload_time, size)
        {
            this.ModifyTime = modify_time;
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
