using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Soundboard.Models
{
    public class BaseNotifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T prop, T value, [CallerMemberName] string fieldName = "")
        {
            if (EqualityComparer<T>.Default.Equals(prop, value))
                return false;
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(fieldName));
            return true;
        }
    }
}
