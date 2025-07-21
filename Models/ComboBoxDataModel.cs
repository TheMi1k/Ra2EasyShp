using Ra2EasyShp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ra2EasyShp.Models
{
    public class ComboBoxDataModel<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Update()
        {
            DisplayName = GData.UIData.LanguageDic.TryGetValue(_key, out string languageValue) ? languageValue : "#" + _key;
        }

        private string _key { get; set; }
        internal string Key
        {
            get => _key;
            set
            {
                _key = value;
                DisplayName = GData.UIData.LanguageDic.TryGetValue(_key, out string languageValue) ? languageValue : "#" + _key;
            }
        }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            private set
            {
                _displayName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayName"));
            }
        }

        public T Value { get; set; }
    }
}
