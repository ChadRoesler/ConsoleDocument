using ConsoleDocumentSystem.Constants;
using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.Helpers;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;
using ConsoleDocumentSystem.Models.Structs;

namespace ConsoleDocumentSystem.Models
{
    public class ConsoleProgressBar : IConsoleBlock, ILiveRenderable, IDisposable
    {
        private readonly string _title;
        private readonly string? _description;
        private readonly Func<ProgressState> _progressProvider;
        private readonly Task _workTask;
        private readonly bool _vtEnabled;

        public PlushBlock ConsolePlushBlock { get; } = new();

        public PlushColor BarColor { get; set; } = PlushColor.Green;
        public PlushColor EmptyColor { get; set; } = PlushColor.DarkGray;
        public PlushColor TextColor { get; set; } = PlushColor.DefaultForeground;

        private ConsoleBlockDimensions? _dims;
        private int _progressRel;
        private int _statusRel;
        private bool _tickToggle;

        private PlushColor _borderColorCache = PlushColor.DarkGray;
        private PlushColor _textColorCache = PlushColor.DefaultForeground;

        // New: track the highest percentage ever displayed to prevent regressions.
        private int _maxPercentDisplayed;

        public ConsoleProgressBar(string title, bool vtEnabled, Func<ProgressState> progressProvider, Task workTask, string? description = null)
        {
            _title = title;
            _progressProvider = progressProvider;
            _workTask = workTask;
            _description = description;
            _vtEnabled = vtEnabled;
        }

        public void Render(ConsoleDocument doc)
        {
            ConsolePlushBlock.Clear();

            _borderColorCache = doc.BorderColor;
            _textColorCache = doc.TextColor;

            ConsolePlushBlock.Block.AddRange(PlushHelpers.CreateTitle(doc, _title));
            ConsolePlushBlock.Block.Add(FullWidthLine(doc, ResourceStrings.GenericBorderVR, ResourceStrings.GenericBorderH, ResourceStrings.GenericBorderVL));

            var ps = SafeProvider();
            int pct = CalcPercent(ps.Current, ps.Total);
            // Initialize monotonic clamp on first render
            _maxPercentDisplayed = Math.Max(_maxPercentDisplayed, pct);

            char glyph = CompletionGlyph(pct, completed: _workTask.IsCompleted);

            _progressRel = ConsolePlushBlock.Block.Count;
            ConsolePlushBlock.Block.Add(BuildProgressLine(doc, pct));

            _statusRel = ConsolePlushBlock.Block.Count;
            ConsolePlushBlock.Block.Add(BuildStatusLine(doc, glyph, ps.Status));

            foreach (var line in BuildDescriptionLines(doc, _description))
                ConsolePlushBlock.Block.Add(line);

            ConsolePlushBlock.Block.Add(FullWidthLine(doc, ResourceStrings.GenericBorderUR, ResourceStrings.GenericBorderH, ResourceStrings.GenericBorderUL));
        }

        // ILiveRenderable
        public void AttachAnchor(ConsoleBlockDimensions dims) => _dims = dims;

        public IReadOnlyList<(int relativeRow, PlushLine line)> BuildFrame(out bool completed)
        {
            completed = _workTask.IsCompleted;

            var ps = SafeProvider();
            int pct = CalcPercent(ps.Current, ps.Total);

            if (completed)
            {
                // Once completed, lock to 100% and never regress.
                _maxPercentDisplayed = 100;
                pct = 100;
            }
            else
            {
                // Enforce monotonic non-decreasing percentage while running.
                if (pct < _maxPercentDisplayed) pct = _maxPercentDisplayed;
                else _maxPercentDisplayed = pct;

                _tickToggle = !_tickToggle;
            }

            var proxy = BuildDocProxy();

            var progress = BuildProgressLine(proxy, pct);
            var glyph = CompletionGlyph(pct, completed);
            var status = BuildStatusLine(proxy, glyph, ps.Status);

            return new List<(int, PlushLine)>
            {
                (_progressRel, progress),
                (_statusRel, status)
            };
        }

