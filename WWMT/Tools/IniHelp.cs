using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace ModelTool.Tools
{
    public class IniHelp
    {
        private static IniHelp _instance;
        public static IniHelp Instance
        {
            get
            {
                return _instance ?? (_instance = new IniHelp());
            }
        }

        public List<string> ReadINI(string path)
        {
            string[] lines = File.ReadAllLines(path);
            return lines.ToList();
        }

        public async Task<List<string>> ReadINIAsync(string path)
        {
            string[] lines = await File.ReadAllLinesAsync(path);
            return lines.ToList();
        }
        public async Task<List<string>> StreamReadINIAsync(string path)
        {
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }
        public bool WriteINI(string path, List<string> lines)
        {
            try
            {
                File.WriteAllLines(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> WriteINIAsync(string path, List<string> lines)
        {
            try
            {
                await File.WriteAllLinesAsync(path, lines);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> StreamWriteINIAsync(string path, List<string> lines)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    foreach (string line in lines)
                    {
                        await writer.WriteLineAsync(line);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
