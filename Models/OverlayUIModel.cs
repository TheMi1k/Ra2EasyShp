using Ra2EasyShp.Data;
using System.ComponentModel;
using System.Windows;

namespace Ra2EasyShp.Models
{
    public class OverlayUIModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void SetDefault()
        {
            _isChangeOverlayMargin = false;

            OverlayOffsetX = 0;
            OverlayOffsetY = 0;
            OverlayXOffsetStr = "0";
            OverlayYOffsetStr = "0";
            OverlayGridThickness = new Thickness(0, 0, 0, 0);
            OverlayMode = Enums.OverlayMode.叠加在上;

            _isChangeOverlayMargin = true;
        }

        internal void ChangeOverlayMargin(int x, int y)
        {
            _isChangeOverlayMargin = false;

            OverlayXOffsetStr = x.ToString();
            OverlayYOffsetStr = y.ToString();

            _isChangeOverlayMargin = true;

            OverlayGridThickness = new Thickness(x, y, 0, 0);
        }

        internal void SetOverlayData(int index = -1)
        {
            if (index == -1)
            {
                index = GData.UIData.NowIndex;
            }

            if (GData.ImageData.Count < index + 1)
            {
                return;
            }

            GData.ImageData[index].OverlayImage.OffsetX = OverlayOffsetX;
            GData.ImageData[index].OverlayImage.OffsetY = OverlayOffsetY;
            GData.ImageData[index].OverlayImage.OverlayMode = OverlayMode;
        }

        public int OverlayOffsetX { get; private set; } = 0;
        public int OverlayOffsetY { get; private set; } = 0;

        private bool _isChangeOverlayMargin { get; set; } = true;

        private string _overlayXOffsetStr { get; set; } = "0";
        public string OverlayXOffsetStr
        {
            get => _overlayXOffsetStr;
            set
            {
                _overlayXOffsetStr = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverlayXOffsetStr"));

                if (value == "")
                {
                    OverlayOffsetX = 0;
                }
                else if (int.TryParse(value, out int x))
                {
                    OverlayOffsetX = x;
                }
                else
                {
                    return;
                }

                if (_isChangeOverlayMargin)
                {
                    OverlayGridThickness = new Thickness(OverlayOffsetX, OverlayOffsetY, 0, 0);
                    SetOverlayData();
                }
            }
        }

        private string _overlayYOffsetStr { get; set; } = "0";
        public string OverlayYOffsetStr
        {
            get => _overlayYOffsetStr;
            set
            {
                _overlayYOffsetStr = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverlayYOffsetStr"));

                if (value == "")
                {
                    OverlayOffsetY = 0;
                }
                else if (int.TryParse(value, out int y))
                {
                    OverlayOffsetY = y;
                }
                else
                {
                    return;
                }

                if (_isChangeOverlayMargin)
                {
                    OverlayGridThickness = new Thickness(OverlayOffsetX, OverlayOffsetY, 0, 0);
                    SetOverlayData();
                }
            }
        }


        private Thickness _overlayGridThickness { get; set; } = new Thickness(0, 0, 0, 0);
        public Thickness OverlayGridThickness
        {
            get => _overlayGridThickness;
            private set
            {
                _overlayGridThickness = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverlayGridThickness"));
            }
        }

        private Enums.OverlayMode _overlayMode { get; set; } = Enums.OverlayMode.叠加在上;
        public Enums.OverlayMode OverlayMode
        {
            get => _overlayMode;
            set
            {
                _overlayMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverlayMode"));
            }
        }
    }
}
