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
                if (GData.UIData.SaveConfig.IsPaletteCustomPath)
                {
                    if (string.IsNullOrEmpty(GData.UIData.SaveConfig.PaletteCustomPath))
                    {
                        throw new Exception(GetTranslateText.Get("Message_PathCanNotEmpty")); // 路径不能为空
                    }
                    if (!Directory.Exists(Path.GetDirectoryName(GData.UIData.SaveConfig.PaletteCustomPath)))
                    {
                        throw new DirectoryNotFoundException(GetTranslateText.Get("Message_PathError")); // 路径错误
                    }

                    using (BinaryWriter bw = new BinaryWriter(File.Open(GData.UIData.SaveConfig.PaletteCustomPath, FileMode.Create)))
                    {
                        foreach (var color in _palette)
                        {
                            bw.Write((byte)color.R);
                            bw.Write((byte)color.G);
                            bw.Write((byte)color.B);
                        }
                    }

                    GData.LastSavePalettePath = GData.UIData.SaveConfig.PaletteCustomPath;
                }
                else
                {
                    if (!string.IsNullOrEmpty(GData.UIData.SaveConfig.PaletteName) && !IsValidString(GData.UIData.SaveConfig.PaletteName))
                    {
                        throw new Exception(GetTranslateText.Get("Message_FileNameError")); // 名称只能为数字和字母
                    }

                    string savePath = GetPath.CreateSavePath(Enums.PathType.Palette);

                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    string saveFileName = string.IsNullOrEmpty(GData.UIData.SaveConfig.PaletteName) ? GetTranslateText.Get("SaveNameEmpty_Palette") : GData.UIData.SaveConfig.PaletteName;

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

                if (GData.UIData.SaveConfig.IsPaletteMapType)
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
                MessageBox.Show($"{GetTranslateText.Get("Message_MessageBoxLoadError")}\n" + ex.Message); // 载入提示框失败
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
                MessageBox.Show($"{GetTranslateText.Get("Message_MessageBoxLoadError")}\n" + ex.Message); // 载入提示框失败
                return false;
            }
        }

        private void Button_Tip_Click(object sender, RoutedEventArgs e)
        {
            // 保存后将文件复制多份，名称后加上地图类型后缀\n例如 unit(sno).pal，unit(tem).pal\n\n如果有同名文件会被覆盖
            ShowMessageBox(GetTranslateText.Get("Message_GeneratePaletteMapTypeFileNameTip"));
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
                Filter = "Palette (*.pal)|*.pal",
                FileName = string.Empty,
                RestoreDirectory = true,
                DefaultExt = "pal"
            };

            GData.UIData.SaveConfig.PaletteCustomPath = (saveFileDialog.ShowDialog() ?? false) ? saveFileDialog.FileName : string.Empty;
        }
    }
}
