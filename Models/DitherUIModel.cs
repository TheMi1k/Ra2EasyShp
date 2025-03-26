using Ra2EasyShp.Data;
using System.ComponentModel;

namespace Ra2EasyShp.Models
{
    public class DitherUIModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void SetDefault()
        {
            TransparentDiffusion = 0.5;
            Lightness = 0.0;
            Alpha = 0;
            IsTransparent = false;
        }

        internal void SetOutImageData(int index = -1)
        {
            if (index == -1)
            {
                index = GData.UIData.NowIndex;
            }

            GData.ImageData[index].EditImage.TransparentDiffusion = TransparentDiffusion;
            GData.ImageData[index].EditImage.Lightness = Lightness;
            GData.ImageData[index].EditImage.Alpha = Alpha;
            GData.ImageData[index].EditImage.IsTransparent = IsTransparent;
        }

        public string TransparentDiffusionStr { get; set; } = "0.50";
        private double _transparentDiffusionStr { get; set; } = 0.5;
        public double TransparentDiffusion
        {
            get => _transparentDiffusionStr;
            set
            {
                _transparentDiffusionStr = value;
                TransparentDiffusionStr = _transparentDiffusionStr.ToString("f2");

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TransparentDiffusion"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TransparentDiffusionStr"));
            }
        }

        public string LightnessStr { get; set; } = "0.00";
        private double _lightness { get; set; } = 0.0;
        public double Lightness
        {
            get => _lightness;
            set
            {
                _lightness = value;
                LightnessStr = _lightness.ToString("f2");

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Lightness"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LightnessStr"));
            }
        }

        private int _alpha { get; set; } = 0;
        public int Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Alpha"));
            }
        }

        private bool _isTransparent { get; set; } = false;
        public bool IsTransparent
        {
            get => _isTransparent;
            set
            {
                _isTransparent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTransparent"));
            }
        }
    }
}
