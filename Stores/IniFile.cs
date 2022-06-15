using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NivDrive.Stores
{
    internal class IniFile
    {

        [DllImport("kernel32", CharSet=CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public static string Read(string key, string section)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", RetVal, 255, "./NivDrive.ini");
            return RetVal.ToString();
        }
    }
}
