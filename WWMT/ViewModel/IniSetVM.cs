using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ModelTool.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ModelTool.ViewModel
{
    public class IniSetVM : BaseVM
    {

        private List<(string,string)> Variables;

        private List<string> lines;

        private ComboBoxItem _selectedComboBoxItem;
        public ComboBoxItem SelectedComboBoxItem
        {
            get => _selectedComboBoxItem;
            set
            {
                if (_selectedComboBoxItem != value)
                {
                    _selectedComboBoxItem = value;
                    OnPropertyChanged();
                    //HandleSelection(value);
                }
            }
        }
        public ICommand SetINI {  get; set; }

        private void _SetINI()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "";
            dialog.Filter = "选择INI文件|*.ini";
            if(dialog.ShowDialog() == true)
            {
                HandleSelection(SelectedComboBoxItem,dialog.FileName);
            }
        }

        public IniSetVM()
        {
            Variables = new List<(string,string)>();
            SetINI = new RelayCommand(_SetINI);
        }

        private async Task HandleSelection(ComboBoxItem item, string Path)
        {
            try
            {
                string content = item?.Content?.ToString();
                switch (content)
                {
                    case "自动生成变量":
                        await AutoGenerateAsync(Path);
                        break;
                    case "...":

                        break;
                    default:
                        MessageBox.Show("未选择功能选项");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex}");
            }
        }

        private async Task AutoGenerateAsync(string Path)
        {
            lines = await IniHelp.Instance.StreamReadINIAsync(Path);
            char Index = 'A';
            for (int i = 0;i< lines.Count;i++)
            {
                if(lines[i].Contains("Draw Component"))
                {

                    //; Draw Component 3.004 name or ; Draw Component 3.003

                    string VName;     //变量名
                    string str;      //注释
                    if(lines[i].Replace("; Draw Component ", "").Trim().Split(' ').Length>1)
                    {
                        str = lines[i].Replace("; Draw Component ", "").Trim().Split(' ')[1];
                        VName = str.GethashCode_8();
                    }else
                    {
                        str = VName = $"{Index++}";
                    }


                    //生成变量名
                    string Variable = $"Variable{VName}";
                    //保存注释及变量名
                    Variables.Add((str, Variable));
                    //插入if
                    lines.Insert(++i, $"        if ${Variable} == 1");
                    lines.Insert(i = i + 2, "        endif");
                }
                if (lines[i].Contains("Shading: Textures")) { break; }
            }
            AutoGenerateVariables();
            await IniHelp.Instance.StreamWriteINIAsync(Path, lines);
        }
        private void AutoGenerateVariables()
        {
            Variables.Reverse();     //倒序
            int i = 0;
            bool SSS = false;
            for (; i < lines.Count; i++)
            {
                if (lines[i].Contains("ModelTool"))
                {
                    SSS = true;
                    break;
                }
                else if (lines[i].Contains("[Present]"))
                {
                    SSS= false;
                    break;
                }
            }
            //------------------
            if(SSS)
            {
                while (!lines[i+1].Contains("EndModelTool"))
                {
                    lines.RemoveAt(i+1);
                }
                //生成变量
                foreach (var item in Variables)
                {
                    lines.Insert(i + 1, $"global {item.Item2} = 1");
                    lines.Insert(i + 1, $";{item.Item1}");
                }
            }
            else
            {
                lines.Insert(i, "; EndModelTool---------------------------------");
                //生成变量
                foreach (var item in Variables)
                {
                    lines.Insert(i, $"global {item.Item2} = 1");
                    lines.Insert(i, $";{item.Item1}");
                }
                lines.Insert(i, "; ModelTool---------------------------------");
                MessageBox.Show("生成成功");
            }
        }
    }
    public static class StringExtensions
    {
        public static string GethashCode_8(this string input)
        {
            int hashCode = input.GetHashCode();
            return hashCode.ToString("X8").ToLower();
        }
    }
}
