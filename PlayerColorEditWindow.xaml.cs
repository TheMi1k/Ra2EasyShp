using ImageProcessor.Imaging.Colors;
using Ra2EasyShp.Data;
using Ra2EasyShp.Funcs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Ra2EasyShp
{
    /// <summary>
    /// CreatePaletteSetWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PlayerColorEditWindow : Window
    {
        private Bitmap _bitmap;
        private int _imgIndex;
        private int _selectIndex = -1;
        private Ra2PaletteColor[] _playerColors = new Ra2PaletteColor[16];

        public PlayerColorEditWindow(int imgIndex)
        {
            InitializeComponent();

            _imgIndex = imgIndex;

            this.ContentRendered += async (sender, e) =>
            {
                Border_Magnify.Visibility = Visibility.Collapsed;
                StackPanel_NowSelectIndexManage.Visibility = Visibility.Hidden;

                _bitmap = await ImageManage.MergeBitmaps(
                       _imgIndex,
                       _imgIndex,
                       GData.ImageData[_imgIndex].OverlayImage.OffsetX,
                       GData.ImageData[_imgIndex].OverlayImage.OffsetY,
                       GData.ImageData[_imgIndex].OverlayImage.OverlayMode
                       );

                Image_SetPlayerColor.Source = ImageTypeConvert.BitmapToImageSource(_bitmap);

                Image_ImgMagnify.Width = _bitmap.Width * 17;
                Image_ImgMagnify.Height = _bitmap.Height * 17;

                Image_ImgMagnify.Source = ImageTypeConvert.BitmapToImageSource(_bitmap);

                LoadColor(GData.PaletteConfig.PalettePlayerColor);
            };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _bitmap.Dispose();

            GC.Collect();

            base.OnClosing(e);
        }

        private void LoadColor(List<Ra2PaletteColor> colors)
        {
            if (colors.Count < 16)
            {
                return;
            }

            for (int i = 0; i < 16; i++)
            {
                _playerColors[i] = colors[i];

                Button btn = StackPanel_Colors.Children[i] as Button;
                btn.Background = Ra2PaletteColorToBrush(colors[i]);
            }
        }

        private SolidColorBrush Ra2PaletteColorToBrush(Ra2PaletteColor color)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, (byte)(color.R * 4), (byte)(color.G * 4), (byte)(color.B * 4)));
        }

        private void Button_SelectColor_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index = StackPanel_Colors.Children.IndexOf(button);

            if (_selectIndex != -1 && _selectIndex == index)
            {
                Border_Magnify.Visibility = Visibility.Collapsed;
                StackPanel_NowSelectIndexManage.Visibility = Visibility.Hidden;

                _selectIndex = -1;
                return;
            }

            _selectIndex = index;

            StackPanel_NowSelectIndexManage.Margin = new Thickness(5, index * 20, 0, 0);
            StackPanel_NowSelectIndexManage.Visibility = Visibility.Visible;
        }

        private void Image_SetPlayerColor_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_Magnify.Visibility = Visibility.Collapsed;
        }

        private void Image_SetPlayerColor_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_selectIndex < 0)
            {
                return;
            }

            Border_Magnify.Visibility = Visibility.Visible;

            var p_border = e.GetPosition(Border_Image);
            var p_img = e.GetPosition(Image_SetPlayerColor);

            SetMagnifyView(p_border, p_img);
        }

        private void Image_SetPlayerColor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_selectIndex < 0)
            {
                return;
            }

            var p_border = e.GetPosition(Border_Image);
            var p_img = e.GetPosition(Image_SetPlayerColor);

            SetMagnifyView(p_border, p_img);
        }

        private void SetMagnifyView(System.Windows.Point point_border, System.Windows.Point point_img)
        {
            double MagnifyTop;
            double MagnifyLeft;

            int offset = 10;

            if (point_border.X + Border_Magnify.Width + offset > Border_Image.Width)
            {
                MagnifyLeft = point_border.X - Border_Magnify.Width - offset;
            }
            else
            {
                MagnifyLeft = point_border.X + offset;
            }

            if (point_border.Y + Border_Magnify.Height + offset > Border_Image.Height)
            {
                MagnifyTop = point_border.Y - Border_Magnify.Height - offset;
            }
            else
            {
                MagnifyTop = point_border.Y + offset;
            }

            Border_Magnify.Margin = new Thickness(MagnifyLeft, MagnifyTop, 0, 0);
            Image_ImgMagnify.Margin = new Thickness(((int)point_img.X - 4) * -17, ((int)point_img.Y - 4) * -17, 0, 0);
        }

        /// <summary>
        /// 选定颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_SetPlayerColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectIndex < 0)
            {
                return;
            }

            var p_img = e.GetPosition(Image_SetPlayerColor);
            var p = _bitmap.GetPixel((int)p_img.X, (int)p_img.Y);

            if (p.A == 0)
            {
                return;
            }

            Ra2PaletteColor color = Ra2PaletteColor.FromColor(p);

            Button btn = StackPanel_Colors.Children[_selectIndex] as Button;
            btn.Background = Ra2PaletteColorToBrush(color);

            _playerColors[_selectIndex] = color;

            _selectIndex = -1;

            Border_Magnify.Visibility = Visibility.Collapsed;
            StackPanel_NowSelectIndexManage.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_SetPlayerColor_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectIndex < 0)
            {
                return;
            }

            Border_Magnify.Visibility = Visibility.Collapsed;
            StackPanel_NowSelectIndexManage.Visibility = Visibility.Hidden;

            _selectIndex = -1;
        }

        private void Button_CreateColors_Click(object sender, RoutedEventArgs e)
        {
            int firstColorIndex = -1;
            int lastColorIndex = -1;
            for (int i = 0; i < _playerColors.Length; i++)
            {
                if (_playerColors[i].A > 0)
                {
                    firstColorIndex = i;
                    break;
                }
            }

            for (int i = _playerColors.Length - 1; i >= 0; i--)
            {
                if (_playerColors[i].A > 0)
                {
                    lastColorIndex = i;
                    break;
                }
            }

            if (firstColorIndex < 0 || lastColorIndex < 0)
            {
                return;
            }

            if (Math.Abs(firstColorIndex - lastColorIndex) <= 1)
            {
                return;
            }

            Ra2PaletteColor firstColor = _playerColors[firstColorIndex];
            Ra2PaletteColor lastColor = _playerColors[lastColorIndex];

            double stepR = ((firstColor.R * 4) - (lastColor.R * 4)) / ((lastColorIndex - firstColorIndex) * 1.0f);
            double stepG = ((firstColor.G * 4) - (lastColor.G * 4)) / ((lastColorIndex - firstColorIndex) * 1.0f);
            double stepB = ((firstColor.B * 4) - (lastColor.B * 4)) / ((lastColorIndex - firstColorIndex) * 1.0f);

            int stepCount = 1;
            for (int i = firstColorIndex + 1; i < lastColorIndex; i++)
            {
                byte r = (byte)Clamp((firstColor.R * 4) - (int)Math.Round(stepR * stepCount));
                byte g = (byte)Clamp((firstColor.G * 4) - (int)Math.Round(stepG * stepCount));
                byte b = (byte)Clamp((firstColor.B * 4) - (int)Math.Round(stepB * stepCount));

                _playerColors[i] = Ra2PaletteColor.FromArgb(255, r, g, b);
                Button btn = StackPanel_Colors.Children[i] as Button;
                btn.Background = Ra2PaletteColorToBrush(_playerColors[i]);

                stepCount++;
            }
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            _playerColors[_selectIndex] = Ra2PaletteColor.FromArgb(0, 0, 0, 0);
            Button btn = StackPanel_Colors.Children[_selectIndex] as Button;
            btn.Background = Ra2PaletteColorToBrush(_playerColors[_selectIndex]);
        }

        private void Button_ClearAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _playerColors.Length; i++)
            {
                _playerColors[i] = Ra2PaletteColor.FromArgb(0, 0, 0, 0);
                Button btn = StackPanel_Colors.Children[i] as Button;
                btn.Background = Ra2PaletteColorToBrush(_playerColors[i]);
            }
        }

        private bool ShowMessageBox(string text, MessageBoxButton type = MessageBoxButton.OK)
        {
            try
            {
                var window = new MyMessageBox(text, type);
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = this;
                window.ShowDialog();

                return window.Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("载入提示框失败\n" + ex.Message);
                return false;
            }
        }

        private void Button_EditColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Ra2PaletteColor selectedColor = Ra2PaletteColor.FromColor(colorDialog.Color);

                _playerColors[_selectIndex] = selectedColor;
                Button btn = StackPanel_Colors.Children[_selectIndex] as Button;
                btn.Background = Ra2PaletteColorToBrush(selectedColor);
            }
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            GData.PaletteConfig.PalettePlayerColor.Clear();

            foreach (var color in _playerColors)
            {
                GData.PaletteConfig.PalettePlayerColor.Add(color);
            }

            this.Close();
        }

        private void Button_CreateColorsFromHsb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_playerColors[0].A == 0)
                {
                    throw new Exception("16号色没有颜色");
                }

                float bStart = float.Parse(TextBox_ColorsFromHsbBStart.Text);
                float bEnd = float.Parse(TextBox_ColorsFromHsbBEnd.Text);
                int colorNum = int.Parse(TextBox_ColorsFromHsbNum.Text);
                int startIndex = int.Parse(TextBox_ColorsFromHsbStartIndex.Text);

                if (bStart < 0 || bStart > 100 || bEnd < 0 || bEnd > 100)
                {
                    throw new Exception("亮度不正确 0 ~ 100");
                }
                if (bStart > bEnd)
                {
                    throw new Exception("亮度左边不能大于右边");
                }
                if (colorNum < 2 || colorNum > 16)
                {
                    throw new Exception("颜色数量不正确 2 ~ 16");
                }
                if (startIndex < 16 || startIndex > 31)
                {
                    throw new Exception("起始位置不正确 16 ~ 31");
                }

                ColorConvert.RGBtoHSB(_playerColors[0].R * 4, _playerColors[0].G * 4, _playerColors[0].B * 4, out float h, out float s, out _);

                for (int i = 0; i < _playerColors.Length; i++)
                {
                    _playerColors[i] = Ra2PaletteColor.FromArgb(0, 0, 0, 0);
                    Button btn = StackPanel_Colors.Children[i] as Button;
                    btn.Background = Ra2PaletteColorToBrush(_playerColors[i]);
                }
                // 0.15 ~ 0.99 step 0.056

                float step = ((bEnd - bStart) * 0.01f) / ((colorNum - 1) * 1.0f);
                float b = bEnd * 0.01f;
                for (int i = startIndex - 16; i < Math.Min((startIndex - 16) + colorNum, _playerColors.Length); i++)
                {
                    var c = ColorConvert.HSBtoRGB(h, s, b);
                    _playerColors[i] = Ra2PaletteColor.FromColor(c);
                    Button btn = StackPanel_Colors.Children[i] as Button;
                    btn.Background = Ra2PaletteColorToBrush(_playerColors[i]);
                    b -= step;
                }
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
        }
    }
}
