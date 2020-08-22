using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

namespace WpfDualColorBall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //双色球规则
        //https://baike.baidu.com/item/%E4%B8%AD%E5%9B%BD%E7%A6%8F%E5%88%A9%E5%BD%A9%E7%A5%A8%E5%8F%8C%E8%89%B2%E7%90%83%E6%B8%B8%E6%88%8F%E8%A7%84%E5%88%99/2331202
        //6红球+1蓝球
        //红球范围1-33,蓝球范围1-16

        //private List<string> RedBallNums = Enumerable.Range(1, 33).Select(a => a.ToString().PadLeft(2, '0')).ToList();
        //private List<string> BlueBallNums = Enumerable.Range(1, 16).Select(a => a.ToString().PadLeft(2, '0')).ToList(); 
        //public Result Result;

        private static readonly ConcurrentDictionary<int, bool> SourceDic = new ConcurrentDictionary<int, bool>(); //存放数字的字典 标记为True 表示已占用，不能再使用 
        private static readonly ConcurrentDictionary<int, int> ResultDic = new ConcurrentDictionary<int, int>(); //存放UI页面上次存放的数字 通过Label的Id 保存最后的结果值 
        private static readonly Random Random = new Random(); //随机数
        private CancellationTokenSource cancelToken = new CancellationTokenSource(); //取消信号源
        private static readonly object LockObj = new object();//锁 

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        void Init()
        {
            //info.Text = string.Join(',', RedBallNums ) ;
            //info.Text+= string.Join(',', BlueBallNums);

            foreach (var i in Enumerable.Range(1, 33)) { SourceDic.TryAdd(i, false); }
            foreach (var i in Enumerable.Range(1, 7)) { ResultDic.TryAdd(i, i); }

            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false; 
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (cancelToken.IsCancellationRequested)
            {
                cancelToken = new CancellationTokenSource();
            }

            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;

            foreach (var i in Enumerable.Range(1, 7))
            {
                Task.Factory.StartNew(o =>
                {
                    var controlId = Convert.ToInt32(o);
                    while (!cancelToken.IsCancellationRequested)
                    {
                        if (controlId == 7)//==7表示是绿球
                        {
                            ResultDic.TryGetValue(7, out var value);
                            ResultDic.TryUpdate(7, Random.Next(1, 17), value);
                        }
                        else
                        {
                            var oldValueKey = ResultDic.GetOrAdd(controlId, controlId);
                            var newValueKey = GetNoDuplicate(oldValueKey);
                            ResultDic.TryUpdate(controlId, newValueKey, oldValueKey);
                        }

                        //UpdateLabel(controlId);
                        //info.Text = string.Join(",", ResultDic);
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            info.Text = string.Join(" , ", ResultDic.Select(a=>a.Value)) ;
                        });
                        Thread.Sleep(Random.Next(1, 5));//1-5毫秒随机停顿
                    }
                }, i);
            }  
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;

            cancelToken.Cancel();
        }


        private static int GetNoDuplicate(int oldKey)
        {
            int key;
            lock (LockObj)
            {
                do
                {
                    key = Random.Next(1, 33);
                }
                while (oldKey == key || SourceDic.GetOrAdd(key, true));
                SourceDic.TryUpdate(oldKey, false, true);
                SourceDic.TryUpdate(key, true, false);
            }
            return key;
        }

        //private void UpdateLabel(int controlId)
        //{  
        //    App.Current.Dispatcher.Invoke(() =>
        //    {
        //        if (App.Current.Controls["label" + controlId] is Label label)
        //        {
        //            label.Content = ResultDic[controlId].ToString();
        //        }

        //        //前6个数字有值相等 报异常
        //        if (ResultDic.Take(6).Select(x => x.Value).Distinct().Count() != 6)
        //        {
        //            throw new Exception("前6位有重复值");
        //        }
        //    }); 
        //}

    }
}
