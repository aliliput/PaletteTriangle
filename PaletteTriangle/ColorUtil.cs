using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using PaletteTriangle.AdobeSwatchExchange;

namespace PaletteTriangle
{
    public static class ColorUtil
    {
        public static Color ToColor(this ColorEntry entry)
        {
            switch (entry.Model)
            {
                case ColorModel.CMYK:
                    // http://lab.till-daylight.org/colors/ColorConversion.Class.phps
                    return Color.FromRgb(
                        (byte)((1 - Math.Min(1, entry.Values[0] * (1 - entry.Values[3]) + entry.Values[3])) * 255),
                        (byte)((1 - Math.Min(1, entry.Values[1] * (1 - entry.Values[3]) + entry.Values[3])) * 255),
                        (byte)((1 - Math.Min(1, entry.Values[2] * (1 - entry.Values[3]) + entry.Values[3])) * 255)
                    );
                case ColorModel.RGB:
                    return Color.FromRgb((byte)(entry.Values[0] * 255), (byte)(entry.Values[1] * 255), (byte)(entry.Values[2] * 255));
                case ColorModel.LAB:
                    // http://komozo.blogspot.jp/2011/04/rgbcielab.html
                    const double Xn = 0.950456;
                    const double Yn = 1.0;
                    const double Zn = 1.088754;
                    const double delta = 6 / 29;
                    var fy = (entry.Values[0] * 100 + 16) / 116;
                    var fx = fy + entry.Values[1] / 500;
                    var fz = fy - entry.Values[2] / 200;
                    var y = fy > delta ? Yn * Math.Pow(fy, 3) : (fy - 16 / 116) * 3 * Math.Pow(delta, 2);
                    var x = fx > delta ? Xn * Math.Pow(fx, 3) : (fx - 16 / 116) * 3 * Math.Pow(delta, 2);
                    var z = fz > delta ? Zn * Math.Pow(fz, 3) : (fz - 16 / 116) * 3 * Math.Pow(delta, 2);
                    var r = 3.240479 * x - 1.53715 * y - 0.498535 * z;
                    var g = -0.969256 * x + 1.875991 * y + 0.041556 * z;
                    var b = 0.055648 * x - 0.204043 * y + 1.057311 * z;
                    return Color.FromScRgb(1, (float)r, (float)g, (float)b);
                default:
                    return Color.FromRgb((byte)(1 - entry.Values[0]), (byte)(1 - entry.Values[0]), (byte)(1 - entry.Values[0])); //逆だったら怖い
            }
        }

        public static string ToHex(this Color color)
        {
            return BitConverter.ToString(new[] { color.R, color.G, color.B }).Replace("-", "");
        }
    }
}
