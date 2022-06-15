using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace NivDrive.Protocol.Types
{
    internal class ClientTypes
    {
        private static DictionaryStringObjectJsonConverter Converter = new DictionaryStringObjectJsonConverter();
        private static Dictionary<string, object> Commands = BuildCommands();

        private static Dictionary<string, object> BuildCommands()
        {
            byte[] data = System.IO.File.ReadAllBytes("./ClientTypes.json");
            Utf8JsonReader reader = new Utf8JsonReader(data);
            reader.Read();

            return (Dictionary<string, object>)Converter.Read(ref reader, null, null)["Commands"];
        }

        private static string GetFileFullPath(string fileName, [CallerFilePath] string sourceFilePath = "")
        {
            string[] subs = sourceFilePath.Split('\\');

            string res = "";
            for (int i = 0; i < subs.Length - 1; i++)
                res += $"{subs[i]}\\";
            res += fileName;
            return res;
        }

        public static Dictionary<string, object> GetCommand(string command)
        {
            return new Dictionary<string, object>((Dictionary<string, object>)Commands[command]);
        }

        public static string Serialize(Dictionary<string, object> jsonCommand)
        {
            return Converter.Write(jsonCommand, typeof(Dictionary<String, Object>), null);
        }
    }
}
