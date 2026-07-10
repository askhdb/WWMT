using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using ModelTool.Tools;
using System.Diagnostics.SymbolStore;

namespace ModelTool.ViewModel
{
    public class PathSetVM : BaseVM
    {
        public AppVM Global { get; set; }

        private string _GameFileDir;

        public string GameFileDir
        {
            get => _GameFileDir;
            set
            {
                if(_GameFileDir != value)
                {
                    _GameFileDir = value;
                    OnPropertyChanged();
                    AppVM.dataPath.GameFileDir = value;
                    JsonHelp.Instance.SaveToJson(AppVM.dataPath);
                }
            }
        }
        private string _WWMIFileDir;
        public string WWMIFileDir
        {
            get => _WWMIFileDir;
            set
            {
                if( _WWMIFileDir != value)
                {
                    _WWMIFileDir = value;
                    OnPropertyChanged();
                    AppVM.dataPath.WWMIFileDir = value;
                    JsonHelp.Instance.SaveToJson(AppVM.dataPath);
                }
            }
        }
        private string _BackgroundImagePath;
        public string BackgroundImagePath
        {
            get { return _BackgroundImagePath; }
            set
            {
                if (_BackgroundImagePath != value)
                {
                    _BackgroundImagePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand OpenGameFileDir { get; set; }
        public ICommand OpenWWMIFileDir { get; set; }
        public ICommand OpenBackgroundImageDir { get; set; }

        private void _OpenGameFileDir()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "选择游戏文件";
            dialog.Filter = "游戏文件|*.exe";
            if(dialog.ShowDialog() == true)
            {
                GameFileDir = dialog.FileName;
            }
        }
        private void _OpenWWMIFileDir()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "选择WWMI文件";
            dialog.Filter = "WWMI目录|*.*";
            if (dialog.ShowDialog() == true)
            {
                WWMIFileDir = dialog.FileName;
            }
        }
        private void _OpenBackgroundImageDir()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "选择图片文件";
            dialog.Filter = "背景图片|*.PNG|背景图片|*.jpg";
            if (dialog.ShowDialog() == true)
            {
                if(Global.IsRandom)   //正在随机播放壁纸时，保存路径到Json文件中
                {
                    BackgroundImagePath = dialog.FileName;
                    AppVM.dataPath.BackgroundImagePath = BackgroundImagePath;
                    JsonHelp.Instance.SaveToJson(AppVM.dataPath);
                }
                else
                {
                    BackgroundImagePath = dialog.FileName;
                    Global.Path = BackgroundImagePath;
                    AppVM.dataPath.BackgroundImagePath = BackgroundImagePath;
                    JsonHelp.Instance.SaveToJson(AppVM.dataPath);
                }
            }
        }

        public PathSetVM(AppVM sharedData)
        {
            Global = sharedData;
            //界面加载时
            Init();
        }
        private void Init()
        {
            OpenGameFileDir = new RelayCommand(_OpenGameFileDir);
            OpenWWMIFileDir = new RelayCommand(_OpenWWMIFileDir);
            OpenBackgroundImageDir = new RelayCommand(_OpenBackgroundImageDir);

            //读取Json文件
            AppVM.dataPath = (DataPath)JsonHelp.Instance.LoadFromJson(AppVM.dataPath);
            if (AppVM.dataPath == null)
            {
                AppVM.dataPath = new DataPath();
            }
            AppVM.data = (Data)JsonHelp.Instance.LoadFromJson(AppVM.data, $"{JsonHelp.appDir}\\data\\data.Json");
            if (AppVM.data == null)
            {
                AppVM.data = new Data();
            }

            //每次加载该界面时，初始化保存的数据
            GameFileDir = AppVM.dataPath.GameFileDir;
            WWMIFileDir = AppVM.dataPath.WWMIFileDir;
            BackgroundImagePath = AppVM.dataPath.BackgroundImagePath;
            Global.IsRandom = AppVM.data.IsRandom;
        }
    }
}
