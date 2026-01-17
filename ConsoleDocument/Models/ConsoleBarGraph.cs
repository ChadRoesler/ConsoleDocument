using ConsoleDocumentSystem.Constants;
using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.ExtensionMethods;
using ConsoleDocumentSystem.Helpers;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Models
{
    public class ConsoleBarGraph : IConsoleBlock
    {
        public ConsoleBarGraph(string titleText, List<ConsoleGraphSegment> graphSegments)
        {
            TitleText = titleText;
            GraphSegments = graphSegments;
            ConsolePlushBlock = new PlushBlock();
        }
        public PlushBlock ConsolePlushBlock { get; private set; }
        public List<ConsoleGraphSegment> GraphSegments { get; set; } = [];
        public string TitleText { get; set; }

        public void Render(ConsoleDocument doc)
        {
            ConsolePlushBlock.Clear();
            int nameColWidth = 8;
            int countStringWidth = 7;

            // Cache Texts and Values to avoid repeated extension method calls
            var texts = GraphSegments.Texts();
            var values = GraphSegments.Values();

            if (texts.Any())
                nameColWidth = Math.Max(8, texts.Max(k => k.Length) + 5);
            if (values.Any())
                countStringWidth = Math.Max(5, values.Max(v => v.ToString().Length) + 6);

            int maxBarLen = doc.Width - nameColWidth - countStringWidth;
            if (maxBarLen < 8) maxBarLen = 8;

            int maxValue = values.Any() ? values.Max() : 1;
            if (maxValue == 0) maxValue = 1;

            double unitsPerBar = (double)maxValue / maxBarLen;
            double barsPerUnit = (double)maxBarLen / maxValue;

            string ratioHeader;
            if (unitsPerBar >= 1)
                ratioHeader = (unitsPerBar % 1 != 0)
                    ? "(1:≈" + unitsPerBar.ToString("0.##") + ")"
                    : "(1:" + unitsPerBar.ToString("0") + ")";
            else
                ratioHeader = (barsPerUnit % 1 != 0)
                    ? "(≈" + barsPerUnit.ToString("0.##") + ":1)"
                    : "(" + barsPerUnit.ToString("0") + ":1)";

            string amountHeader = $"Amount {ratioHeader}";

            ConsolePlushBlock.Block.AddRange(PlushHelpers.CreateTitle(doc, TitleText));
            ConsolePlushBlock.Block.Add(new PlushLine
            {
                Line =
                [
                    new(ResourceStrings.GenericBorderVR, doc.BorderColor),
                    new(new string(ResourceStrings.GenericBorderH, nameColWidth - 2), doc.BorderColor),
                    new(ResourceStrings.GenericBorderDH, doc.BorderColor),
                    new(new string(ResourceStrings.GenericBorderH, maxBarLen + countStringWidth - 1), doc.BorderColor),
                    new(ResourceStrings.GenericBorderVL, doc.BorderColor)
                ]
            });
            ConsolePlushBlock.Block.Add(new PlushLine
            {
                Line =
                [
                    new(ResourceStrings.GenericBorderV, doc.BorderColor),
                    new(" " + "Type".PadRight(nameColWidth - 3), doc.TextColor),
                    new(ResourceStrings.GenericBorderV, doc.BorderColor),
                    new(" " + amountHeader, doc.TextColor),
                    new(new string(' ', maxBarLen + countStringWidth - amountHeader.Length - 2), doc.TextColor),
                    new(ResourceStrings.GenericBorderV, doc.BorderColor)
                ]
            });
            ConsolePlushBlock.Block.Add(new PlushLine
            {
                Line =
                [
                    new(ResourceStrings.GenericBorderVR, doc.BorderColor),
                    new(new string(ResourceStrings.GenericBorderH, nameColWidth - 2), doc.BorderColor),
                    new(ResourceStrings.GenericBorderVH, doc.BorderColor),
                    new(new string(ResourceStrings.GenericBorderH, maxBarLen + countStringWidth - 1), doc.BorderColor),
                    new(ResourceStrings.GenericBorderVL, doc.BorderColor)
                ]
            });

            int i = 0;
            foreach (var gs in GraphSegments)
            {
                int value = gs.Value;
                int barCount = unitsPerBar >= 1
                    ? (int)Math.Round(value / unitsPerBar)
                    : (int)Math.Round(value * barsPerUnit);

                barCount = Math.Max(0, Math.Min(maxBarLen, barCount));
                char barGlyph = doc.AlternateBarGraphColors && i % 2 == 1 ? ResourceStrings.BarMediumShade : ResourceStrings.BarFullShade;
                string bar = barCount > 0 ? new string(barGlyph, barCount) : "";
                string amountStr = $"({value})";

                // Use null-coalescing for nullable PlushColor (per signatures)
                ConsolePlushBlock.Block.Add(new PlushLine
                {
                    Line =
                    [
                        new(ResourceStrings.GenericBorderV, doc.BorderColor),
                        new(" ", doc.TextColor),
                        new(gs.Text, doc.TextColor, PlushTextStyle.Bold),
                        new(new string(' ', nameColWidth - gs.Text.Length - 3), doc.TextColor),
                        new(ResourceStrings.GenericBorderV, doc.BorderColor),
                        new(" ", doc.TextColor),
                        new(bar, gs.ForegroundColor ?? doc.BarGraphColor),
                        new(" ", doc.TextColor),
                        new(new string(' ', maxBarLen - bar.Length + (countStringWidth - amountStr.Length - 4)), doc.TextColor),
                        new(amountStr, doc.TextColor),
                        new(" ", doc.TextColor),
                        new(ResourceStrings.GenericBorderV, doc.BorderColor)
                    ]
                });

                i++;
            }

            ConsolePlushBlock.Block.Add(PlushHelpers.CreateBlockEnd(doc));
        }
    }
}
