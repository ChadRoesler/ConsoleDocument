using ConsoleDocumentSystem.ExtensionMethods;
using System.Text;

namespace ConsoleDocumentSystem.Helpers
{
    public static class PlushGlobalPalette
    {
        public static ConsoleColor DefaultForeground { get; set; } = Console.ForegroundColor;
        public static ConsoleColor? DefaultBackground { get; set; } = null; // null = transparent

        public static string DefaultForegroundHex => DefaultForeground.ToHex();
        public static string? DefaultBackgroundHex => DefaultBackground?.ToHex();

        public static void SetDefaults(ConsoleColor fg, ConsoleColor bg)
        {
            Console.OutputEncoding = Encoding.UTF8;
            DefaultForeground = fg;
            DefaultBackground = bg;
        }
        public static void SetDefaults()
        {
            Console.OutputEncoding = Encoding.UTF8;
        }

        public static void ClearBackground() => DefaultBackground = null; // back to transparent mode
    }

}
