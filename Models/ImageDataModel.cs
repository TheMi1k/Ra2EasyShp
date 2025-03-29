using System.ComponentModel;

namespace Ra2EasyShp.Models
{
    public class ImageDataModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _index { get; set; } = -1;

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                IndexStr = value.ToString().PadLeft(5, '0');
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndexStr"));
            }
        }

        public string IndexStr { get; set; } = "00000";

        private EditImageModel _editImage { get; set; } = new EditImageModel();
        public EditImageModel EditImage
        {
            get => _editImage;
            set
            {
                _editImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditImage"));
            }
        }

        private OverlayImageModel _overlayImage { get; set; } = new OverlayImageModel();
        public OverlayImageModel OverlayImage
        {
            get => _overlayImage;
            set
            {
                _overlayImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverlayImage"));
            }
        }
    }
}
