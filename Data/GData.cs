using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Ra2EasyShp.Funcs;
using Ra2EasyShp.Models;

namespace Ra2EasyShp.Data
{
    public class GData
    {
        internal const string VERSION = "v1.5";

        internal static int[,] BayerMatrix = {
            {  0, 32,  8, 40,  2, 34, 10, 42 },
            { 48, 16, 56, 24, 50, 18, 58, 26 },
            { 12, 44,  4, 36, 14, 46,  6, 38 },
            { 60, 28, 52, 20, 62, 30, 54, 22 },
            {  3, 35, 11, 43,  1, 33,  9, 41 },
            { 51, 19, 59, 27, 49, 17, 57, 25 },
            { 15, 47,  7, 39, 13, 45,  5, 37 },
            { 63, 31, 55, 23, 61, 29, 53, 21 }
        };

        internal const int MatrixSize = 8;

        internal static readonly object Locker_changeAllImg = new object();

        //internal static ObservableCollection<ImageInfoModel> ListViewData = new ObservableCollection<ImageInfoModel>();

        internal static ObservableCollection<ImageDataModel> ImageData = new ObservableCollection<ImageDataModel>();

        public static UIDataModel UIData = new UIDataModel();

        internal static ExportPaletteModel PaletteConfig = new ExportPaletteModel();

        internal static List<ImportPaletteModel> ImportPaletteList = new List<ImportPaletteModel>();

        internal static string LastSavePalettePath;

        internal static List<Ra2PaletteColor> NowPalette = null;

        internal static int[,,] ColorToPaletteTable;

        internal static List<int> PaletteUnableIndex = new List<int>();

        internal static List<int> PaletteTransparentIndex = new List<int>();

        internal static int[,,] ColorToPaletteTransformTable;

        internal static Enums.PreviewPlayerColor PlayerColorView = Enums.PreviewPlayerColor.无;

        internal static readonly Dictionary<Enums.PreviewPlayerColor, byte[]> PlayerColorBaseDic = new Dictionary<Enums.PreviewPlayerColor, byte[]>()
        {
            [Enums.PreviewPlayerColor.黄] = new byte[] { 43, 239, 255 },
            [Enums.PreviewPlayerColor.红] = new byte[] { 0, 230, 255 },
            [Enums.PreviewPlayerColor.蓝] = new byte[] { 153, 214, 212 },
            [Enums.PreviewPlayerColor.绿] = new byte[] { 81, 200, 210 },
            [Enums.PreviewPlayerColor.橙] = new byte[] { 25, 230, 255 },
            [Enums.PreviewPlayerColor.淡蓝] = new byte[] { 131, 200, 230 },
            [Enums.PreviewPlayerColor.粉] = new byte[] { 221, 102, 255 },
            [Enums.PreviewPlayerColor.紫] = new byte[] { 201, 201, 189 },
            [Enums.PreviewPlayerColor.灰] = new byte[] { 0, 0, 131 }
        };

        internal static Dictionary<Enums.PreviewPlayerColor, List<byte[]>> PlayerColorDic = new Dictionary<Enums.PreviewPlayerColor, List<byte[]>>()
        {
            [Enums.PreviewPlayerColor.黄] = new List<byte[]>(),
            [Enums.PreviewPlayerColor.红] = new List<byte[]>(),
            [Enums.PreviewPlayerColor.蓝] = new List<byte[]>(),
            [Enums.PreviewPlayerColor.绿] = new List<byte[]>(),
            [Enums.PreviewPlayerColor.橙] = new List<byte[]>(),
            [Enums.PreviewPlayerColor.淡蓝] = new List<byte[]>(),
            [Enums.PreviewPlayerColor.粉] = new List<byte[]>(),
            [Enums.PreviewPlayerColor.紫] = new List<byte[]>(),
            [Enums.PreviewPlayerColor.灰] = new List<byte[]>()
        };

        internal static string TempPath = string.Empty;

        internal static readonly string TempPublicPath = GetPath.TempPublickPath();
    }
}
