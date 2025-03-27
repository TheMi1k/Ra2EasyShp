using Ra2EasyShp.Data;
using Ra2EasyShp.Funcs;
using System.ComponentModel;

namespace Ra2EasyShp.Models
{
    public class OverlayImageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetIsChanged()
        {
            if (OffsetX != 0)
            {
                IsChanged = true;
                return;
            }
            if (OffsetY != 0)
            {
                IsChanged = true;
                return;
            }
            if (OverlayMode != Enums.OverlayMode.叠加在上)
            {
                IsChanged = true;
                return;
            }

            IsChanged = false;
        }

        internal void ClearItem()
        {
            LocalImageManage.DeleteImageTemp(ImgTempPath);

            ImgPath = string.Empty;
            ImgTempPath = string.Empty;

            Name = string.Empty;
            OffsetX = 0;
            OffsetY = 0;
            Frame = 0;
            FileType = Enums.FileType.Png;
            OverlayMode = Enums.OverlayMode.叠加在上;
            IsChanged = false;
        }

        internal OverlayImageModel Copy()
        {
            return new OverlayImageModel()
            {
                ImgPath = ImgPath,
                ImgTempPath = ImgTempPath,
                Name = Name,
                OffsetX = OffsetX,
                OffsetY = OffsetY,
                Frame = Frame,
                FileType = Enums.FileType.Png,
                OverlayMode = OverlayMode,
                IsChanged = IsChanged
            };
        }

        private string _name { get; set; } = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public string ImgPath { get; set; } = string.Empty;

        public string ImgTempPath { get; set; } = string.Empty;

        //public Bitmap Img { get; set; } = null;

        public Enums.FileType FileType { get; set; } = Enums.FileType.Png;

        public int Frame { get; set; } = 0;

        private int _offsetX { get; set; } = 0;
        public int OffsetX
        {
            get => _offsetX;
            set
            {
                _offsetX = value;
                SetIsChanged();
            }
        }

        private int _offsetY { get; set; } = 0;
        public int OffsetY
        {
            get => _offsetY;
            set
            {
                _offsetY = value;
                SetIsChanged();
            }
        }

        private bool _isChanged { get; set; } = false;

        public bool IsChanged
        {
            get => _isChanged;
            set
            {
                _isChanged = value;

                StrColor = _isChanged ? "#FF0000" : "#000000";

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StrColor"));
            }
        }

        public string StrColor { get; set; } = "#000000";

        private Enums.OverlayMode _overlayMode { get; set; } = Enums.OverlayMode.叠加在上;
        public Enums.OverlayMode OverlayMode
        {
            get => _overlayMode;
            set
            {
                _overlayMode = value;
                SetIsChanged();
            }
        }
    }
}
