using CommunityToolkit.Mvvm.Input;
using ModelTool.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModelTool.ViewModel
{
    public class UpdateSetVM : BaseVM
    {
        public AppVM Global { get; set; }

        public string _Version;
        public string Version
        {
            get => _Version;
            set
            {
                if(_Version != value)
                {
                    _Version = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _AutoUpdate = false;
        public bool Autoupdate
        {
            get => _AutoUpdate;
            set
            {
                if( _AutoUpdate != value)
                {
                    _AutoUpdate = value;
                    OnPropertyChanged();
                    AppVM.data.AutoUpdate = value;
                    JsonHelp.Instance.SaveToJson(AppVM.data, AppVM.Datapath);
                }
            }
        }
        public ICommand Update {  get; set; }
        private void _Update()
        {
            //更新方法

        }
        public UpdateSetVM(AppVM sharedData)
        {
            Global = sharedData;
            Update = new RelayCommand(_Update);

            //初始化获得版本号
            AppVM.data = (Data)JsonHelp.Instance.LoadFromJson(AppVM.data, AppVM.Datapath);
            if (AppVM.data == null)
            {
                AppVM.data = new Data();
            }

            Version = AppVM.data.NowVersion;
            Autoupdate = AppVM.data.AutoUpdate;
        }
    }
}
