using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
                    const double delta = 6.0 / 29.0;
                    var fy = (entry.Values[0] * 100 + 16) / 116;
                    var fx = fy + entry.Values[1] / 500;
                    var fz = fy - entry.Values[2] / 200;
                    var y = fy > delta ? Yn * Math.Pow(fy, 3) : (fy - 16 / 116) * 3 * Math.Pow(delta, 2);
                    var x = fx > delta ? Xn * Math.Pow(fx, 3) : (fx - 16 / 116) * 3 * Math.Pow(delta, 2);
                    var z = fz > delta ? Zn * Math.Pow(fz, 3) : (fz - 16 / 116) * 3 * Math.Pow(delta, 2);
                    // ↑ それぞれに Xn, Yn, Zn を掛けなくていいの？
                    var r = 3.240479 * x - 1.53715 * y - 0.498535 * z;
                    var g = -0.969256 * x + 1.875991 * y + 0.041556 * z;
                    var b = 0.055648 * x - 0.204043 * y + 1.057311 * z;
                    return Color.FromScRgb(1, (float)r, (float)g, (float)b);
                default:
                    return Color.FromRgb((byte)(1 - entry.Values[0]), (byte)(1 - entry.Values[0]), (byte)(1 - entry.Values[0])); //逆だったら怖い
            }
        }

        public static string ToCss(this Color color)
        {
            return color.A == 255
                ? "#" + BitConverter.ToString(new[] { color.R, color.G, color.B }).Replace("-", "")
                : string.Format("rbga({0}, {1}, {2}, {3})", color.R, color.G, color.B, color.A / 255f);
        }

        public static string ToCss(this Brush brush)
        {
            var solid = brush as SolidColorBrush;
            if (solid != null)
                return solid.Color.ToCss();
            var linear = brush as LinearGradientBrush;
            if (linear != null)
                return string.Format("linear-gradient({0})", string.Join(", ", linear.GradientStops.Select(s => s.Color)));

            throw new ArgumentException("対応していない brush です。");
        }

        public static Brush FromCss(string color)
        {
            color = color.Trim();
            
            // Hex
            if (color.StartsWith("#"))
            {
                color = color.Substring(1);
                if (color.Length == 3)
                    color = string.Join("", color.SelectMany(c => Enumerable.Repeat(c, 2)));
                return new SolidColorBrush(Color.FromRgb(
                    byte.Parse(color.Substring(0, 2), NumberStyles.HexNumber),
                    byte.Parse(color.Substring(2, 2), NumberStyles.HexNumber),
                    byte.Parse(color.Substring(4, 2), NumberStyles.HexNumber)
                ));
            }

            // Color name
            var prop = typeof(Brushes).GetProperty(color, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (prop != null) return (SolidColorBrush)prop.GetValue(null);

            // rgb()
            var match = Regex.Match(color, @"^rgb\s*\(\s*(?<r>\d+)\s*,\s*(?<g>\d+)\s*,\s*(?<b>\d+)\s*\)$", RegexOptions.IgnoreCase);
            if (match.Success)
                return new SolidColorBrush(Color.FromRgb(
                    byte.Parse(match.Groups["r"].Value),
                    byte.Parse(match.Groups["g"].Value),
                    byte.Parse(match.Groups["b"].Value)
                ));
            match = Regex.Match(color, @"^rgb\s*\(\s*(?<r>[\d\.]+)%\s*,\s*(?<g>[\d\.]+)%\s*,\s*(?<b>[\d\.]+)%\s*\)$", RegexOptions.IgnoreCase);
            if (match.Success)
                return new SolidColorBrush(Color.FromRgb(
                    (byte)(float.Parse(match.Groups["r"].Value) / 100 * 255),
                    (byte)(float.Parse(match.Groups["g"].Value) / 100 * 255),
                    (byte)(float.Parse(match.Groups["b"].Value) / 100 * 255)
                ));

            // rgba()
            match = Regex.Match(color, @"^rgba\s*\(\s*(?<r>\d+)\s*,\s*(?<g>\d+)\s*,\s*(?<b>\d+)\s*,\s*(?<a>[\d\.]+)\s*\)$", RegexOptions.IgnoreCase);
            if (match.Success)
                return new SolidColorBrush(Color.FromArgb(
                    (byte)(float.Parse(match.Groups["a"].Value) * 255),
                    byte.Parse(match.Groups["r"].Value),
                    byte.Parse(match.Groups["g"].Value),
                    byte.Parse(match.Groups["b"].Value)
                ));
            match = Regex.Match(color, @"^rgba\s*\(\s*(?<r>[\d\.]+)%\s*,\s*(?<g>[\d\.]+)%\s*,\s*(?<b>[\d\.]+)%\s*,\s*(?<a>[\d\.]+)\s*\)$", RegexOptions.IgnoreCase);
            if (match.Success)
                return new SolidColorBrush(Color.FromArgb(
                    (byte)(float.Parse(match.Groups["a"].Value) * 255),
                    (byte)(float.Parse(match.Groups["r"].Value) / 100 * 255),
                    (byte)(float.Parse(match.Groups["g"].Value) / 100 * 255),
                    (byte)(float.Parse(match.Groups["b"].Value) / 100 * 255)
                ));

            // linear-gradient()
            match = Regex.Match(color, @"^linear-gradient\s*\(\s*(.+)\s*\)$", RegexOptions.IgnoreCase);
            if(match.Success)
            {
                var colors = match.Groups[1].Value.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => (FromCss(s) as SolidColorBrush).Color)
                    .ToArray();
                var n = 1.0 / colors.Length;
                return new LinearGradientBrush(new GradientStopCollection(colors.Select((c, i) => new GradientStop(c, n * i))), 90);
            }

            // radial-gradient() なんて知らない

            throw new ArgumentException("対応していないフォーマットです。");
        }
    }
}
