namespace NivDrive.MVVM.Models
{
    internal class User
    {
        public string Name { get; }
        public Folder Main { get; }

        public User(int main_folder_id,string name)
        {
            this.Name = name;
            this.Main = new Folder(main_folder_id, name);
        }
    }
}
