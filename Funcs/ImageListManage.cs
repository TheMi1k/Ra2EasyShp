using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
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

            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var image = System.Drawing.Image.FromStream(fs))
                    {
                        FrameDimension fd = new FrameDimension(image.FrameDimensionsList[0]);
                        int frameCount = image.GetFrameCount(fd);

                        foreach (var frameIndex in frames)
                        {
                            ImageDataModel model = new ImageDataModel();

                            bool isFrameExist = (frameIndex <= frameCount - 1);

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

                                    model.OverlayImage.Name = $"[{frameIndex}]{Path.GetFileName(filePath)}";
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

                            dic.Add(frameIndex, model);
                        }
                    }
                }
            }
            else
            {
                foreach (var frameIndex in frames)
                {
                    ImageDataModel model = new ImageDataModel();

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

                    dic.Add(frameIndex, model);
                }
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
                            model.OverlayImage.Name = $"[{frameIndex}]{Path.GetFileName(filePath)}";
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

        internal static bool IsCanPaste()
        {
            if (_editListClipboard.Count > 0 || _overlayListClipboard.Count > 0)
            {
                return true;
            }

            return false;
        }

        private static ObservableCollection<ImageDataModel> DeepCopy(ObservableCollection<ImageDataModel> dataList)
        {
            var list = dataList.Select(item => new ImageDataModel
            {
                Index = item.Index,
                EditImage = item.EditImage.Copy(),
                OverlayImage = item.OverlayImage.Copy(),
            }).ToList();

            return new ObservableCollection<ImageDataModel>(list);
        }

        internal static void ClearLastEmptyItem()
        {
            if (GData.ImageData.Count != 0)
            {
                int listCount = GData.ImageData.Count - 1;
                for (int i = listCount; i >= 0; i--)
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

        internal static void ClearEditList()
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            Parallel.ForEach(GData.ImageData, item =>
            {
                item.EditImage.ClearItem();
                item.EditImage = new EditImageModel();
            });

            ClearLastEmptyItem();
        }

        internal static void ClearOverlayList()
        {
            if (GData.ImageData.Count == 0)
            {
                return;
            }

            Parallel.ForEach(GData.ImageData, item =>
            {
                item.OverlayImage.ClearItem();
                item.OverlayImage = new OverlayImageModel();
            });

            ClearLastEmptyItem();
        }

        /// <summary>
        /// 删除项目，空项保留
        /// </summary>
        /// <param name="infoList"></param>
        internal static void DeleteItem(int column, List<int> indexList)
        {
            if (indexList.Count == 0 || GData.ImageData.Count == 0)
            {
                return;
            }

            foreach (var index in indexList)
            {
                if (column == 1)
                {
                    GData.ImageData[index].EditImage.ClearItem();
                    GData.ImageData[index].EditImage = new EditImageModel();
                }
                else if (column == 2)
                {
                    GData.ImageData[index].OverlayImage.ClearItem();
                    GData.ImageData[index].OverlayImage = new OverlayImageModel();
                }
            }

            ClearLastEmptyItem();
        }

        /// <summary>
        /// 删除项目，下方数据上移
        /// </summary>
        /// <param name="infoList"></param>
        internal static void DeleteItemRemoveEmpty(int column, List<int> indexList)
        {
            if (indexList.Count == 0 || GData.ImageData.Count == 0)
            {
                return;
            }

            if (column == 1)
            {
                List<int> removeList = new List<int>();
                foreach (var index in indexList)
                {
                    if (!removeList.Contains(index))
                    {
                        removeList.Add(index);
                    }
                }

                removeList.Sort();
                removeList.Reverse();

                foreach (int removeIndex in removeList)
                {
                    GData.ImageData[removeIndex].EditImage.ClearItem();
                }
                foreach (int removeIndex in removeList)
                {
                    for (int i = removeIndex; i < GData.ImageData.Count - 1; i++)
                    {
                        GData.ImageData[i].EditImage = GData.ImageData[i + 1].EditImage;
                    }

                    GData.ImageData[GData.ImageData.Count - 1].EditImage = new EditImageModel();
                }
            }

            if (column == 2)
            {
                List<int> removeList = new List<int>();
                foreach (var index in indexList)
                {
                    if (!removeList.Contains(index))
                    {
                        removeList.Add(index);
                    }
                }

                removeList.Sort();
                removeList.Reverse();

                foreach (int removeIndex in removeList)
                {
                    GData.ImageData[removeIndex].OverlayImage.ClearItem();
                }
                foreach (int removeIndex in removeList)
                {
                    for (int i = removeIndex; i < GData.ImageData.Count - 1; i++)
                    {
                        GData.ImageData[i].OverlayImage = GData.ImageData[i + 1].OverlayImage;
                    }

                    GData.ImageData[GData.ImageData.Count - 1].OverlayImage = new OverlayImageModel();
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

        internal static void InsertEmptyItem(int column, int startIndex, int insertCount)
        {
            if (column < 0)
            {
                return;
            }

            for (int _ = 0; _ < insertCount; _++)
            {
                if (startIndex > GData.ImageData.Count - 1)
                {
                    GData.ImageData.Add(new ImageDataModel());
                    continue;
                }
                GData.ImageData.Insert(startIndex, new ImageDataModel());
            }

            if (column == 1) // 操作列表
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
            else if (column == 2) // 叠加列表
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

            //ClearLastEmptyItem();

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

        internal static void CopyItem(int column, List<int> indexList)
        {
            if (indexList.Count == 0 || GData.ImageData.Count == 0)
            {
                return;
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

            foreach (var index in indexList)
            {
                if (column == 1)
                {
                    _editListClipboard.Add(GData.ImageData[index].EditImage.Copy());
                }
                else if (column == 2)
                {
                    _overlayListClipboard.Add(GData.ImageData[index].OverlayImage.Copy());
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

        internal static void CutItem(int column, List<int> indexList, bool removeEmpty)
        {
            if (indexList.Count == 0 || GData.ImageData.Count == 0)
            {
                return;
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

            foreach (var index in indexList)
            {
                if (column == 1)
                {
                    _editListClipboard.Add(GData.ImageData[index].EditImage.Copy());
                }
                else if (column == 2)
                {
                    _overlayListClipboard.Add(GData.ImageData[index].OverlayImage.Copy());
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

                if (removeEmpty)
                {
                    DeleteItemRemoveEmpty(column, indexList);
                }
                else
                {
                    DeleteItem(column, indexList);
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

                if (removeEmpty)
                {
                    DeleteItemRemoveEmpty(column, indexList);
                }
                else
                {
                    DeleteItem(column, indexList);
                }
            }
        }

        internal static void PasteItem(int column, int pasteIndex)
        {
            if (_editListClipboard.Count == 0 && _overlayListClipboard.Count == 0)
            {
                return;
            }

            int pasteList;
            if (column == 0 || column == 1)
            {
                pasteList = 1;
            }
            else if (column == 2)
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

        internal enum PasteItemInsertMode
        {
            /// <summary>
            /// 上方
            /// </summary>
            Above,

            /// <summary>
            /// 下方
            /// </summary>
            Below
        }

        internal static void PasteItemInsert(int column, int pasteIndex, PasteItemInsertMode insertMode)
        {
            if (_editListClipboard.Count == 0 && _overlayListClipboard.Count == 0)
            {
                return;
            }

            int pasteList;
            if (column == 0 || column == 1)
            {
                pasteList = 1;
            }
            else if (column == 2)
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
                if (insertMode == PasteItemInsertMode.Below)
                {
                    pasteIndex += 1;
                }

                InsertEmptyItem(pasteList, pasteIndex, _editListClipboard.Count);

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
                if (insertMode == PasteItemInsertMode.Below)
                {
                    pasteIndex += 1;
                }

                InsertEmptyItem(pasteList, pasteIndex, _overlayListClipboard.Count);

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

        internal static Dictionary<int, List<int>> GetDataGirdSelectInfo(List<DataGridCellInfo> infoList)
        {
            Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>()
            {
                [0] = new List<int>(),
                [1] = new List<int>(),
                [2] = new List<int>(),
            };

            foreach (var cellInfo in infoList)
            {
                int column = cellInfo.Column.DisplayIndex;

                if (column > 2)
                {
                    continue;
                }

                var rowData = cellInfo.Item;
                ImageDataModel item = rowData as ImageDataModel;
                dic[column].Add(item.Index);
            }

            foreach (var kv in dic)
            {
                kv.Value.Sort();
            }

            return dic;
        }
    }
}
