using ConsoleDocumentSystem.Constants;
using ConsoleDocumentSystem.Helpers;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Models
{
    public class ConsoleDividedBarGraph : IConsoleBlock
    {
        public ConsoleDividedBarGraph(string titleText, List<ConsoleGraphSegment> graphSegments)
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
            int barMaxWidth = doc.Width - 4;
            double total = GraphSegments.Sum(x => x.Value);
            if (total <= 0 || barMaxWidth <= 0)
            {
                // Render empty bar and exit
                ConsolePlushBlock.Block.Add(new PlushLine
                {
                    Line =
                    [
                        new(ResourceStrings.GenericBorderV, doc.BorderColor),
                        new(new string(' ', barMaxWidth + 2), doc.TextColor),
                        new(ResourceStrings.GenericBorderV, doc.BorderColor)
                    ]
                });
                ConsolePlushBlock.Block.Add(PlushHelpers.CreateBlockEnd(doc));
                return;
            }

            // Calculate segment lengths
            var barUnits = GraphSegments
                .Select(s => (Segment: s, Length: (int)Math.Round((s.Value / total) * barMaxWidth)))
                .ToList();

            // Adjust bar lengths to fit exactly
            int barWidth = barUnits.Sum(x => x.Length);
            int diff = barMaxWidth - barWidth;
            if (diff != 0)
            {
                // Distribute rounding error: add/subtract 1 to segments with largest/smallest remainder
                var remainders = GraphSegments
                    .Select(s => new
                    {
                        Segment = s,
                        Remainder = ((s.Value / total) * barMaxWidth) - Math.Floor((s.Value / total) * barMaxWidth)
                    })
                    .ToList();

                var order = diff > 0
                    ? remainders.OrderByDescending(x => x.Remainder).ToList()
                    : remainders.OrderBy(x => x.Remainder).ToList();

                for (int i = 0; diff != 0 && i < order.Count; i++)
                {
                    int idx = barUnits.FindIndex(x => x.Segment == order[i].Segment);
                    if (idx == -1) continue;
                    if (diff > 0)
                    {
                        barUnits[idx] = (barUnits[idx].Segment, barUnits[idx].Length + 1);
                        diff--;
                    }
                    else if (diff < 0 && barUnits[idx].Length > 0)
                    {
                        barUnits[idx] = (barUnits[idx].Segment, barUnits[idx].Length - 1);
                        diff++;
                    }
                }
            }

            // Bar line
            var barLine = new PlushLine
            {
                Line =
                [
                    new(ResourceStrings.GenericBorderV, doc.BorderColor),
                    new(" ", doc.TextColor)
                ]
            };
            foreach (var (seg, len) in barUnits)
            {
                if (len > 0)
                    barLine.Line.Add(new PlushLineSegment(new string(ResourceStrings.BarFullShade, len), seg.ForegroundColor ?? doc.BarGraphColor));
            }
            barLine.Line.Add(new(" ", doc.TextColor));
            barLine.Line.Add(new(ResourceStrings.GenericBorderV, doc.BorderColor));
            ConsolePlushBlock.Block.Add(barLine);

            // Legend lines
            var legendRows = new List<List<ConsoleGraphSegment>>();
            var current = new List<ConsoleGraphSegment>();
            int legendRowWidth = 0;
            int legendMaxWidth = doc.Width - 4;
            foreach (var s in GraphSegments)
            {
                string legend = $"{ResourceStrings.TreeNode} {s.Text} {Math.Round(100.0 * s.Value / total, 1)}% ";
                if (legendRowWidth + legend.Length > legendMaxWidth && current.Count > 0)
                {
                    legendRows.Add(current);
                    current = [];
                    legendRowWidth = 0;
                }
                current.Add(s);
                legendRowWidth += legend.Length;
            }
            if (current.Count > 0) legendRows.Add(current);

            foreach (var row in legendRows)
            {
                var line = new PlushLine
                {
                    Line =
                    [
                        new(ResourceStrings.GenericBorderV, doc.BorderColor),
                        new(" ", doc.TextColor)
                    ]
                };
                foreach (var seg in row)
                {
                    string legend = $"{ResourceStrings.TreeNode} {seg.Text} {Math.Round(100.0 * seg.Value / total, 1)}% ";
                    line.Line.Add(new(legend, seg.ForegroundColor ?? doc.BarGraphColor));
                }
                int rowLen = 2 + row.Sum(x => ($"{ResourceStrings.TreeNode} {x.Text} {Math.Round(100.0 * x.Value / total, 1)}% ").Length);
                int fillLen = doc.Width - rowLen - 1;
                if (fillLen > 0)
                    line.Line.Add(new(new string(' ', fillLen)));
                line.Line.Add(new(ResourceStrings.GenericBorderV, doc.BorderColor));
                ConsolePlushBlock.Block.Add(line);
            }

            ConsolePlushBlock.Block.Add(PlushHelpers.CreateBlockEnd(doc));
        }
    }
}
