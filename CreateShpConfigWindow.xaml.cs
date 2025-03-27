using Ra2EasyShp.Data;
using Ra2EasyShp.Funcs;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;

namespace Ra2EasyShp
{
    /// <summary>
    /// CreateShpConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CreateShpConfigWindow : Window
    {
        byte[] _shpData;
        private string _customSavePath = string.Empty;

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
                string _saveFile = string.Empty;

                if (string.IsNullOrEmpty(_customSavePath))
                {
                    string savePath = string.Empty;
                    string name = TextBox_FileName.Text;

                    if (!string.IsNullOrEmpty(name) && !IsValidString(name))
                    {
                        throw new Exception("名称只能为数字和字母");
                    }

                    string saveFileName = string.IsNullOrEmpty(name) ? "输出" : name;

                    savePath = GetPath.CreateSavePath(Enums.PathType.SHP);
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    File.WriteAllBytes(Path.Combine(savePath, $"{saveFileName}.shp"), _shpData);

                    _saveFile = Path.Combine(savePath, $"{saveFileName}.shp");
                }
                else
                {
                    File.WriteAllBytes(_customSavePath, _shpData);

                    _saveFile = _customSavePath;
                }

                string path = Path.GetDirectoryName(_saveFile);

                if (CheckBox_MapTypeForName.IsChecked == true)
                {
                    char[] mapTypeArray = { 'a', 'u', 't', 'd', 'l', 'g' };
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_saveFile);

                    if (fileNameWithoutExtension.ToCharArray().Length > 1)
                    {
                        foreach (var mapType in mapTypeArray)
                        {
                            char[] renameChar = fileNameWithoutExtension.ToCharArray();
                            renameChar[1] = mapType;
                            string rename = new string(renameChar);
                            if (rename != fileNameWithoutExtension)
                            {
                                File.Copy(_saveFile, Path.Combine(path, $"{rename}.shp"), true);
                            }
                        }
                    }
                }

                if (OpenSaveSuccessWindow(path))
                {
                    Process.Start("explorer.exe", path);
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
            ShowMessageBox("保存后将文件复制多份，名称第二个字母修改为地图类型字母\n例如 c(a)pill.shp，c(g)pill.shp等\n\n如果有同名文件会被覆盖");
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
            //if (CheckBox_MapTypeForName.IsChecked == true)
            //{
            //    StackPanel_MapTypeForName.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    StackPanel_MapTypeForName.Visibility = Visibility.Collapsed;
            //}
        }

        private void CheckBox_SetSavePath(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "选择保存文件",
                Filter = "shp文件(*.shp)|*.shp",
                FileName = string.Empty,
                RestoreDirectory = true,
                DefaultExt = "shp"
            };

            if (saveFileDialog.ShowDialog() == false)
            {
                _customSavePath = string.Empty;
                CheckBox_CustomSavePath.IsChecked = false;
                return;
            }

            StackPanel_SaveFileName.Visibility = Visibility.Collapsed;
            StackPanel_CustomSaveFileName.Visibility = Visibility.Visible;

            _customSavePath = saveFileDialog.FileName;
            TextBox_SavePath.Text = _customSavePath;
        }

        private void CheckBox_ClearSavePath(object sender, RoutedEventArgs e)
        {
            StackPanel_SaveFileName.Visibility = Visibility.Visible;
            StackPanel_CustomSaveFileName.Visibility = Visibility.Collapsed;
            _customSavePath = string.Empty;
        }
    }
}
