using Ra2EasyShp.Data;
using Ra2EasyShp.Funcs;
using Ra2EasyShp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Ra2EasyShp
{
    /// <summary>
    /// CreatePaletteViewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CreatePaletteConfigWindow : Window
    {
        public bool Result { get; private set; } = false;

        private List<Ra2PaletteColor> _palette;
        public CreatePaletteConfigWindow(List<Ra2PaletteColor> colorList)
        {
            InitializeComponent();

            if (GData.SaveConfigModel.IsPaletteCustomPath)
            {
                StackPanel_SaveFileName.Visibility = Visibility.Collapsed;
                StackPanel_CustomSaveFileName.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanel_SaveFileName.Visibility = Visibility.Visible;
                StackPanel_CustomSaveFileName.Visibility = Visibility.Collapsed;
            }

            this.DataContext = GData.SaveConfigModel;

            Init(colorList);

            _palette = colorList;
        }

        private void Init(List<Ra2PaletteColor> colorList)
        {
            StackPanel_Color.Children.Clear();

            int index = 0;
            for (int j = 0; j < 8; j++)
            {
                StackPanel stackPanel = new StackPanel()
                {
                    Width = 20,
                    Height = 320,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Orientation = Orientation.Vertical,
                };

                for (int i = 0; i < 32; i++)
                {
                    stackPanel.Children.Add(new System.Windows.Shapes.Rectangle()
                    {
                        Width = 20,
                        Height = 10,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)Clamp(colorList[index].R * 4), (byte)Clamp(colorList[index].G * 4), (byte)Clamp(colorList[index].B * 4))),
                    });

                    index++;
                }

                StackPanel_Color.Children.Add(stackPanel);
            }
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GData.SaveConfigModel.IsPaletteCustomPath)
                {
                    if (string.IsNullOrEmpty(GData.SaveConfigModel.PaletteCustomPath))
                    {
                        throw new Exception("路径不能为空");
                    }
                    if (!Directory.Exists(Path.GetDirectoryName(GData.SaveConfigModel.PaletteCustomPath)))
                    {
                        throw new DirectoryNotFoundException("路径非法");
                    }

                    using (BinaryWriter bw = new BinaryWriter(File.Open(GData.SaveConfigModel.PaletteCustomPath, FileMode.Create)))
                    {
                        foreach (var color in _palette)
                        {
                            bw.Write((byte)color.R);
                            bw.Write((byte)color.G);
                            bw.Write((byte)color.B);
                        }
                    }

                    GData.LastSavePalettePath = GData.SaveConfigModel.PaletteCustomPath;
                }
                else
                {
                    if (!string.IsNullOrEmpty(GData.SaveConfigModel.PaletteName) && !IsValidString(GData.SaveConfigModel.PaletteName))
                    {
                        throw new Exception("名称只能为数字和字母");
                    }

                    string savePath = GetPath.CreateSavePath(Enums.PathType.Palette);

                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    string saveFileName = string.IsNullOrEmpty(GData.SaveConfigModel.PaletteName) ? "色盘" : GData.SaveConfigModel.PaletteName;

                    using (BinaryWriter bw = new BinaryWriter(File.Open($@"{savePath}\{saveFileName}.pal", FileMode.Create)))
                    {
                        foreach (var color in _palette)
                        {
                            bw.Write((byte)color.R);
                            bw.Write((byte)color.G);
                            bw.Write((byte)color.B);
                        }
                    }

                    GData.LastSavePalettePath = $@"{savePath}\{saveFileName}.pal";
                }

                string path = Path.GetDirectoryName(GData.LastSavePalettePath);

                if (GData.SaveConfigModel.IsPaletteMapType)
                {
                    string[] mapTypeArray = { "urb", "ubn", "tem", "sno", "lun", "des" };
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(GData.LastSavePalettePath);

                    foreach (var mapType in mapTypeArray)
                    {
                        File.Copy(GData.LastSavePalettePath, Path.Combine(path, $"{fileNameWithoutExtension}{mapType}.pal"), true);
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

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        private bool IsValidString(string input)
        {
            return Regex.IsMatch(input, @"^[0-9a-zA-Z]+$");
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

        private void Button_Tip_Click(object sender, RoutedEventArgs e)
        {
            ShowMessageBox("保存后将文件复制多份，名称后加上地图类型后缀\n例如 unit(sno).pal，unit(tem).pal\n\n如果有同名文件会被覆盖");
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
                Title = "选择保存文件",
                Filter = "PAL 色盘文件(*.pal)|*.pal",
                FileName = string.Empty,
                RestoreDirectory = true,
                DefaultExt = "pal"
            };
            GData.SaveConfigModel.PaletteCustomPath = (saveFileDialog.ShowDialog() ?? false) ? saveFileDialog.FileName : string.Empty;
        }
    }
}
