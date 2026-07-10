using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModelTool.View
{
    /// <summary>
    /// PathSet.xaml 的交互逻辑
    /// </summary>
    public partial class PathSet : UserControl
    {
        public PathSet()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.PathSetVM(ViewModel.AppVM.Instance);
        }
    }
}
