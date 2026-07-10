using ModelTool.jector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelTool.Model
{
    public class HomeModel
    {
        public HomeModel()
        {

        }
        public async Task<bool> StartProcess(string exePath)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = exePath;
                process.StartInfo.UseShellExecute = true;

                process.Start();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
