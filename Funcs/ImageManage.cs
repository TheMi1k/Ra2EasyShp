using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Ra2EasyShp.Data;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows.Ink;

namespace Ra2EasyShp.Funcs
{
    internal class ImageManage
    {
        internal static void ConvertImage(int index)
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }
            if (index > GData.ImageData.Count - 1)
            {
                return;
            }

            Bitmap baseBitmap = LocalImageManage.LoadEditBaseImageBitmap(index);

            if (baseBitmap == null)
            {
                return;
            }

            GData.ImageData[index].EditImage.IsChanged = true;

            int alpha = GData.ImageData[index].EditImage.Alpha;
            double transparentDiffusion = GData.ImageData[index].EditImage.TransparentDiffusion;
            double lightness = GData.ImageData[index].EditImage.Lightness;
            bool isTransparent = GData.ImageData[index].EditImage.IsTransparent;

            double outlineTransparentStep = 0;

            double[,] distanceMatrix = null;

            if (isTransparent)
            {
                distanceMatrix = GetDistanceMatrix(baseBitmap);

                int rows = distanceMatrix.GetLength(0);
                int cols = distanceMatrix.GetLength(1);

                double maxDistance = -1;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (distanceMatrix[r, c] > maxDistance)
                        {
                            maxDistance = distanceMatrix[r, c];
                        }
                    }
                }

                outlineTransparentStep = 255 / maxDistance;
            }

            unsafe
            {
                Rectangle rect = new Rectangle(0, 0, baseBitmap.Width, baseBitmap.Height);
                BitmapData bmpData = baseBitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                Bitmap outBitmap = new Bitmap(baseBitmap.Width, baseBitmap.Height);
                BitmapData bmpDataOutImg = outBitmap.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                byte* ptrImg = (byte*)bmpData.Scan0;
                byte* ptrOutImg = (byte*)bmpDataOutImg.Scan0;

                int stride = bmpData.Stride;
                int width = baseBitmap.Width;
                int height = baseBitmap.Height;

                try
                {
                    Parallel.For(0, height, y =>
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int newAlpha = -1;

                            byte* pixel = ptrImg + (y * stride) + (x * 4);
                            byte* pixelOutImg = ptrOutImg + (y * stride) + (x * 4);

                            byte pixelB = pixel[0];
                            byte pixelG = pixel[1];
                            byte pixelR = pixel[2];
                            byte pixelA = pixel[3];

                            int _alpha = pixelA;

                            if (isTransparent && pixelA > 0)
                            {
                                _alpha = (int)Math.Round(distanceMatrix[y, x] * outlineTransparentStep);
                                if (_alpha > 255)
                                {
                                    _alpha = 255;
                                }
                                if (_alpha < 0)
                                {
                                    _alpha = 0;
                                }

                                newAlpha = _alpha;

                                if (pixelA > 0)
                                {
                                    _alpha -= alpha;
                                }
                            }
                            else
                            {
                                if (pixelA > 0)
                                {
                                    _alpha = pixelA - alpha;
                                }
                            }

                            if (_alpha > 255)
                            {
                                _alpha = 255;
                            }
                            if (_alpha < 0)
                            {
                                _alpha = 0;
                            }

                            if (_alpha >= 255)
                            {
                                byte[] colors = SetPixelLightness(pixelA, pixelR, pixelG, pixelB, lightness);

                                pixelOutImg[0] = colors[3]; // B
                                pixelOutImg[1] = colors[2]; // G
                                pixelOutImg[2] = colors[1]; // R
                                pixelOutImg[3] = colors[0]; // A
                            }
                            else if (_alpha <= 0)
                            {
                                pixelOutImg[0] = 255; // B
                                pixelOutImg[1] = 255; // G
                                pixelOutImg[2] = 255; // R
                                pixelOutImg[3] = 0; // A
                            }
                            else
                            {
                                int matrixValue = GData.BayerMatrix[y % GData.MatrixSize, x % GData.MatrixSize];
                                if (_alpha * transparentDiffusion > matrixValue)
                                {
                                    byte[] colors = SetPixelLightness(pixelA, pixelR, pixelG, pixelB, lightness, newAlpha);

                                    pixelOutImg[0] = colors[3]; // B
                                    pixelOutImg[1] = colors[2]; // G
                                    pixelOutImg[2] = colors[1]; // R
                                    pixelOutImg[3] = colors[0]; // A
                                }
                                else
                                {
                                    pixelOutImg[0] = 255; // B
                                    pixelOutImg[1] = 255; // G
                                    pixelOutImg[2] = 255; // R
                                    pixelOutImg[3] = 0; // A
                                }
                            }
                        }
                    });
                }
                finally
                {
                    baseBitmap.UnlockBits(bmpData);
                    baseBitmap.Dispose();
                    outBitmap.UnlockBits(bmpDataOutImg);

                    LocalImageManage.DeleteImageTemp(GData.ImageData[index].EditImage.OutImgTempPath);

                    GData.ImageData[index].EditImage.OutImgTempPath = GetPath.CreateImageTempPath();
                    LocalImageManage.SaveBitmapToTemp(outBitmap, GData.ImageData[index].EditImage.OutImgTempPath);
                    outBitmap.Dispose();
                }
            }

            GC.Collect();
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        private static byte[] SetPixelLightness(byte pixelA, byte pixelR, byte pixelG, byte pixelB, double k, int newAlpha = -1)
        {
            byte[] result = new byte[4];

            int A;
            if (newAlpha != -1)
            {
                A = newAlpha;
            }
            else
            {
                A = pixelA;
            }

            float alphaFactor = A / 255.0f;

            int R, G, B;

            if (k < 0)
            {
                // 变黑 (k 负)
                float darkFactor = (float)Math.Pow(alphaFactor, Math.Abs(k));
                R = (int)(pixelR * darkFactor);
                G = (int)(pixelG * darkFactor);
                B = (int)(pixelB * darkFactor);
            }
            else
            {
                // 变白 (k 正)
                R = (int)(pixelR + (255 - pixelR) * (1 - alphaFactor) * k);
                G = (int)(pixelG + (255 - pixelG) * (1 - alphaFactor) * k);
                B = (int)(pixelB + (255 - pixelB) * (1 - alphaFactor) * k);
            }

            result[0] = 255;
            result[1] = (byte)Clamp(R);
            result[2] = (byte)Clamp(G);
            result[3] = (byte)Clamp(B);

            return result;
        }

        internal static List<Bitmap> ExtractGifFrames(string gifPath)
        {
            List<Bitmap> frames = new List<Bitmap>();

            using (FileStream fs = new FileStream(gifPath, FileMode.Open, FileAccess.Read))
            {
                var gifDecoder = new GifBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                foreach (var frame in gifDecoder.Frames)
                {
                    frames.Add(ImageTypeConvert.BitmapSourceToBitmap(frame));
                }
            }

            return frames;
        }

        internal static void DisposeBitmap(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                try
                {
                    bitmap.Dispose();

                    //if (!Win32Funcs.DeleteObject(bitmap.GetHbitmap()))
                    //{
                    //    throw new Win32Exception();
                    //}
                }
                catch
                {

                }
            }
        }

        private static double[,] GetDistanceMatrix(Bitmap bitmap)
        {
            byte[,] matrix = new byte[bitmap.Height, bitmap.Width];

            unsafe
            {
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                byte* ptr = (byte*)bmpData.Scan0;

                int stride = bmpData.Stride;
                int width = bitmap.Width;
                int height = bitmap.Height;

                Parallel.For(0, height, y =>
                {
                    byte* row = ptr + (y * stride);

                    for (int x = 0; x < width; x++)
                    {
                        byte* pixel = row + (x * 4);

                        byte alpha = pixel[3];

                        matrix[y, x] = (alpha == 0) ? (byte)0 : (byte)1;
                    }
                });

                bitmap.UnlockBits(bmpData);
            }

            double[,] distanceMatrix = GetDistance(matrix);

            return distanceMatrix;
        }

        private static double[,] GetDistance(byte[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] dist = new double[rows, cols];
            Queue<(int, int)> queue = new Queue<(int, int)>();

            (int dx, int dy, double cost)[] directions = {
                (-1, 0, 1), (1, 0, 1), (0, -1, 1), (0, 1, 1), // 上下左右
                (-1, -1, 1.4141), (-1, 1, 1.4141), (1, -1, 1.4141), (1, 1, 1.4141)  // 对角线
            };

            object locker = new object();

            // 初始化距离矩阵
            Parallel.For(0, rows, r =>
            {
                for (int c = 0; c < cols; c++)
                {
                    if (matrix[r, c] == 0)
                    {
                        dist[r, c] = 0;
                        lock (locker)
                        {
                            queue.Enqueue((r, c));
                        }
                    }
                    else
                    {
                        dist[r, c] = double.MaxValue;
                    }
                }
            });

            // BFS 计算距离
            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                foreach (var (dx, dy, cost) in directions)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && ny >= 0 && nx < rows && ny < cols)
                    {
                        double newDistance = dist[x, y] + cost;
                        if (newDistance < dist[nx, ny])
                        {
                            dist[nx, ny] = newDistance;
                            queue.Enqueue((nx, ny));
                        }
                    }
                }
            }

            return dist;
        }


        /// <summary>
        /// 叠加图片
        /// </summary>
        /// <param name="outImgIndex"></param>
        /// <param name="overlayImgIndex"></param>
        /// <param name="overlayXOffset"></param>
        /// <param name="overlayYOffset"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        internal static async Task<Bitmap> MergeBitmaps(int outImgIndex, int overlayImgIndex, int overlayXOffset, int overlayYOffset, Enums.OverlayMode mode)
        {
            Bitmap result = null;

            if (string.IsNullOrEmpty(GData.ImageData[overlayImgIndex].OverlayImage.ImgPath) && string.IsNullOrEmpty(GData.ImageData[overlayImgIndex].OverlayImage.ImgTempPath))
            {
                return LocalImageManage.LoadEditOutImageBitmap(outImgIndex);
            }

            using (Bitmap outImg = LocalImageManage.LoadEditOutImageBitmap(outImgIndex))
            {
                using (Bitmap overlayImg = LocalImageManage.LoadOverlayImageBitmap(overlayImgIndex))
                {
                    if (outImg == null && overlayImg == null)
                    {
                        return null;
                    }
                    if (overlayImg == null)
                    {
                        return new Bitmap(outImg);
                    }

                    await Task.Run(() =>
                    {
                        int width;
                        int height;

                        if (outImg == null)
                        {
                            width = overlayImg.Width + overlayXOffset;
                            height = overlayImg.Height + overlayYOffset;
                        }
                        else
                        {
                            width = outImg.Width;
                            height = outImg.Height;
                        }

                        BitmapSource outImgBitmapSource = null;
                        if (outImg != null)
                        {
                            outImgBitmapSource = ImageTypeConvert.BitmapToBitmapSource(outImg);
                        }

                        BitmapSource overlayImgBitmapSource = ImageTypeConvert.BitmapToBitmapSource(overlayImg);

                        DrawingVisual visual = new DrawingVisual();
                        using (DrawingContext dc = visual.RenderOpen())
                        {
                            if (outImgBitmapSource == null)
                            {
                                dc.DrawImage(overlayImgBitmapSource, new Rect(overlayXOffset, overlayYOffset, overlayImg.Width, overlayImg.Height));
                            }
                            else if (mode == Enums.OverlayMode.叠加在上)
                            {
                                // 下层
                                dc.DrawImage(outImgBitmapSource, new Rect(0, 0, outImg.Width, outImg.Height));

                                // 上层
                                dc.DrawImage(overlayImgBitmapSource, new Rect(overlayXOffset, overlayYOffset, overlayImg.Width, overlayImg.Height));
                            }
                            else
                            {
                                // 下层
                                dc.DrawImage(overlayImgBitmapSource, new Rect(overlayXOffset, overlayYOffset, overlayImg.Width, overlayImg.Height));

                                // 上层
                                dc.DrawImage(outImgBitmapSource, new Rect(0, 0, outImg.Width, outImg.Height));
                            }
                        }

                        RenderTargetBitmap mergedBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                        mergedBitmap.Render(visual);

                        result = ImageTypeConvert.BitmapSourceToBitmap(mergedBitmap);
                    });
                }
            }
            
            return result;
        }

        internal static async Task Resize(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new Exception("输入尺寸不正确");
            }

            await Task.Run(() =>
            {
                int suc = 0;
                int max = GData.ImageData.Count;
                object locker = new object();

                GData.UIData.SetProgressUI(suc, max);

                try
                {
                    Parallel.For(0, GData.ImageData.Count, index =>
                    {
                        LocalImageManage.DeleteImageTemp(GData.ImageData[index].EditImage.ImgResizePath);
                        LocalImageManage.DeleteImageTemp(GData.ImageData[index].EditImage.ImgReMarginPath);
                        GData.ImageData[index].EditImage.ImgResizePath = string.Empty;
                        GData.ImageData[index].EditImage.ImgReMarginPath = string.Empty;
                        using (Bitmap bitmap = LocalImageManage.LoadEditBaseImageBitmap(index))
                        {
                            if (bitmap != null)
                            {
                                using (Bitmap resizeBitmap = new Bitmap(width, height))
                                {
                                    using (Graphics g = Graphics.FromImage(resizeBitmap))
                                    {
                                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                                        g.PixelOffsetMode = PixelOffsetMode.Half;
                                        g.DrawImage(bitmap, 0, 0, width, height);
                                    }

                                    GData.ImageData[index].EditImage.ImgResizePath = GetPath.CreateImageTempPath();
                                    LocalImageManage.SaveImageToTemp(resizeBitmap, GData.ImageData[index].EditImage.ImgResizePath);
                                    LocalImageManage.SaveImageToTemp(resizeBitmap, GData.ImageData[index].EditImage.OutImgTempPath);

                                    GData.ImageData[index].EditImage.IsChanged = false;

                                    lock (locker)
                                    {
                                        suc++;
                                    }

                                    GData.UIData.SetProgressUI(suc, max);
                                }
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            });
        }

        internal static async Task CancelResize()
        {
            await Task.Run(() =>
            {
                int suc = 0;
                int max = GData.ImageData.Count;
                object locker = new object();

                GData.UIData.SetProgressUI(suc, max);

                try
                {
                    Parallel.For(0, GData.ImageData.Count, index =>
                    {
                        if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgResizePath) || !string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgReMarginPath))
                        {
                            LocalImageManage.DeleteImageTemp(GData.ImageData[index].EditImage.ImgResizePath);
                            LocalImageManage.DeleteImageTemp(GData.ImageData[index].EditImage.ImgReMarginPath);
                            GData.ImageData[index].EditImage.ImgResizePath = string.Empty;
                            GData.ImageData[index].EditImage.ImgReMarginPath = string.Empty;
                            using (Bitmap bitmap = LocalImageManage.LoadEditBaseImageBitmap(index))
                            {
                                LocalImageManage.SaveImageToTemp(bitmap, GData.ImageData[index].EditImage.OutImgTempPath);

                                GData.ImageData[index].EditImage.IsChanged = false;
                            }
                        }

                        lock (locker)
                        {
                            suc++;
                        }

                        GData.UIData.SetProgressUI(suc, max);
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            });
        }

        internal static async Task ReMargin(int left, int top, int right, int bottom, bool isCutImage)
        {
            await Task.Run(() =>
            {
                int suc = 0;
                int max = GData.ImageData.Count;
                object locker = new object();

                GData.UIData.SetProgressUI(suc, max);

                int cutHorizontal = 0;
                int cutVertical = 0;

                if (isCutImage)
                {
                    ConcurrentBag<int> cutHorizontals = new ConcurrentBag<int>();
                    ConcurrentBag<int> cutVerticals = new ConcurrentBag<int>();

                    Parallel.For(0, GData.ImageData.Count, index =>
                    {
                        LocalImageManage.DeleteImageTemp(GData.ImageData[index].EditImage.ImgReMarginPath);
                        GData.ImageData[index].EditImage.ImgReMarginPath = string.Empty;
                        using (Bitmap bitmap = LocalImageManage.LoadEditBaseImageBitmap(index))
                        {
                            if (GetCutRange(bitmap, out int cutH, out int cutV))
                            {
                                cutHorizontals.Add(cutH);
                                cutVerticals.Add(cutV);
                            }
                        }
                    });

                    if (cutHorizontals.Count() > 0 && cutVerticals.Count() > 0)
                    {
                        cutHorizontal = cutHorizontals.Min();
                        cutVertical = cutVerticals.Min();
                    }
                }

                try
                {
                    Parallel.For(0, GData.ImageData.Count, index =>
                    {
                        LocalImageManage.DeleteImageTemp(GData.ImageData[index].EditImage.ImgReMarginPath);
                        GData.ImageData[index].EditImage.ImgReMarginPath = string.Empty;
                        using (Bitmap bitmap = LocalImageManage.LoadEditBaseImageBitmap(index))
                        {
                            if (bitmap != null)
                            {
                                int newW = bitmap.Width + left + right - (cutHorizontal * 2);
                                int newH = bitmap.Height + top + bottom - (cutVertical * 2);

                                if (newW > 0 && newH > 0)
                                {
                                    using (Bitmap reMarginBitmap = new Bitmap(newW, newH))
                                    {
                                        using (Graphics g = Graphics.FromImage(reMarginBitmap))
                                        {
                                            g.InterpolationMode = InterpolationMode.NearestNeighbor;
                                            g.PixelOffsetMode = PixelOffsetMode.Half;
                                            g.DrawImage(bitmap, left - cutHorizontal, top - cutVertical, bitmap.Width, bitmap.Height);
                                        }

                                        GData.ImageData[index].EditImage.ImgReMarginPath = GetPath.CreateImageTempPath();
                                        LocalImageManage.SaveImageToTemp(reMarginBitmap, GData.ImageData[index].EditImage.ImgReMarginPath);
                                        LocalImageManage.SaveImageToTemp(reMarginBitmap, GData.ImageData[index].EditImage.OutImgTempPath);

                                        GData.ImageData[index].EditImage.IsChanged = false;
                                    }
                                }
                            }

                            lock (locker)
                            {
                                suc++;
                            }

                            GData.UIData.SetProgressUI(suc, max);
                        }
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            });
        }

        internal static async Task CancelReMargin()
        {
            await Task.Run(() =>
            {
                int suc = 0;
                int max = GData.ImageData.Count;
                object locker = new object();

                GData.UIData.SetProgressUI(suc, max);

                try
                {
                    Parallel.For(0, GData.ImageData.Count, index =>
                    {
                        if (!string.IsNullOrEmpty(GData.ImageData[index].EditImage.ImgReMarginPath))
                        {
                            LocalImageManage.DeleteImageTemp(GData.ImageData[index].EditImage.ImgReMarginPath);
                            GData.ImageData[index].EditImage.ImgReMarginPath = string.Empty;
                            using (Bitmap bitmap = LocalImageManage.LoadEditBaseImageBitmap(index))
                            {
                                LocalImageManage.SaveImageToTemp(bitmap, GData.ImageData[index].EditImage.OutImgTempPath);

                                GData.ImageData[index].EditImage.IsChanged = false;
                            }
                        }

                        lock (locker)
                        {
                            suc++;
                        }

                        GData.UIData.SetProgressUI(suc, max);
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            });
        }

        private static bool GetCutRange(Bitmap bitmap, out int cutHorizontal, out int cutVertical)
        {
            cutHorizontal = 0;
            cutVertical = 0;

            if (bitmap == null)
            {
                return false;
            }

            int left = bitmap.Width;
            int top = bitmap.Height;
            int right = 0;
            int bottom = 0;

            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                int width = bitmap.Width;
                int height = bitmap.Height;
                int stride = bitmapData.Stride;

                bool noPixel = true;

                unsafe
                {
                    byte* ptr = (byte*)bitmapData.Scan0;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            byte* pixel = ptr + (y * stride) + (x * 4);

                            if (pixel[3] != 0)
                            {
                                left = x < left ? x : left;
                                top = y < top ? y : top;
                                right = x > right ? x : right;
                                bottom = y > bottom ? y : bottom;

                                noPixel = false;
                            }
                        }
                    }
                }

                if (noPixel)
                {
                    return false;
                }

                cutVertical = Math.Min(top, height - bottom);
                cutHorizontal = Math.Min(left, width - right);

                return true;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
