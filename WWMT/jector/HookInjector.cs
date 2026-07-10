using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ModelTool.jector
{
    public class HookInjector
    {
        // Win32 API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private const int WH_GETMESSAGE = 3;

        public static async Task<int> InjectWithHook(string targetExePath, string dllPath, int timeoutMs = 60000)
        {
            //启动主进程
            Process process = new Process();
            process.StartInfo.FileName = targetExePath;
            process.StartInfo.UseShellExecute = true;
            process.Start();

            int targetPid = await WaitForProcessWithWindow("Client-Win64-Shipping", timeoutMs);

            if (targetPid == -1)
                return -1;

            //SetWindowsHookEx注入
            IntPtr hHook = SetWindowsHookEx(WH_GETMESSAGE, (nCode, wParam, lParam) =>
            {
                //钩子被触发时加载DLL
                if (nCode >= 0)
                {
                    LoadLibrary(dllPath);
                }
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }, GetModuleHandle(Path.GetFileNameWithoutExtension(dllPath)), 0);

            if (hHook == IntPtr.Zero)
            {
                Console.WriteLine($"SetWindowsHookEx 失败，错误码：{Marshal.GetLastWin32Error()}");
                return -1;
            }

            // 等待游戏完全启动
            await Task.Delay(5000);

            UnhookWindowsHookEx(hHook);
            return targetPid;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private static async Task<int> WaitForProcessWithWindow(string processName, int timeoutMs)
        {
            int elapsed = 0;
            while (elapsed < timeoutMs)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    try
                    {
                        process.Refresh();
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            return process.Id;
                        }
                    }
                    catch { }
                }
                await Task.Delay(500);
                elapsed += 500;
            }
            return -1;
        }
    }
}
