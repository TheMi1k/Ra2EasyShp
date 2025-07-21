using Newtonsoft.Json;
using Ra2EasyShp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace Ra2EasyShp.Models
{
    public class UIDataModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void SetAllDefault()
        {
            NowIndex = 0;
            DitherUI.SetDefault();
            OverlayUI.SetDefault();
        }

        private ComboBoxData _comboBoxData = null;

        public ComboBoxData ComboBoxData
        {
            get => _comboBoxData;
            set
            {
                _comboBoxData = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ComboBoxData"));
            }
        }

        public Dictionary<string, string> LanguageDic { get; set; } = new Dictionary<string, string>();

        public void LoadLanguage(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception("语言文件不存在\nLanguage file is not exists");
            }

            string json = File.ReadAllText(filePath);
            LanguageDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (_comboBoxData == null)
            {
                ComboBoxData = new ComboBoxData();
            }

            ComboBoxData.Update();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public SaveConfigModel SaveConfig { get; set; } = new SaveConfigModel();

        private bool _imgIsPlaying { get; set; } = false;
        internal bool ImgIsPlaying
        {
            get => _imgIsPlaying;
            set
            {
                _imgIsPlaying = value;
                UIEnable = !value;
            }
        }

        private bool _uiEnable { get; set; } = true;
        public bool UIEnable
        {
            get => _uiEnable;
            set
            {
                _uiEnable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UIEnable"));
            }
        }


        private static readonly object _lockerProgressUi = new object();

        internal void SetProgressUI(int suc, int max)
        {
            lock(_lockerProgressUi)
            {
                ProgressUi = $"{suc} / {max}";
            }
        }

        private string _progressUi { get; set; } = "0 / 0";
        public string ProgressUi
        {
            get => _progressUi;
            private set
            {
                _progressUi = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressUi"));
            }
        }

        private int _nowIndex { get; set; } = 0;
        public int NowIndex
        {
            get => _nowIndex;

            set
            {
                _nowIndex = value;
                IndexStr = _nowIndex.ToString().PadLeft(5, '0');

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NowIndex"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndexStr"));
            }
        }

        public string IndexStr { get; set; } = "00000";

        private double _imgViewScaleTransform { get; set; } = 1.0;
        public double ImageViewScaleTransform
        {
            get => _imgViewScaleTransform;
            set
            {
                _imgViewScaleTransform = value;
                ImageViewScaleTransformStr = $"{(int)(_imgViewScaleTransform * 100)}%";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageViewScaleTransform"));
            }
        }

        private string _imageViewScaleTransformStr { get; set; } = "100%";
        public string ImageViewScaleTransformStr
        {
            get => _imageViewScaleTransformStr;
            set
            {
                _imageViewScaleTransformStr = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageViewScaleTransformStr"));
            }
        }

        private Brush _viewBackground { get; set; }
        public Brush ViewBackground
        {
            get => _viewBackground;
            set
            {
                _viewBackground = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ViewBackground"));
            }
        }

        private int _viewImagePointX { get; set; }
        public int ViewImagePointX
        {
            get => _viewImagePointX;
            set
            {
                _viewImagePointX = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ViewImagePointX"));
            }
        }

        private int _viewImagePointY { get; set; }
        public int ViewImagePointY
        {
            get => _viewImagePointY;
            set
            {
                _viewImagePointY = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ViewImagePointY"));
            }
        }

        private int _viewImagePointPaletteIndex { get; set; }
        public int ViewImagePointPaletteIndex
        {
            get => _viewImagePointPaletteIndex;
            set
            {
                _viewImagePointPaletteIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ViewImagePointPaletteIndex"));
            }
        }
        
        public ResizeUIModel ResizeUI { get; set; } = new ResizeUIModel();

        public DitherUIModel DitherUI { get; set; } = new DitherUIModel();

        public OverlayUIModel OverlayUI { get; set; } = new OverlayUIModel();
    }
}
