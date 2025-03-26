using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ra2EasyShp.Data;
using Ra2EasyShp.Funcs;
using Ra2EasyShp.Models;
using Color = System.Drawing.Color;
using System.Windows.Media.Imaging;
using System.Data;
using System.Reflection;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace Ra2EasyShp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Init();

            InitPlayerColor();

            CreateRa2Grids();
        }

        private void Init()
        {
            string _getVersion()
            {
                string v = GData.VERSION.ToString();
                if (!v.Contains('.'))
                {
                    v += ".0";
                }

                return v;
            }

            this.Title += $" v{_getVersion()}";
            //this.Title += $" v1.0 beta.2";

            this.DataContext = GData.UIData;

            ComboBox_OverlayMode.SelectionChanged += (sender, e) =>
            {
                if (GData.UIData.OverlayUI.OverlayMode == Enums.OverlayMode.叠加在上)
                {
                    Panel.SetZIndex(Grid_OutImg, 0);
                    Panel.SetZIndex(Grid_OutImgOverlay, 1);
                }
                else
                {
                    Panel.SetZIndex(Grid_OutImg, 1);
                    Panel.SetZIndex(Grid_OutImgOverlay, 0);
                }

                if (GData.ImageData.Count == 0)
                {
                    return;
                }

                GData.ImageData[GData.UIData.NowIndex].OverlayImage.OverlayMode = GData.UIData.OverlayUI.OverlayMode;
            };

            (StackPanel_PaletteHeaderColor.Children[0] as Button).Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 252));
            GData.PaletteConfig.PaletteHeaderColor[0] = Ra2PaletteColor.FromArgb(255, 0, 0, 252);

            ComboBox_CreatePalMode.ItemsSource = Enum.GetValues(typeof(Enums.CreatePalMode));
            ComboBox_CreatePalMode.SelectedIndex = 0;

            ComboBox_OverlayMode.ItemsSource = Enum.GetValues(typeof(Enums.OverlayMode));
            ComboBox_OverlayMode.SelectedIndex = 0;

            ComboBox_ShpCompression.ItemsSource = Enum.GetValues(typeof(Enums.ShpCompressionMode));
            ComboBox_ShpCompression.SelectedIndex = 0;

            GData.TempPath = GetPath.CreateTempPath();
            if (!Directory.Exists(GData.TempPath))
            {
                Directory.CreateDirectory(GData.TempPath);
            }

            string[] _getResPalette()
            {
                try
                {
                    List<string> palList = new List<string>();
                    foreach (var file in Directory.EnumerateFiles(Path.Combine(GetPath.RunPath, @"Res\Palette"), "*.pal"))
                    {
                        palList.Add(Path.GetFileName(file));
                    }

                    return palList.ToArray();
                }
                catch
                {
                    return new string[] { };
                }
            }

            ComboBox_Palette.ItemsSource = _getResPalette();
            ComboBox_Palette.SelectedIndex = 0;

            ComboBox_PlayerColorView.ItemsSource = Enum.GetValues(typeof(Enums.ViewPlayerColor));
            ComboBox_PlayerColorView.SelectedIndex = 0;

            DataGrid_Images.ItemsSource = GData.ImageData;

#if DEBUG
            Button_Test.Visibility = Visibility.Visible;
