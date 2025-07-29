namespace Ra2EasyShp.Data
{
    public class Enums
    {
        public enum OverlayMode
        {
            叠加在上,
            叠加在下
        }

        public enum CreatePalMode
        {
            主要颜色,
            主要颜色加补全,
            OctreeQuantizer
        }

        public enum ShpSaveMode
        {
            最佳文件大小,
            单位建筑,
            鼠标动画
        }

        public enum FileType
        {
            Png,
            Gif,
            Shp
        }

        internal enum PathType
        {
            Palette,
            PNG,
            SHP
        }

        public enum PreviewPlayerColor
        {
            无,
            黄,
            红,
            蓝,
            绿,
            橙,
            淡蓝,
            粉,
            紫,
            灰
        }

        public enum ColorDither
        {
            无,
            有所属色,
            无所属色
        }
    }
}
