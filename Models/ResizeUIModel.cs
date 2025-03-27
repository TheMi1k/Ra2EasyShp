using System;
using System.ComponentModel;
using System.Windows;

namespace Ra2EasyShp.Models
{
    public class ResizeUIModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void SetMaintainAspectRatio()
        {
            if (ImageOriginalWidth == 0 || ImageOriginalHeight == 0)
            {
                return;
            }    
            float a = (ImageOriginalWidth * 1.0f) / (ImageOriginalHeight * 1.0f);
            ImageNowHeight = (int)Math.Round(ImageNowWidth / a);
        }

        internal void ResetReMargin()
        {
            ReMarginLeft = "0";
            ReMarginTop = "0";
            ReMarginRight = "0";
            ReMarginBottom = "0";

            NowReMargin = new Thickness(0, 0, 0, 0);
        }

        internal int ImageOriginalWidth { get; set; } = 0;
        internal int ImageOriginalHeight { get; set; } = 0;

        private int _imageNowWidth { get; set; } = 0;
        public int ImageNowWidth
        {
            get => _imageNowWidth;
            set
            {
                _imageNowWidth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageNowWidthStr"));
            }
        }
        public string ImageNowWidthStr
        {
            get => _imageNowWidth.ToString();
            set
            {
                if (value == "")
                {
                    _imageNowWidth = 0;
                }
                else if (int.TryParse(value, out int num))
                {
                    _imageNowWidth = num;
                }
                else
                {
                    return;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageNowWidthStr"));
            }
        }

        private int _imageNowHeight { get; set; } = 0;
        public int ImageNowHeight
        {
            get => _imageNowHeight;
            set
            {
                _imageNowHeight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageNowHeightStr"));
            }
        }
        public string ImageNowHeightStr
        {
            get => _imageNowHeight.ToString();
            set
            {
                if (value == "")
                {
                    _imageNowHeight = 0;
                }
                else if (int.TryParse(value, out int num))
                {
                    _imageNowHeight = num;
                }
                else
                {
                    return;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageNowHeightStr"));
            }
        }

        private string _reMarginLeft { get; set; } = "0";
        public string ReMarginLeft
        {
            get => _reMarginLeft;
            set
            {
                _reMarginLeft = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReMarginLeft"));
            }
        }

        private string _reMarginTop { get; set; } = "0";
        public string ReMarginTop
        {
            get => _reMarginTop;
            set
            {
                _reMarginTop = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReMarginTop"));
            }
        }

        private string _reMarginRight { get; set; } = "0";
        public string ReMarginRight
        {
            get => _reMarginRight;
            set
            {
                _reMarginRight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReMarginRight"));
            }
        }

        private string _reMarginBottom { get; set; } = "0";
        public string ReMarginBottom
        {
            get => _reMarginBottom;
            set
            {
                _reMarginBottom = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReMarginBottom"));
            }
        }

        private bool _reMarginCutImage { get; set; } = false;
        public bool ReMarginCutImage
        {
            get => _reMarginCutImage;
            set
            {
                _reMarginCutImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReMarginCutImage"));
            }
        }

        internal Thickness NowReMargin { get; set; } = new Thickness(0, 0, 0, 0);
    }
}
