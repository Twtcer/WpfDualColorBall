using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WpfDualColorBall
{
    public class Result : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<int> data { get; set; } = Enumerable.Range(1, 7).Select(a => a).ToList();
        public List<int> Data {
            get { return data; }
            set
            {
                data = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Data"));
            }
                }

    }
}
