using Ra2EasyShp.Data;
using Ra2EasyShp.Funcs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

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
                    Width = 28,
                    Height = 384,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Orientation = Orientation.Vertical,
                };

                for (int i = 0; i < 32; i++)
                {
                    stackPanel.Children.Add(new System.Windows.Shapes.Rectangle()
                    {
                        Width = 28,
                        Height = 12,
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
                string savePath = string.Empty;
                string name = TextBox_FileName.Text;

                if (CheckBox_MapTypeForName.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new Exception("名称不能为空");
                    }

                    if (!IsValidString(name))
                    {
                        throw new Exception("名称只能为数字和字母");
                    }

                    string[] mapType = { "urb", "ubn", "tem", "sno", "lun", "des" };

                    savePath = GetPath.CreateSavePath(Enums.PathType.Palette);
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    foreach (var type in mapType)
                    {
                        using (BinaryWriter bw = new BinaryWriter(File.Open($@"{savePath}\{name}{type}.pal", FileMode.Create)))
                        {
                            foreach (var color in _palette)
                            {
                                bw.Write((byte)color.R);
                                bw.Write((byte)color.G);
                                bw.Write((byte)color.B);
                            }
                        }
                    }

                    GData.LastSavePalettePath = $@"{savePath}\{name}{mapType[0]}.pal";
                }
                else
                {
                    savePath = GetPath.CreateSavePath(Enums.PathType.Palette);
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }

                    string saveFileName = string.IsNullOrEmpty(name) ? "色盘" : name;

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
            ShowMessageBox("将文件命名为 名称 * 生成多份\n* 为地图类型，例如 unitsno.pal，unittem.pal\nunit为名称，sno、tem为地图类型");
        }
    }
}
