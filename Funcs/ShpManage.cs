using Ra2EasyShp.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

namespace Ra2EasyShp.Funcs
{
    internal class ShpManage
    {
        private struct ShpHeader
        {
            internal ushort Width;
            internal ushort Height;
            internal ushort FrameCount;
        }

        private class ShpFrame
        {
            internal ushort OffsetX;
            internal ushort OffsetY;
            internal ushort Width;
            internal ushort Height;
            internal byte Flag;
            internal byte[] Align;
            internal uint Color;
            internal uint Reserved;
            internal uint FrameOffset;
            internal byte[] FrameData;
        }

        private static void ViewBitmapSetColor(byte r, byte g, byte b, List<Ra2PaletteColor> palette, bool backGround, out byte oA, out byte oR, out byte oG, out byte oB)
        {
            int palIndex = PaletteManage.GetPaletteIndex(r, g, b);

            if (palIndex == 0 && !backGround)
            {
                oA = 0;
                oR = 0;
                oG = 0;
                oB = 0;

                return;
            }

            if (palIndex < 16 || palIndex > 31 || GData.PlayerColorView == Enums.ViewPlayerColor.无)
            {
                oA = (byte)255;
                oR = (byte)(palette[palIndex].R * 4);
                oG = (byte)(palette[palIndex].G * 4);
                oB = (byte)(palette[palIndex].B * 4);

                return;
            }

            oA = (byte)255;
            oR = GData.PlayerColorDic[GData.PlayerColorView][palIndex - 16][0];
            oG = GData.PlayerColorDic[GData.PlayerColorView][palIndex - 16][1];
            oB = GData.PlayerColorDic[GData.PlayerColorView][palIndex - 16][2];
        }

        internal static Bitmap BitmapOnPalette(Bitmap bitmap, List<Ra2PaletteColor> palette, bool background)
        {
            if (bitmap == null)
            {
                return null;
            }

            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            Bitmap resultBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            BitmapData resultBitmapData = resultBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    byte* ptr = (byte*)bitmapData.Scan0;
                    byte* resultPtr = (byte*)resultBitmapData.Scan0;

                    int stride = bitmapData.Stride;
                    int w = bitmap.Width;
                    int h = bitmap.Height;

                    Parallel.For(0, h, y =>
                    {
                        for (int x = 0; x < w; x++)
                        {
                            byte* pixel = ptr + (y * stride) + (x * 4);
                            byte* resultPixel = resultPtr + (y * resultBitmapData.Stride) + (x * 4);

                            if (pixel[3] == 0)
                            {
                                if (background)
                                {
                                    resultPixel[3] = 255;
                                }
                                else
                                {
                                    resultPixel[3] = 0;
                                }
                                resultPixel[2] = (byte)(palette[0].R * 4);
                                resultPixel[1] = (byte)(palette[0].G * 4);
                                resultPixel[0] = (byte)(palette[0].B * 4);
                            }
                            else
                            {
                                ViewBitmapSetColor(pixel[2], pixel[1], pixel[0], palette, background, out byte oA, out byte oR, out byte oG, out byte oB);

                                resultPixel[3] = oA;
                                resultPixel[2] = oR;
                                resultPixel[1] = oG;
                                resultPixel[0] = oB;
                            }
                        }
                    });
                }

