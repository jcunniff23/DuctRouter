using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DuctRouter
{

    public class MainViewModel : INotifyPropertyChanged
    {
        private string _selectedElementInfo;
        public string SelectedElementInfo
        {
            get => _selectedElementInfo;
            set
            {
                if (_selectedElementInfo != value)
                {
                    _selectedElementInfo = value;
                    OnPropertyChanged(nameof(SelectedElementInfo));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
