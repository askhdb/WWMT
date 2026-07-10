using ModelTool.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ModelTool.ViewModel
{
    public class StartSetVM : BaseVM
    {
        public AppVM Global { get; set; }

        public List<string> ComboBoxItems { get; } = new List<string>
        {
            "启动原版游戏",
            "启动Model"
        };

        private string _SelectedComboBoxItem;
        public string SelectedComboBoxItem
        {
            get => _SelectedComboBoxItem;
            set
            {
                if(_SelectedComboBoxItem != value)
                {
                    _SelectedComboBoxItem = value;
                    OnPropertyChanged();
                    AppVM.data.StartSelection = value;
                    JsonHelp.Instance.SaveToJson(AppVM.data, AppVM.Datapath);
                }
            }
        }

        public StartSetVM(AppVM sharedData)
        {
            Global = sharedData;

            //初始化
            AppVM.data = (Data)JsonHelp.Instance.LoadFromJson(AppVM.data, AppVM.Datapath);
            if( AppVM.data == null )
            {
                AppVM.data = new Data();
            }

            SelectedComboBoxItem = AppVM.data.StartSelection;
        }
    }
}
