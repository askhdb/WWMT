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
    public static class DllInjector
    {
        #region Win32 API 导入
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcess(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        #endregion

        // 常量
        public const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
        public const uint CREATE_SUSPENDED = 0x00000004;
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint PAGE_READWRITE = 0x04;
        public const uint INFINITE = 0xFFFFFFFF;

        public static bool Inject(int processId, string dllPath)
        {
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero) return false;

            try
            {
                //分配内存
                IntPtr pRemoteMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)(dllPath.Length + 1) * 2, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                if (pRemoteMemory == IntPtr.Zero) return false;

                byte[] dllPathBytes = System.Text.Encoding.Unicode.GetBytes(dllPath);
                if (!WriteProcessMemory(hProcess, pRemoteMemory, dllPathBytes, (uint)dllPathBytes.Length, out _)) return false;

                IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
                IntPtr pLoadLibraryW = GetProcAddress(hKernel32, "LoadLibraryW");
                if (pLoadLibraryW == IntPtr.Zero) return false;

                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pLoadLibraryW, pRemoteMemory, 0, IntPtr.Zero);
                if (hThread == IntPtr.Zero) return false;

                CloseHandle(hThread);
                return true;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }
        private static async Task<bool> InjectToProcess(IntPtr hProcess, string dllPath, int timeoutMs)
        {
            try
            {
                //分配内存
                uint dllPathSize = (uint)(dllPath.Length + 1) * 2;
                IntPtr pRemoteMemory = VirtualAllocEx(
                    hProcess,
                    IntPtr.Zero,
                    dllPathSize,
                    MEM_COMMIT | MEM_RESERVE,
                    PAGE_READWRITE
                );

                if (pRemoteMemory == IntPtr.Zero)
                {
                    Console.WriteLine($"VirtualAllocEx 失败，错误码：{Marshal.GetLastWin32Error()}");
                    return false;
                }

                //写入 DLL 路径
                byte[] dllPathBytes = Encoding.Unicode.GetBytes(dllPath);
                bool written = WriteProcessMemory(
                    hProcess,
                    pRemoteMemory,
                    dllPathBytes,
                    (uint)dllPathBytes.Length,
                    out _
                );

                if (!written)
                {
                    Console.WriteLine($"WriteProcessMemory 失败，错误码：{Marshal.GetLastWin32Error()}");
                    return false;
                }

                //获取 LoadLibraryW 地址
                IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
                IntPtr pLoadLibraryW = GetProcAddress(hKernel32, "LoadLibraryW");

                if (pLoadLibraryW == IntPtr.Zero)
                {
                    Console.WriteLine("GetProcAddress 失败");
                    return false;
                }

                //创建远程线程
                IntPtr hThread = CreateRemoteThread(
                    hProcess,
                    IntPtr.Zero,
                    0,
                    pLoadLibraryW,
                    pRemoteMemory,
                    0,
                    IntPtr.Zero
                );

                if (hThread == IntPtr.Zero)
                {
                    Console.WriteLine($"CreateRemoteThread 失败，错误码：{Marshal.GetLastWin32Error()}");
                    return false;
                }

                //等待DLL加载完成
                bool threadCompleted = await Task.Run(() =>
                {
                    uint result = WaitForSingleObject(hThread, (uint)timeoutMs);
                    return result == 0;
                });

                CloseHandle(hThread);

                if (!threadCompleted)
                {
                    Console.WriteLine($"DLL 加载超时（{timeoutMs}ms）");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"注入异常：{ex.Message}");
                return false;
            }
        }
        public static async Task<int> WaitForProcessReady(string processName, int timeoutMs = 15000, int checkInterval = 100)
        {
            int elapsed = 0;

            while (elapsed < timeoutMs)
            {
                try
                {
                    //查找匹配的进程
                    Process[] processes = Process.GetProcessesByName(processName);

                    if (processes.Length > 0)
                    {
                        return processes[0].Id;
                    }
                }
                catch
                {

                }

                await Task.Delay(checkInterval);
                elapsed += checkInterval;
            }
            return -1;
        }
        public static async Task<(bool success, int processId)> StartAndInject(string exePath, string dllPath, int timeoutMs = 15000)
        {
            try
            {
                //以挂起状态创建进程
                STARTUPINFO si = new STARTUPINFO();
                si.cb = Marshal.SizeOf(si);

                string arguments = "-dx11 -d3d11 -force-d3d11";

                bool created = CreateProcess(
                    exePath,
                    arguments,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    CREATE_SUSPENDED,  // 进程挂起
                    IntPtr.Zero,
                    System.IO.Path.GetDirectoryName(exePath),
                    ref si,
                    out PROCESS_INFORMATION pi
                );

                if (!created)
                {
                    int error = Marshal.GetLastWin32Error();
                    return (false, -1);
                }

                //注入到Wuthering Waves.exe
                bool injectLauncherSuccess = await InjectToProcess(
                pi.hProcess,
                dllPath,
                timeoutMs
                );
                if (!injectLauncherSuccess)
                {
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    return (false, -1);
                }

                //Wuthering Waves.exe主线程
                ResumeThread(pi.hThread);

                int targetPid = await WaitForProcessReady("Client-Win64-Shipping", 60000);
                if (targetPid == -1)
                {
                    Console.WriteLine("等待目标进程超时");
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    return (false, -1);
                }

                // 打开目标进程获取句柄
                IntPtr hTargetProcess = OpenProcess(PROCESS_ALL_ACCESS, false, targetPid);
                if (hTargetProcess == IntPtr.Zero)
                {
                    return (false, -1);
                }

                //注入到Client-Win64-Shipping.exe
                bool injectGameSuccess = await InjectToProcess(
                    hTargetProcess,
                    dllPath,
                    timeoutMs
                );
                CloseHandle(hTargetProcess);

                if (!injectGameSuccess)
                {
                    CloseHandle(pi.hProcess);
                    return (false, -1);
                }

                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);

                return (true, targetPid);
            }
            catch (Exception ex)
            {
                return (false, -1);
            }
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }
}
