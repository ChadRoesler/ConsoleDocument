using ConsoleDocumentSystem.Constants;
using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;
using System.Data;

namespace ConsoleDocumentSystem.Helpers
{
    internal class PlushHelpers
    {
        internal static List<string> WrapText(string text, int maxWidth)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                result.Add(string.Empty);
                return result;
            }

            // Normalize line breaks
            var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            foreach (var raw in lines)
            {
                string t = raw;
                while (t.Length > maxWidth)
                {
                    // Try to find a space before maxWidth
                    int wrapPos = t.LastIndexOf(' ', maxWidth);
                    if (wrapPos <= 0)
                    {
                        // No space, hard split
                        wrapPos = maxWidth;
                    }

                    result.Add(t[..wrapPos].TrimEnd());
                    t = t[wrapPos..].TrimStart();
                }

                result.Add(t);
            }

            return result;
        }

        internal static PlushLine CreateBlockEnd(ConsoleDocument doc)
        {
            const int BorderSize = 2; // ║ ... ║
            int innerWidth = doc.Width - BorderSize;

            // Precompute the horizontal Border string to avoid repeated allocation
            string horizontal = innerWidth == 1
                ? ResourceStrings.GenericBorderH.ToString()
                : new string(ResourceStrings.GenericBorderH, innerWidth);

            return new PlushLine
            {
                Line =
                [
                    new PlushLineSegment(ResourceStrings.GenericBorderUR, doc.BorderColor, PlushTextStyle.None),
                    new PlushLineSegment(horizontal, doc.BorderColor, PlushTextStyle.None),
                    new PlushLineSegment(ResourceStrings.GenericBorderUL, doc.BorderColor, PlushTextStyle.None)
                ]
            };
        }
        internal static List<PlushLine> CreateTitle(ConsoleDocument doc, string title)
        {
            const int BorderSize = 2; // ║ ... ║
            const int leftPadding = 1; // space after Border
            const int glyphAndSpace = 4; // [+] and one space
            const int rightPadding = 2; // always 2 at right

            int available = doc.Width - BorderSize - leftPadding - glyphAndSpace - rightPadding;

            string shownTitle;
            if (title.Length > available)
            {
                shownTitle = title.Substring(0, available);
            }
            else
            {
                shownTitle = title;
            }

            int fillRight = doc.Width - BorderSize - leftPadding - glyphAndSpace - shownTitle.Length - rightPadding;
            if (fillRight < 0) fillRight = 0;

            // Precompute strings to avoid repeated allocations
            string horizontalLine = new(ResourceStrings.GenericBorderH, doc.Width - 2);
            string leftPad = leftPadding == 1 ? " " : new string(' ', leftPadding);
            string rightPad = fillRight + rightPadding == 1 ? " " : new string(' ', fillRight + rightPadding);

            return new List<PlushLine>(3)
            {
                new() {
                    Line =
                    [
                        new PlushLineSegment(ResourceStrings.GenericBorderDR, doc.BorderColor),
                        new PlushLineSegment(horizontalLine, doc.BorderColor),
                        new PlushLineSegment(ResourceStrings.GenericBorderDL, doc.BorderColor)
                    ]
                },
                new() {
                    Line =
                    [
                        new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor),
                        new PlushLineSegment(leftPad),
                        new PlushLineSegment(ResourceStrings.RootGlyph, doc.RootGlyphColor, PlushTextStyle.Bold),
                        new PlushLineSegment(" " + shownTitle, doc.TextColor, PlushTextStyle.Bold),
                        new PlushLineSegment(rightPad, doc.TextColor),
                        new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor)
                    ]
                }
            };
        }

        internal static List<int> DistributeColumnWidths(List<int> naturalWidths, int innerWidth)
        {
            var widths = new List<int>(naturalWidths);
            int currentSum = widths.Sum();

            if (currentSum > innerWidth)
            {
                // Shrink widest first until it fits
                while (currentSum > innerWidth)
                {
                    int maxIdx = widths.IndexOf(widths.Max());
                    if (widths[maxIdx] > 5) // minimum floor
                        widths[maxIdx]--;
                    currentSum = widths.Sum();
                }
            }
            else if (currentSum < innerWidth)
            {
                // Expand evenly until it fits
                int i = 0;
                while (currentSum < innerWidth)
                {
                    widths[i % widths.Count]++;
                    currentSum = widths.Sum();
                    i++;
                }
            }

            return widths;
        }

    }
}
