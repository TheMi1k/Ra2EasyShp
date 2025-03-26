using System;
using System.Drawing;

namespace Ra2EasyShp.Data
{
    public struct Ra2PaletteColor
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        private Ra2PaletteColor(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public static Ra2PaletteColor FromColor(Color color)
        {
            return FromArgb(color.A, color.R, color.G, color.B);
        }

        public static Ra2PaletteColor FromArgb(byte a, byte r, byte g, byte b)
        {
            r = (byte)Math.Floor(r / 4f);
            g = (byte)Math.Floor(g / 4f);
            b = (byte)Math.Floor(b / 4f);

            return new Ra2PaletteColor(a, r, g, b);
        }

        public static Ra2PaletteColor FromPaletteColor(byte r, byte g, byte b)
        {
            return new Ra2PaletteColor(255, r, g, b);
        }
    }
}
