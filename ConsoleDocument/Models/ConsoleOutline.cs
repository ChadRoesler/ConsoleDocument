using ConsoleDocumentSystem.Constants;
using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.Helpers;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Models
{
    public class ConsoleOutline : IConsoleBlock
    {
        public ConsoleOutline(string titleText)
        {
            TitleText = titleText;
            ConsoleNodes = [];
            ConsolePlushBlock = new PlushBlock();
        }

        public ConsoleOutline(string titleText, List<ConsoleNode> nodes)
        {
            TitleText = titleText;
            ConsoleNodes = nodes;
            ConsolePlushBlock = new PlushBlock();
        }
        public PlushBlock ConsolePlushBlock { get; private set; }
        public List<ConsoleNode> ConsoleNodes { get; set; }
        public string TitleText { get; set; }

        public void Render(ConsoleDocument doc)
        {
            int BorderSize = 2;
            int paddingRight = 2;
            int maxTextWidth = doc.Width - BorderSize - paddingRight;

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
            foreach (var node in ConsoleNodes)
            {
                RenderNode(node, doc, ConsolePlushBlock, 0, maxTextWidth);
            }
            ConsolePlushBlock.Block.Add(PlushHelpers.CreateBlockEnd(doc));
        }



        private static void RenderNode(ConsoleNode node, ConsoleDocument doc, PlushBlock plushBlock, int depth, int maxTextWidth)
        {
            string glyph;
            PlushColor glyphColor;
            if (depth % 2 == 1)
            {
                glyph = ResourceStrings.OutlineGlyphSub1;
                glyphColor = doc.OutlineColors[1];
            }
            else if (depth > 0)
            {
                glyph = ResourceStrings.OutlineGlyphSub2;
                glyphColor = doc.OutlineColors[2];
            }
            else
            {
                glyph = ResourceStrings.OutlineGlyph;
                glyphColor = doc.OutlineColors[0];
            }

            int indentSpaces = (depth * 3) + 1;
            int availableTextWidth = maxTextWidth - indentSpaces - 4;

            var wrappedLines = PlushHelpers.WrapText(node.Text, availableTextWidth);

            int padLen = maxTextWidth - indentSpaces - glyph.Length;
            for (int i = 0; i < wrappedLines.Count; i++)
            {
                var line = wrappedLines[i];
                var segments = new List<PlushLineSegment>(7);

                if (i == 0)
                {
                    segments.Add(new(ResourceStrings.GenericBorderV, doc.BorderColor));
                    segments.Add(new(new string(' ', indentSpaces), doc.TextColor, PlushTextStyle.None));
                    segments.Add(new(glyph, glyphColor, PlushTextStyle.None));
                    segments.Add(new(" ", doc.TextColor, PlushTextStyle.None));
                    segments.Add(new(line, doc.TextColor, PlushTextStyle.None));
                    segments.Add(new(new string(' ', padLen - line.Length + 1), doc.TextColor, PlushTextStyle.None));
                    segments.Add(new(ResourceStrings.GenericBorderV, doc.BorderColor));
                }
                else
                {
                    segments.Add(new(ResourceStrings.GenericBorderV, doc.BorderColor));
                    segments.Add(new(new string(' ', glyph.Length + indentSpaces + 1), doc.TextColor, PlushTextStyle.None));
                    segments.Add(new(line, doc.TextColor, PlushTextStyle.None));
                    segments.Add(new(new string(' ', padLen - line.Length + 1), doc.TextColor, PlushTextStyle.None));
                    segments.Add(new(ResourceStrings.GenericBorderV, doc.BorderColor));
                }

                plushBlock.Block.Add(new PlushLine { Line = segments });
            }

            if (node.ConsoleNodes != null && node.ConsoleNodes.Count > 0)
            {
                foreach (var child in node.ConsoleNodes)
                {
                    RenderNode(child, doc, plushBlock, depth + 1, maxTextWidth);
                }
            }
        }
    }
}
