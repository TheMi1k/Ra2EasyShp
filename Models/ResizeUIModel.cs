using Ra2EasyShp.Data;
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

        private bool _isSyncResizeWidthAndHeight = true;

        private bool _resizeMaintainAspectRatio = false;
        public bool ResizeMaintainAspectRatio
        {
            get => _resizeMaintainAspectRatio;
            set
            {
                _resizeMaintainAspectRatio = value;
            }
        }



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
            get
            {
                if (_imageNowWidth > 0)
                {
                    return _imageNowWidth.ToString();
                }

                return string.Empty;
            }
            set
            {
                if (int.TryParse(value, out int num))
                {
                    _imageNowWidth = num;
                    float temp = num / (ImageOriginalWidth * 1.0f) * 100.0f;
                    if (temp > 0 && temp < 1.0f)
                    {
                        ImageNowWidthPercentage = 1;
                    }
                    else
                    {
                        ImageNowWidthPercentage = (int)Math.Round(temp);
                    }

                    if (ResizeMaintainAspectRatio && _isSyncResizeWidthAndHeight)
                    {
                        _isSyncResizeWidthAndHeight = false;
                        ImageNowHeightStr = Math.Round(ImageOriginalHeight * (Math.Round(temp) / 100.0f)).ToString();
                        _isSyncResizeWidthAndHeight = true;
                    }
                }
                else
                {
                    _imageNowWidth = -1;
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
            get
            {
                if (_imageNowHeight > 0)
                {
                    return _imageNowHeight.ToString();
                }

                return string.Empty;
            }
            set
            {
                if (int.TryParse(value, out int num))
                {
                    _imageNowHeight = num;

                    float temp = num / (ImageOriginalHeight * 1.0f) * 100.0f;
                    if (temp > 0 && temp < 1.0f)
                    {
                        ImageNowHeightPercentage = 1;
                    }
                    else
                    {
                        ImageNowHeightPercentage = (int)Math.Round(temp);
                    }

                    if (ResizeMaintainAspectRatio && _isSyncResizeWidthAndHeight)
                    {
                        _isSyncResizeWidthAndHeight = false;
                        ImageNowWidthStr = Math.Round(ImageOriginalWidth * (Math.Round(temp) / 100.0f)).ToString();
                        _isSyncResizeWidthAndHeight = true;
                    }
                }
                else
                {
                    _imageNowHeight = -1;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageNowHeightStr"));
            }
        }


        private int _imageNowWidthPercentage { get; set; } = 0;
        public int ImageNowWidthPercentage
        {
            get => _imageNowWidthPercentage;
            set
            {
                _imageNowWidthPercentage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageNowWidthPercentageStr"));
            }
        }
        public string ImageNowWidthPercentageStr
        {
            get
            {
                if (_imageNowWidthPercentage > 0)
                {
                    return _imageNowWidthPercentage.ToString();
                }

                return string.Empty;
            }
            set
            {
                if (int.TryParse(value, out int num))
                {
                    _imageNowWidthPercentage = num;

                    float temp = num * (ImageOriginalWidth / 100f);
                    if (temp > 0 && temp < 1.0f)
                    {
                        ImageNowWidth = 1;
                    }
                    else
                    {
                        ImageNowWidth = (int)Math.Round(temp);
                    }

                    if (ResizeMaintainAspectRatio && _isSyncResizeWidthAndHeight)
                    {
                        _isSyncResizeWidthAndHeight = false;
                        ImageNowHeightPercentageStr = _imageNowWidthPercentage.ToString();
                        _isSyncResizeWidthAndHeight = true;
                    }
                }
                else
                {
                    _imageNowWidthPercentage = -1;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageNowWidthPercentageStr"));
            }
        }

        private int _imageNowHeightPercentage { get; set; } = 0;
        public int ImageNowHeightPercentage
        {
            get => _imageNowHeightPercentage;
            set
            {
                _imageNowHeightPercentage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageNowHeightPercentageStr"));
            }
        }
        public string ImageNowHeightPercentageStr
        {
            get
            {
                if (_imageNowHeightPercentage > 0)
                {
                    return _imageNowHeightPercentage.ToString();
                }

                return string.Empty;
            }
            set
            {
                if (int.TryParse(value, out int num))
                {
                    _imageNowHeightPercentage = num;

                    float temp = num * (ImageOriginalHeight / 100f);
                    if (temp > 0 && temp < 1.0f)
                    {
                        ImageNowHeight = 1;
                    }
                    else
                    {
                        ImageNowHeight = (int)Math.Round(temp);
                    }

                    if (ResizeMaintainAspectRatio && _isSyncResizeWidthAndHeight)
                    {
                        _isSyncResizeWidthAndHeight = false;
                        ImageNowWidthPercentageStr = _imageNowHeightPercentage.ToString();
                        _isSyncResizeWidthAndHeight = true;
                    }
                }
                else
                {
                    _imageNowHeightPercentage = -1;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageNowHeightPercentageStr"));
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
