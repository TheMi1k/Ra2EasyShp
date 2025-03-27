using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Ra2EasyShp.Data;
using Ra2EasyShp.Models;

namespace Ra2EasyShp.Funcs
{
    internal class ImageListManage
    {
        private static int GetGifFrameCount(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var gifDecoder = new GifBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                    return gifDecoder.Frames.Count;
                }
            }

            return 0;
        }

        internal static async Task OpenFile(string[] filesPath, int startIndex, bool isOverlay)
        {
            if (filesPath.Length == 0)
            {
                return;
            }

            List<ImageDataModel> insertList = new List<ImageDataModel>();
            List<Dictionary<int, ImageDataModel>> dicList = new List<Dictionary<int, ImageDataModel>>();
            for (int i = 0; i < filesPath.Length; i++)
            {
                dicList.Add(new Dictionary<int, ImageDataModel>());
            }

            object locker = new object();

            int fileCount = filesPath.Length;
            int sucCount = 0;
            GData.UIData.SetProgressUI(0, fileCount);

            await Task.Run(() =>
            {
                Parallel.For(0, filesPath.Length, taskIndex =>
                {
                    string filePath = filesPath[taskIndex];

                    if (string.IsNullOrEmpty(filePath))
                    {
                        return;
                    }

                    string extension = Path.GetExtension(filePath);

                    // PNG
                    if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        Dictionary<int, ImageDataModel> dic = new Dictionary<int, ImageDataModel>()
                        {
                            [0] = LoadPng(filePath, isOverlay)
                        };

                        lock (locker)
                        {
                            dicList[taskIndex] = dic;
                            sucCount++;
                        }

                        GData.UIData.SetProgressUI(sucCount, fileCount);
                    }
                    // GIF
                    else if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
                    {
                        int gifFrameCount = GetGifFrameCount(filePath);
                        if (gifFrameCount > 0)
                        {
                            List<int> frameIndexList = new List<int>();
                            for (int frameIndex = 0; frameIndex < gifFrameCount; frameIndex++)
                            {
                                frameIndexList.Add(frameIndex);
                            }

                            Dictionary<int, ImageDataModel> dic = LoadGif(filePath, frameIndexList.ToArray(), isOverlay);

                            lock (locker)
                            {
                                dicList[taskIndex] = dic;
                                sucCount++;
                            }

                            GData.UIData.SetProgressUI(sucCount, fileCount);
                        }
                    }
                    // SHP
                    else if (extension.Equals(".shp", StringComparison.OrdinalIgnoreCase))
                    {
                        int shpFrameCount = ShpManage.GetShpFrameCount(filePath);
                        if (shpFrameCount > 0)
                        {
                            List<int> frameIndexList = new List<int>();
                            for (int frameIndex = 0; frameIndex < shpFrameCount; frameIndex++)
                            {
                                frameIndexList.Add(frameIndex);
                            }

                            Dictionary<int, ImageDataModel> dic = LoadShp(filePath, frameIndexList.ToArray(), isOverlay, GData.NowPalette);

                            lock (locker)
                            {
                                dicList[taskIndex] = dic;
                                sucCount++;
                            }

                            GData.UIData.SetProgressUI(sucCount, fileCount);
                        }
                    }
                });
            });

            foreach (var item in dicList)
            {
                var frameIndexList = item.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
                foreach (var frameIndex in frameIndexList)
                {
                    insertList.Add(item[frameIndex]);
                }
            }

            if (insertList.Count == 0)
            {
                return;
            }

            if (GData.ImageData.Count < startIndex + insertList.Count)
            {
                int addCount = (startIndex + insertList.Count) - GData.ImageData.Count;
                for (int i = 0; i < addCount; i++)
                {
                    GData.ImageData.Add(new ImageDataModel());
                }
            }

            int dataIndex = 0;
            foreach (var item in GData.ImageData)
            {
                item.Index = dataIndex;
                dataIndex++;
            }

            int insertIndex = 0;
            for (int i = startIndex; i < startIndex + insertList.Count; i++)
            {
                if (isOverlay)
                {
                    GData.ImageData[i].OverlayImage.ClearItem();
                    GData.ImageData[i].OverlayImage = insertList[insertIndex].OverlayImage;
                }
                else
                {
                    GData.ImageData[i].EditImage.ClearItem();
                    GData.ImageData[i].EditImage = insertList[insertIndex].EditImage;
                }

                insertIndex++;
            }

            if (GData.ImageData.Count != 0)
            {
                if (LocalImageManage.GetBaseImageOriginalSize(0, out int originalWidth, out int originalHeight))
                {
                    GData.UIData.ResizeUI.ImageOriginalWidth = originalWidth;
                    GData.UIData.ResizeUI.ImageOriginalHeight = originalHeight;
                }

                if (LocalImageManage.GetBaseImageNowSize(0, out int nowWidth, out int nowHeight))
                {
                    GData.UIData.ResizeUI.ImageNowWidth = nowWidth;
                    GData.UIData.ResizeUI.ImageNowHeight = nowHeight;
                }
            }
        }

        internal static ImageDataModel LoadPng(string filePath, bool isOverlay)
        {
            ImageDataModel model = new ImageDataModel();

            if (isOverlay == false)
            {
                if (File.Exists(filePath))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (Bitmap bitmap = new Bitmap(fs))
                        {
                            //model.EditImage.Img = new Bitmap(fs);
                            //model.EditImage.OutImg = new Bitmap(fs);
                            model.EditImage.OutImgTempPath = GetPath.CreateImageTempPath();
                            LocalImageManage.SaveBitmapToTemp(bitmap, model.EditImage.OutImgTempPath);
                            model.EditImage.Name = Path.GetFileName(filePath);
                            model.EditImage.ImgPath = filePath;
                            //model.EditImage.ImgPath = GetPath.CreateImageTempPath();
                            //LocalImageManage.SaveBitmapToTemp(bitmap, model.EditImage.ImgPath);
                        }
                    }
                }
                else
                {
                    model.EditImage.Name = StringConst.ImageFileNotExist;
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        //model.OverlayImage.Img = new Bitmap(fs);
                        model.OverlayImage.Name = Path.GetFileName(filePath);
                        model.OverlayImage.ImgPath = filePath;
                    }
                }
                else
                {
                    model.OverlayImage.Name = StringConst.ImageFileNotExist;
                }
            }

            return model;
        }

        internal static Dictionary<int, ImageDataModel> LoadGif(string filePath, int[] frames, bool isOverlay)
        {
            Dictionary<int, ImageDataModel> dic = new Dictionary<int, ImageDataModel>();

            bool fileExist = File.Exists(filePath);

            foreach (var frameIndex in frames)
            {
                ImageDataModel model = new ImageDataModel();

                if (fileExist)
                {
                    bool isFrameExist = true;
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var image = System.Drawing.Image.FromStream(fs))
                        {
                            FrameDimension fd = new FrameDimension(image.FrameDimensionsList[0]);

                            if (frameIndex > image.GetFrameCount(fd) - 1)
                            {
                                isFrameExist = false;
                            }

                            if (isOverlay == false)
                            {
                                if (isFrameExist)
                                {
                                    image.SelectActiveFrame(fd, frameIndex);

                                    //model.EditImage.OutImg = new Bitmap(image);
                                    model.EditImage.OutImgTempPath = GetPath.CreateImageTempPath();
                                    LocalImageManage.SaveImageToTemp(image, model.EditImage.OutImgTempPath);

                                    model.EditImage.Name = $"[{frameIndex}]{Path.GetFileName(filePath)}";
                                    model.EditImage.ImgPath = filePath;
                                    model.EditImage.FileType = Enums.FileType.Gif;
                                    model.EditImage.Frame = frameIndex;

                                    model.EditImage.ImgTempPath = GetPath.CreateImageTempPath();
                                    LocalImageManage.SaveImageToTemp(image, model.EditImage.ImgTempPath);
                                }
                                else
                                {
                                    model.EditImage.Name = StringConst.ImageFileNotExist;
                                    model.EditImage.FileType = Enums.FileType.Gif;
                                    model.EditImage.Frame = frameIndex;
                                }
                            }
                            else
                            {
                                if (isFrameExist)
                                {
                                    image.SelectActiveFrame(fd, frameIndex);

                                    //model.OverlayImage.Img = new Bitmap(image);
                                    model.OverlayImage.ImgPath = filePath;

                                    model.EditImage.Name = $"[{frameIndex}]{Path.GetFileName(filePath)}";
                                    model.OverlayImage.FileType = Enums.FileType.Gif;
                                    model.OverlayImage.Frame = frameIndex;

                                    model.OverlayImage.ImgTempPath = GetPath.CreateImageTempPath();
                                    LocalImageManage.SaveImageToTemp(image, model.OverlayImage.ImgTempPath);
                                }
                                else
                                {
                                    model.OverlayImage.Name = StringConst.ImageFileNotExist;
                                    model.OverlayImage.FileType = Enums.FileType.Gif;
                                    model.OverlayImage.Frame = frameIndex;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (isOverlay == false)
                    {
                        model.EditImage.Name = StringConst.ImageFileNotExist;
                        model.EditImage.FileType = Enums.FileType.Gif;
                        model.EditImage.Frame = frameIndex;
                    }
                    else
                    {
                        model.OverlayImage.Name = StringConst.ImageFileNotExist;
                        model.OverlayImage.FileType = Enums.FileType.Gif;
                        model.OverlayImage.Frame = frameIndex;
                    }
                }

                dic.Add(frameIndex, model);
            }

            return dic;
        }

        internal static Dictionary<int, ImageDataModel> LoadShp(string filePath, int[] frames, bool isOverlay, List<Ra2PaletteColor> palette)
        {
            Dictionary<int, ImageDataModel> dic = new Dictionary<int, ImageDataModel>();

            bool fileExist = File.Exists(filePath);

            Dictionary<int, Bitmap> bitmapDic = null;
            if (fileExist)
            {
                bitmapDic = ShpManage.ShpToBitmaps(filePath, frames, palette);
            }

            foreach (var frameIndex in frames)
            {
                ImageDataModel model = new ImageDataModel();

                if (fileExist)
                {
                    bool isFrameExist = bitmapDic.ContainsKey(frameIndex) && bitmapDic[frameIndex] != null;

                    if (isOverlay == false)
                    {
                        if (isFrameExist)
                        {
                            //model.EditImage.Img = new Bitmap(bitmapDic[frameIndex]);
                            //model.EditImage.OutImg = new Bitmap(bitmapDic[frameIndex]);

                            model.EditImage.OutImgTempPath = GetPath.CreateImageTempPath();
                            LocalImageManage.SaveBitmapToTemp(bitmapDic[frameIndex], model.EditImage.OutImgTempPath);

                            model.EditImage.Name = $"[{frameIndex}]{Path.GetFileName(filePath)}";
                            model.EditImage.ImgPath = filePath;
                            model.EditImage.FileType = Enums.FileType.Shp;
                            model.EditImage.Frame = frameIndex;

                            model.EditImage.ImgTempPath = GetPath.CreateImageTempPath();
                            LocalImageManage.SaveBitmapToTemp(bitmapDic[frameIndex], model.EditImage.ImgTempPath);

                            bitmapDic[frameIndex].Dispose();
                        }
                        else
                        {
                            model.EditImage.Name = StringConst.ImageFileNotExist;
                            model.EditImage.FileType = Enums.FileType.Shp;
                            model.EditImage.Frame = frameIndex;
                        }
                    }
                    else
                    {
                        if (isFrameExist)
                        {
                            //model.OverlayImage.Img = new Bitmap(bitmapDic[frameIndex]);
                            model.EditImage.Name = $"[{frameIndex}]{Path.GetFileName(filePath)}";
                            model.OverlayImage.ImgPath = filePath;
                            model.OverlayImage.FileType = Enums.FileType.Shp;
                            model.OverlayImage.Frame = frameIndex;

                            model.OverlayImage.ImgTempPath = GetPath.CreateImageTempPath();
                            LocalImageManage.SaveBitmapToTemp(bitmapDic[frameIndex], model.OverlayImage.ImgTempPath);

                            bitmapDic[frameIndex].Dispose();
                        }
                        else
                        {
                            model.OverlayImage.Name = StringConst.ImageFileNotExist;
                            model.OverlayImage.FileType = Enums.FileType.Shp;
                            model.OverlayImage.Frame = frameIndex;
                        }
                    }
                }
                else
                {
                    if (isOverlay == false)
                    {
                        model.EditImage.Name = StringConst.ImageFileNotExist;
                        model.EditImage.FileType = Enums.FileType.Shp;
                        model.EditImage.Frame = frameIndex;
                    }
                    else
                    {
                        model.OverlayImage.Name = StringConst.ImageFileNotExist;
                        model.OverlayImage.FileType = Enums.FileType.Shp;
                        model.OverlayImage.Frame = frameIndex;
                    }
                }

                dic.Add(frameIndex, model);
            }

            return dic;
        }

        internal static void ClearLastEmptyItem()
        {
            if (GData.ImageData.Count != 0)
            {
                int count = GData.ImageData.Count - 1;
                for (int i = count; i >= 0; i--)
                {
                    if (GData.ImageData[i].EditImage.Name == "" && GData.ImageData[i].OverlayImage.Name == "")
                    {
                        GData.ImageData[i].EditImage.ClearItem();
                        GData.ImageData[i].OverlayImage.ClearItem();
                        GData.ImageData.RemoveAt(i);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 删除项目，空项保留
        /// </summary>
        /// <param name="infoList"></param>
        internal static void DeleteItem(List<DataGridCellInfo> infoList)
        {
            if (infoList.Count == 0)
            {
                return;
            }

            foreach (var cellInfo in infoList)
            {
                int columnIndex = cellInfo.Column.DisplayIndex;

                if (columnIndex == 0)
                {
                    var rowData = cellInfo.Item;
                    ImageDataModel item = rowData as ImageDataModel;

                    item.EditImage.ClearItem();
                    item.OverlayImage.ClearItem();
                }
                else if (columnIndex == 1)
                {
                    var rowData = cellInfo.Item;
                    ImageDataModel item = rowData as ImageDataModel;

                    item.EditImage.ClearItem();
                }
                else if (columnIndex == 2)
                {
                    var rowData = cellInfo.Item;
                    ImageDataModel item = rowData as ImageDataModel;

                    item.OverlayImage.ClearItem();
                }
            }

            ClearLastEmptyItem();

            GC.Collect();
        }

        /// <summary>
        /// 删除项目，下方数据上移
        /// </summary>
        /// <param name="infoList"></param>
        internal static void DeleteItemShift(List<DataGridCellInfo> infoList)
        {
            List<int> removeList1 = new List<int>();
            List<int> removeList2 = new List<int>();

            foreach (var cellInfo in infoList)
            {
                int columnIndex = cellInfo.Column.DisplayIndex;

                var rowData = cellInfo.Item;
                ImageDataModel item = rowData as ImageDataModel;

                if (columnIndex == 0)
                {
                    if (!removeList1.Contains(item.Index))
                    {
                        removeList1.Add(item.Index);
                    }
                    if (!removeList2.Contains(item.Index))
                    {
                        removeList2.Add(item.Index);
                    }
                }
                else if (columnIndex == 1)
                {
                    if (!removeList1.Contains(item.Index))
                    {
                        removeList1.Add(item.Index);
                    }
                }
                else if (columnIndex == 2)
                {
                    if (!removeList2.Contains(item.Index))
                    {
                        removeList2.Add(item.Index);
                    }
                }
            }

            removeList1.Sort();
            removeList1.Reverse();
            removeList2.Sort();
            removeList2.Reverse();

            foreach (int removeIndex in removeList1)
            {
                GData.ImageData[removeIndex].EditImage.ClearItem();
            }

            foreach (int removeIndex in removeList1)
            {
                for (int i = removeIndex; i < GData.ImageData.Count - 1; i++)
                {
                    GData.ImageData[i].EditImage = GData.ImageData[i + 1].EditImage;
                }

                GData.ImageData[GData.ImageData.Count - 1].EditImage = new EditImageModel();
            }

            foreach (int removeIndex in removeList2)
            {
                GData.ImageData[removeIndex].OverlayImage.ClearItem();
            }
            foreach (int removeIndex in removeList2)
            {
                for (int i = removeIndex; i < GData.ImageData.Count - 1; i++)
                {
                    GData.ImageData[i].OverlayImage = GData.ImageData[i + 1].OverlayImage;
                }

                GData.ImageData[GData.ImageData.Count - 1].OverlayImage = new OverlayImageModel();
            }
            
            ClearLastEmptyItem();

            // 重新设置index
            int indexNum = 0;
            foreach (var item in GData.ImageData)
            {
                item.Index = indexNum;
                indexNum++;
            }
        }

        internal static void InsertEmptyItem(Enums.ListName cell, int startIndex, int insertCount)
        {
            if (cell == Enums.ListName.空)
            {
                return;
            }

            for (int _ = 0; _ < insertCount; _++)
            {
                GData.ImageData.Insert(startIndex, new ImageDataModel());
            }

            if (cell == Enums.ListName.操作)
            {
                for (int _ = 0; _ < insertCount; _++)
                {
                    for (int i = startIndex; i < GData.ImageData.Count - 1; i++)
                    {
                        GData.ImageData[i].OverlayImage = GData.ImageData[i + 1].OverlayImage;
                    }
                    GData.ImageData[GData.ImageData.Count - 1].OverlayImage = new OverlayImageModel();
                }
            }
            else if (cell == Enums.ListName.叠加)
            {
                for (int _ = 0; _ < insertCount; _++)
                {
                    for (int i = startIndex; i < GData.ImageData.Count - 1; i++)
                    {
                        GData.ImageData[i].EditImage = GData.ImageData[i + 1].EditImage;
                    }
                    GData.ImageData[GData.ImageData.Count - 1].EditImage = new EditImageModel();
                }
            }

            ClearLastEmptyItem();

            // 重新设置index
            int indexNum = 0;
            foreach (var item in GData.ImageData)
            {
                item.Index = indexNum;
                indexNum++;
            }
        }

        // Clipboard

        private static List<EditImageModel> _editListClipboard = new List<EditImageModel>();
        private static List<OverlayImageModel> _overlayListClipboard = new List<OverlayImageModel>();

        internal static void CopyItem(List<DataGridCellInfo> infoList)
        {
            if (infoList.Count == 0)
            {
                return;
            }

            int columnIndexTemp = infoList[0].Column.DisplayIndex;
            foreach (var cellInfo in infoList)
            {
                if (cellInfo.Column.DisplayIndex != columnIndexTemp)
                {
                    throw new Exception("复制、粘贴只能在单个列表操作");
                }
            }

            foreach (var item in _editListClipboard)
            {
                item.ClearItem();
            }
            _editListClipboard.Clear();

            foreach (var item in _overlayListClipboard)
            {
                item.ClearItem();
            }
            _overlayListClipboard.Clear();

            foreach (var cellInfo in infoList)
            {
                int columnIndex = cellInfo.Column.DisplayIndex;

                var rowData = cellInfo.Item;
                ImageDataModel item = rowData as ImageDataModel;

                if (columnIndex == 0 || columnIndex == 1)
                {
                    _editListClipboard.Add(item.EditImage.Copy());
                }
                else if (columnIndex == 2)
                {
                    _overlayListClipboard.Add(item.OverlayImage.Copy());
                }
            }

            if (_editListClipboard.Count != 0)
            {
                foreach (var editClipboardItem in _editListClipboard)
                {
                    if (!string.IsNullOrEmpty(editClipboardItem.ImgTempPath))
                    {
                        string copyName = GetPath.CreateImageTempPath();
                        File.Copy(editClipboardItem.ImgTempPath, copyName);
                        editClipboardItem.ImgTempPath = copyName;
                    }

                    if (!string.IsNullOrEmpty(editClipboardItem.OutImgTempPath))
                    {
                        string copyName = GetPath.CreateImageTempPath();
                        File.Copy(editClipboardItem.OutImgTempPath, copyName);
                        editClipboardItem.OutImgTempPath = copyName;
                    }

                    if (!string.IsNullOrEmpty(editClipboardItem.ImgReMarginPath))
                    {
                        string copyName = GetPath.CreateImageTempPath();
                        File.Copy(editClipboardItem.ImgReMarginPath, copyName);
                        editClipboardItem.ImgReMarginPath = copyName;
                    }

                    if (!string.IsNullOrEmpty(editClipboardItem.ImgResizePath))
                    {
                        string copyName = GetPath.CreateImageTempPath();
                        File.Copy(editClipboardItem.ImgResizePath, copyName);
                        editClipboardItem.ImgResizePath = copyName;
                    }
                }
            }
            else if (_overlayListClipboard.Count != 0)
            {
                foreach (var overlayClipboardItem in _overlayListClipboard)
                {
                    if (!string.IsNullOrEmpty(overlayClipboardItem.ImgTempPath))
                    {
                        string copyName = GetPath.CreateImageTempPath();
                        File.Copy(overlayClipboardItem.ImgTempPath, copyName);
                        overlayClipboardItem.ImgTempPath = copyName;
                    }
                }
            }
        }

        internal static void PasteItem(Enums.ListName cell, int pasteIndex)
        {
            if (_editListClipboard.Count == 0 && _overlayListClipboard.Count == 0)
            {
                return;
            }

            int pasteList;
            if (cell == Enums.ListName.全部 || cell == Enums.ListName.操作)
            {
                pasteList = 1;
            }
            else if (cell == Enums.ListName.叠加)
            {
                pasteList = 2;
            }
            else
            {
                return;
            }

            int index;
            if (_editListClipboard.Count == 0)
            {
                index = 2;
            }
            else
            {
                index = 1;
            }

            if (pasteList != index)
            {
                throw new Exception("粘贴只能应用于复制源相同的列表");
            }
            
            if (index == 1)
            {
                foreach (var editClipboardItem in _editListClipboard)
                {
                    if (pasteIndex > GData.ImageData.Count - 1)
                    {
                        GData.ImageData.Add(new ImageDataModel());
                    }

                    GData.ImageData[pasteIndex].EditImage.ClearItem(); // 清理原先该位置的信息
                    GData.ImageData[pasteIndex].EditImage = editClipboardItem.Copy(); // 将剪切板内容复制到列表

                    if (!string.IsNullOrEmpty(editClipboardItem.ImgTempPath))
                    {
                        string copyName = GetPath.CreateImageTempPath();
                        File.Copy(editClipboardItem.ImgTempPath, copyName);
                        GData.ImageData[pasteIndex].EditImage.ImgTempPath = copyName;
                    }

                    if (!string.IsNullOrEmpty(editClipboardItem.OutImgTempPath))
                    {
                        string copyName = GetPath.CreateImageTempPath();
                        File.Copy(editClipboardItem.OutImgTempPath, copyName);
                        GData.ImageData[pasteIndex].EditImage.OutImgTempPath = copyName;
                    }

                    if (!string.IsNullOrEmpty(editClipboardItem.ImgReMarginPath))
                    {
                        string copyName = GetPath.CreateImageTempPath();
                        File.Copy(editClipboardItem.ImgReMarginPath, copyName);
                        GData.ImageData[pasteIndex].EditImage.ImgReMarginPath = copyName;
                    }

                    if (!string.IsNullOrEmpty(editClipboardItem.ImgResizePath))
                    {
                        string copyName = GetPath.CreateImageTempPath();
                        File.Copy(editClipboardItem.ImgResizePath, copyName);
                        GData.ImageData[pasteIndex].EditImage.ImgResizePath = copyName;
                    }

                    pasteIndex++;
                }
            }
            else
            {
                foreach (var overlayClipboardItem in _overlayListClipboard)
                {
                    if (pasteIndex > GData.ImageData.Count - 1)
                    {
                        GData.ImageData.Add(new ImageDataModel());
                    }
                    GData.ImageData[pasteIndex].OverlayImage.ClearItem();
                    GData.ImageData[pasteIndex].OverlayImage = overlayClipboardItem.Copy();

                    if (!string.IsNullOrEmpty(overlayClipboardItem.ImgTempPath))
                    {
                        string copyName = GetPath.CreateImageTempPath();
                        File.Copy(overlayClipboardItem.ImgTempPath, copyName);
                        GData.ImageData[pasteIndex].OverlayImage.ImgTempPath = copyName;
                    }

                    pasteIndex++;
                }
            }

            // 重新设置index
            int indexNum = 0;
            foreach (var item in GData.ImageData)
            {
                item.Index = indexNum;
                indexNum++;
            }
        }
    }
}
