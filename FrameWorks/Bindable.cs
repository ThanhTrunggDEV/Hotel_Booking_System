using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Manager.FrameWorks
{
    public class Bindable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void Set<T>(ref T prop, T value, [CallerMemberName] string propertyName = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new(propertyName));
        }
    }
}
