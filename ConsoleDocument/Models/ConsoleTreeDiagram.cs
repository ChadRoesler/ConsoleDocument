using ConsoleDocumentSystem.Constants;
using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.Helpers;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Models
{
    public class ConsoleTreeDiagram : IConsoleBlock
    {
        public ConsoleTreeDiagram(string titleText, ConsoleNode node)
        {
            TitleText = titleText;
            RootNode = node;
            ConsolePlushBlock = new PlushBlock();
        }
        public PlushBlock ConsolePlushBlock { get; private set; }
        public ConsoleNode RootNode { get; set; }
        public string TitleText { get; set; }

        // Main Render method for ConsoleTreeDiagram
        private static void RenderChildNode(ConsoleNode node, PlushBlock plushBlock, ConsoleDocument doc, List<bool> ancestorHasSibling, bool isLast)
        {
            // Build prefix efficiently
            int ancestorCount = ancestorHasSibling.Count;
            int prefixLen = 2 + ancestorCount * 3;
            char[] prefixArr = new char[prefixLen];
            prefixArr[0] = ' ';
            prefixArr[1] = ' ';
            for (int i = 0; i < ancestorCount; i++)
            {
                if (ancestorHasSibling[i])
                {
                    prefixArr[2 + i * 3] = ResourceStrings.TreeNodeVL;
                    prefixArr[2 + i * 3 + 1] = ' ';
                    prefixArr[2 + i * 3 + 2] = ' ';
                }
                else
                {
                    prefixArr[2 + i * 3] = ' ';
                    prefixArr[2 + i * 3 + 1] = ' ';
                    prefixArr[2 + i * 3 + 2] = ' ';
                }
            }
            string prefix = new(prefixArr);

            string branch = isLast ? ResourceStrings.TreeNodeEndGlyph : ResourceStrings.TreeNodeMidGlyph;
            int fillLen = doc.Width - prefix.Length - branch.Length - node.Text.Length - 4;
            if (fillLen < 0) fillLen = 0;

            var segs = new List<PlushLineSegment>(6)
            {
                new(ResourceStrings.GenericBorderV, doc.BorderColor),
                new(prefix, doc.TreeNodeColor),
                new(branch + "■ ", doc.TreeNodeColor),
                new(node.Text, doc.TextColor),
                new(new string(' ', fillLen), doc.TextColor),
                new(ResourceStrings.GenericBorderV, doc.BorderColor)
            };
            plushBlock.Block.Add(new PlushLine { Line = segs });

            // Prepare next ancestor list: add true if this node is *not* last
            var nextAncestors = new List<bool>(ancestorHasSibling) { !isLast };

            // Children
            var children = node.ConsoleNodes;
            int childCount = children?.Count ?? 0;
            for (int i = 0; i < childCount; i++)
            {
                bool childIsLast = (i == childCount - 1);
                RenderChildNode(children[i], plushBlock, doc, nextAncestors, childIsLast);
            }
        }

        public void Render(ConsoleDocument doc)
        {
            ConsolePlushBlock.Block.Clear();

            // Title line
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
            int rootTextLen = RootNode.Text.Length;
            int fillLen = doc.Width - 6 - rootTextLen;
            if (fillLen < 0) fillLen = 0;

            ConsolePlushBlock.Block.Add(new PlushLine
            {
                Line =
                [
                    new(ResourceStrings.GenericBorderV, doc.BorderColor),
                    new(new string(' ', 2), doc.TextColor),
                    new(ResourceStrings.TreeNode, doc.TreeNodeColor),
                    new(" ", doc.TextColor),
                    new(RootNode.Text, doc.TextColor, PlushTextStyle.Bold),
                    new(new string(' ', fillLen), doc.TextColor),
                    new(ResourceStrings.GenericBorderV, doc.BorderColor),
                ]
            });

            var rootChildren = RootNode.ConsoleNodes;
            int rootChildCount = rootChildren?.Count ?? 0;
            for (int i = 0; i < rootChildCount; i++)
            {
                bool isLast = (i == rootChildCount - 1);
                RenderChildNode(rootChildren[i], ConsolePlushBlock, doc, [], isLast);
                if (!isLast)
                {
                    ConsolePlushBlock.Block.Add(new PlushLine
                    {
                        Line =
                        [
                            new(ResourceStrings.GenericBorderVR, doc.BorderColor),
                            new(new string(ResourceStrings.GenericBorderH, 2), doc.BorderColor),
                            new(ResourceStrings.TreeNodeVL, doc.TreeNodeColor),
                            new(new string(ResourceStrings.GenericBorderH, doc.Width - 5), doc.BorderColor),
                            new(ResourceStrings.GenericBorderVL, doc.BorderColor)
                        ]
                    });
                }
            }

            ConsolePlushBlock.Block.Add(PlushHelpers.CreateBlockEnd(doc));
        }
    }
}
