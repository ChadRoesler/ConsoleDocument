using ConsoleDocumentSystem.Helpers;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;
using ConsoleDocumentSystem.Constants;
using ConsoleDocumentSystem.Enums;

namespace ConsoleDocumentSystem.Models
{
    public class ConsoleTable : IConsoleBlock
    {
        public ConsoleTable(string titleText, ConsoleTableHierarchy consoleTableHierarchy)
        {
            TitleText = titleText;
            Table = consoleTableHierarchy;
            ConsolePlushBlock = new PlushBlock();
        }

        private string TitleText { get; }
        public ConsoleTableHierarchy Table { get; set; }
        public PlushBlock ConsolePlushBlock { get; private set; }

        private sealed record LeafRecord(string[] Keys, string Value); // Keys length = (columnCount-1)

        public void Render(ConsoleDocument doc)
        {
            ConsolePlushBlock.Clear();

            if (Table.Columns.Count < 2)
            {
                ConsolePlushBlock.Block.AddRange(PlushHelpers.CreateTitle(doc, TitleText));
                ConsolePlushBlock.Block.Add(PlushHelpers.CreateBlockEnd(doc));
                return;
            }

            int columnCount = Table.Columns.Count;
            int lastColIndex = columnCount - 1;

            // 1. Widths
            var widths = new List<int>(new int[columnCount]);
            for (int i = 0; i < columnCount; i++)
                widths[i] = Table.Columns[i].Length + 2;

            void Accumulate(ConsoleTableNode node)
            {
                if (!string.IsNullOrEmpty(node.Key))
                {
                    int len = node.Key!.Length + 2;
                    if (len > widths[node.Depth])
                        widths[node.Depth] = len;
                }

                if (node.Depth == lastColIndex - 1)
                {
                    foreach (var v in node.Values)
                    {
                        int len = (v?.Length ?? 0) + 2;
                        if (len > widths[lastColIndex])
                            widths[lastColIndex] = len;
                    }
                }

                foreach (var c in node.Children.Values)
                    Accumulate(c);
            }
            foreach (var root in Table.RootNodes.Values)
                Accumulate(root);

            int innerWidth = doc.Width - 2;
            int separators = columnCount - 1;
            int targetCellSum = innerWidth - separators;
            widths = PlushHelpers.DistributeColumnWidths(widths, targetCellSum);

            // 2. Flatten (preserve insertion order)
            var leaves = new List<LeafRecord>();
            string[] pathKeys = new string[lastColIndex];

            void Collect(ConsoleTableNode node)
            {
                pathKeys[node.Depth] = node.Key ?? string.Empty;
                if (node.Depth == lastColIndex - 1)
                {
                    foreach (var val in node.Values)
                        leaves.Add(new LeafRecord((string[])pathKeys.Clone(), val ?? string.Empty));
                }
                else
                {
                    foreach (var child in node.Children.Values)
                        Collect(child);
                }
            }
            foreach (var root in Table.RootNodes.Values)
                Collect(root);

            // 3. Header
            ConsolePlushBlock.Block.AddRange(PlushHelpers.CreateTitle(doc, TitleText));
            ConsolePlushBlock.Block.Add(BuildFullSeparatorLine(widths, header: true, doc));
            ConsolePlushBlock.Block.Add(BuildHeaderTitlesLine(widths, doc));
            ConsolePlushBlock.Block.Add(BuildFullSeparatorLine(widths, header: false, doc));

            if (leaves.Count == 0)
            {
                // Bottom border for empty table
                ConsolePlushBlock.Block.Add(BuildBottomSeparatorLine(widths, doc));
                return;
            }

            // Helper: grouping flows left->right. If any left column changed, current column key shows.
            static bool ShouldShowKey(string[]? prev, string[] cur, int col)
            {
                if (prev is null) return true;
                for (int i = 0; i < col; i++)
                    if (!string.Equals(prev[i], cur[i], StringComparison.Ordinal)) return true;
                return !string.Equals(prev[col], cur[col], StringComparison.Ordinal);
            }

            // 4. Body (group consecutive leaves that share all non-last keys; render without extra blank lines)
            string[]? prevKeys = null;
            bool suppressNextLeadingSeparator = false; // prevents duplicate separator when we already drew it inline
            for (int i = 0; i < leaves.Count;)
            {
                var first = leaves[i];

                // Emit separator vs previous group (unless already drawn inline on the last row of the previous block)
                if (i > 0)
                {
                    if (suppressNextLeadingSeparator)
                    {
                        suppressNextLeadingSeparator = false;
                    }
                    else
                    {
                        int changeDepth = GetChangeDepth(prevKeys!, first.Keys);
                        if (changeDepth >= 0)
                            ConsolePlushBlock.Block.Add(BuildPartialSeparatorLine(widths, changeDepth, lastColIndex, doc));
                        else
                            ConsolePlushBlock.Block.Add(BuildPartialSeparatorLine(widths, lastColIndex, lastColIndex, doc));
                    }
                }

                // Find range [i..j] where all non-last keys are identical
                int j = i;
                for (; j + 1 < leaves.Count; j++)
                {
                    bool same = true;
                    for (int c = 0; c < lastColIndex; c++)
                    {
                        if (!string.Equals(leaves[j + 1].Keys[c], first.Keys[c], StringComparison.Ordinal))
                        {
                            same = false; break;
                        }
                    }
                    if (!same) break;
                }

                // Prepare wrapped lines per non-last column (only if we should show this column key now)
                var colWrapped = new List<List<string>>(lastColIndex);
                int nonLastMax = 0;
                for (int c = 0; c < lastColIndex; c++)
                {
                    bool show = ShouldShowKey(prevKeys, first.Keys, c);
                    if (show)
                    {
                        var w = PlushHelpers.WrapText(first.Keys[c], Math.Max(1, widths[c] - 1));
                        colWrapped.Add(w);
                        if (w.Count > nonLastMax) nonLastMax = w.Count;
                    }
                    else
                    {
                        // zero lines => does not force row height
                        colWrapped.Add(new List<string>(0));
                    }
                }

                // Build last column interleaved (value lines, then separator, then next value lines, ...)
                int valueCount = j - i + 1;
                var lastLines = new List<(bool isSep, string text)>(valueCount * 2 - 1);
                for (int k = 0; k < valueCount; k++)
                {
                    var vWrap = PlushHelpers.WrapText(leaves[i + k].Value, Math.Max(1, widths[lastColIndex] - 1));
                    if (vWrap.Count == 0) vWrap.Add(string.Empty);
                    foreach (var vLine in vWrap)
                        lastLines.Add((false, vLine));
                    if (k < valueCount - 1)
                        lastLines.Add((true, string.Empty)); // separator line between values
                }

                int totalRows = Math.Max(nonLastMax, lastLines.Count);
                int nextChangeDepth = (j + 1 < leaves.Count) ? GetChangeDepth(leaves[j].Keys, leaves[j + 1].Keys) : -1;

                // Emit combined block rows
                for (int r = 0; r < totalRows; r++)
                {
                    bool isLastSepRow = (r < lastLines.Count && lastLines[r].isSep);

                    if (isLastSepRow)
                    {
                        // Inline separator across last column
                        var line = new PlushLine { Line = [] };
                        line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));

                        // Columns before last column, render normally
                        for (int c = 0; c < Math.Max(0, lastColIndex - 1); c++)
                        {
                            string text = (r < colWrapped[c].Count) ? colWrapped[c][r] : string.Empty;
                            string cell = " " + text;
                            if (cell.Length > widths[c]) cell = cell[..widths[c]];
                            cell = cell.PadRight(widths[c]);
                            line.Line.Add(new PlushLineSegment(cell, doc.TextColor));
                            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));
                        }

