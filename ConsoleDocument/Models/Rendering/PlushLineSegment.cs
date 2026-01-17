using ConsoleDocumentSystem.Enums;

namespace ConsoleDocumentSystem.Models.Rendering
{
    public class PlushLineSegment
    {
        public string Text { get; set; }
        public PlushColor ForegroundColor { get; set; }
        public PlushColor BackgroundColor { get; set; }
        public PlushTextStyle Style { get; set; }
        public PlushLineSegment(string text, PlushColor foregroundColor = PlushColor.DefaultForeground, PlushTextStyle style = PlushTextStyle.None, PlushColor backgroundColor = PlushColor.DefaultBackground)
        {
            Text = text;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Style = style;
        }

        public PlushLineSegment(char text, PlushColor foregroundColor = PlushColor.DefaultForeground, PlushTextStyle style = PlushTextStyle.None, PlushColor backgroundColor = PlushColor.DefaultBackground)
        {
            Text = text.ToString();
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Style = style;
        }
    }
}
