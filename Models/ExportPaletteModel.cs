using Ra2EasyShp.Data;
using System.Collections.Generic;

namespace Ra2EasyShp.Models
{
    internal class ExportPaletteModel
    {
        internal List<Ra2PaletteColor> PalettePlayerColor { get; set; } = new List<Ra2PaletteColor>();

        internal Ra2PaletteColor[] PaletteHeaderColor { get; set; } = new Ra2PaletteColor[3];
    }
}