                        // The cell at (lastColIndex-1)
                        if (lastColIndex - 1 >= 0)
                        {
                            int c = lastColIndex - 1;
                            string text = (r < colWrapped[c].Count) ? colWrapped[c][r] : string.Empty;
                            string cell = " " + text;
                            if (cell.Length > widths[c]) cell = cell[..widths[c]];
                            cell = cell.PadRight(widths[c]);
                            line.Line.Add(new PlushLineSegment(cell, doc.TextColor));
                        }

                        // Inline separator for the last column
                        line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVR, doc.BorderColor)); // ╠
                        line.Line.Add(new PlushLineSegment(new string(ResourceStrings.GenericBorderH, widths[lastColIndex]), doc.BorderColor));
                        line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVL, doc.BorderColor)); // ╣

                        ConsolePlushBlock.Block.Add(line);
                    }
                    else
                    {
                        // Should we inline the separator to the NEXT group on this last extra row?
                        bool drawInlineNextSep =
                            (nextChangeDepth > 0) &&                 // only between inner columns (not full-width change)
                            (lastLines.Count < totalRows) &&         // there are extra non-last lines
                            (r == totalRows - 1);                    // last row of this block

                        if (drawInlineNextSep)
                        {
                            // Render columns before startDepth normally, but skip the boundary at startDepth-1
                            var line = new PlushLine { Line = [] };
                            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));

                            int sd = nextChangeDepth;

                            for (int c = 0; c < sd; c++)
                            {
                                string text = (r < colWrapped[c].Count) ? colWrapped[c][r] : string.Empty;
                                string cell = " " + text;
                                if (cell.Length > widths[c]) cell = cell[..widths[c]];
                                cell = cell.PadRight(widths[c]);
                                line.Line.Add(new PlushLineSegment(cell, doc.TextColor));

                                if (c != sd - 1)
                                    line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));
                                // else: skip vertical; we'll place ╠ next
                            }

                            // Start junction and horizontal runs from startDepth to end
                            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVR, doc.BorderColor)); // ╠
                            line.Line.Add(new PlushLineSegment(new string(ResourceStrings.GenericBorderH, widths[sd]), doc.BorderColor));

                            for (int c = sd + 1; c <= lastColIndex; c++)
                            {
                                line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVH, doc.BorderColor)); // ╬
                                line.Line.Add(new PlushLineSegment(new string(ResourceStrings.GenericBorderH, widths[c]), doc.BorderColor));
                            }

                            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVL, doc.BorderColor)); // ╣
                            ConsolePlushBlock.Block.Add(line);

                            // Prevent the next group's leading partial separator (we just drew it inline)
                            suppressNextLeadingSeparator = true;
                        }
                        else
                        {
                            // Normal row (possibly printing a value in the last column)
                            var line = new PlushLine { Line = [] };
                            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));

                            for (int c = 0; c < lastColIndex; c++)
                            {
                                string text = (r < colWrapped[c].Count) ? colWrapped[c][r] : string.Empty;
                                string cell = " " + text;
                                if (cell.Length > widths[c]) cell = cell[..widths[c]];
                                cell = cell.PadRight(widths[c]);
                                line.Line.Add(new PlushLineSegment(cell, doc.TextColor));
                                line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));
                            }

                            // Last column value (if present for this row)
                            string lastText = (r < lastLines.Count && !lastLines[r].isSep) ? lastLines[r].text : string.Empty;
                            string lastCell = " " + lastText;
                            if (lastCell.Length > widths[lastColIndex]) lastCell = lastCell[..widths[lastColIndex]];
                            lastCell = lastCell.PadRight(widths[lastColIndex]);
                            line.Line.Add(new PlushLineSegment(lastCell, doc.TextColor));
                            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));

                            ConsolePlushBlock.Block.Add(line);
                        }
                    }
                }

                prevKeys = first.Keys;
                i = j + 1;
            }

            // Bottom border with proper junctions (╩) at internal splits
            ConsolePlushBlock.Block.Add(BuildBottomSeparatorLine(widths, doc));
        }

        private static int GetChangeDepth(string[] previous, string[] current)
        {
            for (int i = 0; i < previous.Length; i++)
                if (!string.Equals(previous[i], current[i], StringComparison.Ordinal))
                    return i;
            return -1;
        }

        private PlushLine BuildFullSeparatorLine(List<int> widths, bool header, ConsoleDocument doc)
        {
            char midChar = header ? ResourceStrings.GenericBorderDH : ResourceStrings.GenericBorderVH;
            var line = new PlushLine { Line = [] };

            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVR, doc.BorderColor));
            for (int i = 0; i < widths.Count; i++)
            {
                line.Line.Add(new PlushLineSegment(new string(ResourceStrings.GenericBorderH, widths[i]), doc.BorderColor));
                if (i < widths.Count - 1)
                    line.Line.Add(new PlushLineSegment(midChar, doc.BorderColor));
            }
            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVL, doc.BorderColor));
            return line;
        }

        // Bottom border with junctions: ╚ ... ╩ ... ╝
        private PlushLine BuildBottomSeparatorLine(List<int> widths, ConsoleDocument doc)
        {
            var line = new PlushLine { Line = [] };
            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderUR, doc.BorderColor)); // ╚
            for (int i = 0; i < widths.Count; i++)
            {
                line.Line.Add(new PlushLineSegment(new string(ResourceStrings.GenericBorderH, widths[i]), doc.BorderColor));
                if (i < widths.Count - 1)
                    line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderUH, doc.BorderColor)); // ╩
            }
            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderUL, doc.BorderColor)); // ╝
            return line;
        }

        // Partial separator (standalone line)
        private PlushLine BuildPartialSeparatorLine(List<int> widths, int startDepth, int lastColIndex, ConsoleDocument doc)
        {
            if (startDepth <= 0)
                return BuildFullSeparatorLine(widths, header: false, doc);

            var line = new PlushLine { Line = [] };
            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));

            // Columns before startDepth: blanks + verticals (skip the boundary right before startDepth)
            for (int col = 0; col < startDepth; col++)
            {
                line.Line.Add(new PlushLineSegment(new string(' ', widths[col]), doc.TextColor));
                if (col != startDepth - 1)
                    line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));
            }

            // Start at depth
            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVR, doc.BorderColor)); // ╠
            line.Line.Add(new PlushLineSegment(new string(ResourceStrings.GenericBorderH, widths[startDepth]), doc.BorderColor));

            if (startDepth == lastColIndex)
            {
                line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVL, doc.BorderColor)); // ╣
                return line;
            }

            for (int col = startDepth + 1; col <= lastColIndex; col++)
            {
                line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVH, doc.BorderColor)); // ╬
                line.Line.Add(new PlushLineSegment(new string(ResourceStrings.GenericBorderH, widths[col]), doc.BorderColor));
            }

            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderVL, doc.BorderColor)); // ╣
            return line;
        }

        private PlushLine BuildHeaderTitlesLine(List<int> colWidths, ConsoleDocument doc)
        {
            var line = new PlushLine { Line = [] };
            line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));
            for (int i = 0; i < colWidths.Count; i++)
            {
                string name = Table.Columns[i];
                string cell = " " + name;
                if (cell.Length > colWidths[i]) cell = cell[..colWidths[i]];
                cell = cell.PadRight(colWidths[i]);
                line.Line.Add(new PlushLineSegment(cell, doc.TextColor, PlushTextStyle.Bold));
                line.Line.Add(new PlushLineSegment(ResourceStrings.GenericBorderV, doc.BorderColor));
            }
            return line;
        }
    }
}