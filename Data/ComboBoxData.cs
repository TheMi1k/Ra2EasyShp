using Ra2EasyShp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Ra2EasyShp.Data
{
    public class ComboBoxData
    {
        public void Update()
        {
            foreach (var item in OverlayMode)
            {
                item.Update();
            }

            foreach (var item in CreatePalMode)
            {
                item.Update();
            }

            foreach (var item in ShpSaveMode)
            {
                item.Update();
            }

            foreach (var item in PlayerColor)
            {
                item.Update();
            }

            foreach (var item in ColorDither)
            {
                item.Update();
            }
        }

        public ObservableCollection<ComboBoxDataModel<Enums.OverlayMode>> OverlayMode { get; set; } = new ObservableCollection<ComboBoxDataModel<Enums.OverlayMode>>()
        {
            new ComboBoxDataModel<Enums.OverlayMode>() { Key = "ComboBoxItem_Overlay_TopLayer", Value = Enums.OverlayMode.叠加在上 },
            new ComboBoxDataModel<Enums.OverlayMode>() { Key = "ComboBoxItem_Overlay_BottomLayer", Value = Enums.OverlayMode.叠加在下 }
        };

        public ObservableCollection<ComboBoxDataModel<Enums.CreatePalMode>> CreatePalMode { get; set; } = new ObservableCollection<ComboBoxDataModel<Enums.CreatePalMode>>()
        {
            new ComboBoxDataModel<Enums.CreatePalMode>() { Key = "ComboBoxItem_CreatePalette_DominantColor", Value = Enums.CreatePalMode.主要颜色 },
            new ComboBoxDataModel<Enums.CreatePalMode>() { Key = "ComboBoxItem_CreatePalette_DominantColorExtra", Value = Enums.CreatePalMode.主要颜色加补全 },
            new ComboBoxDataModel<Enums.CreatePalMode>() { Key = "ComboBoxItem_CreatePalette_OctreeQuantizer", Value = Enums.CreatePalMode.OctreeQuantizer }
        };

        public ObservableCollection<ComboBoxDataModel<Enums.ShpSaveMode>> ShpSaveMode { get; set; } = new ObservableCollection<ComboBoxDataModel<Enums.ShpSaveMode>>()
        {
            new ComboBoxDataModel<Enums.ShpSaveMode>() { Key = "ComboBoxItem_CreateShp_SaveMode_BetterFileSize", Value = Enums.ShpSaveMode.最佳文件大小 },
            new ComboBoxDataModel<Enums.ShpSaveMode>() { Key = "ComboBoxItem_CreateShp_SaveMode_ForUnitBuilding", Value = Enums.ShpSaveMode.单位建筑 },
            new ComboBoxDataModel<Enums.ShpSaveMode>() { Key = "ComboBoxItem_CreateShp_SaveMode_ForMouseAnimation", Value = Enums.ShpSaveMode.鼠标动画 }
        };

        public ObservableCollection<ComboBoxDataModel<Enums.PreviewPlayerColor>> PlayerColor { get; set; } = new ObservableCollection<ComboBoxDataModel<Enums.PreviewPlayerColor>>()
        {
            new ComboBoxDataModel<Enums.PreviewPlayerColor>() { Key = "ComboboxItem_PlayerColor_None", Value = Enums.PreviewPlayerColor.无 },
            new ComboBoxDataModel<Enums.PreviewPlayerColor>() { Key = "ComboboxItem_PlayerColor_Yellow", Value = Enums.PreviewPlayerColor.黄 },
            new ComboBoxDataModel<Enums.PreviewPlayerColor>() { Key = "ComboboxItem_PlayerColor_Red", Value = Enums.PreviewPlayerColor.红 },
            new ComboBoxDataModel<Enums.PreviewPlayerColor>() { Key = "ComboboxItem_PlayerColor_Blue", Value = Enums.PreviewPlayerColor.蓝 },
            new ComboBoxDataModel<Enums.PreviewPlayerColor>() { Key = "ComboboxItem_PlayerColor_Green", Value = Enums.PreviewPlayerColor.绿 },
            new ComboBoxDataModel<Enums.PreviewPlayerColor>() { Key = "ComboboxItem_PlayerColor_Orange", Value = Enums.PreviewPlayerColor.橙 },
            new ComboBoxDataModel<Enums.PreviewPlayerColor>() { Key = "ComboboxItem_PlayerColor_SkyBlue", Value = Enums.PreviewPlayerColor.淡蓝 },
            new ComboBoxDataModel<Enums.PreviewPlayerColor>() { Key = "ComboboxItem_PlayerColor_Pink", Value = Enums.PreviewPlayerColor.粉 },
            new ComboBoxDataModel<Enums.PreviewPlayerColor>() { Key = "ComboboxItem_PlayerColor_Purple", Value = Enums.PreviewPlayerColor.紫 },
            new ComboBoxDataModel<Enums.PreviewPlayerColor>() { Key = "ComboboxItem_PlayerColor_Grey", Value = Enums.PreviewPlayerColor.灰 }
        };

        public ObservableCollection<ComboBoxDataModel<Enums.ColorDither>> ColorDither { get; set; } = new ObservableCollection<ComboBoxDataModel<Enums.ColorDither>>()
        {
            new ComboBoxDataModel<Enums.ColorDither>() { Key = "ComboboxItem_ColorDither_None", Value = Enums.ColorDither.无 },
            new ComboBoxDataModel<Enums.ColorDither>() { Key = "ComboboxItem_ColorDither_PlayerColor", Value = Enums.ColorDither.有所属色 },
            new ComboBoxDataModel<Enums.ColorDither>() { Key = "ComboboxItem_PlayerColor_NotPlayerColor", Value = Enums.ColorDither.无所属色 }
        };
    }
}
