using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.ExtensionMethods;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Helpers
{
    public static class PlushAnsiHelper
    {
        private static bool? _ansiSupported;
        public static bool IsAnsiSupported()
        {
            if (_ansiSupported is not null) return _ansiSupported.Value;
            // Simple check for Windows 10+ and/or TERM env
            _ansiSupported = !Console.IsOutputRedirected &&
                             ((OperatingSystem.IsWindows() && Environment.OSVersion.Version.Major >= 10)
                              || Environment.GetEnvironmentVariable("TERM") is not null);
            return _ansiSupported.Value;
        }

        // Hex to RGB (from "#RRGGBB" or "RRGGBB")
        public static (int R, int G, int B) HexToRgb(string hex)
        {
            string h = hex.TrimStart('#');
            if (h.Length == 3)
            {
                var arr = new char[6];
                for (int i = 0; i < 3; i++)
                {
                    arr[i * 2] = h[i];
                    arr[i * 2 + 1] = h[i];
                }
                h = new string(arr);
            }
            int r = Convert.ToInt32(h.Substring(0, 2), 16);
            int g = Convert.ToInt32(h.Substring(2, 2), 16);
            int b = Convert.ToInt32(h.Substring(4, 2), 16);
            return (r, g, b);
        }

        // Convert hex to nearest ConsoleColor (fallback)
        public static ConsoleColor HexToConsoleColor(string hex)
        {
            var (r, g, b) = HexToRgb(hex);
            // 16 base console colors as RGB
            var colorMap = new (ConsoleColor cc, int R, int G, int B)[]
            {
                (ConsoleColor.Black,0,0,0),(ConsoleColor.DarkBlue,0,0,128),
                (ConsoleColor.DarkGreen,0,128,0),(ConsoleColor.DarkCyan,0,128,128),
                (ConsoleColor.DarkRed,128,0,0),(ConsoleColor.DarkMagenta,128,0,128),
                (ConsoleColor.DarkYellow,128,128,0),(ConsoleColor.Gray,192,192,192),
                (ConsoleColor.DarkGray,128,128,128),(ConsoleColor.Blue,0,0,255),
                (ConsoleColor.Green,0,255,0),(ConsoleColor.Cyan,0,255,255),
                (ConsoleColor.Red,255,0,0),(ConsoleColor.Magenta,255,0,255),
                (ConsoleColor.Yellow,255,255,0),(ConsoleColor.White,255,255,255)
            };
            int minDist = int.MaxValue;
            ConsoleColor best = ConsoleColor.Black;
            foreach (var x in colorMap)
            {
                int dist = (x.R - r) * (x.R - r) + (x.G - g) * (x.G - g) + (x.B - b) * (x.B - b);
                if (dist < minDist)
                {
                    minDist = dist;
                    best = x.cc;
                }
            }
            return best;
        }

        public static string StyleToAnsi(PlushTextStyle style)
        {
            if (style == PlushTextStyle.None) return "";
            var list = new List<string>(6);
            if (style.HasFlag(PlushTextStyle.Bold)) list.Add("1");
            if (style.HasFlag(PlushTextStyle.Dim)) list.Add("2");
            if (style.HasFlag(PlushTextStyle.Italic)) list.Add("3");
            if (style.HasFlag(PlushTextStyle.Underline)) list.Add("4");
            if (style.HasFlag(PlushTextStyle.Blink)) list.Add("5");
            if (style.HasFlag(PlushTextStyle.Inverse)) list.Add("7");
            if (style.HasFlag(PlushTextStyle.Strikethrough)) list.Add("9");
            return $"\x1b[{string.Join(';', list)}m";
        }

        public static string ColorToAnsi(string? fg, string? bg)
        {
            string fgAnsi = "", bgAnsi = "";
            if (!string.IsNullOrEmpty(fg))
            {
                var (r, g, b) = HexToRgb(fg);
                fgAnsi = $"\x1b[38;2;{r};{g};{b}m";
            }
            if (!string.IsNullOrEmpty(bg))
            {
                var (r, g, b) = HexToRgb(bg);
                bgAnsi = $"\x1b[48;2;{r};{g};{b}m";
            }
            return fgAnsi + bgAnsi;
        }

        public static void WriteSegment(PlushLineSegment seg)
        {
            if (IsAnsiSupported())
            {
                var styleAnsi = StyleToAnsi(seg.Style);

                var fgHex = seg.ForegroundColor.ToHex();
                var bgHex = seg.BackgroundColor.ToHex();

                var fgAnsi = !string.IsNullOrEmpty(fgHex)
                    ? $"\x1b[38;2;{HexToRgb(fgHex).R};{HexToRgb(fgHex).G};{HexToRgb(fgHex).B}m"
                    : "";

                var bgAnsi = !string.IsNullOrEmpty(bgHex)
                    ? $"\x1b[48;2;{HexToRgb(bgHex).R};{HexToRgb(bgHex).G};{HexToRgb(bgHex).B}m"
                    : ""; // <- skip if null

                Console.Write(styleAnsi + fgAnsi + bgAnsi);
                Console.Write(seg.Text);
                Console.Write("\x1b[0m");
            }
            else
            {
                var fgHex = seg.ForegroundColor.ToHex();
                var bgHex = seg.BackgroundColor.ToHex();

                if (!string.IsNullOrEmpty(fgHex))
                    Console.ForegroundColor = HexToConsoleColor(fgHex);

                if (!string.IsNullOrEmpty(bgHex))
                    Console.BackgroundColor = HexToConsoleColor(bgHex);
                // else leave Console.BackgroundColor unchanged

                Console.Write(seg.Text);
                Console.ResetColor();
            }
        }


        public static void Reset()
        {
            if (IsAnsiSupported())
                Console.Write("\x1b[0m");
            else
                Console.ResetColor();
        }
    }
}
