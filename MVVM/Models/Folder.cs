using System;
using System.Collections.Generic;

namespace NivDrive.MVVM.Models
{
    internal class Folder
    {
        public int Id { get; }
        public string Name { get; }
        public Folder? HeadFolder { get; }
        public List<Folder> Folders { get; }
        public List<File> Files { get; }

        public Folder(int id, string name, Folder? headFolder = null, List<Folder>? folders = null, List<File>? files = null)
        {
            this.Id = id;
            this.Name = name;
            this.HeadFolder = headFolder;
            this.Folders = folders is not null ? folders : new List<Folder>();
            this.Files = files is not null ? files : new List<File>();
        }

        public Folder AddAndGetFolder(int id, string name)
        {
            Folder f = new Folder(id, name, this);
            Folders.Add(f);
            return f;
        }

        public File AddAndGetFile(int id, string name, string extension, long upload_time, long size)
        {
            File f = new File(id, name, extension, upload_time, size);
            Files.Add(f);
            return f;
        }

        public File AddAndGetFile(int id, string name, string extension, long upload_time, long size, long modify_time)
        {
            File f = new File(id, name, extension, upload_time, size, modify_time);
            Files.Add(f);
            return f;
        }

        public File RemoveAndGetFile(File f)
        {
            Files.Remove(f);
            return f;
        }

        public string GetPathToFolder()
        {
            string GetPathRec(Folder curr)
            {
                if (curr.HeadFolder is null)
                    return curr.Name;
                return GetPathRec(curr.HeadFolder) + $"\\{curr.Name}";
            }
            if (this.HeadFolder is null)
                return "";
            return GetPathRec(this.HeadFolder);
        }
        public string GetPathIncludeFolder()
        {
            string pathToFolder = GetPathToFolder();
            if (!string.IsNullOrEmpty(pathToFolder))
                return pathToFolder + $"\\{this.Name}";
            return this.Name;
        }

    }
}
