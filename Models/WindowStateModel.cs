using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ra2EasyShp.Models
{
    public class WindowStateModel
    {
        public double Width { get; set; } = -1;

        public double Height { get; set; } = -1;

        public double Left { get; set; } = -1;

        public double Top { get; set; } = -1;

        public string Language { get; set; } = string.Empty;

        public TextDecorationCollection TipsUnderline { get; set; } = TextDecorations.Underline;
    }
}
