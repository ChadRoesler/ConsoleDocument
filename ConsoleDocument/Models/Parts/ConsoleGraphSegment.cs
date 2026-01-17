using ConsoleDocumentSystem.Enums;

namespace ConsoleDocumentSystem.Models.Parts
{
    public class ConsoleGraphSegment
    {
        public ConsoleGraphSegment(string text, PlushColor foregroundColor, int value)
        {
            Text = text;
            ForegroundColor = foregroundColor;
            Value = value;
        }

        public ConsoleGraphSegment(string text, int value)
        {
            Text = text;
            Value = value; // Default value if not specified
        }
        public string Text { get; set; } = string.Empty;
        public PlushColor? ForegroundColor { get; set; }
        public int Value { get; set; } = 0;
    }
}
