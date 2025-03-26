using Ra2EasyShp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ra2EasyShp.Models
{
    internal class ImportPaletteModel
    {
        internal string Path { get; set; } = string.Empty;

        internal List<Ra2PaletteColor> Colors { get; set; }

    }
}
