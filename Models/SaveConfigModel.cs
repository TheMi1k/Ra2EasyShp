using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ra2EasyShp.Models
{
    public class SaveConfigModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isPaletteCustomPath { get; set; } = false;
        public bool IsPaletteCustomPath
        {
            get => _isPaletteCustomPath;
            set
            {
                _isPaletteCustomPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsPaletteCustomPath"));
            }
        }

        private string _paletteCustomPath { get; set; } = string.Empty;
        public string PaletteCustomPath
        {
            get => _paletteCustomPath;
            set
            {
                _paletteCustomPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PaletteCustomPath"));
            }
        }

        private bool _isPaletteMapType { get; set; } = false;
        public bool IsPaletteMapType
        {
            get => _isPaletteMapType;
            set
            {
                _isPaletteMapType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsPaletteMapType"));
            }
        }

        private string _paletteName { get; set; } = string.Empty;
        public string PaletteName
        {
            get => _paletteName;
            set
            {
                _paletteName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PaletteName"));
            }
        }




        private bool _isShpCustomPath { get; set; } = false;
        public bool IsShpCustomPath
        {
            get => _isShpCustomPath;
            set
            {
                _isShpCustomPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsShpCustomPath"));
            }
        }

        private string _shpCustomPath { get; set; } = string.Empty;
        public string ShpCustomPath
        {
            get => _shpCustomPath;
            set
            {
                _shpCustomPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShpCustomPath"));
            }
        }

        private bool _isShpMapType { get; set; } = false;
        public bool IsShpMapType
        {
            get => _isShpMapType;
            set
            {
                _isShpMapType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsShpMapType"));
            }
        }

        private string _shpName { get; set; } = string.Empty;
        public string ShpName
        {
            get => _shpName;
            set
            {
                _shpName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShpName"));
            }
        }
    }
}
