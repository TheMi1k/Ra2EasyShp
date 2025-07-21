using Ra2EasyShp.Data;
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
    /// SaveSuccessWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SaveSuccessWindow : Window
    {
        public SaveSuccessWindow(string savePath)
        {
            InitializeComponent();

            this.DataContext = GData.UIData;

            TextBox_SavePath.Text = savePath;
        }

        public bool Result { get; private set; } = false;

        private void Button_Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
