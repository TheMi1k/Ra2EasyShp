using System.ComponentModel;

namespace Ra2EasyShp.Models
{
    public class MainWindowSizeModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _mainWindowW { get; set; }
        public int MainWindowW
        {
            get => _mainWindowW;
            set
            {
                if (value - 560 < 0)
                {
                    return;
                }
                _mainWindowW = value;
                Border_ImgViewW = value - 560;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MainWindowW"));
            }
        }

        private int _mainWindowH { get; set; }
        public int MainWindowH
        {
            get => _mainWindowH;
            set
            {
                if (value - 360 < 0)
                {
                    return;
                }
                _mainWindowH = value;
                DataGrid_ImagesH = value - 110;
                Border_ImgViewH = value - 360;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MainWindowH"));
            }
        }

        private int _dataGrid_ImagesH { get; set; }
        public int DataGrid_ImagesH
        {
            get => _dataGrid_ImagesH;
            set
            {
                _dataGrid_ImagesH = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DataGrid_ImagesH"));
            }
        }

        private int _border_ImgViewW { get; set; }
        public int Border_ImgViewW
        {
            get => _border_ImgViewW;
            set
            {
                _border_ImgViewW = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Border_ImgViewW"));
            }
        }

        private int _border_ImgViewH {  get; set; }
        public int Border_ImgViewH
        {
            get => _border_ImgViewH;
            set
            {
                _border_ImgViewH = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Border_ImgViewH"));
            }
        }
    }
}
