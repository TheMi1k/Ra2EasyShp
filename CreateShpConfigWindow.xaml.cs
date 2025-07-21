using Ra2EasyShp.Data;
using Ra2EasyShp.Funcs;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

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

            this.DataContext = GData.UIData;

            if (GData.UIData.SaveConfig.IsPaletteCustomPath)
            {
                StackPanel_SaveFileName.Visibility = Visibility.Collapsed;
                StackPanel_CustomSaveFileName.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanel_SaveFileName.Visibility = Visibility.Visible;
                StackPanel_CustomSaveFileName.Visibility = Visibility.Collapsed;
            }

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

                if (GData.UIData.SaveConfig.IsShpCustomPath)
                {
                    if (string.IsNullOrEmpty(GData.UIData.SaveConfig.ShpCustomPath))
                    {
                        throw new Exception(GetTranslateText.Get("Message_PathCanNotEmpty")); // 路径不能为空
                    }
                    if (!Directory.Exists(Path.GetDirectoryName(GData.UIData.SaveConfig.ShpCustomPath)))
                    {
                        throw new DirectoryNotFoundException(GetTranslateText.Get("Message_PathError")); // 路径错误
                    }

                    File.WriteAllBytes(GData.UIData.SaveConfig.ShpCustomPath, _shpData);

                    _saveFile = GData.UIData.SaveConfig.ShpCustomPath;
                }
                else
                {
                    string savePath = string.Empty;

                    if (!string.IsNullOrEmpty(GData.UIData.SaveConfig.ShpName) && !IsValidString(GData.UIData.SaveConfig.ShpName))
                    {
                        throw new Exception(GetTranslateText.Get("Message_FileNameError")); // 名称只能为数字和字母
                    }

                    string saveFileName = string.IsNullOrEmpty(GData.UIData.SaveConfig.ShpName) ? GetTranslateText.Get("SaveNameEmpty_Shp") : GData.UIData.SaveConfig.ShpName;

                    savePath = GetPath.CreateSavePath(Enums.PathType.SHP);
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    File.WriteAllBytes(Path.Combine(savePath, $"{saveFileName}.shp"), _shpData);

                    _saveFile = Path.Combine(savePath, $"{saveFileName}.shp");
                }

                string path = Path.GetDirectoryName(_saveFile);

                if (GData.UIData.SaveConfig.IsShpMapType)
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
            // 保存后将文件复制多份，名称第二个字母修改为地图类型字母\n例如 c(a)pill.shp，c(g)pill.shp等\n\n如果有同名文件会被覆盖
            ShowMessageBox(GetTranslateText.Get("Message_GenerateShpMapTypeFileNameTip"));
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
                MessageBox.Show($"{GetTranslateText.Get("Message_MessageBoxLoadError")}\n" + ex.Message); // 载入提示框失败
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
                MessageBox.Show($"{GetTranslateText.Get("Message_MessageBoxLoadError")}\n" + ex.Message); // 载入提示框失败
                return false;
            }
        }

        private bool IsValidString(string input)
        {
            return Regex.IsMatch(input, @"^[0-9a-zA-Z]+$");
        }

        private void CheckBox_CustomSaveMode(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox checkBox) || !checkBox.IsMouseOver)
                return;

            StackPanel_SaveFileName.Visibility = Visibility.Collapsed;
            StackPanel_CustomSaveFileName.Visibility = Visibility.Visible;
        }

        private void CheckBox_AutoSaveMode(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox checkBox) || !checkBox.IsMouseOver)
                return;

            StackPanel_SaveFileName.Visibility = Visibility.Visible;
            StackPanel_CustomSaveFileName.Visibility = Visibility.Collapsed;
        }

        private void CheckBox_SetCustomPath(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !button.IsMouseOver)
                return;

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = GetTranslateText.Get("Title_SelectFile"),
                Filter = "SHP (*.shp)|*.shp",
                FileName = string.Empty,
                RestoreDirectory = true,
                DefaultExt = "shp"
            };

            GData.UIData.SaveConfig.ShpCustomPath = (saveFileDialog.ShowDialog() ?? false) ? saveFileDialog.FileName : string.Empty;
        }
    }
}
