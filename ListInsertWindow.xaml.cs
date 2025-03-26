using System;
using System.Windows;

namespace Ra2EasyShp
{
    /// <summary>
    /// ListInsertWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ListInsertWindow : Window
    {
        public ListInsertWindow(int index, string listName)
        {
            InitializeComponent();

            TextBlock_OptionTip.Text = $"往 [ {listName}图片列表 ]\n[ {index.ToString().PadLeft(5, '0')} ] 位置插入空行\n[ {index.ToString().PadLeft(5, '0')} ] 及下方内容往下移动";
        }

        internal int InsertLineCount { get; private set; } = 0;

        private void Button_Option1_Click(object sender, RoutedEventArgs e)
        {
            InsertLineCount = 0;
            Close();
        }

        private void Button_Option2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int lineCount = int.Parse(TextBox_InsertLine.Text);

                if (lineCount < 1)
                {
                    throw new Exception("最小插入1行");
                }

                InsertLineCount = lineCount;
                Close();
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
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
    }
}
