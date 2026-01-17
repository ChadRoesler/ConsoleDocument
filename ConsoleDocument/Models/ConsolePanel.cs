using ConsoleDocumentSystem.Constants;
using ConsoleDocumentSystem.Helpers;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Rendering;


namespace ConsoleDocumentSystem.Models
{
    public class ConsolePanel : IConsoleBlock
    {
        public ConsolePanel(string titleText, string panelText)
        {
            TitleText = titleText;
            PanelText = panelText;
            ConsolePlushBlock = new PlushBlock();
        }

        public PlushBlock ConsolePlushBlock { get; private set; }
        public string TitleText { get; set; }
        public string PanelText { get; set; }

        public void Render(ConsoleDocument doc)
        {
            ConsolePlushBlock.Clear();
            ConsolePlushBlock.Block.AddRange(PlushHelpers.CreateTitle(doc, TitleText));
            ConsolePlushBlock.Block.Add(new PlushLine
            {
                Line =
                [
                    new(ResourceStrings.GenericBorderVR, doc.BorderColor),
                    new(new string(ResourceStrings.GenericBorderH, doc.Width - 2), doc.BorderColor),
                    new(ResourceStrings.GenericBorderVL, doc.BorderColor)
                ]
            });
            int maxTextWidth = doc.Width - 4; // 2 for Borders, 2 for padding
            var splitText = PlushHelpers.WrapText(PanelText, maxTextWidth);

            foreach (var line in splitText)
            {
                int padding = maxTextWidth - line.Length;
                if (padding < 0) padding = 0;
                ConsolePlushBlock.Block.Add(new PlushLine
                {
                    Line =
                    [
                        new(ResourceStrings.GenericBorderV, doc.BorderColor),
                        new(" " , doc.TextColor),
                        new(line, doc.TextColor),
                        new(new string(' ', doc.Width - line.Length - 3), doc.TextColor),
                        new(ResourceStrings.GenericBorderV, doc.BorderColor)
                    ]
                });
            }
            ConsolePlushBlock.Block.Add(PlushHelpers.CreateBlockEnd(doc));
        }
    }
}
