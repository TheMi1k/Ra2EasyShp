using Ra2EasyShp.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ra2EasyShp.Funcs
{
    internal class LocalImageManage
    {
        internal static void GetPngSize(string filePath, out int width, out int height)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                BitmapFrame frame = decoder.Frames[0];

                width = frame.PixelWidth;
                height = frame.PixelHeight;
            }
        }

        internal static Bitmap LoadBitmapFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return new Bitmap(fs);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 加载Edit原始图片Bitmap（优先resize->temp）
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static Bitmap LoadEditBaseImageBitmap(int index)
        {
            if (index > GData.ImageData.Count - 1)
            {
                return null;
            }

            string path;
            if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgReMarginPath))
            {
                path = GData.ImageData[index].EditImage.ImgReMarginPath;
            }
            else if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgResizePath))
            {
                path = GData.ImageData[index].EditImage.ImgResizePath;
            }
            else if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgTempPath))
            {
                path = GData.ImageData[index].EditImage.ImgTempPath;
            }
            else if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgPath))
            {
                path = GData.ImageData[index].EditImage.ImgPath;
            }
            else
            {
                return null;
            }

            return LoadBitmapFromFile(path);
        }

        /// <summary>
        /// 加载Edit输出图片Bitmap
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static Bitmap LoadEditOutImageBitmap(int index)
        {
            if (index > GData.ImageData.Count - 1)
            {
                return null;
            }

            string path = GData.ImageData[index].EditImage.OutImgTempPath;
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return LoadBitmapFromFile(path);
        }

        /// <summary>
        /// 加载Overlay图片Bitmap（优先temp）
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static Bitmap LoadOverlayImageBitmap(int index)
        {
            if (index > GData.ImageData.Count - 1)
            {
                return null;
            }

            string path;
            if (!string.IsNullOrEmpty(GData.ImageData[index].OverlayImage.ImgTempPath))
            {
                path = GData.ImageData[index].OverlayImage.ImgTempPath;
            }
            else if (!string.IsNullOrEmpty(GData.ImageData[index].OverlayImage.ImgPath))
            {
                path = GData.ImageData[index].OverlayImage.ImgPath;
            }
            else
            {
                return null;
            }

            return LoadBitmapFromFile(path);
        }

        /// <summary>
        /// 加载Edit原始图片 用于显示
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static ImageSource LoadBaseImageSource(int index)
        {
            using (var bitmap = LoadEditBaseImageBitmap(index))
            {
                return ImageTypeConvert.BitmapToImageSource(bitmap);
            }
        }

        /// <summary>
        /// 加载Edit输出图片 用于显示
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static ImageSource LoadOutImageSource(int index)
        {
            using (var bitmap = LoadEditOutImageBitmap(index))
            {
                return ImageTypeConvert.BitmapToImageSource(bitmap);
            }
        }

        /// <summary>
        /// 加载Overlay图片 用于显示
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static ImageSource LoadOverlayImageSource(int index)
        {
            using (var bitmap = LoadOverlayImageBitmap(index))
            {
                return ImageTypeConvert.BitmapToImageSource(bitmap);
            }
        }

        /// <summary>
        /// 将图片保存到硬盘缓存
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="filePath"></param>
        internal static void SaveBitmapToTemp(Bitmap bitmap, string filePath)
        {
            if (bitmap == null || string.IsNullOrEmpty(filePath))
            {
                return;
            }

            try
            {
                bitmap.Save(filePath, ImageFormat.Png);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 将Image图片保存到硬盘缓存 (仅用于GIF载入)
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="filePath"></param>
        internal static void SaveImageToTemp(Image image, string filePath)
        {
            if (image == null || string.IsNullOrEmpty(filePath))
            {
                return;
            }

            try
            {
                using (Bitmap bitmap = new Bitmap(image))
                {
                    bitmap.Save(filePath, ImageFormat.Png);
                }
            }
            catch
            {

            }
        }

        internal static bool GetBaseImageOriginalSize(int index, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (index > GData.ImageData.Count - 1)
            {
                return false;
            }

            string path;

            if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgTempPath))
            {
                path = GData.ImageData[index].EditImage.ImgTempPath;
            }
            else if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgPath))
            {
                path = GData.ImageData[index].EditImage.ImgPath;
            }
            else
            {
                return false;
            }

            if (!File.Exists(path))
            {
                return false;
            }

            GetPngSize(path, out width, out height);

            return true;
        }

        internal static bool GetBaseImageNowSize(int index, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (index > GData.ImageData.Count - 1)
            {
                return false;
            }

            string path;
            if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgResizePath))
            {
                path = GData.ImageData[index].EditImage.ImgResizePath;
            }
            else if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgResizePath))
            {
                path = GData.ImageData[index].EditImage.ImgResizePath;
            }
            else if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgTempPath))
            {
                path = GData.ImageData[index].EditImage.ImgTempPath;
            }
            else if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgPath))
            {
                path = GData.ImageData[index].EditImage.ImgPath;
            }
            else
            {
                return false;
            }

            if (!File.Exists(path))
            {
                return false;
            }

            GetPngSize(path, out width, out height);

            return true;
        }

        internal static void DeleteImageTemp(string filePath)
        {
            Task.Run(() =>
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                try
                {
                    File.Delete(filePath);
                }
                catch
                {

                }
            });
        }
    }
}
