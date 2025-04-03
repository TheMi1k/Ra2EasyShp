using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ra2EasyShp.Models;

namespace Ra2EasyShp.Data
{
    public class GData
    {
        internal const string VERSION = "v1.1";

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

        internal static SaveConfigModel SaveConfigModel = new SaveConfigModel();

        public static UIDataModel UIData = new UIDataModel();

        internal static ExportPaletteModel PaletteConfig = new ExportPaletteModel();

        internal static List<ImportPaletteModel> ImportPaletteList = new List<ImportPaletteModel>();

        internal static string LastSavePalettePath;

        internal static List<Ra2PaletteColor> NowPalette = null;

        internal static int[,,] ColorToPaletteTable;

        internal static List<int> PaletteUnableIndex = new List<int>();

        internal static List<int> PaletteTransparentIndex = new List<int>();

        internal static int[,,] ColorToPaletteTransformTable;

        internal static Enums.ViewPlayerColor PlayerColorView = Enums.ViewPlayerColor.无;

        internal static readonly Dictionary<Enums.ViewPlayerColor, byte[]> PlayerColorBaseDic = new Dictionary<Enums.ViewPlayerColor, byte[]>()
        {
            [Enums.ViewPlayerColor.黄] = new byte[] { 43, 239, 255 },
            [Enums.ViewPlayerColor.红] = new byte[] { 0, 230, 255 },
            [Enums.ViewPlayerColor.蓝] = new byte[] { 153, 214, 212 },
            [Enums.ViewPlayerColor.绿] = new byte[] { 81, 200, 210 },
            [Enums.ViewPlayerColor.橙] = new byte[] { 25, 230, 255 },
            [Enums.ViewPlayerColor.淡蓝] = new byte[] { 131, 200, 230 },
            [Enums.ViewPlayerColor.粉] = new byte[] { 221, 102, 255 },
            [Enums.ViewPlayerColor.紫] = new byte[] { 201, 201, 189 },
            [Enums.ViewPlayerColor.灰] = new byte[] { 0, 0, 131 }
        };

        internal static Dictionary<Enums.ViewPlayerColor, List<byte[]>> PlayerColorDic = new Dictionary<Enums.ViewPlayerColor, List<byte[]>>()
        {
            [Enums.ViewPlayerColor.黄] = new List<byte[]>(),
            [Enums.ViewPlayerColor.红] = new List<byte[]>(),
            [Enums.ViewPlayerColor.蓝] = new List<byte[]>(),
            [Enums.ViewPlayerColor.绿] = new List<byte[]>(),
            [Enums.ViewPlayerColor.橙] = new List<byte[]>(),
            [Enums.ViewPlayerColor.淡蓝] = new List<byte[]>(),
            [Enums.ViewPlayerColor.粉] = new List<byte[]>(),
            [Enums.ViewPlayerColor.紫] = new List<byte[]>(),
            [Enums.ViewPlayerColor.灰] = new List<byte[]>()
        };

        internal static string TempPath = string.Empty;
    }
}