                return resultBitmap;
            }
            catch
            {
                return null;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
                resultBitmap.UnlockBits(resultBitmapData);
            }
        }

        private static void BitmapToShpData(Bitmap bitmap, List<Ra2PaletteColor> palette, Enums.ShpCompressionMode shpCompressionMode, bool isShadow, out byte[] frameHeader, out byte[] frameData)
        {
            if (bitmap == null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write((ushort)0);
                        bw.Write((ushort)0);
                        bw.Write((ushort)0);
                        bw.Write((ushort)0);

                        if (shpCompressionMode == Enums.ShpCompressionMode.鼠标动画)
                        {
                            bw.Write((byte)1); // 压缩方式 flag
                        }
                        else
                        {
                            bw.Write((byte)3); // 压缩方式 flag
                        }

                        bw.Write(new byte[] { 0, 0, 0 }); // 补位
                        bw.Write((uint)0); // Color
                        bw.Write((uint)0); // 保留
                        bw.Write((uint)0); // 帧数据偏移
                    }

                    frameHeader = ms.ToArray();
                }

                frameData = new byte[] { };

                return;
            }

            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int minX = bitmap.Width;
            int minY = bitmap.Height;
            int maxX = 0;
            int maxY = 0;

            int offsetX, offsetY, width, height;

            try
            {
                unsafe
                {
                    byte* ptr = (byte*)bitmapData.Scan0;

                    int stride = bitmapData.Stride;
                    int w = bitmap.Width;
                    int h = bitmap.Height;

                    bool noPixel = true;
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            byte* pixel = ptr + (y * stride) + (x * 4);

                            if (pixel[3] != 0)
                            {
                                minX = x < minX ? x : minX;
                                minY = y < minY ? y : minY;
                                maxX = x > maxX ? x : maxX;
                                maxY = y > maxY ? y : maxY;
                                noPixel = false;
                            }
                        }
                    }

                    // 全透明帧
                    if (noPixel)
                    {
                        offsetX = 0;
                        offsetY = 0;
                        width = 0;
                        height = 0;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (BinaryWriter bw = new BinaryWriter(ms))
                            {
                                bw.Write((ushort)offsetX);
                                bw.Write((ushort)offsetY);
                                bw.Write((ushort)width);
                                bw.Write((ushort)height);

                                if (shpCompressionMode == Enums.ShpCompressionMode.鼠标动画)
                                {
                                    bw.Write((byte)1); // 压缩方式 flag
                                }
                                else
                                {
                                    bw.Write((byte)3); // 压缩方式 flag
                                }

                                bw.Write(new byte[] { 0, 0, 0 }); // 补位
                                bw.Write((uint)0); // Color
                                bw.Write((uint)0); // 保留
                                bw.Write((uint)0); // 帧数据偏移
                            }

                            frameHeader = ms.ToArray();
                        }

                        frameData = new byte[] { };

                        return;
                    }

                    offsetX = minX;
                    offsetY = minY;
                    width = maxX - minX + 1;
                    height = maxY - minY + 1;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (BinaryWriter bw = new BinaryWriter(ms))
                        {
                            bw.Write((ushort)offsetX);
                            bw.Write((ushort)offsetY);
                            bw.Write((ushort)width);
                            bw.Write((ushort)height);
                            bw.Write((byte)1); // 压缩方式 flag
                            bw.Write(new byte[] { 0, 0, 0 }); // 补位
                            bw.Write((uint)0); // Color
                            bw.Write((uint)0); // 保留
                            bw.Write((uint)0); // 帧数据偏移
                        }

                        frameHeader = ms.ToArray();
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (BinaryWriter bw = new BinaryWriter(ms))
                        {
                            for (int y = offsetY; y < offsetY + height; y++)
                            {
                                for (int x = offsetX; x < offsetX + width; x++)
                                {
                                    byte* pixel = ptr + (y * stride) + (x * 4);

                                    if (pixel[3] == 0)
                                    {
                                        bw.Write((byte)0);
                                        continue;
                                    }

                                    if (isShadow)
                                    {
                                        bw.Write((byte)1);
                                    }
                                    else
                                    {
                                        bw.Write((byte)PaletteManage.GetPaletteIndex(pixel[2], pixel[1], pixel[0]));
                                    }
                                }
                            }
                        }

                        frameData = ms.ToArray();
                    }
                }

                if (shpCompressionMode == Enums.ShpCompressionMode.鼠标动画)
                {
                    return;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        int byteIndex = 0;
                        for (int y = offsetY; y < offsetY + height; y++)
                        {
                            List<byte> line = new List<byte>();
                            for (int x = offsetX; x < offsetX + width; x++)
                            {
                                line.Add(frameData[byteIndex]);
                                byteIndex++;
                            }

                            List<byte> compression = new List<byte>();
                            byte count = 0;
                            foreach (byte b in line)
                            {
                                if (b == 0)
                                {
                                    count++;
                                    if (count == 0xff)
                                    {
                                        compression.Add(0);
                                        compression.Add(count);
                                        count = 0;
                                    }
                                }
                                else
                                {
                                    if (count > 0)
                                    {
                                        compression.Add(0);
                                        compression.Add(count);
                                        count = 0;
                                    }
                                    compression.Add(b);
                                }
                            }
                            if (count > 0)
                            {
                                compression.Add(0);
                                compression.Add(count);
                            }

                            bw.Write((ushort)(compression.Count + 2));
                            bw.Write(compression.ToArray());
                        }
                    }

                    var frameDataCompression = ms.ToArray();

                    if (shpCompressionMode == Enums.ShpCompressionMode.最佳文件大小)
                    {
                        if (frameDataCompression.Length < frameData.Length)
                        {
                            frameData = frameDataCompression;
                            frameHeader[8] = 3;
                        }
                    }
                    else if (shpCompressionMode == Enums.ShpCompressionMode.单位建筑)
                    {
                        frameData = frameDataCompression;
                        frameHeader[8] = 3;
                    }
                }
            }
            catch
            {
                frameHeader = null;
                frameData = null;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        internal static byte[] BitmapToShp(List<string> bitmapPathList, List<Ra2PaletteColor> palette, Enums.ShpCompressionMode shpCompressionMode, int shadowFrameStart, int shadowFrameEnd)
        {
            GData.UIData.SetProgressUI(0, bitmapPathList.Count);

            bool isError = false;

            ConcurrentBag<int> widths = new ConcurrentBag<int>();
            ConcurrentBag<int> heights = new ConcurrentBag<int>();

            Parallel.ForEach(bitmapPathList, filePath =>
            {
                if (File.Exists(filePath))
                {
                    LocalImageManage.GetPngSize(filePath, out int width, out int height);
                    widths.Add(width);
                    heights.Add(height);
                }
            });

            // 获取最大尺寸图片的宽高作为画布大小
            int maxW = widths.Max();
            int maxH = heights.Max();

            byte[] shpHeader;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)0); // 头部空00 00
                    bw.Write((ushort)maxW); // 宽度
                    bw.Write((ushort)maxH); // 高度
                    bw.Write((ushort)bitmapPathList.Count); // 帧数
                }

                shpHeader = ms.ToArray();
            }

            List<byte[]> frameHeaderList = new List<byte[]>();
            List<byte[]> frameDataList = new List<byte[]>();

            for (int i = 0; i < bitmapPathList.Count; i++)
            {
                frameHeaderList.Add(null);
                frameDataList.Add(null);
            }

            object sucCountLocker = new object();
            int sucCount = 0;
            Parallel.For(0, bitmapPathList.Count, i =>
            {
                bool isShadow = (i >= shadowFrameStart && i <= shadowFrameEnd);

                using (Bitmap bitmap = LocalImageManage.LoadBitmapFromFile(bitmapPathList[i]))
                {
                    BitmapToShpData(bitmap, palette, shpCompressionMode, isShadow, out byte[] freamHeader, out byte[] freamData);
                    if (freamHeader == null || freamData == null)
                    {
                        lock (sucCountLocker)
                        {
                            isError = true;
                        }
                    }
                    frameHeaderList[i] = freamHeader;
                    frameDataList[i] = freamData;
                }

                lock (sucCountLocker)
                {
                    sucCount++;
                }

                GData.UIData.SetProgressUI(sucCount, bitmapPathList.Count);
            });

            if (isError)
            {
                return null;
            }

            int dataStartOffset = shpHeader.Length;
            foreach (var fh in frameHeaderList)
            {
                dataStartOffset += fh.Length;
            }
            List<int> frameOffsetList = new List<int>();

            // 统计帧数据的偏移
            for (int i = 0; i < frameDataList.Count; i++)
            {
                if (frameDataList[i].Length > 0)
                {
                    frameOffsetList.Add(dataStartOffset);
                    dataStartOffset += frameDataList[i].Length;
                }
                else
                {
                    frameOffsetList.Add(0);
                }
            }

            // 设置帧头中的数据位置偏移
            for (int i = 0; i < frameOffsetList.Count; i++)
            {
                using (MemoryStream ms = new MemoryStream(frameHeaderList[i]))
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Seek(frameHeaderList[i].Length - 4, SeekOrigin.Begin);
                        bw.Write((uint)frameOffsetList[i]);
                    }

                    frameHeaderList[i] = ms.ToArray();
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(shpHeader); // shp头

                    // 写入帧头
                    for (int i = 0; i < frameHeaderList.Count; i++)
                    {
                        bw.Write(frameHeaderList[i]);
                    }

                    // 写入帧数据
                    for (int i = 0; i < frameDataList.Count; i++)
                    {
                        if (frameDataList[i].Length > 0)
                        {
                            bw.Write(frameDataList[i]);
                        }
                    }
                }

                return ms.ToArray();
            }
        }

        internal static Dictionary<int, Bitmap> ShpToBitmaps(string filePath, int[] frameIndexArray, List<Ra2PaletteColor> palette)
        {
            ConcurrentDictionary<int, Bitmap> resultDic = new ConcurrentDictionary<int, Bitmap>();

            ShpHeader shpHeader = new ShpHeader();
            List<ShpFrame> frames = new List<ShpFrame>();

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    br.BaseStream.Seek(2, SeekOrigin.Begin);

                    shpHeader.Width = br.ReadUInt16();
                    shpHeader.Height = br.ReadUInt16();
                    shpHeader.FrameCount = br.ReadUInt16();

                    for (int i = 0; i < shpHeader.FrameCount; i++)
                    {
                        frames.Add(new ShpFrame()
                        {
                            OffsetX = br.ReadUInt16(),
                            OffsetY = br.ReadUInt16(),
                            Width = br.ReadUInt16(),
                            Height = br.ReadUInt16(),
                            Flag = br.ReadByte(),
                            Align = br.ReadBytes(3),
                            Color = br.ReadUInt32(),
                            Reserved = br.ReadUInt32(),
                            FrameOffset = br.ReadUInt32()
                        });
                    }

                    List<int> frameDataLengthList = new List<int>();
                    for (int i = 0; i < shpHeader.FrameCount; i++)
                    {
                        if (frames[i].FrameOffset == 0)
                        {
                            frameDataLengthList.Add(0);
                            continue;
                        }

                        int length = (int)(fs.Length - frames[i].FrameOffset);
                        int startOffset = (int)frames[i].FrameOffset;

                        // 不是最后一帧
                        if (i < shpHeader.FrameCount - 1)
                        {
                            for (int j = i + 1; j < shpHeader.FrameCount - 1; j++)
                            {
                                if (frames[j].FrameOffset != 0)
                                {
                                    length = (int)(frames[j].FrameOffset - startOffset);
                                    break;
                                }
                            }
                        }

                        frameDataLengthList.Add(length);
                    }

                    for (int i = 0; i < shpHeader.FrameCount; i++)
                    {
                        // 空白帧
                        if (frameDataLengthList[i] == 0)
                        {
                            frames[i].FrameData = new byte[] { };
                            continue;
                        }

                        br.BaseStream.Seek(frames[i].FrameOffset, SeekOrigin.Begin);
                        frames[i].FrameData = br.ReadBytes(frameDataLengthList[i]);
                    }
                }
            }

            Parallel.For(0, frameIndexArray.Length, frameIndexArrayIndex =>
            {
                int frameIndex = frameIndexArray[frameIndexArrayIndex];
                if (frameIndex > shpHeader.FrameCount - 1)
                {
                    return;
                }

                Bitmap bitmap = new Bitmap(shpHeader.Width, shpHeader.Height);
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                try
                {
                    // 空白帧
                    if (frames[frameIndex].FrameData.Length == 0)
                    {
                        unsafe
                        {
                            byte* ptr = (byte*)bmpData.Scan0;

                            int stride = bmpData.Stride;
                            int width = bitmap.Width;
                            int height = bitmap.Height;

                            for (int y = 0; y < height; y++)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    byte* pixel = ptr + (y * stride) + (x * 4);

                                    pixel[0] = (byte)(palette[0].B * 4);
                                    pixel[1] = (byte)(palette[0].G * 4);
                                    pixel[2] = (byte)(palette[0].R * 4);
                                    pixel[3] = 0;
                                }
                            }
                        }
                    }
                    else if ((frames[frameIndex].Flag & 0x2) != 0)
                    {
                        //Console.WriteLine("压缩了");
                        unsafe
                        {
                            byte* ptr = (byte*)bmpData.Scan0;

                            int stride = bmpData.Stride;
                            int width = bitmap.Width;
                            int height = bitmap.Height;

                            int byteOffset = 0;
                            for (int y = 0; y < height; y++)
                            {
                                int y1 = y - frames[frameIndex].OffsetY;
                                List<byte> line = new List<byte>();

                                if (y >= frames[frameIndex].OffsetY && y < frames[frameIndex].OffsetY + frames[frameIndex].Height)
                                {
                                    var count = BitConverter.ToUInt16(frames[frameIndex].FrameData, byteOffset);
                                    int index = byteOffset + 2;
                                    while (index < byteOffset + count)
                                    {
                                        if (frames[frameIndex].FrameData[index] == 0x00)
                                        {
                                            for (int _ = 0; _ < frames[frameIndex].FrameData[index + 1]; _++)
                                            {
                                                line.Add(frames[frameIndex].FrameData[index]);
                                            }
                                            index += 2;
                                        }
                                        else
                                        {
                                            line.Add(frames[frameIndex].FrameData[index]);
                                            index += 1;
                                        }
                                    }
                                    byteOffset += count;
                                }

                                for (int x = 0; x < width; x++)
                                {
                                    byte* pixel = ptr + (y * stride) + (x * 4);

                                    int x1 = x - frames[frameIndex].OffsetX;
                                    if (line.Count > 0 && x1 >= 0 && x1 < frames[frameIndex].Width)
                                    {
                                        int paletteIndex = line[x1];
                                        pixel[0] = (byte)(palette[paletteIndex].B * 4);
                                        pixel[1] = (byte)(palette[paletteIndex].G * 4);
                                        pixel[2] = (byte)(palette[paletteIndex].R * 4);
                                        if (paletteIndex == 0)
                                        {
                                            pixel[3] = 0;
                                        }
                                        else
                                        {
                                            pixel[3] = 255;
                                        }
                                    }
                                    else
                                    {
                                        pixel[0] = (byte)(palette[0].B * 4);
                                        pixel[1] = (byte)(palette[0].G * 4);
                                        pixel[2] = (byte)(palette[0].R * 4);
                                        pixel[3] = 0;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("未压缩");
                        unsafe
                        {
                            byte* ptr = (byte*)bmpData.Scan0;

                            int stride = bmpData.Stride;
                            int width = bitmap.Width;
                            int height = bitmap.Height;

                            int byteIndex = 0;
                            for (int y = 0; y < height; y++)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    byte* pixel = ptr + (y * stride) + (x * 4);

                                    int x1 = x - frames[frameIndex].OffsetX;
                                    int y1 = y - frames[frameIndex].OffsetY;
                                    if (x1 >= 0 && x1 < frames[frameIndex].Width && y1 >= 0 && y1 < frames[frameIndex].Height)
                                    {
                                        int paletteIndex = frames[frameIndex].FrameData[byteIndex];
                                        pixel[0] = (byte)(palette[paletteIndex].B * 4);
                                        pixel[1] = (byte)(palette[paletteIndex].G * 4);
                                        pixel[2] = (byte)(palette[paletteIndex].R * 4);
                                        if (paletteIndex == 0)
                                        {
                                            pixel[3] = 0;
                                        }
                                        else
                                        {
                                            pixel[3] = 255;
                                        }
                                        byteIndex++;
                                    }
                                    else
                                    {
                                        pixel[0] = (byte)(palette[0].B * 4);
                                        pixel[1] = (byte)(palette[0].G * 4);
                                        pixel[2] = (byte)(palette[0].R * 4);
                                        pixel[3] = 0;
                                    }
                                }
                            }
                        }
                    }

                    resultDic[frameIndex] = bitmap;
                }
                finally
                {
                    bitmap.UnlockBits(bmpData);
                }
            });

            return resultDic.ToDictionary(x => x.Key, x => x.Value);
        }

        internal static int GetShpFrameCount(string filePath)
        {
            ShpHeader shpHeader = new ShpHeader();

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    br.BaseStream.Seek(2, SeekOrigin.Begin);

                    shpHeader.Width = br.ReadUInt16();
                    shpHeader.Height = br.ReadUInt16();
                    shpHeader.FrameCount = br.ReadUInt16();
                }
            }

            return shpHeader.FrameCount;
        }
    }
}
