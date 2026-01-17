namespace ConsoleDocumentSystem.ExtensionMethods
{
    internal static class PlushConsoleColorExtensions
    {
        internal static string ToHex(this ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => "#000000",
                ConsoleColor.DarkBlue => "#00008B",
                ConsoleColor.DarkGreen => "#006400",
                ConsoleColor.DarkCyan => "#008B8B",
                ConsoleColor.DarkRed => "#8B0000",
                ConsoleColor.DarkMagenta => "#8B008B",
                ConsoleColor.DarkYellow => "#B8860B",
                ConsoleColor.Gray => "#C0C0C0",
                ConsoleColor.DarkGray => "#808080",
                ConsoleColor.Blue => "#0000FF",
                ConsoleColor.Green => "#00FF00",
                ConsoleColor.Cyan => "#00FFFF",
                ConsoleColor.Red => "#FF0000",
                ConsoleColor.Magenta => "#FF00FF",
                ConsoleColor.Yellow => "#FFFF00",
                ConsoleColor.White => "#FFFFFF",
                _ => "#000000"
            };
        }
    }
}
