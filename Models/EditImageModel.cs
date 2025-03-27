using Ra2EasyShp.Data;
using Ra2EasyShp.Funcs;
using System;
using System.ComponentModel;
using static Ra2EasyShp.Data.Enums;

namespace Ra2EasyShp.Models
{
    [Serializable]
    public class EditImageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void ClearItem()
        {
            LocalImageManage.DeleteImageTemp(ImgTempPath);
            LocalImageManage.DeleteImageTemp(OutImgTempPath);
            LocalImageManage.DeleteImageTemp(ImgReMarginPath);
            LocalImageManage.DeleteImageTemp(ImgResizePath);

            ImgPath = string.Empty;
            ImgTempPath = string.Empty;
            OutImgTempPath = string.Empty;
            ImgReMarginPath = string.Empty;
            ImgResizePath = string.Empty;

            Name = string.Empty;

            Alpha = 0;
            Lightness = 0;
            TransparentDiffusion = 0.5;
            IsTransparent = false;
            IsChanged = false;

            Frame = 0;
            FileType = FileType.Png;
        }

        internal EditImageModel Copy()
        {
            return new EditImageModel()
            {
                ImgPath = ImgPath,
                ImgTempPath = ImgTempPath,
                OutImgTempPath = OutImgTempPath,
                ImgReMarginPath = ImgReMarginPath,
                ImgResizePath = ImgResizePath,

                Name = Name,

                Alpha = Alpha,
                Lightness = Lightness,
                TransparentDiffusion = TransparentDiffusion,
                IsTransparent = IsTransparent,
                IsChanged = IsChanged,

                Frame = Frame,
                FileType = FileType,
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

        public string ImgResizePath { get; set; } = string.Empty;

        public string ImgReMarginPath { get; set; } = string.Empty;


        public Enums.FileType FileType { get; set; } = Enums.FileType.Png;

        public int Frame { get; set; } = 0;

        //public Bitmap OutImg { get; set; } = null;

        public string OutImgTempPath = string.Empty;

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

        public int Alpha { get; set; } = 0;

        public double Lightness { get; set; } = 0;

        public double TransparentDiffusion { get; set; } = 0.5;

        /// <summary>
        /// 是否应用对图像轮廓透明过渡
        /// </summary>
        public bool IsTransparent { get; set; } = false;
    }
}
