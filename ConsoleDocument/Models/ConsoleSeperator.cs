using ConsoleDocumentSystem.Constants;
using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Models
{
    public class ConsoleSeperator : IConsoleBlock
    {
        private readonly string PaddedText;
        public ConsoleSeperator(string text = "")
        {
            Text = text;
            PaddedText = string.IsNullOrEmpty(text) ? "" : $" {Text} ";
            ConsolePlushBlock = new PlushBlock();
        }
        public PlushBlock ConsolePlushBlock { get; private set; }
        public string Text { get; set; } = string.Empty;

        public void Render(ConsoleDocument doc)
        {
            ConsolePlushBlock.Clear();
            int Border = 2;
            int contentWidth = doc.Width - Border;
            int textLen = PaddedText.Length;
            int padLen = doc.Width - textLen;
            int modulo = padLen % 2;
            int spacings = padLen / 2;
            int leftSpacing = spacings - 1;
            int rightSpacing = spacings + modulo - 1;
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

            if (!string.IsNullOrWhiteSpace(Text))
            {
                ConsolePlushBlock.Block.Add(new PlushLine
                {
                    Line =
                    [
                        new(ResourceStrings.BlockBorderV, doc.BorderColor),
                        new(new string(' ', leftSpacing)),
                        new(PaddedText, doc.TextColor, PlushTextStyle.Bold),
                        new(new string(' ', rightSpacing)),
                        new(ResourceStrings.BlockBorderV, doc.BorderColor)
                    ]
                });
            }

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
