using Ra2EasyShp.Data;
using Ra2EasyShp.Funcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ra2EasyShp
{
    /// <summary>
    /// DrawImageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DrawImageWindow : Window
    {
        public DrawImageWindow()
        {
            InitializeComponent();

            var bitmap = LocalImageManage.LoadBitmapFromFile(@"");
            Image_input.Source = ImageTypeConvert.BitmapToImageSource(bitmap);
        }

        private double _scaleFactor = 1.0;

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // 滚轮向上滚动
                if (e.Delta > 0)
                {
                    _scaleFactor += 1;
                }
                else if (e.Delta < 0)
                {
                    _scaleFactor -= 1;
                }

                _scaleFactor = Math.Max(1.0, _scaleFactor);
                _scaleFactor = Math.Min(9.0, _scaleFactor);

                CanvasScale.ScaleX = _scaleFactor;
                CanvasScale.ScaleY = _scaleFactor;

                e.Handled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void Image_ViewImg_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}
