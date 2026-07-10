using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModelTool.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModelTool.ViewModel
{
    public class MainWindowVM : BaseVM
    {
        public AppVM Global { get; set; }


        private object _CurrentContent;
        public object CurrentContent
        {
            get => _CurrentContent;
            set { _CurrentContent = value; OnPropertyChanged(); }
        }

        public ICommand ToSeting { get; set; }
        public ICommand ToHome { get; set; }
        public ICommand ToBili { get; set; }
        public ICommand ToGitHub { get; set; }

        private void ToSetingExecute() => CurrentContent = new SetingVM();
        private void ToHomeExecute() => CurrentContent = new HomeVM(AppVM.Instance);
        private void ToBiliExecute()
        {
            string url = "https://space.bilibili.com/399621873?spm_id_from=333.1007.0.0";
            ToWeb(url);
        }
        private void ToGitHubExecute()
        {
            string url = "https://github.com/askhdb";
            ToWeb(url);
        }
        public MainWindowVM()
        {
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

            ToSeting = new RelayCommand(ToSetingExecute);
            ToHome = new RelayCommand(ToHomeExecute);
            ToBili = new RelayCommand(ToBiliExecute);
            ToGitHub = new RelayCommand(ToGitHubExecute);

            CurrentContent = new HomeVM(AppVM.Instance);
        }
        public MainWindowVM(AppVM sharedData) : this()
        {
            Global = sharedData;
            Global.Path = AppVM.dataPath.BackgroundImagePath;
            Global.IsRandom = AppVM.data.IsRandom;
        }
        public void ToWeb(string url)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };

            Process.Start(psi);
        }
    }
}
