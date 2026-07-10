using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModelTool.Model;
using ModelTool.jector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ModelTool.ViewModel
{
    public enum Status
    {
        _,
        正在打开游戏,
        游戏打开失败,
        等待进程就绪,
        正在注入d3d11,
        游戏注入失败,
        等待游戏启动,
        游戏启动成功,

        等待超时,
    }
    public class HomeVM : BaseVM
    {
        #region 图片资源
        private string _SetImageDir;

        public static string appDir = AppDomain.CurrentDomain.BaseDirectory;
        public string SetImageDir
        {
            get => _SetImageDir;
            set { _SetImageDir = value; OnPropertyChanged(); }
        }
        #endregion

        public AppVM Global { get; set; }
        private HomeModel model = new HomeModel();
        private Status _status;
        
        public string status
        {
            get => _status.ToString();
            set
            {
                if(_status.ToString() != value)
                {
                    _status = Enum.Parse<Status>(value);
                    OnPropertyChanged();
                }
            }
        }


        public ICommand Start { get; set; }
        private async void _Start()
        {
            if(String.IsNullOrEmpty(AppVM.dataPath.GameFileDir))
            {
                MessageBox.Show("未设置游戏路径");
                return;
            }
            if(!File.Exists(AppVM.dataPath.GameFileDir) || AppVM.dataPath.WWMIFileDir.EndsWith("Wuthering Waves.exe"))
            {
                MessageBox.Show("请设置正确游戏路径");
                return;
            }
            if (String.IsNullOrEmpty(AppVM.dataPath.WWMIFileDir))
            {
                MessageBox.Show("未设置WWMI路径");
                return;
            }
            if (!File.Exists(AppVM.dataPath.WWMIFileDir))
            {
                MessageBox.Show("请设置正确WWMI路径");
                return;
            }

            if (String.IsNullOrEmpty(AppVM.data.StartSelection) || AppVM.data.StartSelection == "启动Model")
            {
                await StartMMMI();
            }
            else if(AppVM.data.StartSelection == "启动原版游戏")
            {
                await StartGame();
            }
        }
        private async Task StartMMMI()
        {
            //int re = await HookInjector.InjectWithHook(PathSetVM.dataPath.GameFileDir, PathSetVM.dataPath.WWMIFileDir);
            //if (re == -1)
            //{
            //    status = Status.游戏注入失败.ToString();
            //    return;
            //}
            //status = Status.等待游戏启动.ToString();

            status = Status.正在打开游戏.ToString();
            bool OpenRe = await model.StartProcess(AppVM.dataPath.GameFileDir);
            if (!OpenRe)
            {
                status = Status.游戏打开失败.ToString();
                return;
            }

            status = Status.等待进程就绪.ToString();

            int re = await DllInjector.WaitForProcessReady("Client-Win64-Shipping", 60000);
            if (re == -1)
            {
                status = Status.等待超时.ToString();
                return;
            }
            else
            {
                status = Status.正在注入d3d11.ToString();
                bool injectSuccess = DllInjector.Inject(re, AppVM.dataPath.WWMIFileDir);
                if (injectSuccess)
                {
                    status = Status.等待游戏启动.ToString();
                }
                else
                {
                    status = Status.游戏注入失败.ToString();
                }
            }
        }
        private async Task StartGame()
        {
            status = Status.正在打开游戏.ToString();
            bool OpenRe = await model.StartProcess(AppVM.dataPath.GameFileDir);
            if (!OpenRe)
            {
                status = Status.游戏打开失败.ToString();
                return;
            }
            status = Status.等待游戏启动.ToString();
        }

        public HomeVM(AppVM sharedData)
        {
            SetImageDir += appDir + "Images\\设置.png";

            Global = sharedData;

            Start = new RelayCommand(_Start);
        }
    }
}
