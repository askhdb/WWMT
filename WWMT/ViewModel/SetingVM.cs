using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModelTool.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModelTool.ViewModel
{
    public class SetingVM : BaseVM
    {
        private object _CurrentContent;
        public object CurrentContent
        {
            get => _CurrentContent;
            set { _CurrentContent = value; OnPropertyChanged(); }
        }
        public ICommand ToPathSet { get; set; }
        public ICommand ToIniSet { get; set; }
        public ICommand ToStartSet { get; set; }
        public ICommand ToUpdateSet { get; set; }

        private void ToPathSetExecute() => CurrentContent = new PathSetVM(AppVM.Instance);
        private void ToIniSetExecute() => CurrentContent = new IniSetVM();
        private void ToStartSetExecute() => CurrentContent = new StartSetVM(AppVM.Instance);
        private void ToUpdateSetExecute() => CurrentContent = new UpdateSetVM(AppVM.Instance);

        public SetingVM()
        {
            ToPathSet = new RelayCommand(ToPathSetExecute);
            ToIniSet = new RelayCommand(ToIniSetExecute);
            ToStartSet = new RelayCommand(ToStartSetExecute);
            ToUpdateSet = new RelayCommand(ToUpdateSetExecute);

            CurrentContent = new PathSetVM(AppVM.Instance);
        }
    }
}
