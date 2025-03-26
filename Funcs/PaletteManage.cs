using ImageProcessor.Imaging.Formats;
using ImageProcessor.Imaging.Quantizers;
using ImageProcessor;
using Ra2EasyShp.Data;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ra2EasyShp.Funcs
{
    internal class PaletteManage
    {
        /// <summary>
        /// 移除相似颜色
        /// </summary>
        /// <param name="colorDic"></param>
        private static void RemoveSimilarColors(Dictionary<Ra2PaletteColor, int> colorDic)
        {
            List<Ra2PaletteColor> allColorList = new List<Ra2PaletteColor>(colorDic.Keys);
            HashSet<Ra2PaletteColor> removeList = new HashSet<Ra2PaletteColor>();

            for (int i = 0; i < allColorList.Count; i++)
            {
                for (int j = 0; j < allColorList.Count; j++)
                {
                    if (i != j && !removeList.Contains(allColorList[i]))
                    {
                        if (Math.Abs(allColorList[i].R - allColorList[j].R) > 1 || Math.Abs(allColorList[i].G - allColorList[j].G) > 1 || Math.Abs(allColorList[i].B - allColorList[j].B) > 1)
                        {
                            continue;
                        }

                        if (Math.Abs(allColorList[i].R - allColorList[j].R) + Math.Abs(allColorList[i].G - allColorList[j].G) + Math.Abs(allColorList[i].B - allColorList[j].B) <= 2)
                        {
                            if (colorDic[allColorList[i]] > colorDic[allColorList[j]])
                            {
                                removeList.Add(allColorList[j]);
                            }
                        }
                    }
                }
            }

            foreach (var c in removeList)
            {
                colorDic.Remove(c);
            }
        }

        /// <summary>
        /// 移除玩家所属色(主要颜色用的)
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="playerColors"></param>
        private static void RemovePlayerColors(List<Ra2PaletteColor> colors, List<Ra2PaletteColor> playerColors)
        {
            if (colors.Count == 0)
            {
                return;
            }

            List<Ra2PaletteColor> removeList = new List<Ra2PaletteColor>();

            foreach (var c in colors)
            {
                foreach (var pc in playerColors)
                {
                    if (pc.A == 0)
                    {
                        continue;
                    }

                    if (Math.Abs(c.R - pc.R) + Math.Abs(c.G - pc.G) + Math.Abs(c.B - pc.B) <= 8)
                    {
                        removeList.Add(c);
                        break;
                    }
                }
            }

            foreach (var item in removeList)
            {
                colors.Remove(item);
            }
        }

        /// <summary>
        /// 移除玩家所属色(八叉树量化用的)
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="playerColors"></param>
        private static void RemovePlayerColors(HashSet<Color> colors, List<Ra2PaletteColor> playerColors)
        {
            if (colors.Count == 0)
            {
                return;
            }

            List<Color> removeList = new List<Color>();

            foreach (var c in colors)
            {
                foreach (var pc in playerColors)
                {
                    if (pc.A == 0)
                    {
                        continue;
                    }

                    if (Math.Abs((c.R / 4) - pc.R) + Math.Abs((c.G / 4) - pc.G) + Math.Abs((c.B / 4) - pc.B) <= 8)
                    {
                        removeList.Add(c);
                        break;
                    }
                }
            }

            foreach (var item in removeList)
            {
                colors.Remove(item);
            }
        }

        internal static async Task<List<Ra2PaletteColor>> CreatePalette(int palColorNum, Ra2PaletteColor[] paletteHeaderColor, List<Ra2PaletteColor> playerColorList, string mode)
        {
            List<Ra2PaletteColor> playerColorSetBackgroud = null;
            if (playerColorList != null)
            {
                playerColorSetBackgroud = new List<Ra2PaletteColor>();
                foreach (var color in playerColorList)
                {
                    if (color.A == 0)
                    {
                        playerColorSetBackgroud.Add(paletteHeaderColor[0]);
                    }
                    else
                    {
                        playerColorSetBackgroud.Add(color);
                    }
                }
            }

            List<Ra2PaletteColor> headerColors = new List<Ra2PaletteColor>();
            foreach (var phc in paletteHeaderColor)
            {
                if (phc.A != 0)
                {
                    headerColors.Add(phc);
                }
            }

            List<Ra2PaletteColor> palette = null;

            await Task.Run(async () =>
            {
                if (mode == Enums.CreatePalMode.主要颜色补小像素.ToString())
                {
                    if (palColorNum < 150)
                    {
                        throw new Exception("该色盘生成方法颜色数量不能小于150");
                    }
                    palette = await Create256ColorPalette_Mode3(palColorNum, headerColors.ToArray(), playerColorSetBackgroud);
                }
                else if (mode == Enums.CreatePalMode.主要颜色.ToString())
                {
                    palette = await Create256ColorPalette_Mode1(palColorNum, headerColors.ToArray(), playerColorSetBackgroud);
                }
                else
                {
                    palette = await Create256ColorPalette_Mode2(palColorNum, headerColors.ToArray(), playerColorSetBackgroud);
                }
            });

            return palette;
        }

        /// <summary>
        /// 生成色盘(主要颜色优先)
        /// </summary>
        /// <param name="palColorNum"></param>
        /// <returns></returns>
        private static async Task<List<Ra2PaletteColor>> Create256ColorPalette_Mode1(int palColorNum, Ra2PaletteColor[] paletteHeaderColor, List<Ra2PaletteColor> playerColorList)
        {
            Dictionary<Ra2PaletteColor, int> colorCounts = new Dictionary<Ra2PaletteColor, int>();

            int sucCount = 0;
            int maxCount = GData.ImageData.Count;

            GData.UIData.SetProgressUI(sucCount, maxCount);

            for (int index = 0; index < GData.ImageData.Count; index++)
            {
                Bitmap resultImg = await ImageManage.MergeBitmaps(
                    index,
                    index,
                    GData.ImageData[index].OverlayImage.OffsetX,
                    GData.ImageData[index].OverlayImage.OffsetY, 
                    GData.ImageData[index].OverlayImage.OverlayMode);

                if (resultImg != null)
                {
                    unsafe
                    {
                        Rectangle rect = new Rectangle(0, 0, resultImg.Width, resultImg.Height);
                        BitmapData bmpData = resultImg.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        byte* ptr = (byte*)bmpData.Scan0;

                        int stride = bmpData.Stride;
                        int width = resultImg.Width;
                        int height = resultImg.Height;

                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                byte* pixel = ptr + (y * stride) + (x * 4);

                                byte pixelB = pixel[0];
                                byte pixelG = pixel[1];
                                byte pixelR = pixel[2];
                                byte pixelA = pixel[3];

                                if (pixelA == 0)
                                {
                                    continue;
                                }

                                Ra2PaletteColor c = Ra2PaletteColor.FromArgb(255, pixelR, pixelG, pixelB);

                                if (colorCounts.ContainsKey(c))
                                {
                                    colorCounts[c]++;
                                }
                                else
                                {
                                    colorCounts[c] = 1;
                                }
                            }
                        }

                        resultImg.UnlockBits(bmpData);
                    }

                    ImageManage.DisposeBitmap(resultImg);
                }

                sucCount += 1;

                GData.UIData.SetProgressUI(sucCount, maxCount);
            }

            RemoveSimilarColors(colorCounts);

            var result = colorCounts.OrderByDescending(c => c.Value)
                              .Select(c => c.Key)
                              .ToList();

            if (playerColorList != null)
            {
                RemovePlayerColors(result, playerColorList);
            }

            // 插入头部颜色
            for (int i = paletteHeaderColor.Length - 1; i >= 0; i--)
            {
                result.Insert(0, paletteHeaderColor[i]);
            }

            // 插入玩家所属色
            if (playerColorList != null)
            {
                result.InsertRange(16, playerColorList);
            }

            result = result.Take(palColorNum).ToList();

            while (true)
            {
                if (result.Count >= 256)
                {
                    break;
                }

                result.Add(paletteHeaderColor[0]);
            }

            GC.Collect();

            return result;
        }

        /// <summary>
        /// 生成色盘(OctreeQuantizer)
        /// </summary>
        /// <param name="palColorNum"></param>
        /// <returns></returns>
        private static async Task<List<Ra2PaletteColor>> Create256ColorPalette_Mode2(int palColorNum, Ra2PaletteColor[] paletteHeaderColor, List<Ra2PaletteColor> playerColorList)
        {
            HashSet<Color> hs = new HashSet<Color>();

            int sucCount = 0;
            int maxCount = GData.ImageData.Count;

            GData.UIData.SetProgressUI(sucCount, maxCount);

            for (int index = 0; index < GData.ImageData.Count; index++)
            {
                Bitmap resultImg = await ImageManage.MergeBitmaps(
                    index,
                    index,
                    GData.ImageData[index].OverlayImage.OffsetX,
                    GData.ImageData[index].OverlayImage.OffsetY,
                    GData.ImageData[index].OverlayImage.OverlayMode);

                if (resultImg != null)
                {
                    unsafe
                    {
                        Rectangle rect = new Rectangle(0, 0, resultImg.Width, resultImg.Height);
                        BitmapData bmpData = resultImg.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        byte* ptr = (byte*)bmpData.Scan0;

                        int stride = bmpData.Stride;
                        int width = resultImg.Width;
                        int height = resultImg.Height;

                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                byte* pixel = ptr + (y * stride) + (x * 4);

                                byte pixelB = pixel[0];
                                byte pixelG = pixel[1];
                                byte pixelR = pixel[2];
                                byte pixelA = pixel[3];

                                if (pixelA == 0)
                                {
                                    continue;
                                }

                                Color c = Color.FromArgb(255, pixelR, pixelG, pixelB);

                                hs.Add(c);
                            }
                        }

                        resultImg.UnlockBits(bmpData);
                    }

                    ImageManage.DisposeBitmap(resultImg);
                }

                sucCount += 1;

                GData.UIData.SetProgressUI(sucCount, maxCount);
            }

            int numSub = 0;
            if (playerColorList != null)
            {
                RemovePlayerColors(hs, playerColorList);
                numSub = 16;
            }

            int allColorBitmapW = (int)Math.Sqrt(hs.Count);
            int allColorBitmapH = (int)Math.Ceiling((double)(hs.Count / (allColorBitmapW * 1.0f)));

            //
            //.WriteLine(allColorBitmapW + ", " + allColorBitmapH);
            Bitmap allColorBitmap = new Bitmap(allColorBitmapW, allColorBitmapH);

            int xCount = 0;
            int yCount = 0;
            foreach (var c in hs)
            {
                allColorBitmap.SetPixel(xCount, yCount, c);
                xCount++;
                if (xCount >= allColorBitmapW)
                {
                    xCount = 0;
                    yCount++;
                }
            }

            //allColorBitmap.Save(@"C:\Users\Milk\Desktop\xxxxx.png");

            ImageFactory factory = new ImageFactory();
            factory.Load(allColorBitmap);
            ISupportedImageFormat format = new PngFormat { Quality = 100, IsIndexed = true, Quantizer = new OctreeQuantizer(palColorNum - paletteHeaderColor.Length - numSub, 8) };
            factory.Format(format);
            MemoryStream stream = new MemoryStream();
            factory.Save(stream);
            //factory.Save(@"C:\Users\Milk\Desktop\xxx22xx.png");
            Bitmap src = new Bitmap(stream);
            stream.Dispose();
            ImageManage.DisposeBitmap(allColorBitmap);

            HashSet<Ra2PaletteColor> set = new HashSet<Ra2PaletteColor>();

            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    set.Add(Ra2PaletteColor.FromColor(src.GetPixel(x, y)));
                }
            }

            ImageManage.DisposeBitmap(src);

            var result = set.ToList();

            // 插入头部颜色
            for (int i = paletteHeaderColor.Length - 1; i >= 0; i--)
            {
                result.Insert(0, paletteHeaderColor[i]);
            }

            // 插入玩家所属色
            if (playerColorList != null && playerColorList.Count == 16)
            {
                result.InsertRange(16, playerColorList);
            }

            result = result.Take(palColorNum).ToList();

            while (true)
            {
                if (result.Count >= 256)
                {
                    break;
                }

                result.Add(paletteHeaderColor[0]);
            }

            GC.Collect();

            return result;
        }

        /// <summary>
        /// 生成色盘(补全少部分像素细节)
        /// </summary>
        /// <param name="palColorNum"></param>
        /// <returns></returns>
        private static async Task<List<Ra2PaletteColor>> Create256ColorPalette_Mode3(int palColorNum, Ra2PaletteColor[] paletteHeaderColor, List<Ra2PaletteColor> playerColorList)
        {
            Dictionary<Ra2PaletteColor, int> colorCounts = new Dictionary<Ra2PaletteColor, int>();

            int sucCount = 0;
            int maxCount = GData.ImageData.Count;

            GData.UIData.SetProgressUI(sucCount, maxCount);

            List<Ra2PaletteColor> colorBase = new List<Ra2PaletteColor>();
            for (int r = 4; r <= 244; r += 60)
            {
                for (int g = 4; g <= 244; g += 60)
                {
                    for (int b = 4; b <= 244; b += 60)
                    {
                        colorBase.Add(Ra2PaletteColor.FromArgb(255, (byte)r, (byte)g, (byte)b));
                    }
                }
            }

            for (int index = 0; index < GData.ImageData.Count; index++)
            {
                Bitmap resultImg = await ImageManage.MergeBitmaps(
                    index,
                    index,
                    GData.ImageData[index].OverlayImage.OffsetX,
                    GData.ImageData[index].OverlayImage.OffsetY,
                    GData.ImageData[index].OverlayImage.OverlayMode);

                if (resultImg != null)
                {
                    unsafe
                    {
                        Rectangle rect = new Rectangle(0, 0, resultImg.Width, resultImg.Height);
                        BitmapData bmpData = resultImg.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        byte* ptr = (byte*)bmpData.Scan0;

                        int stride = bmpData.Stride;
                        int width = resultImg.Width;
                        int height = resultImg.Height;

                        object locker = new object();

                        Parallel.For(0, height, y =>
                        {
                            for (int x = 0; x < width; x++)
                            {
                                byte* pixel = ptr + (y * stride) + (x * 4);

                                byte pixelB = pixel[0];
                                byte pixelG = pixel[1];
                                byte pixelR = pixel[2];
                                byte pixelA = pixel[3];

                                if (pixelA == 0)
                                {
                                    continue;
                                }

                                Ra2PaletteColor c = Ra2PaletteColor.FromArgb(255, pixelR, pixelG, pixelB);

                                lock (locker)
                                {
                                    if (colorCounts.ContainsKey(c))
                                    {
                                        colorCounts[c]++;
                                    }
                                    else
                                    {
                                        colorCounts[c] = 1;
                                    }
                                }
                            }
                        });

                        resultImg.UnlockBits(bmpData);
                    }

                    ImageManage.DisposeBitmap(resultImg);
                }

                sucCount += 1;

                GData.UIData.SetProgressUI(sucCount, maxCount);
            }

            HashSet<Ra2PaletteColor> appendColor = new HashSet<Ra2PaletteColor>();

            if (colorCounts.Count > palColorNum - 1)
            {
                RemoveSimilarColors(colorCounts);

                // 获取原图片中对于base颜色中最相近的颜色
                Ra2PaletteColor ac = Ra2PaletteColor.FromArgb(0, 0, 0, 0);

                object locker = new object();
                Parallel.For(0, colorBase.Count, i =>
                {
                    Ra2PaletteColor cb = colorBase[i];
                    int minOffset = int.MaxValue;
                    foreach (var kv in colorCounts)
                    {
                        int offset = Math.Abs(kv.Key.R - cb.R) + Math.Abs(kv.Key.G - cb.G) + Math.Abs(kv.Key.B - cb.B);
                        if (offset <= 15)
                        {
                            if (offset < minOffset)
                            {
                                minOffset = offset;
                                ac = kv.Key;
                            }
                        }
                    }

                    if (minOffset != int.MaxValue)
                    {
                        lock (locker)
                        {
                            appendColor.Add(ac);
                        }
                    }
                });
            }

            var result = colorCounts.OrderByDescending(c => c.Value).Select(c => c.Key).ToList();

            if (playerColorList != null)
            {
                RemovePlayerColors(result, playerColorList);
            }

            // 插入头部颜色
            for (int i = paletteHeaderColor.Length - 1; i >= 0; i--)
            {
                result.Insert(0, paletteHeaderColor[i]);
            }

            // 插入玩家所属色
            if (playerColorList != null)
            {
                result.InsertRange(16, playerColorList);
            }

            result = result.Take(palColorNum).ToList();

            // 插入细节颜色
            if (appendColor.Count > 0)
            {
                // 判断当前色盘中有没有这个色
                List<Ra2PaletteColor> insertBaseColorList = new List<Ra2PaletteColor>();
                foreach (var cb in appendColor)
                {
                    bool isContains = false;
                    foreach (var cr in result)
                    {
                        if (Math.Abs(cr.R - cb.R) + Math.Abs(cr.G - cb.G) + Math.Abs(cr.B - cb.B) <= 20)
                        {
                            isContains = true;
                        }
                    }

                    if (!isContains)
                    {
                        insertBaseColorList.Add(cb);
                    }
                }

                if (playerColorList != null)
                {
                    RemovePlayerColors(insertBaseColorList, playerColorList);
                }

                if (insertBaseColorList.Count > 0)
                {
                    result.RemoveRange(result.Count - insertBaseColorList.Count - 1, insertBaseColorList.Count);
                    result.AddRange(insertBaseColorList);
                }
            }

            while (true)
            {
                if (result.Count >= 256)
                {
                    break;
                }

                result.Add(paletteHeaderColor[0]);
            }

            GC.Collect();

            return result;
        }

        internal static void InitColorToPaletteTable(List<Ra2PaletteColor> palette)
        {
            GData.ColorToPaletteTable = new int[64, 64, 64];
            GData.ColorToPaletteTransformTable = new int[64, 64, 64];
            GData.PaletteUnableIndex.Clear();

            Parallel.For(0, 64, r =>
            {
                for (int g = 0; g < 64; g++)
                {
                    for (int b = 0; b < 64; b++)
                    {
                        GData.ColorToPaletteTable[r, g, b] = MatchColor((byte)r, (byte)g, (byte)b, palette);
                        GData.ColorToPaletteTransformTable[r, g, b] = -1;
                    }
                }
            });
        }

        internal static async Task SetPaletteTableDisable(List<Ra2PaletteColor> palette, List<int> disableList, List<int> transparentList)
        {
            await Task.Run(() =>
            {
                if (disableList.Count == 0 && transparentList.Count == 0)
                {
                    Parallel.For(0, 64, r =>
                    {
                        for (int g = 0; g < 64; g++)
                        {
                            for (int b = 0; b < 64; b++)
                            {
                                GData.ColorToPaletteTransformTable[r, g, b] = -1;
                            }
                        }
                    });

                    return;
                }

                List<Ra2PaletteColor> paletteEnable = new List<Ra2PaletteColor>();

                for (int i = 0; i < palette.Count; i++)
                {
                    if (disableList.Contains(i))
                    {
                        // alpha设置100 不参与颜色匹配
                        paletteEnable.Add(Ra2PaletteColor.FromArgb(100, (byte)(palette[i].R * 4), (byte)(palette[i].G * 4), (byte)(palette[i].B * 4)));
                        continue;
                    }

                    paletteEnable.Add(Ra2PaletteColor.FromPaletteColor(palette[i].R, palette[i].G, palette[i].B));
                }

                Parallel.For(0, 64, r =>
                {
                    for (int g = 0; g < 64; g++)
                    {
                        for (int b = 0; b < 64; b++)
                        {
                            bool notTrans = true;
                            if (disableList.Contains(GData.ColorToPaletteTable[r, g, b]))
                            {
                                GData.ColorToPaletteTransformTable[r, g, b] = MatchColorUnable((byte)r, (byte)g, (byte)b, paletteEnable);

                                // 如果重新匹配后的颜色在设置透明色里 设为0
                                if (transparentList.Contains(GData.ColorToPaletteTransformTable[r, g, b]))
                                {
                                    GData.ColorToPaletteTransformTable[r, g, b] = 0;
                                }
                                notTrans = false;
                            }
                            if (transparentList.Contains(GData.ColorToPaletteTable[r, g, b]))
                            {
                                GData.ColorToPaletteTransformTable[r, g, b] = 0;
                                notTrans = false;
                            }

                            if (notTrans)
                            {
                                GData.ColorToPaletteTransformTable[r, g, b] = -1;
                            }
                        }
                    }
                });
            });
        }

        internal static int GetPaletteIndex(byte r, byte g, byte b)
        {
            int index = GData.ColorToPaletteTransformTable[r / 4, g / 4, b / 4];
            if (index >= 0)
            {
                return index;
            }
            else
            {
                return GData.ColorToPaletteTable[r / 4, g / 4, b / 4];
            }
        }

        private static int MatchColor(byte r, byte g, byte b, List<Ra2PaletteColor> palette)
        {
            int resultIndex = 0;
            int min = int.MaxValue;
            float hueMin = float.MaxValue;

            for (int i = 0; i < palette.Count; i++)
            {
                if (palette[i].R == r && palette[i].G == g && palette[i].B == b)
                {
                    return i;
                }

                int num = Math.Abs(palette[i].R - r) + Math.Abs(palette[i].G - g) + Math.Abs(palette[i].B - b);

                if (num < min)
                {
                    ColorConvert.RGBtoHSB(palette[i].R * 4, palette[i].G * 4, palette[i].B * 4, out float h1, out _, out _);
                    ColorConvert.RGBtoHSB(r * 4, g * 4, b * 4, out float h2, out _, out _);

                    min = num;
                    resultIndex = i;
                    hueMin = Math.Abs(h1 - h2);
                }
                else if (num == min)
                {
                    ColorConvert.RGBtoHSB(palette[i].R * 4, palette[i].G * 4, palette[i].B * 4, out float h1, out _, out _);
                    ColorConvert.RGBtoHSB(r * 4, g * 4, b * 4, out float h2, out _, out _);
                    float hueSub = Math.Abs(h1 - h2);

                    if (hueSub < hueMin)
                    {
                        min = num;
                        hueMin = hueSub;
                        resultIndex = i;
                    }
                }
            }

            return resultIndex;
        }

        private static int MatchColorUnable(byte r, byte g, byte b, List<Ra2PaletteColor> palette)
        {
            int resultIndex = 0;
            int min = int.MaxValue;
            float hueMin = float.MaxValue;

            for (int i = 0; i < palette.Count; i++)
            {
                if (palette[i].A != 100)
                {
                    if (palette[i].R == r && palette[i].G == g && palette[i].B == b)
                    {
                        return i;
                    }

                    int num = Math.Abs(palette[i].R - r) + Math.Abs(palette[i].G - g) + Math.Abs(palette[i].B - b);

                    if (num < min)
                    {
                        ColorConvert.RGBtoHSB(palette[i].R * 4, palette[i].G * 4, palette[i].B * 4, out float h1, out _, out _);
                        ColorConvert.RGBtoHSB(r * 4, g * 4, b * 4, out float h2, out _, out _);

                        min = num;
                        resultIndex = i;
                        hueMin = Math.Abs(h1 - h2);
                    }
                    else if (num == min)
                    {
                        ColorConvert.RGBtoHSB(palette[i].R * 4, palette[i].G * 4, palette[i].B * 4, out float h1, out _, out _);
                        ColorConvert.RGBtoHSB(r * 4, g * 4, b * 4, out float h2, out _, out _);
                        float hueSub = Math.Abs(h1 - h2);

                        if (hueSub < hueMin)
                        {
                            min = num;
                            hueMin = hueSub;
                            resultIndex = i;
                        }
                    }
                }
            }

            return resultIndex;
        }
    }
}
