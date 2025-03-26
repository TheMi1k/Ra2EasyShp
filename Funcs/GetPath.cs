using Ra2EasyShp.Data;
using System;
using System.IO;

namespace Ra2EasyShp.Funcs
{
    internal class GetPath
    {
        internal static string RunPath = AppDomain.CurrentDomain.BaseDirectory;

        internal static string GetExportImagePath()
        {
            //return $@"{RunPath}输出图像";
            return Path.Combine(RunPath, "输出图像");
        }

        internal static string GetExportPalPath()
        {
            //return $@"{RunPath}输出色盘";
            return Path.Combine(RunPath, "输出色盘");
        }

        internal static string GetExportShpPath()
        {
            //return $@"{RunPath}输出SHP";
            return Path.Combine(RunPath, "输出SHP");
        }

        internal static string CreateTempPath()
        {
            return Path.Combine(RunPath, $@"Data\Temp\{DateTime.Now.Day}{DateTime.Now.Hour}{Guid.NewGuid():N}");
        }

        internal static string CreateImageTempPath()
        {
            return Path.Combine(GData.TempPath, $@"{Guid.NewGuid():N}.png");
        }

        internal static string CreateSavePath(Enums.PathType pathType)
        {
            string timeStr = $"{DateTime.Now.Year}年{DateTime.Now.Month}月{DateTime.Now.Day}日{DateTime.Now.Hour}时{DateTime.Now.Minute}分{DateTime.Now.Second}秒";

            if (pathType == Enums.PathType.PNG)
            {
                return Path.Combine(GetExportImagePath(), timeStr);
            }
            if (pathType == Enums.PathType.Palette)
            {
                return Path.Combine(GetExportPalPath(), timeStr);
            }
            if (pathType == Enums.PathType.SHP)
            {
                return Path.Combine(GetExportShpPath(), timeStr);
            }

            return string.Empty;
        }
    }
}
