using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;

namespace Ra2EasyShp.Funcs
{
    internal class ImageTypeConvert
    {
        internal static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);

                return new Bitmap(stream);
            }
        }

        internal static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapDecoder decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }

        internal static ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }

            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            if (!Win32Funcs.DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return imageSource;
        }

        internal static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(bitmapImage.StreamSource);
            bitmapImage.StreamSource.Position = 0;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            Win32Funcs.DeleteObject(hBitmap);
            return bitmapImage;
        }
    }
}
