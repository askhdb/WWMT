using ModelTool.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelTool.ViewModel
{
    public class AppVM : BaseVM
    {
        private static AppVM _instance;
        public static AppVM Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppVM();
                }
                return _instance;
            }
        }
        #region 图片资源
        private string _BiliImage;
        public string BiliImage
        {
            get { return _BiliImage; }
            set { _BiliImage = value; OnPropertyChanged(); }
        }
        private string _GitHubImage;
        public string GitHubImage
        {
            get { return _GitHubImage; }
            set { _GitHubImage = value; OnPropertyChanged(); }
        }

        #endregion
        #region 保存的数据
        public static DataPath dataPath = new DataPath();
        public static Data data = new Data();
        public static string Datapath = $"{JsonHelp.appDir}\\data\\data.Json";
        #endregion

        public List<string> ImagePathList { get; set; } = new List<string>();

        private static Stopwatch stopwatch = new Stopwatch();
        private Thread thread;

        public static string GouImagePath { get; set; } = HomeVM.appDir + "Images\\勾.png";

        //随机切换壁纸
        private bool _IsRandom = false;
        public bool IsRandom
        {
            get { return _IsRandom; }
            set
            {
                if(_IsRandom != value)
                {
                    _IsRandom = value;
                    OnPropertyChanged();
                    data.IsRandom = value;
                    JsonHelp.Instance.SaveToJson(data, Datapath);
                }

                if(_IsRandom)
                {
                    //唤醒线程
                    thread.Interrupt();
                }
                else
                {
                    //设置当前壁纸 = 后台壁纸
                    thread.Interrupt();          //结束
                    Path = dataPath.BackgroundImagePath;
                }
            }
        }


        private string _Path = "";
        public string Path
        {
            get { return _Path; }
            set
            {
                if (!IsRandom)
                {
                    if (_Path != value)
                    {
                        _Path = value;
                        OnPropertyChanged();
                        dataPath.BackgroundImagePath = value;
                        JsonHelp.Instance.SaveToJson(dataPath);
                    }
                }
                else
                {
                    if(_Path != value)
                    {
                        _Path = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        AppVM()
        {
            BiliImage = HomeVM.appDir + $"Images\\哔哩哔哩.png";
            GitHubImage = HomeVM.appDir + $"Images\\github.png";

            ImagePathList = Directory.GetFiles(HomeVM.appDir + "BackgroundImage\\").Where((x) => { if (x.Contains(".png")) { return true; } else { return false; } }).ToList();
            thread = new Thread(OnStartThread);
            thread.IsBackground = true;
            thread.Start();
        }

        private void OnStartThread()
        {
            while(true)
            {
                try
                {
                    while (IsRandom)
                    {
                        Thread.Sleep(5000);
                        Random random = new Random();
                        string randomPath;
                        do
                        {
                            int randomNum = random.Next(0, ImagePathList.Count);
                            randomPath = ImagePathList[randomNum];
                        } while (Path == randomPath);
                        Path = randomPath;
                    }
                    Thread.Sleep(2000);
                }
                catch(Exception e)
                {

                }
            }
        }
    }
    public class DataPath
    {
        public string GameFileDir { get; set; } = "";
        public string WWMIFileDir { get; set; } = "";
        public string BackgroundImagePath { get; set; } = "";
    }
    public class Data
    {
        public bool IsRandom { get; set; } = false;
        public string StartSelection { get; set; } = "";
        public string NowVersion { get; set; } = "";
        public bool AutoUpdate { get; set; } = false;
    }
}