        private ConsoleDocument BuildDocProxy()
        {
            int width = Math.Clamp(_dims?.Width ?? 100, 50, 200);
            var proxy = new ConsoleDocument(width, _vtEnabled)
            {
                BorderColor = _borderColorCache,
                TextColor = _textColorCache
            };
            return proxy;
        }

        private static int CalcPercent(long current, long total)
        {
            if (total <= 0) return 0;
            if (current <= 0) return 0;
            if (current >= total) return 100;
            return (int)Math.Floor((current * 100.0) / total);
        }

        private char CompletionGlyph(int percent, bool completed)
            => (completed || percent >= 100) ? '■' : (_tickToggle ? '▪' : '□');

        private PlushLine FullWidthLine(ConsoleDocument doc, char left, char mid, char right)
        {
            return new PlushLine
            {
                Line =
                [
                    new(left, doc.BorderColor),
                    new(new string(mid, Math.Max(0, doc.Width - 2)), doc.BorderColor),
                    new(right, doc.BorderColor)
                ]
            };
        }

        private PlushLine BuildProgressLine(ConsoleDocument doc, int percent)
        {
            int inner = Math.Max(0, doc.Width - 2);

            string digits = percent.ToString();
            int leftPad = Math.Max(0, 4 - digits.Length);
            string pctAligned = new string(' ', leftPad) + digits + "%";

            int barWidth = Math.Max(1, inner - 9);
            int filled = (int)Math.Floor(barWidth * (percent / 100.0));
            filled = Math.Clamp(filled, 0, barWidth);

            string filledBar = filled > 0 ? new string(ResourceStrings.BarFullShade, filled) : string.Empty;
            string emptyBar = (barWidth - filled) > 0 ? new string(ResourceStrings.BarLightShade, barWidth - filled) : string.Empty;

            var line = new PlushLine { Line = [] };
            line.Line.Add(new(ResourceStrings.GenericBorderV, doc.BorderColor));
            line.Line.Add(new(" ", TextColor));
            line.Line.Add(new("[", doc.BorderColor));
            if (filledBar.Length > 0) line.Line.Add(new(filledBar, BarColor));
            if (emptyBar.Length > 0) line.Line.Add(new(emptyBar, EmptyColor));
            line.Line.Add(new("]", doc.BorderColor));
            line.Line.Add(new(pctAligned, TextColor));
            line.Line.Add(new(" ", TextColor));
            line.Line.Add(new(ResourceStrings.GenericBorderV, doc.BorderColor));
            return line;
        }

        private PlushLine BuildStatusLine(ConsoleDocument doc, char glyph, string status)
        {
            int inner = Math.Max(0, doc.Width - 2);
            string payload = $" {glyph} {status ?? string.Empty}";
            if (payload.Length > inner) payload = payload[..inner];
            payload = payload.PadRight(inner);

            return new PlushLine
            {
                Line =
                [
                    new(ResourceStrings.GenericBorderV, doc.BorderColor),
                    new(payload, TextColor),
                    new(ResourceStrings.GenericBorderV, doc.BorderColor)
                ]
            };
        }

        private IEnumerable<PlushLine> BuildDescriptionLines(ConsoleDocument doc, string? description)
        {
            if (string.IsNullOrEmpty(description))
                yield break;

            int inner = Math.Max(0, doc.Width - 2);
            var wrapped = PlushHelpers.WrapText(description, Math.Max(1, inner - 1));
            foreach (var w in wrapped)
            {
                string content = " " + (w ?? string.Empty);
                if (content.Length > inner) content = content[..inner];
                content = content.PadRight(inner);

                yield return new PlushLine
                {
                    Line =
                    [
                        new(ResourceStrings.GenericBorderV, doc.BorderColor),
                        new(content, TextColor),
                        new(ResourceStrings.GenericBorderV, doc.BorderColor)
                    ]
                };
            }
        }

        private ProgressState SafeProvider()
        {
            try { return _progressProvider?.Invoke() ?? new ProgressState(0, 0, string.Empty); }
            catch { return new ProgressState(0, 0, string.Empty); }
        }

        public void Dispose()
        {
            // Nothing to dispose for the pull-based progress bar.
        }
    }
}