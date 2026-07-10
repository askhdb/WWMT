using ModelTool.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModelTool.Tools
{
    public class JsonHelp
    {
        private static JsonHelp _instance;
        public static string appDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string defaultFilePath = appDir + "data\\Path.Json";
        public static JsonHelp Instance
        {
            get
            {
                return _instance ?? (_instance = new JsonHelp());
            }
        }

        public bool SaveToJson(object data)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(defaultFilePath, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SaveToJson(object data, string filePath)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public object LoadFromJson(object obj)
        {
            try
            {
                string jsonString = System.IO.File.ReadAllText(defaultFilePath);
                return JsonSerializer.Deserialize(jsonString, obj.GetType());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public object LoadFromJson(object obj, string filePath)
        {
            try
            {
                string jsonString = System.IO.File.ReadAllText(filePath);
                return JsonSerializer.Deserialize(jsonString, obj.GetType());
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
