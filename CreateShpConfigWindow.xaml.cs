using Ra2EasyShp.Data;
using Ra2EasyShp.Funcs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// CreateShpConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CreateShpConfigWindow : Window
    {
        byte[] _shpData;
        public CreateShpConfigWindow(byte[] shpData)
        {
            InitializeComponent();

            _shpData = shpData;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string savePath = string.Empty;
                string nameFirst = TextBox_FileNameFirst.Text;
                string name = TextBox_FileName.Text;

                if (CheckBox_MapTypeForName.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(nameFirst) || string.IsNullOrEmpty(name))
                    {
                        throw new Exception("启用地图类型命名前缀和名称不能为空");
                    }

                    if (nameFirst.Length != 1 || int.TryParse(nameFirst, out _))
                    {
                        throw new Exception("前缀只能为一个字母");
                    }

                    if (!IsValidString(nameFirst) || !IsValidString(name))
                    {
                        throw new Exception("名称只能为数字和字母");
                    }

                    char[] mapType = { 'a', 'u', 't', 'd', 'l', 'g' };

                    savePath = GetPath.CreateSavePath(Enums.PathType.SHP);
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    foreach (var c in mapType)
                    {
                        File.WriteAllBytes($@"{savePath}\{nameFirst}{c}{name}.shp", _shpData);
                    }
                }
                else
                {
                    savePath = GetPath.CreateSavePath(Enums.PathType.SHP);
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    string saveFileName = string.IsNullOrEmpty(name) ? "输出" : name;
                    File.WriteAllBytes($@"{savePath}\{saveFileName}.shp", _shpData);
                }

                if (OpenSaveSuccessWindow(savePath))
                {
                    Process.Start("explorer.exe", savePath);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
        }

        private void Button_Tip_Click(object sender, RoutedEventArgs e)
        {
            ShowMessageBox("将文件命名为 前缀字母 * 名称生成多份\n* 为地图类型，例如 capill.shp，cgpill.shp等\nc为前缀，a、g等第二个字母为地图类型，pill为名称");
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

        private bool OpenSaveSuccessWindow(string savePath)
        {
            try
            {
                var window = new SaveSuccessWindow(savePath);
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

        private bool IsValidString(string input)
        {
            return Regex.IsMatch(input, @"^[0-9a-zA-Z]+$");
        }

        private void CheckBox_MapTypeForName_Changed(object sender, RoutedEventArgs e)
        {
            if (CheckBox_MapTypeForName.IsChecked == true)
            {
                StackPanel_MapTypeForName.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanel_MapTypeForName.Visibility = Visibility.Collapsed;
            }
        }
    }
}