#endif

        }

        private void InitPlayerColor()
        {
            foreach (var kv in GData.PlayerColorBaseDic)
            {
                float H = kv.Value[0] / 255.0f * 360.0f;
                float S = kv.Value[1] / 255.0f;
                float B = kv.Value[2] / 255.0f;

                var step = (B - 0.13f) / 15.0f;
                for (int i = 0; i < 16; i++)
                {
                    var c = ColorConvert.HSBtoRGB(H, S, B - (step * i));
                    GData.PlayerColorDic[kv.Key].Add(new byte[] { c.R, c.G, c.B });
                }
            }
        }

        private void CreateRa2Grids()
        {
            Grid_Ra2Grids_Grid.Children.Clear();

            int l = 1000;
            int r1 = 1000;
            int r2 = 0;
            while (true)
            {
                if (l >= 2000)
                {
                    break;
                }

                var lineL = new System.Windows.Shapes.Line()
                {
                    X1 = l,
                    Y1 = l - 1000,
                    X2 = l - 1000,
                    Y2 = l,
                    StrokeThickness = 2,
                    Stroke = System.Windows.Media.Brushes.Black,
                };
                RenderOptions.SetEdgeMode(lineL, EdgeMode.Aliased);

                // 左侧画线
                Grid_Ra2Grids_Grid.Children.Add(lineL);


                var lineR = new System.Windows.Shapes.Line()
                {
                    X1 = r1,
                    Y1 = r2,
                    X2 = r1 + 1000,
                    Y2 = r2 + 1000,
                    StrokeThickness = 2,
                    Stroke = System.Windows.Media.Brushes.Black,
                };
                RenderOptions.SetEdgeMode(lineR, EdgeMode.Aliased);

                // 右侧画线
                Grid_Ra2Grids_Grid.Children.Add(lineR);

                l += 30;
                r1 -= 30;
                r2 += 30;
            }
        }

        private int _mainWindowNormalW;
        private int _mainWindowNormalH;
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                _mainWindowNormalW = GData.UIData.MainWindowSize.MainWindowW;
                _mainWindowNormalH = GData.UIData.MainWindowSize.MainWindowH;
                GData.UIData.MainWindowSize.MainWindowW = (int)ActualWidth;
                GData.UIData.MainWindowSize.MainWindowH = (int)ActualHeight;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                GData.UIData.MainWindowSize.MainWindowW = _mainWindowNormalW;
                GData.UIData.MainWindowSize.MainWindowH = _mainWindowNormalH;
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

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!ShowMessageBox("请确定当前项目是否已经保存\n是否退出？", MessageBoxButton.YesNo))
            {
                e.Cancel = true;
            }

            try
            {
                if (Directory.Exists(GData.TempPath))
                {
                    Directory.Delete(GData.TempPath, true);
                }
            }
            catch
            {
                ShowMessageBox($"清理缓存失败，可稍后手动清理\n{GData.TempPath}");
            }
        }

        private void SelectViewBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color selectedColor = colorDialog.Color;

                //Grid_ImgView.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B));
                GData.UIData.ViewBackground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B));
            }
        }

        private void SetBackgroundImage(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            using (var img = System.Drawing.Image.FromFile(files[0]))
            {
                // 切换为图片背景
                ImageBrush imageBrush = new ImageBrush
                {
                    ImageSource = ImageTypeConvert.BitmapToImageSource(new Bitmap(img)),
                    Stretch = Stretch.None
                };

                //Grid_ImgView.Background = imageBrush;
                GData.UIData.ViewBackground = imageBrush;
            }
        }

        private async void Img_Drop(object sender, DragEventArgs e)
        {
            StackPanel_Tips.Visibility = Visibility.Collapsed;

            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            if (!Directory.Exists(GData.TempPath))
            {
                Directory.CreateDirectory(GData.TempPath);
            }

            var point = e.GetPosition(dataGrid);
            var row = dataGrid.InputHitTest(point) as DependencyObject;

            while (row != null && !(row is DataGridRow))
            {
                row = VisualTreeHelper.GetParent(row);
            }

            int insertIndex = GData.ImageData.Count;
            if (row is DataGridRow dataGridRow)
            {
                insertIndex = dataGrid.ItemContainerGenerator.IndexFromContainer(dataGridRow);
            }

            GData.UIData.NowIndex = 0;
            LoadImageOption(GData.UIData.NowIndex);

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            System.Windows.Point position = e.GetPosition((UIElement)sender);

            Grid_Main.IsEnabled = false;

            if (position.X <= 140)
            {
                try
                {
                    await ImageListManage.OpenFile(files, insertIndex, false);

                    await ReloadImageSource(0);
                }
                catch (Exception ex)
                {
                    ShowMessageBox($"加载图片错误\n{ex.Message}");
                }
            }
            else
            {
                try
                {
                    await ImageListManage.OpenFile(files, insertIndex, true);

                    await ReloadImageSource(0);
                }
                catch (Exception ex)
                {
                    ShowMessageBox($"加载图片错误\n{ex.Message}");
                }
            }

            Grid_Main.IsEnabled = true;
        }

        private async void Img_DropToEnd(object sender, DragEventArgs e)
        {
            System.Windows.Point position = e.GetPosition((UIElement)sender);

            StackPanel_Tips.Visibility = Visibility.Collapsed;

            GData.UIData.NowIndex = 0;
            LoadImageOption(GData.UIData.NowIndex);

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            Grid_Main.IsEnabled = false;

            try
            {
                if (position.X <= 140)
                {
                    await ImageListManage.OpenFile(files, GData.ImageData.Count, false);

                    await ReloadImageSource(0);
                }
                else
                {
                    await ImageListManage.OpenFile(files, GData.ImageData.Count, true);

                    await ReloadImageSource(0);
                }
            }
            catch (Exception ex)
            {
                ShowMessageBox($"加载图片错误\n{ex.Message}");
            }

            Grid_Main.IsEnabled = true;
        }

        /// <summary>
        /// 处理图像并重载
        /// </summary>
        private async void ConvertImageAndReload()
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            await Task.Run(() =>
            {
                GData.UIData.DitherUI.SetOutImageData(GData.UIData.NowIndex);
                ImageManage.ConvertImage(GData.UIData.NowIndex);
            });

            await ReloadImageSource(GData.UIData.NowIndex);

            RadioButton_ViewMode_OutImg.IsChecked = true;
        }

        private async void Button_ResetNowIndex_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            try
            {
                GData.ImageData[GData.UIData.NowIndex].EditImage.IsChanged = false;

                using (Bitmap bitmap = LocalImageManage.LoadEditBaseImageBitmap(GData.UIData.NowIndex))
                {
                    LocalImageManage.SaveBitmapToTemp(bitmap, GData.ImageData[GData.UIData.NowIndex].EditImage.OutImgTempPath);
                }

                await ReloadImageSource(GData.UIData.NowIndex);

                ShowMessageBox("重置当前图像完毕");
            }
            catch (Exception ex)
            {
                ShowMessageBox($"重置当前图像失败\n{ex.Message}");
            }
        }

        private async void Button_ResetAll_Click(object sender, RoutedEventArgs e)
        {
            GData.UIData.SetProgressUI(0, GData.ImageData.Count);

            if (GData.ImageData.Count == 0)
            {
                return;
            }

            try
            {
                object locker = new object();

                int suc = 0;
                Parallel.For(0, GData.ImageData.Count, index =>
                {
                    GData.ImageData[index].EditImage.IsChanged = false;

                    using (Bitmap bitmap = LocalImageManage.LoadEditBaseImageBitmap(index))
                    {
                        LocalImageManage.SaveBitmapToTemp(bitmap, GData.ImageData[index].EditImage.OutImgTempPath);
                    }

                    lock (locker)
                    {
                        suc++;
                    }

                    GData.UIData.SetProgressUI(suc, GData.ImageData.Count);
                });

                await ReloadImageSource(GData.UIData.NowIndex);

                ShowMessageBox("重置所有图像完毕");
            }
            catch (Exception ex)
            {
                ShowMessageBox($"重置所有图像失败\n{ex.Message}");
            }
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ConvertImageAndReload();
        }

        private void LoadImageOption(int index)
        {
            try
            {
                GData.UIData.NowIndex = GData.ImageData[index].Index;
                GData.UIData.DitherUI.TransparentDiffusion = GData.ImageData[index].EditImage.TransparentDiffusion;
                GData.UIData.DitherUI.Lightness = GData.ImageData[index].EditImage.Lightness;
                GData.UIData.DitherUI.Alpha = GData.ImageData[index].EditImage.Alpha;
                GData.UIData.DitherUI.IsTransparent = GData.ImageData[index].EditImage.IsTransparent;
                GData.UIData.OverlayUI.ChangeOverlayMargin(GData.ImageData[index].OverlayImage.OffsetX, GData.ImageData[index].OverlayImage.OffsetY);
                GData.UIData.OverlayUI.OverlayMode = GData.ImageData[index].OverlayImage.OverlayMode;
            }
            catch
            {

            }
        }

        private async void DataGrid_Images_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 不是鼠标左键
            if (e.ButtonState != MouseButtonState.Pressed || e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (GData.ImageData.Count == 0)
            {
                return;
            }

            var triggeredControl = sender as FrameworkElement;

            if (triggeredControl == null)
            {
                return;
            }

            var dataGrid = triggeredControl as System.Windows.Controls.DataGrid;
            DataGridCellInfo cellInfo = dataGrid.SelectedCells[0];
            var rowData = cellInfo.Item;

            if (rowData == null)
            {
                return;
            }

            ImageDataModel item = rowData as ImageDataModel;

            try
            {
                GData.UIData.NowIndex = item.Index;

                {
                    if (LocalImageManage.GetBaseImageOriginalSize(GData.UIData.NowIndex, out int width, out int height))
                    {
                        GData.UIData.ResizeUI.ImageOriginalWidth = width;
                        GData.UIData.ResizeUI.ImageOriginalHeight = height;
                    }
                }

                {
                    if (LocalImageManage.GetBaseImageNowSize(GData.UIData.NowIndex, out int width, out int height))
                    {
                        GData.UIData.ResizeUI.ImageNowWidth = width;
                        GData.UIData.ResizeUI.ImageNowHeight = height;
                    }
                }

                await ReloadImageSource(GData.UIData.NowIndex);

                LoadImageOption(item.Index);
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
        }

        private async void Button_ExportAllImage_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            Grid_Main.IsEnabled = false;

            string savePath = GetPath.CreateSavePath(Enums.PathType.PNG);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            int sucCount = 0;
            int failCount = 0;

            GData.UIData.SetProgressUI(sucCount, GData.ImageData.Count);

            await Task.Run(async () =>
            {
                int imgOutIndex = 0;
                for (int index = 0; index < GData.ImageData.Count; index++)
                {
                    try
                    {
                        string imgName = $"{imgOutIndex.ToString().PadLeft(5, '0')}.png";

                        using (Bitmap result = await ImageManage.MergeBitmaps(
                            index, 
                            index,
                            GData.ImageData[index].OverlayImage.OffsetX,
                            GData.ImageData[index].OverlayImage.OffsetY, 
                            GData.ImageData[index].OverlayImage.OverlayMode))
                        {
                            result?.Save($@"{savePath}\{imgName}", System.Drawing.Imaging.ImageFormat.Png);
                        }

                        sucCount += 1;

                        GData.UIData.SetProgressUI(sucCount, GData.ImageData.Count);

                        imgOutIndex += 1;
                    }
                    catch
                    {
                        failCount += 1;
                        imgOutIndex += 1;
                    }
                }
            });

            Grid_Main.IsEnabled = true;

            if (OpenSaveSuccessWindow(savePath))
            {
                Process.Start("explorer.exe", savePath);
            }
        }

        private async void Button_ExportAllPaletteImage_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            if ((Enums.ViewPlayerColor)ComboBox_PlayerColorView.SelectedItem != Enums.ViewPlayerColor.无)
            {
                ShowMessageBox("请先将预览所属色改为 [无]");
                return;
            }

            Grid_Main.IsEnabled = false;

            string savePath = GetPath.CreateSavePath(Enums.PathType.PNG);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            int sucCount = 0;
            int failCount = 0;

            GData.UIData.SetProgressUI(sucCount, GData.ImageData.Count);

            await Task.Run(async () =>
            {
                int imgOutIndex = 0;
                for (int index = 0; index < GData.ImageData.Count; index++)
                {
                    try
                    {
                        string imgName = $"{imgOutIndex.ToString().PadLeft(5, '0')}.png";

                        using (Bitmap result = await ImageManage.MergeBitmaps(
                            index,
                            index,
                            GData.ImageData[index].OverlayImage.OffsetX,
                            GData.ImageData[index].OverlayImage.OffsetY,
                            GData.ImageData[index].OverlayImage.OverlayMode))
                        {
                            using (Bitmap bitmapOnPal = ShpManage.BitmapOnPalette(result, GData.NowPalette))
                            {
                                bitmapOnPal?.Save($@"{savePath}\{imgName}", System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }

                        sucCount += 1;

                        GData.UIData.SetProgressUI(sucCount, GData.ImageData.Count);

                        imgOutIndex += 1;
                    }
                    catch
                    {
                        failCount += 1;
                        imgOutIndex += 1;
                    }
                }
            });

            Grid_Main.IsEnabled = true;

            if (OpenSaveSuccessWindow(savePath))
            {
                Process.Start("explorer.exe", savePath);
            }
        }

        private async void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                GData.UIData.ImgIsPlaying = false;
                Button_Play.Content = "播放";
                return;
            }

            if (Button_Play.Content.ToString() == "播放")
            {
                GData.UIData.ImgIsPlaying = true;
                Button_Play.Content = "停止";
            }
            else
            {
                GData.UIData.ImgIsPlaying = false;
                Button_Play.Content = "播放";
                return;
            }

            await Task.Run(async () =>
            {
                int gcCount = 0;
                while (true)
                {
                    try
                    {
                        if (GData.UIData.NowIndex + 1 >= GData.ImageData.Count)
                        {
                            GData.UIData.NowIndex = 0;
                        }
                        else
                        {
                            GData.UIData.NowIndex++;
                        }

                        await Dispatcher.InvokeAsync(async () =>
                        {
                            await ReloadImageSource(GData.UIData.NowIndex);
                            LoadImageOption(GData.UIData.NowIndex);
                        });

                        await Task.Delay(50);

                        gcCount++;
                        if (gcCount > 5)
                        {
                            GC.Collect();
                            gcCount = 0;
                        }
                    }
                    catch
                    {

                    }

                    if (!GData.UIData.ImgIsPlaying)
                    {
                        GC.Collect();
                        break;
                    }
                }
            });
        }

        private async void Button_Test_Click(object sender, RoutedEventArgs e)
        {
            GData.UIData.NowIndex = 0;
            //GData.ImageData.Clear();

            StackPanel_Tips.Visibility = Visibility.Collapsed;

            //string folderPath = @"D:\RA2Scripts\VEH8-马牛";
            //string folderPath = @"C:\Users\Milk\Desktop\数字图";
            //string folderPath = @"C:\Users\Milk\Desktop\气垫船32";

            //string[] files = Directory.GetFiles(folderPath);
            string[] files = { @"C:\Users\Milk\Desktop\QQ20250323-005711.png" };


            //string[] files = { @"D:\Ra2EasyShp\bin\x64\Debug\输出SHP\2025年3月18日17时8分5秒\输出.shp" };

            //string[] files = { @"D:\Ra2EasyShp\bin\x64\Debug\输出SHP\2025年3月22日20时54分9秒\输出.shp" };
            string[] fileso = { };

            try
            {
                await ImageListManage.OpenFile(files, GData.ImageData.Count, false);
                //ImageListManage.OpenFile(files, 0, false);
                await ImageListManage.OpenFile(fileso, GData.ImageData.Count, true);


                Image_input.Source = LocalImageManage.LoadBaseImageSource(0);
                Image_output.Source = LocalImageManage.LoadOutImageSource(0);
            }
            catch (Exception ex)
            {
                ShowMessageBox($"加载图片出错\n{ex.Message}");
            }
        }

        /// <summary>
        /// 应用到所有叠加图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ApplyToAllOverlayImg_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            try
            {
                for (int i = 0; i < GData.ImageData.Count; i++)
                {
                    GData.UIData.OverlayUI.SetOverlayData(i);
                }

                GData.UIData.OverlayUI.ChangeOverlayMargin(GData.UIData.OverlayUI.OverlayOffsetX, GData.UIData.OverlayUI.OverlayOffsetY);

                ShowMessageBox("更改完成");
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
        }

        /// <summary>
        /// 应用到所有操作图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_ApplyToAllImg_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            Grid_Main.IsEnabled = false;

            RadioButton_ViewMode_OutImg.IsChecked = true;

            object locker = new object();
            int maxCount = GData.ImageData.Count;
            int sucCount = 0;
            GData.UIData.SetProgressUI(0, maxCount);

            await Task.Run(() =>
            {
                Parallel.For(0, GData.ImageData.Count, i =>
                {
                    GData.UIData.DitherUI.SetOutImageData(i);
                    ImageManage.ConvertImage(i);

                    lock (locker)
                    {
                        sucCount++;
                    }

                    GData.UIData.SetProgressUI(sucCount, maxCount);
                });
            });

            GC.Collect();

            await ReloadImageSource(GData.UIData.NowIndex);

            Grid_Main.IsEnabled = true;

            ShowMessageBox("应用到所有图像完成");
        }

        private async void Button_CreatePal_Click(object sender, RoutedEventArgs e)
        {
            int palColorNum;
            try
            {
                palColorNum = int.Parse(TextBox_PalColorNum.Text);
                if (palColorNum < 2 || palColorNum > 256)
                {
                    throw new Exception("色盘颜色数量只能为2-256");
                }

                if (GData.ImageData.Count == 0)
                {
                    return;
                }

                Grid_Main.IsEnabled = false;

                bool isPlayerColor = (bool)CheckBox_PlayerColor.IsChecked;

                List<Ra2PaletteColor> palette = await PaletteManage.CreatePalette(palColorNum, GData.PaletteConfig.PaletteHeaderColor, isPlayerColor ? GData.PaletteConfig.PalettePlayerColor : null, ComboBox_CreatePalMode.SelectedItem.ToString());

                CreatePaletteConfigWindow window = new CreatePaletteConfigWindow(palette);
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = this;
                window.ShowDialog();

                if (!window.Result)
                {
                    Grid_Main.IsEnabled = true;
                    return;
                }

                string savePath = GetPath.CreateSavePath(Enums.PathType.Palette);
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                using (BinaryWriter writer = new BinaryWriter(File.Open($@"{savePath}\色盘.pal", FileMode.Create)))
                {
                    foreach (var color in palette)
                    {
                        writer.Write((byte)color.R);
                        writer.Write((byte)color.G);
                        writer.Write((byte)color.B);
                    }
                }

                GData.LastSavePalettePath = $@"{savePath}\色盘.pal";

                if (OpenSaveSuccessWindow(savePath))
                {
                    Process.Start("explorer.exe", savePath);
                }
            }
            catch (Exception ex)
            {
                ShowMessageBox($"生成色盘失败\n{ex.Message}");
            }

            Grid_Main.IsEnabled = true;
        }

        /// <summary>
        /// 预览窗口透明度更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_ArtTranslucencyChanged(object sender, RoutedEventArgs e)
        {
            if (Image_output == null || Image_outputOverlay == null || Image_outputOverlay == null)
            {
                return;
            }

            if (RadioButton_ArtTranslucency_none.IsChecked == true)
            {
                Image_output.Opacity = 1.0;
                Image_outputOverlay.Opacity = 1.0;
                Image_PaletteImg.Opacity = 1.0;
            }
            else if (RadioButton_ArtTranslucency_25.IsChecked == true)
            {
                Image_output.Opacity = 0.75;
                Image_outputOverlay.Opacity = 0.75;
                Image_PaletteImg.Opacity = 0.75;
            }
            else if (RadioButton_ArtTranslucency_50.IsChecked == true)
            {
                Image_output.Opacity = 0.5;
                Image_outputOverlay.Opacity = 0.5;
                Image_PaletteImg.Opacity = 0.5;
            }
            else if (RadioButton_ArtTranslucency_75.IsChecked == true)
            {
                Image_output.Opacity = 0.25;
                Image_outputOverlay.Opacity = 0.25;
                Image_PaletteImg.Opacity = 0.25;
            }
        }

        private async void RadioButton_ViewModeChanged(object sender, RoutedEventArgs e)
        {
            if (Grid_InImg == null || Grid_OutImg == null || Grid_PaletteImg == null)
            {
                return;
            }

            if (GData.ImageData.Count == 0)
            {
                return;
            }

            try
            {
                if (RadioButton_ViewMode_InImg.IsChecked == true)
                {
                    Grid_InImg.Visibility = Visibility.Visible;
                    Grid_OutImg.Visibility = Visibility.Collapsed;
                    Grid_OutImgOverlay.Visibility = Visibility.Visible;
                    Grid_PaletteImg.Visibility = Visibility.Collapsed;
                }
                else if (RadioButton_ViewMode_OutImg.IsChecked == true)
                {
                    Grid_InImg.Visibility = Visibility.Collapsed;
                    Grid_OutImg.Visibility = Visibility.Visible;
                    Grid_OutImgOverlay.Visibility = Visibility.Visible;
                    Grid_PaletteImg.Visibility = Visibility.Collapsed;
                }
                else if (RadioButton_ViewMode_OutImgOnPalette.IsChecked == true)
                {
                    Grid_InImg.Visibility = Visibility.Collapsed;
                    Grid_OutImg.Visibility = Visibility.Collapsed;
                    Grid_OutImgOverlay.Visibility = Visibility.Collapsed;
                    Grid_PaletteImg.Visibility = Visibility.Visible;
                }

                await ReloadImageSource(GData.UIData.NowIndex);

                if (CheckBox_Ra2Grids.IsChecked == true)
                {
                    LoadRa2Grids();
                }
            }
            catch
            {

            }
        }

        private void Button_ClearList1_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count != 0)
            {
                try
                {
                    Parallel.ForEach(GData.ImageData, item =>
                    {
                        item.EditImage.ClearItem();
                    });

                    ImageListManage.ClearLastEmptyItem();

                    GData.UIData.NowIndex = 0;
                    Image_input.Source = null;
                    Image_output.Source = null;
                    Image_PaletteImg.Source = null;

                    GC.Collect();
                }
                catch (Exception ex)
                {
                    ShowMessageBox(ex.Message);
                    return;
                }
            }
        }

        private void Button_ClearList2_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count != 0)
            {
                try
                {
                    Parallel.ForEach(GData.ImageData, item =>
                    {
                        item.OverlayImage.ClearItem();
                    });

                    ImageListManage.ClearLastEmptyItem();

                    GData.UIData.NowIndex = 0;

                    Image_outputOverlay.Source = null;

                    GC.Collect();
                }
                catch (Exception ex)
                {
                    ShowMessageBox(ex.Message);
                    return;
                }
            }
        }

        private void ListView_Images_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ImageListManage.DeleteItemShift(DataGrid_Images.SelectedCells.ToList());

                GData.UIData.NowIndex = 0;
                Image_input.Source = null;
                Image_output.Source = null;
                Image_PaletteImg.Source = null;
                Image_outputOverlay.Source = null;

                GData.UIData.SetAllDefault();

                DataGrid_Images.ItemsSource = null;
                DataGrid_Images.ItemsSource = GData.ImageData;
                return;
            }

            if (e.Key == Key.Delete)
            {
                ImageListManage.DeleteItem(DataGrid_Images.SelectedCells.ToList());

                GData.UIData.NowIndex = 0;
                Image_input.Source = null;
                Image_output.Source = null;
                Image_PaletteImg.Source = null;
                Image_outputOverlay.Source = null;

                GData.UIData.SetAllDefault();

                DataGrid_Images.ItemsSource = null;
                DataGrid_Images.ItemsSource = GData.ImageData;

                return;
            }
        }

        private void CheckBox_OutlineTransparent_Changed(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            bool now = GData.ImageData[GData.UIData.NowIndex].EditImage.IsTransparent;

            if (CheckBox_OutlineTransparent.IsChecked == true)
            {
                //Slider_OutlineTransparentOffset.IsEnabled = true;
                GData.ImageData[GData.UIData.NowIndex].EditImage.IsTransparent = true;
                GData.UIData.DitherUI.IsTransparent = true;
            }
            else
            {
                //Slider_OutlineTransparentOffset.IsEnabled = false;
                GData.ImageData[GData.UIData.NowIndex].EditImage.IsTransparent = false;
                GData.UIData.DitherUI.IsTransparent = false;
            }

            if (now != GData.ImageData[GData.UIData.NowIndex].EditImage.IsTransparent)
            {
                ConvertImageAndReload();
            }
        }

        private void TextBox_Overlay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            e.Handled = !Regex.IsMatch(newText, @"^-?\d*$");
        }

        private int _insertIndex = -1;
        private Enums.ListName _insertListName = Enums.ListName.空;
        private void DataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _insertIndex = -1;
            _insertListName = Enums.ListName.空;

            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            // 获取鼠标点击的位置
            var point = e.GetPosition(dataGrid);
            var hit = dataGrid.InputHitTest(point) as DependencyObject;

            while (hit != null && !(hit is DataGridRow))
            {
                hit = VisualTreeHelper.GetParent(hit);
            }

            if (hit is DataGridRow dataGridRow)
            {
                _insertIndex = dataGrid.ItemContainerGenerator.IndexFromContainer(dataGridRow);
                if (point.X < 41)
                {
                    MenuItem1.Header = $"往两个列表 {_insertIndex.ToString().PadLeft(5, '0')} 位置插入行";
                    _insertListName = Enums.ListName.全部;
                }
                else if (point.X < 141)
                {
                    MenuItem1.Header = $"往操作列表 {_insertIndex.ToString().PadLeft(5, '0')} 位置插入行";
                    _insertListName = Enums.ListName.操作;
                }
                else
                {
                    MenuItem1.Header = $"往叠加列表 {_insertIndex.ToString().PadLeft(5, '0')} 位置插入行";
                    _insertListName = Enums.ListName.叠加;
                }
            }
            else
            {
                DataGridContextMenu.IsOpen = false;
                return;
            }

            if (hit is DataGridRow || hit is System.Windows.Controls.DataGridCell)
            {
                // 在鼠标右键点击位置显示上下文菜单
                DataGridContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;
                DataGridContextMenu.PlacementRectangle = new Rect(point.X, point.Y + 10, 0, 0);
                DataGridContextMenu.PlacementTarget = DataGrid_Images;
                DataGridContextMenu.IsOpen = true;
            }
            else
            {
                DataGridContextMenu.IsOpen = false;
            }
        }

        private void InsertMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_insertIndex == -1 || _insertListName == Enums.ListName.空)
            {
                return;
            }

            var window = new ListInsertWindow(_insertIndex, _insertListName.ToString());
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.ShowDialog();

            if (window.InsertLineCount > 0)
            {
                ImageListManage.InsertEmptyItem(_insertListName, _insertIndex, window.InsertLineCount);

                DataGrid_Images.ItemsSource = null;
                DataGrid_Images.ItemsSource = GData.ImageData;
            }

            _insertIndex = -1;
            _insertListName = Enums.ListName.空;
        }

        private void DataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            var window = new AboutWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.ShowDialog();
        }

        private void Button_EditPlayerColor_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(GData.ImageData[GData.UIData.NowIndex].EditImage.OutImgTempPath)
                && string.IsNullOrEmpty(GData.ImageData[GData.UIData.NowIndex].OverlayImage.ImgPath)
                && string.IsNullOrEmpty(GData.ImageData[GData.UIData.NowIndex].OverlayImage.ImgTempPath))
            {
                return;
            }

            var window = new PlayerColorEditWindow(GData.UIData.NowIndex);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.ShowDialog();
        }

        private void CheckBox_PlayerColor_Changed(object sender, RoutedEventArgs e)
        {
            if (CheckBox_PlayerColor.IsChecked == true)
            {
                Button_EditPlayerColor.IsEnabled = true;
            }
            else
            {
                Button_EditPlayerColor.IsEnabled = false;
            }

            if (GData.PaletteConfig.PalettePlayerColor.Count < 16)
            {
                GData.PaletteConfig.PalettePlayerColor.Count();

                for (int _ = 0; _ < 16; _++)
                {
                    GData.PaletteConfig.PalettePlayerColor.Add(Ra2PaletteColor.FromArgb(0, 0, 0, 0));
                }
            }
        }

        private void Button_SetPalHeaderColor_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index = StackPanel_PaletteHeaderColor.Children.IndexOf(button);

            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Ra2PaletteColor selectedColor = Ra2PaletteColor.FromColor(colorDialog.Color);

                GData.PaletteConfig.PaletteHeaderColor[index] = selectedColor;
                (StackPanel_PaletteHeaderColor.Children[index] as Button).Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)(selectedColor.R * 4), (byte)(selectedColor.G * 4), (byte)(selectedColor.B * 4)));
            }
        }

        private void ClearPalHeaderColor(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            int index = StackPanel_PaletteHeaderColor.Children.IndexOf(button);
            GData.PaletteConfig.PaletteHeaderColor[index] = Ra2PaletteColor.FromArgb(0, 0, 0, 0);
            (StackPanel_PaletteHeaderColor.Children[index] as Button).Background = System.Windows.Media.Brushes.Transparent;
        }

        /// <summary>
        /// 生成shp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_CreateShp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GData.ImageData.Count == 0)
                {
                    return;
                }

                if (GData.NowPalette == null || GData.NowPalette.Count == 0)
                {
                    throw new Exception("没有选择色盘");
                }

                int shadowStart = -1;
                int shadowEnd = -1;

                if (CheckBox_ShpShadow.IsChecked == true)
                {
                    shadowStart = int.Parse(TextBox_ShpShadowFrameStart.Text);
                    shadowEnd = int.Parse(TextBox_ShpShadowFrameEnd.Text);
                }

                Enums.ShpCompressionMode shpCompressionMode = (Enums.ShpCompressionMode)ComboBox_ShpCompression.SelectedItem;

                Grid_Main.IsEnabled = false;

                byte[] shpData = null;
                await Task.Run(() =>
                {
                    List<string> bitmapTempList = new List<string>();
                    for (int _ = 0; _ < GData.ImageData.Count; _++)
                    {
                        bitmapTempList.Add(GetPath.CreateImageTempPath());
                    }

                    Parallel.For(0, GData.ImageData.Count, index =>
                    {
                        using (Bitmap bitmap = ImageManage.MergeBitmaps(
                               index,
                               index,
                               GData.ImageData[index].OverlayImage.OffsetX,
                               GData.ImageData[index].OverlayImage.OffsetY,
                               GData.ImageData[index].OverlayImage.OverlayMode).Result)
                        {
                            LocalImageManage.SaveBitmapToTemp(bitmap, bitmapTempList[index]);
                        }
                    });

                    shpData = ShpManage.BitmapToShp(bitmapTempList, GData.NowPalette, shpCompressionMode, shadowStart, shadowEnd);

                    foreach (var filePath in bitmapTempList)
                    {
                        LocalImageManage.DeleteImageTemp(filePath);
                    }

                    GC.Collect();
                });

                var window = new CreateShpConfigWindow(shpData);
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = this;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowMessageBox($"生成SHP失败\n{ex.Message}");
            }

            Grid_Main.IsEnabled = true;
        }

        private async void Button_LoadLastSavePalette_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GData.LastSavePalettePath))
            {
                return;
            }

            Grid_Main.IsEnabled = false;
            await LoadPalette(GData.LastSavePalettePath);

            if (RadioButton_ViewMode_OutImgOnPalette.IsChecked == true)
            {
                Grid_PaletteImg.Visibility = Visibility;

                if (GData.NowPalette == null || GData.NowPalette.Count == 0)
                {
                    return;
                }

                await Task.Run(async () =>
                {
                    using (Bitmap bitmap = await ImageManage.MergeBitmaps(
                        GData.UIData.NowIndex,
                        GData.UIData.NowIndex,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OffsetX,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OffsetY,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OverlayMode
                    ))
                    {
                        using (Bitmap bitmapOnPal = ShpManage.BitmapOnPalette(bitmap, GData.NowPalette))
                        {
                            await Dispatcher.InvokeAsync(() =>
                            {
                                Image_PaletteImg.Source = ImageTypeConvert.BitmapToImageSource(bitmapOnPal);
                            });
                        }
                    }
                });
            }

            Grid_Main.IsEnabled = true;
        }

        private async void Button_LoadPalette_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择打开文件",
                Filter = "Pal色盘文件(*.pal)|*.pal",
                FileName = string.Empty,
                RestoreDirectory = true,
                DefaultExt = "pal"
            };

            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }

            string fileName = openFileDialog.FileName;

            if (string.IsNullOrEmpty(fileName))
            {
                throw new Exception("打开的文件不正确");
            }

            Grid_Main.IsEnabled = false;
            await LoadPalette(fileName);

            if (RadioButton_ViewMode_OutImgOnPalette.IsChecked == true)
            {
                Grid_PaletteImg.Visibility = Visibility;

                if (GData.NowPalette == null || GData.NowPalette.Count == 0)
                {
                    return;
                }

                await Task.Run(async () =>
                {
                    using (Bitmap bitmap = await ImageManage.MergeBitmaps(
                        GData.UIData.NowIndex,
                        GData.UIData.NowIndex,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OffsetX,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OffsetY,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OverlayMode
                    ))
                    {
                        using (Bitmap bitmapOnPal = ShpManage.BitmapOnPalette(bitmap, GData.NowPalette))
                        {
                            await Dispatcher.InvokeAsync(() =>
                            {
                                Image_PaletteImg.Source = ImageTypeConvert.BitmapToImageSource(bitmapOnPal);
                            });
                        }
                    }
                });
            }

            Grid_Main.IsEnabled = true;
        }

        private async void ComboBox_Palette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string palPath = Path.Combine(GetPath.RunPath, $@"Res\Palette\{ComboBox_Palette.SelectedItem}");

            Grid_Main.IsEnabled = false;
            await LoadPalette(palPath);

            if (RadioButton_ViewMode_OutImgOnPalette.IsChecked == true)
            {
                Grid_PaletteImg.Visibility = Visibility;

                if (GData.NowPalette == null || GData.NowPalette.Count == 0)
                {
                    return;
                }

                await Task.Run(async () =>
                {
                    using (Bitmap bitmap = await ImageManage.MergeBitmaps(
                        GData.UIData.NowIndex,
                        GData.UIData.NowIndex,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OffsetX,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OffsetY,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OverlayMode
                    ))
                    {
                        using (Bitmap bitmapOnPal = ShpManage.BitmapOnPalette(bitmap, GData.NowPalette))
                        {
                            await Dispatcher.InvokeAsync(() =>
                            {
                                Image_PaletteImg.Source = ImageTypeConvert.BitmapToImageSource(bitmapOnPal);
                            });
                        }
                    }
                });
            }

            Grid_Main.IsEnabled = true;
        }

        private DataTable _dataTablePaletteView;
        private async Task LoadPalette(string palPath)
        {
            try
            {
                if (!File.Exists(palPath))
                {
                    throw new Exception("色盘不存在");
                }

                FileInfo fileInfo = new FileInfo(palPath);
                if (fileInfo.Length != 768)
                {
                    throw new Exception("色盘文件不正确");
                }

                GData.NowPalette = new List<Ra2PaletteColor>();
                using (FileStream fs = new FileStream(palPath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        while (true)
                        {
                            byte[] rgb = br.ReadBytes(3);
                            if (rgb.Length == 0)
                            {
                                break;
                            }

                            GData.NowPalette.Add(Ra2PaletteColor.FromPaletteColor(rgb[0], rgb[1], rgb[2]));
                        }
                    }
                }

                int rows = 32;
                int cols = 8;

                if (_dataTablePaletteView == null)
                {
                    _dataTablePaletteView = new DataTable();

                    for (int j = 0; j < cols; j++)
                    {
                        _dataTablePaletteView.Columns.Add($"Col{j}", typeof(string));
                        _dataTablePaletteView.Columns.Add($"Col{j}_Color", typeof(SolidColorBrush));
                        _dataTablePaletteView.Columns.Add($"Col{j}_TextColor", typeof(SolidColorBrush));
                        _dataTablePaletteView.Columns.Add($"Col{j}_DisableOpacity", typeof(double));
                        _dataTablePaletteView.Columns.Add($"Col{j}_TransparentOpacity", typeof(double));
                        
                    }

                    for (int i = 0; i < rows; i++)
                    {
                        _dataTablePaletteView.Rows.Add(_dataTablePaletteView.NewRow());
                    }
                }

                int index = 0;
                for (int j = 0; j < cols; j++) 
                {
                    for (int i = 0; i < rows; i++)
                    {
                        var c = System.Windows.Media.Color.FromRgb((byte)(GData.NowPalette[index].R * 4), (byte)(GData.NowPalette[index].G * 4), (byte)(GData.NowPalette[index].B * 4));
                        ColorConvert.RGBtoHSB(c.R, c.G, c.B, out _, out _, out float brightness);
                        var textColorTemp = ColorConvert.HSBtoRGB(0, 0, brightness > 0.5f ? 0 : 1.0f);
                        var textColor = System.Windows.Media.Color.FromRgb(textColorTemp.R, textColorTemp.G, textColorTemp.B);

                        _dataTablePaletteView.Rows[i][$"Col{j}"] = $"{index}";
                        _dataTablePaletteView.Rows[i][$"Col{j}_Color"] = new SolidColorBrush(c);
                        _dataTablePaletteView.Rows[i][$"Col{j}_TextColor"] = new SolidColorBrush(textColor);
                        _dataTablePaletteView.Rows[i][$"Col{j}_DisableOpacity"] = 0;
                        _dataTablePaletteView.Rows[i][$"Col{j}_TransparentOpacity"] = 0;

                        index++;
                    }
                }

                DataGrid_Palette.ItemsSource = _dataTablePaletteView.DefaultView;

                await Task.Run(() =>
                {
                    PaletteManage.InitColorToPaletteTable(GData.NowPalette);
                });
            }
            catch (Exception ex)
            {
                ShowMessageBox($"载入色盘失败\n{ex.Message}");
            }
        }

        private void LoadRa2Grids()
        {
            int width = 0;
            int height = 0;
            BitmapSource bitmapSource = null;

            if (RadioButton_ViewMode_InImg.IsChecked == true)
            {
                bitmapSource = Image_input.Source as BitmapSource;
            }
            else if (RadioButton_ViewMode_OutImg.IsChecked == true)
            {
                bitmapSource = Image_output.Source as BitmapSource;
            }
            else if (RadioButton_ViewMode_OutImgOnPalette.IsChecked == true)
            {
                bitmapSource = Image_PaletteImg.Source as BitmapSource;
            }

            if (bitmapSource != null)
            {
                width = bitmapSource.PixelWidth;
                height = bitmapSource.PixelHeight;
            }

            Grid_Ra2Grids.Width = width;
            Grid_Ra2Grids.Height = height;
            Grid_Ra2Grids_Grid.Margin = new Thickness((width * 0.5f) - 1001, (height * 0.5f) - 1, 0, 0);
        }

        private void CheckBox_Ra2Grids_Changed(object sender, RoutedEventArgs e)
        {
            if (CheckBox_Ra2Grids == null)
            {
                return;
            }

            if (CheckBox_Ra2Grids.IsChecked == true)
            {
                LoadRa2Grids();
                Grid_Ra2Grids.Visibility = Visibility.Visible;
            }
            else
            {
                Grid_Ra2Grids.Width = 0;
                Grid_Ra2Grids.Height = 0;
                Grid_Ra2Grids.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_SetRa2GridsColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color selectedColor = colorDialog.Color;

                var backgroundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B));
                foreach (var line in Grid_Ra2Grids_Grid.Children)
                {
                    (line as System.Windows.Shapes.Line).Stroke = backgroundColor;
                }

                Button_SetRa2GridsColor.Background = backgroundColor;
            }
        }

        private async void Button_Resize_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            Grid_Main.IsEnabled = false;

            try
            {
                if (GData.UIData.ResizeUI.ImageOriginalWidth == 0 || GData.UIData.ResizeUI.ImageOriginalHeight == 0)
                {
                    throw new Exception("没有图片信息，请双击列表选择一张图片后再调整");
                }

                await ImageManage.Resize(GData.UIData.ResizeUI.ImageNowWidth, GData.UIData.ResizeUI.ImageNowHeight);

                await ReloadImageSource(GData.UIData.NowIndex);

                if (CheckBox_Ra2Grids.IsChecked == true)
                {
                    LoadRa2Grids();
                }

                ShowMessageBox("缩放完成");
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }

            Grid_Main.IsEnabled = true;
        }

        private async void Button_CancelResize_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            Grid_Main.IsEnabled = false;

            try
            {
                await ImageManage.CancelResize();

                await ReloadImageSource(GData.UIData.NowIndex);

                if (CheckBox_Ra2Grids.IsChecked == true)
                {
                    LoadRa2Grids();
                }

                ShowMessageBox("已取消所有缩放");
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }

            Grid_Main.IsEnabled = true;
        }

        private void TextBox_Resize_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CheckBox_MaintainAspectRatio.IsChecked == false)
            {
                return;
            }

            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            GData.UIData.ResizeUI.SetMaintainAspectRatio();
        }

        private void CheckBox_MaintainAspectRatio_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox == null)
            {
                return;
            }

            if (checkBox.IsChecked == true)
            {
                StackPanel_ResizeHeight.IsEnabled = false;
                GData.UIData.ResizeUI.SetMaintainAspectRatio();
            }
            else
            {
                StackPanel_ResizeHeight.IsEnabled = true;
            }
        }

        private async void Button_ReMargin_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            Grid_Main.IsEnabled = false;

            try
            {
                if (GData.UIData.ResizeUI.ImageOriginalWidth == 0 || GData.UIData.ResizeUI.ImageOriginalHeight == 0)
                {
                    throw new Exception("没有图片信息，请双击列表选择一张图片后再调整");
                }

                await ImageManage.ReMargin(
                    int.Parse(GData.UIData.ResizeUI.ReMarginLeft),
                    int.Parse(GData.UIData.ResizeUI.ReMarginTop),
                    int.Parse(GData.UIData.ResizeUI.ReMarginRight),
                    int.Parse(GData.UIData.ResizeUI.ReMarginBottom),
                    GData.UIData.ResizeUI.ReMarginCutImage
                    );

                await ReloadImageSource(GData.UIData.NowIndex);

                if (CheckBox_Ra2Grids.IsChecked == true)
                {
                    LoadRa2Grids();
                }

                ShowMessageBox("调整画布完成");
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }

            Grid_Main.IsEnabled = true;
        }

        private async void Button_CancelReMargin_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            Grid_Main.IsEnabled = false;

            try
            {
                await ImageManage.CancelReMargin();

                await ReloadImageSource(GData.UIData.NowIndex);

                if (CheckBox_Ra2Grids.IsChecked == true)
                {
                    LoadRa2Grids();
                }

                ShowMessageBox("已取消画布调整");
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }

            Grid_Main.IsEnabled = true;
        }

        private void Button_ShpShadowFrameAutoSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GData.ImageData.Count % 2 != 0)
                {
                    throw new Exception("列表图片数量不为偶数，无法自动设置");
                }

                TextBox_ShpShadowFrameStart.Text = (GData.ImageData.Count / 2).ToString();
                TextBox_ShpShadowFrameEnd.Text = (GData.ImageData.Count - 1).ToString();
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
        }


        private async void DataGrid_Palette_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            Grid_Main.IsEnabled = false;
            await LoadPalette(files[0]);

            if (RadioButton_ViewMode_OutImgOnPalette.IsChecked == true)
            {
                Grid_PaletteImg.Visibility = Visibility;

                if (GData.NowPalette == null || GData.NowPalette.Count == 0)
                {
                    return;
                }

                await Task.Run(async () =>
                {
                    using (Bitmap bitmap = await ImageManage.MergeBitmaps(
                        GData.UIData.NowIndex,
                        GData.UIData.NowIndex,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OffsetX,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OffsetY,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OverlayMode
                    ))
                    {
                        using (Bitmap bitmapOnPal = ShpManage.BitmapOnPalette(bitmap, GData.NowPalette))
                        {
                            await Dispatcher.InvokeAsync(() =>
                            {
                                Image_PaletteImg.Source = ImageTypeConvert.BitmapToImageSource(bitmapOnPal);
                            });
                        }
                    }
                });
            }

            Grid_Main.IsEnabled = true;
        }

        private async void ComboBox_PlayerColorView_Changed(object sender, SelectionChangedEventArgs e)
        {
            GData.PlayerColorView = (Enums.ViewPlayerColor)ComboBox_PlayerColorView.SelectedIndex;

            if (!GData.UIData.ImgIsPlaying && RadioButton_ViewMode_OutImgOnPalette.IsChecked == true)
            {
                Grid_PaletteImg.Visibility = Visibility;

                if (GData.NowPalette == null || GData.NowPalette.Count == 0)
                {
                    return;
                }

                await Task.Run(async () =>
                {
                    using (Bitmap bitmap = await ImageManage.MergeBitmaps(
                        GData.UIData.NowIndex,
                        GData.UIData.NowIndex,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OffsetX,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OffsetY,
                        GData.ImageData[GData.UIData.NowIndex].OverlayImage.OverlayMode
                    ))
                    {
                        using (Bitmap bitmapOnPal = ShpManage.BitmapOnPalette(bitmap, GData.NowPalette))
                        {
                            await Dispatcher.InvokeAsync(() =>
                            {
                                Image_PaletteImg.Source = ImageTypeConvert.BitmapToImageSource(bitmapOnPal);
                            });
                        }
                    }
                });
            }
        }

        private double _scaleFactor = 1.0;

        private void Grid_ImgView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // 滚轮向上滚动
                if (e.Delta > 0) 
                {
                    _scaleFactor += 1;
                }
                else if (e.Delta < 0)
                {
                    _scaleFactor -= 1;
                }

                _scaleFactor = Math.Max(1.0, _scaleFactor);
                _scaleFactor = Math.Min(9.0, _scaleFactor);

                GData.UIData.ImageViewScaleTransform = _scaleFactor;

                e.Handled = true;
            }
        }

        private async Task ReloadImageSource(int index)
        {
            if (RadioButton_ViewMode_InImg.IsChecked == true)
            {
                Image_input.Source = LocalImageManage.LoadBaseImageSource(index);
                Image_outputOverlay.Source = LocalImageManage.LoadOverlayImageSource(index);
                Image_output.Source = null;
                Image_PaletteImg.Source = null;
            }
            else if (RadioButton_ViewMode_OutImg.IsChecked == true)
            {
                Image_output.Source = LocalImageManage.LoadOutImageSource(index);
                Image_outputOverlay.Source = LocalImageManage.LoadOverlayImageSource(index);
                Image_input.Source = null;
                Image_PaletteImg.Source = null;
            }
            else if (RadioButton_ViewMode_OutImgOnPalette.IsChecked == true)
            {
                Image_input.Source = null;
                Image_outputOverlay.Source = null;
                Image_output.Source = null;

                if (GData.NowPalette == null || GData.NowPalette.Count == 0)
                {
                    return;
                }

                using (Bitmap bitmap = await ImageManage.MergeBitmaps(
                    index,
                    index,
                    GData.ImageData[index].OverlayImage.OffsetX,
                    GData.ImageData[index].OverlayImage.OffsetY,
                    GData.ImageData[index].OverlayImage.OverlayMode
                    ))
                {
                    using (Bitmap bitmapOnPal = ShpManage.BitmapOnPalette(bitmap, GData.NowPalette))
                    {
                        Image_PaletteImg.Source = ImageTypeConvert.BitmapToImageSource(bitmapOnPal);
                    }
                }
            }
        }

        private void DataGrid_Palette_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            // 获取鼠标点击的位置
            var point = e.GetPosition(dataGrid);

            // 在鼠标右键点击位置显示上下文菜单
            DataGrid_PaletteContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;
            DataGrid_PaletteContextMenu.PlacementRectangle = new Rect(point.X + 10, point.Y + 10, 0, 0);
            DataGrid_PaletteContextMenu.PlacementTarget = DataGrid_Palette;
            DataGrid_PaletteContextMenu.IsOpen = true;
        }

        private async void Button_PletteIndexDisable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Grid_Main.IsEnabled = false;

                foreach (var cellInfo in DataGrid_Palette.SelectedCells.ToList())
                {
                    int columnIndex = DataGrid_Palette.Columns.IndexOf(cellInfo.Column);
                    int rowIndex = DataGrid_Palette.Items.IndexOf(cellInfo.Item);

                    int paletteIndex = columnIndex * 32 + rowIndex;

                    _dataTablePaletteView.Rows[rowIndex][$"Col{columnIndex}_DisableOpacity"] = 1;
                    _dataTablePaletteView.Rows[rowIndex][$"Col{columnIndex}_TransparentOpacity"] = 0;
                    _dataTablePaletteView.Rows[rowIndex][$"Col{columnIndex}_TextColor"] = System.Windows.Media.Brushes.White;

                    GData.PaletteTransparentIndex.Remove(paletteIndex);
                    if (!GData.PaletteUnableIndex.Contains(paletteIndex))
                    {
                        GData.PaletteUnableIndex.Add(paletteIndex);
                    }
                }

                await PaletteManage.SetPaletteTableDisable(GData.NowPalette, GData.PaletteUnableIndex, GData.PaletteTransparentIndex);

                await ReloadImageSource(GData.UIData.NowIndex);
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
            finally
            {
                Grid_Main.IsEnabled = true;
            }
        }

        private async void Button_PletteIndexTransparent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Grid_Main.IsEnabled = false;

                foreach (var cellInfo in DataGrid_Palette.SelectedCells.ToList())
                {
                    int columnIndex = DataGrid_Palette.Columns.IndexOf(cellInfo.Column);
                    int rowIndex = DataGrid_Palette.Items.IndexOf(cellInfo.Item);

                    int paletteIndex = columnIndex * 32 + rowIndex;

                    _dataTablePaletteView.Rows[rowIndex][$"Col{columnIndex}_DisableOpacity"] = 0;
                    _dataTablePaletteView.Rows[rowIndex][$"Col{columnIndex}_TransparentOpacity"] = 1;
                    _dataTablePaletteView.Rows[rowIndex][$"Col{columnIndex}_TextColor"] = System.Windows.Media.Brushes.White;

                    GData.PaletteUnableIndex.Remove(paletteIndex);
                    if (!GData.PaletteTransparentIndex.Contains(paletteIndex))
                    {
                        GData.PaletteTransparentIndex.Add(paletteIndex);
                    }
                }

                await PaletteManage.SetPaletteTableDisable(GData.NowPalette, GData.PaletteUnableIndex, GData.PaletteTransparentIndex);

                await ReloadImageSource(GData.UIData.NowIndex);
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
            finally
            {
                Grid_Main.IsEnabled = true;
            }
        }

        private async void Button_PletteIndexReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Grid_Main.IsEnabled = false;

                foreach (var cellInfo in DataGrid_Palette.SelectedCells.ToList())
                {
                    int columnIndex = DataGrid_Palette.Columns.IndexOf(cellInfo.Column);
                    int rowIndex = DataGrid_Palette.Items.IndexOf(cellInfo.Item);

                    int paletteIndex = columnIndex * 32 + rowIndex;

                    _dataTablePaletteView.Rows[rowIndex][$"Col{columnIndex}_DisableOpacity"] = 0;
                    _dataTablePaletteView.Rows[rowIndex][$"Col{columnIndex}_TransparentOpacity"] = 0;

                    var c = System.Windows.Media.Color.FromRgb((byte)(GData.NowPalette[paletteIndex].R * 4), (byte)(GData.NowPalette[paletteIndex].G * 4), (byte)(GData.NowPalette[paletteIndex].B * 4));
                    ColorConvert.RGBtoHSB(c.R, c.G, c.B, out _, out _, out float brightness);
                    var textColorTemp = ColorConvert.HSBtoRGB(0, 0, brightness > 0.5f ? 0 : 1.0f);
                    var textColor = System.Windows.Media.Color.FromRgb(textColorTemp.R, textColorTemp.G, textColorTemp.B);

                    _dataTablePaletteView.Rows[rowIndex][$"Col{columnIndex}_TextColor"] = new SolidColorBrush(textColor);

                    GData.PaletteUnableIndex.Remove(paletteIndex);
                    GData.PaletteTransparentIndex.Remove(paletteIndex);
                }

                await PaletteManage.SetPaletteTableDisable(GData.NowPalette, GData.PaletteUnableIndex, GData.PaletteTransparentIndex);

                await ReloadImageSource(GData.UIData.NowIndex);
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
            finally
            {
                Grid_Main.IsEnabled = true;
            }
        }

        private void Image_ViewImg_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Image img = sender as Image;
                if (img != null)
                {
                    Point position = e.GetPosition(img);

                    GData.UIData.ViewImagePointX = (int)position.X;
                    GData.UIData.ViewImagePointY = (int)position.Y;

                    if (RadioButton_ViewMode_OutImgOnPalette.IsChecked == true)
                    {
                        BitmapSource bitmap = img.Source as BitmapSource;
                        int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
                        byte[] pixelData = new byte[bytesPerPixel];
                        Int32Rect rect = new Int32Rect(GData.UIData.ViewImagePointX, GData.UIData.ViewImagePointY, 1, 1);
                        bitmap.CopyPixels(rect, pixelData, bytesPerPixel, 0);

                        GData.UIData.ViewImagePointPaletteIndex = GData.ColorToPaletteTable[pixelData[2] / 4, pixelData[1] / 4, pixelData[0] / 4];

                        int rowIndex = GData.UIData.ViewImagePointPaletteIndex % 32;
                        int columnIndex = GData.UIData.ViewImagePointPaletteIndex / 32;

                        DataGrid_Palette.SelectedCells.Clear();
                        DataGridCellInfo cellInfo = new DataGridCellInfo(DataGrid_Palette.Items[rowIndex], DataGrid_Palette.Columns[columnIndex]);
                        DataGrid_Palette.SelectedCells.Add(cellInfo);

                        // 设置当前单元格并滚动到视图
                        DataGrid_Palette.CurrentCell = cellInfo;
                        DataGrid_Palette.ScrollIntoView(DataGrid_Palette.Items[rowIndex], DataGrid_Palette.Columns[columnIndex]);
                    }
                    else
                    {
                        DataGrid_Palette.SelectedCells.Clear();
                        GData.UIData.ViewImagePointPaletteIndex = -1;
                    }
                }
            }
            catch
            {
                DataGrid_Palette.SelectedCells.Clear();
                GData.UIData.ViewImagePointX = -1;
                GData.UIData.ViewImagePointY = -1;
                GData.UIData.ViewImagePointPaletteIndex = -1;
            }
        }

        private void Image_ViewImg_MouseLeave(object sender, MouseEventArgs e)
        {
            //GData.UIData.ViewImagePointX = -1;
            //GData.UIData.ViewImagePointY = -1;
            //GData.UIData.ViewImagePointPaletteIndex = -1;
        }

        private void Image_PaletteImg_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image_PaletteImg.IsHitTestVisible = false;

            // 获取鼠标点击的位置
            var point = e.GetPosition(Border_ImgView);

            // 在鼠标右键点击位置显示上下文菜单
            Image_PaletteImgContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;
            Image_PaletteImgContextMenu.PlacementRectangle = new Rect(point.X + 10, point.Y + 10, 0, 0);
            Image_PaletteImgContextMenu.PlacementTarget = Border_ImgView;
            Image_PaletteImgContextMenu.IsOpen = true;
        }

        private void Image_PaletteImgContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            Image_PaletteImg.IsHitTestVisible = true;
        }






        private void StackPanel_PaletteColorNum_MouseEnter(object sender, MouseEventArgs e)
        {
            Border_PaletteTip.Visibility = Visibility.Visible;
        }

        private void StackPanel_PaletteColorNum_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_PaletteTip.Visibility = Visibility.Collapsed;
        }

        private void ArtTranslucency_MouseEnter(object sender, MouseEventArgs e)
        {
            Border_ArtTranslucencyTip.Visibility = Visibility.Visible;
        }

        private void ArtTranslucency_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_ArtTranslucencyTip.Visibility = Visibility.Collapsed;
        }

        private void ComboBox_CreatePalMode_MouseEnter(object sender, MouseEventArgs e)
        {
            Border_CreatePaletteModeTip.Visibility = Visibility.Visible;
        }

        private void ComboBox_CreatePalMode_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_CreatePaletteModeTip.Visibility = Visibility.Collapsed;
        }

        private void Resize_MouseEnter(object sender, MouseEventArgs e)
        {
            Border_ResizeTip.Visibility = Visibility.Visible;
        }

        private void Resize_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_ResizeTip.Visibility = Visibility.Collapsed;
        }

        private void PlayerColor_MouseEnter(object sender, MouseEventArgs e)
        {
            Border_PlayerColorTip.Visibility = Visibility.Visible;
        }

        private void PlayerColor_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_PlayerColorTip.Visibility = Visibility.Collapsed;
        }

        private void ViewScale_MouseEnter(object sender, MouseEventArgs e)
        {
            Border_ViewScaleTip.Visibility = Visibility.Visible;
        }

        private void ViewScale_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_ViewScaleTip.Visibility = Visibility.Collapsed;
        }
    }
}
