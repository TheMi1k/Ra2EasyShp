using System;
using System.Windows;

namespace Ra2EasyShp
{
    /// <summary>
    /// MyMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class MyMessageBox : Window
    {
        public MyMessageBox(string text, MessageBoxButton type)
        {
            InitializeComponent();

            if (type == MessageBoxButton.OK)
            {
                Button_No.Visibility = Visibility.Collapsed;
            }

            string str = "";

            float lineMax = 40;

            var temp = text.Split('\n');
            foreach (var line in temp)
            {
                if (line.Length <= lineMax)
                {
                    str += line + "\n";
                }
                else
                {
                    var tempStr = line;
                    for (int i = 1; i <= Math.Floor(line.Length / lineMax); i++)
                    {
                        tempStr = tempStr.Insert((i * (int)lineMax) + i - 1, "\n");
                    }
                    str += tempStr + "\n";
                }
            }

            TextBlock_Text.Text = str;

            Loaded += UiLoaded;
        }

        private void UiLoaded(object sender, RoutedEventArgs e)
        {
            var w = TextBlock_Text.ActualWidth + 80;
            var h = TextBlock_Text.ActualHeight + 150;

            if (w < 300)
            {
                w = 300;
            }
            if (h < 220)
            {
                h = 220;
            }

            Width = w;
            Height = h;
            MainGrid.Width = w - 2;
            MainGrid.Height = h - 60;
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
