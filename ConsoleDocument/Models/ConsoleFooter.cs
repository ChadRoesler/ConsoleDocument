using ConsoleDocumentSystem.Constants;
using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Models
{
    public class ConsoleFooter : IConsoleBlock
    {
        private readonly string PaddedText;
        public ConsoleFooter(string text)
        {
            Text = text;
            PaddedText = $" {Text} ";
            ConsolePlushBlock = new PlushBlock();
        }
        public PlushBlock ConsolePlushBlock { get; private set; }
        public string Text { get; set; }

        void IConsoleBlock.Render(ConsoleDocument doc)
        {
            ConsolePlushBlock.Clear();
            int Border = 2;
            int contentWidth = doc.Width - Border;
            int padLen = doc.Width - PaddedText.Length;
            int leftSpacing = padLen / 2 - 2;
            int rightSpacing = padLen - leftSpacing - 4; // -4 for the two Border chars and two inner Border chars
            if (leftSpacing < 0) leftSpacing = 0;
            if (rightSpacing < 0) rightSpacing = 0;

            // Top Border
            ConsolePlushBlock.Block.Add(new PlushLine
            {
                Line =
                [
                    new($"{ResourceStrings.BlockBorderDR}{new string(ResourceStrings.BlockBorderH, contentWidth)}{ResourceStrings.BlockBorderDL}", doc.BorderColor)
                ]
            });
            // Inner Top Border
            ConsolePlushBlock.Block.Add(new PlushLine
            {
                Line =
                [
                    new(ResourceStrings.BlockBorderV, doc.BorderColor),
                    new(new string(' ', leftSpacing)),
                    new(ResourceStrings.BlockBorderDR, doc.BorderColor),
                    new(new string(ResourceStrings.BlockBorderH, PaddedText.Length), doc.BorderColor),
                    new(ResourceStrings.BlockBorderDL, doc.BorderColor),
                    new(new string(' ', rightSpacing)),
                    new(ResourceStrings.BlockBorderV, doc.BorderColor)
                ]
            });
            // Middle line with text
            ConsolePlushBlock.Block.Add(new PlushLine
            {
                Line =
                [
                    new(ResourceStrings.BlockBorderV, doc.BorderColor),
                    new(new string(' ', leftSpacing)),
                    new(ResourceStrings.BlockBorderV, doc.BorderColor),
                    new(PaddedText, doc.TextColor, PlushTextStyle.Blink),
                    new(ResourceStrings.BlockBorderV, doc.BorderColor),
                    new(new string(' ', rightSpacing)),
                    new(ResourceStrings.BlockBorderV, doc.BorderColor)
                ]
            });
            // Inner Bottom Border
            ConsolePlushBlock.Block.Add(new PlushLine
            {
                Line =
                [
                    new(ResourceStrings.BlockBorderV, doc.BorderColor),
                    new(new string(' ', leftSpacing)),
                    new(ResourceStrings.BlockBorderUR, doc.BorderColor),
                    new(new string(ResourceStrings.BlockBorderH, PaddedText.Length), doc.BorderColor),
                    new(ResourceStrings.BlockBorderUL, doc.BorderColor),
                    new(new string(' ', rightSpacing)),
                    new(ResourceStrings.BlockBorderV, doc.BorderColor)
                ]
            });
            // Bottom Border
            ConsolePlushBlock.Block.Add(new PlushLine
            {
                Line =
                [
                    new($"{ResourceStrings.BlockBorderUR}{new string(ResourceStrings.BlockBorderH, contentWidth)}{ResourceStrings.BlockBorderUL}", doc.BorderColor)
                ]
            });
        }
    }
}
