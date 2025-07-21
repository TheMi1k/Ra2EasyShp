using Ra2EasyShp.Data;
using System.Windows;

namespace Ra2EasyShp
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            this.DataContext = GData.UIData;
        }
    }
}
