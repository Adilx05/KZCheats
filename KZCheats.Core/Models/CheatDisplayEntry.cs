using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KZCheats.Core.Models
{
    public class CheatDisplayEntry : INotifyPropertyChanged
    {
        private string _value;

        public string Name { get; set; }
        public string Type { get; set; }
        public string AddressOrPointer { get; set; }
        public bool Freeze { get; set; }

        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
