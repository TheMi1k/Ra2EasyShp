using System;
using System.Drawing;

namespace Ra2EasyShp.Funcs
{
    internal class ColorConvert
    {
        internal static void RGBtoHSB(Color color, out float hue, out float saturation, out float brightness)
        {
            RGBtoHSB(color.R, color.G, color.B, out hue, out saturation, out brightness);
        }

        internal static void RGBtoHSB(int colorR, int colorG, int colorB, out float hue, out float saturation, out float brightness)
        {
            float r = colorR / 255f;
            float g = colorG / 255f;
            float b = colorB / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            if (delta == 0)
            {
                hue = 0;
            }
            else if (max == r)
            {
                hue = 60 * (((g - b) / delta) % 6);
            }
            else if (max == g)
            {
                hue = 60 * (((b - r) / delta) + 2);
            }
            else
            {
                hue = 60 * (((r - g) / delta) + 4);
            }

            if (hue < 0) hue += 360;

            saturation = max == 0 ? 0 : (delta / max);

            brightness = max;
        }

        internal static Color HSBtoRGB(float hue, float saturation, float brightness)
        {
            float c = brightness * saturation;
            float x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            float m = brightness - c;

            float r = 0, g = 0, b = 0;

            if (hue >= 0 && hue < 60)
            {
                r = c; g = x; b = 0;
            }
            else if (hue >= 60 && hue < 120)
            {
                r = x; g = c; b = 0;
            }
            else if (hue >= 120 && hue < 180)
            {
                r = 0; g = c; b = x;
            }
            else if (hue >= 180 && hue < 240)
            {
                r = 0; g = x; b = c;
            }
            else if (hue >= 240 && hue < 300)
            {
                r = x; g = 0; b = c;
            }
            else if (hue >= 300 && hue < 360)
            {
                r = c; g = 0; b = x;
            }

            int red = (int)((r + m) * 255);
            int green = (int)((g + m) * 255);
            int blue = (int)((b + m) * 255);

            return Color.FromArgb(red, green, blue);
        }
    }
}
