using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;
using ConsoleDocumentSystem.Enums;

namespace ConsoleDocumentSystem.Helpers
{
    internal sealed class FullScreenLiveSession
    {
        private readonly ConsoleDocument _doc;
        private readonly List<IConsoleBlock> _blocks;
        private readonly List<PlushLine> _full; // flattened document lines
        private readonly List<(ILiveRenderable Live, int StartRow, int Height, IConsoleBlock Block)> _liveMap;
        private readonly int _refreshMs;
        private readonly bool _vtEnabled;
        private int _viewportTop;
        private int _screenRows;

        public FullScreenLiveSession(ConsoleDocument doc, IEnumerable<IConsoleBlock> blocks, bool vtEnabled, int refreshMs)
        {
            _doc = doc;
            _blocks = blocks.ToList();
            _full = new List<PlushLine>(1024);
            _liveMap = new List<(ILiveRenderable, int, int, IConsoleBlock)>();
            _refreshMs = refreshMs;
            _vtEnabled = vtEnabled;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            // Build virtual document and live map
            BuildVirtualDocument();

            // Initial viewport = top of document; height = window height (clamped)
            _screenRows = Math.Max(1, Console.WindowHeight);
            _viewportTop = 0;

            // Paint initial viewport
            HideCursor();
            try
            {
                PaintViewport(_viewportTop);

                // Attach anchors for live blocks (relative to absolute screen rows)
                foreach (var (live, startRow, height, _) in _liveMap)
                {
                    // Anchor top/bottom: live region is the whole screen; we update rows by viewport mapping
                    int left = 0;
                    int right = Math.Min(Console.BufferWidth - 1, Math.Max(0, _doc.Width - 1));
                    var dims = new ConsoleBlockDimensions(left, startRow, right, startRow + Math.Max(1, height) - 1);
                    live.AttachAnchor(dims);
                }

                // Track which lives are done to stop polling them but keep the session open for user
                var doneLives = new HashSet<ILiveRenderable>();
                bool exitRequested = false;

                // Live loop
                while (!ct.IsCancellationRequested && !exitRequested)
                {
                    // Handle resize
                    if (_screenRows != Console.WindowHeight)
                    {
                        _screenRows = Math.Max(1, Console.WindowHeight);
                        // Reserve one row for the navigation legend so content doesn't overlap it
                        int visibleRows = Math.Max(0, _screenRows - 1);
                        // Clamp viewport if document shorter than screen (excluding legend)
                        _viewportTop = Math.Clamp(_viewportTop, 0, Math.Max(0, _full.Count - visibleRows));
                        PaintViewport(_viewportTop);
                    }

                    // Handle input (non-blocking)
                    while (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        int visible = Math.Max(0, _screenRows - 1);
                        switch (key.Key)
                        {
                            case ConsoleKey.UpArrow:
                                _viewportTop = Math.Max(0, _viewportTop - 1);
                                PaintViewport(_viewportTop);
                                break;
                            case ConsoleKey.DownArrow:
                                _viewportTop = Math.Min(Math.Max(0, _full.Count - visible), _viewportTop + 1);
                                PaintViewport(_viewportTop);
                                break;
                            case ConsoleKey.PageUp:
                                _viewportTop = Math.Max(0, _viewportTop - visible);
                                PaintViewport(_viewportTop);
                                break;
                            case ConsoleKey.PageDown:
                                _viewportTop = Math.Min(Math.Max(0, _full.Count - visible), _viewportTop + visible);
                                PaintViewport(_viewportTop);
                                break;
                            case ConsoleKey.Home:
                                _viewportTop = 0;
                                PaintViewport(_viewportTop);
                                break;
                            case ConsoleKey.End:
                                _viewportTop = Math.Max(0, _full.Count - visible);
                                PaintViewport(_viewportTop);
                                break;
                            case ConsoleKey.Escape:
                                exitRequested = true; // user explicitly exits
                                break;
                        }
                    }

                    // Pull frames from each live block and update only visible rows
                    int visibleContentRows = Math.Max(0, _screenRows - 1); // reserve last row for legend
                    foreach (var (live, startRow, _, _) in _liveMap)
                    {
                        if (doneLives.Contains(live))
                            continue;

                        var frames = live.BuildFrame(out bool completed);
                        if (completed) doneLives.Add(live);

                        foreach (var (relRow, line) in frames)
                        {
                            int docRow = startRow + relRow;

                            // IMPORTANT: keep backing document in sync so scrolling/repainters
                            // use the latest content (prevents reverting to initial 1%).
                            if (docRow >= 0 && docRow < _full.Count)
                                _full[docRow] = line;

                            int screenRow = docRow - _viewportTop;
                            // never write into the last row (legend)
                            if (screenRow < 0 || screenRow >= visibleContentRows)
                                continue; // not visible

                            WriteLineAt(line, 0, _doc.Width, screenRow);
                        }
                    }

                    // Ensure legend row is painted (after any live updates)
                    PaintLegend();

                    // Do NOT exit when all lives completed; keep session open until ESC or cancellation.
                    await Task.Delay(_refreshMs, ct).ConfigureAwait(false);
                }
            }
            finally
            {
                ShowCursor();
                // Place caret after last screen row and newline to avoid prompt collision
                try
                {
                    int after = Math.Min(Math.Max(0, Console.BufferHeight - 1), _screenRows);
                    Console.SetCursorPosition(0, after);
                    Console.WriteLine();
                }
                catch { }
            }
        }

        private void BuildVirtualDocument()
        {
            _full.Clear();
            _liveMap.Clear();

            int start = 0;
            foreach (var block in _blocks)
            {
                // Render block for current width
                block.Render(_doc);

                int height = block.ConsolePlushBlock.Block.Count;
                _full.AddRange(block.ConsolePlushBlock.Block);

                if (block is ILiveRenderable live)
                    _liveMap.Add((live, start, height, block));

                start += height;
            }
        }

        private void PaintViewport(int top)
        {
            // Reserve last row for legend
            int visibleRows = Math.Max(0, _screenRows - 1);

            int rows = Math.Min(visibleRows, _full.Count - top);
            rows = Math.Max(0, rows);

            // Clear visible content area (do not touch legend row)
            for (int r = 0; r < visibleRows; r++)
            {
                WriteLineAt(new PlushLine { Line = [new PlushLineSegment(string.Empty)] }, 0, _doc.Width, r);
            }

            // Draw visible lines (only content rows)
            for (int r = 0; r < rows; r++)
            {
                WriteLineAt(_full[top + r], 0, _doc.Width, r);
            }

            // Draw the navigation legend on the reserved last row
            PaintLegend();
        }

        private void PaintLegend()
        {
            try
            {
                int legendRow = Math.Max(0, _screenRows - 1);
                // legend source text: bracketed parts should NOT be inverse; everything else should be inverse
                string source = "↑ pg up [Navigate up] ↓ pg dn [Navigate down]  home [Navigate top] end [Navigate bottom]  esc [Exit]";
                int width = Math.Max(0, _doc.Width);

                // Parse source into segments where bracketed text (including brackets) is a single segment.
                var parsed = new List<(string Text, bool IsBracket)>();
                int i = 0;
                while (i < source.Length)
                {
                    if (source[i] == '[')
                    {
                        int start = i;
                        int end = source.IndexOf(']', start);
                        if (end == -1) end = source.Length - 1;
                        string seg = source.Substring(start, end - start + 1);
                        parsed.Add((seg, true));
                        i = end + 1;
                    }
                    else
                    {
                        int start = i;
                        int nextBracket = source.IndexOf('[', start);
                        int end = nextBracket == -1 ? source.Length : nextBracket;
                        string seg = source.Substring(start, end - start);
                        parsed.Add((seg, false));
                        i = end;
                    }
                }

                // Build styled segments, trimming/padding to exactly the document width.
                var styled = new List<PlushLineSegment>();
                int written = 0;
                foreach (var (text, isBracket) in parsed)
                {
                    if (written >= width) break;
                    if (string.IsNullOrEmpty(text)) continue;

                    int remaining = width - written;
                    string take = text.Length > remaining ? text[..remaining] : text;
                    var style = isBracket ? PlushTextStyle.Inverse : PlushTextStyle.None;
                    styled.Add(new PlushLineSegment(take, _doc.TextColor, style));
                    written += take.Length;
                }

                if (written < width)
                {
                    // pad remaining with inverse style
                    styled.Add(new PlushLineSegment(new string(' ', width - written), _doc.TextColor, PlushTextStyle.Inverse));
                }

                var legendLine = new PlushLine
                {
                    Line = styled
                };

                // Use WriteLineAt to respect width/clamping
                WriteLineAt(legendLine, 0, _doc.Width, legendRow);
            }
            catch
            {
                // best-effort; do not throw from rendering legend
            }
        }

        private void WriteLineAt(PlushLine line, int left, int width, int screenRow)
        {
            width = Math.Max(0, width);
            if (width == 0) return;

            int bufferWidth = Math.Max(1, Console.BufferWidth);
            int leftClamped = Math.Clamp(left, 0, bufferWidth - 1);
            int rightClamped = Math.Clamp(left + width - 1, 0, bufferWidth - 1);
            int writable = Math.Max(0, rightClamped - leftClamped + 1);
            if (writable == 0) return;

            int topClamped = Math.Clamp(screenRow, 0, Math.Max(0, Console.BufferHeight - 1));
            Console.SetCursorPosition(leftClamped, topClamped);

            int written = 0;
            foreach (var seg in line.Line)
            {
                if (written >= writable) break;

                string text = seg.Text ?? string.Empty;
                int remaining = writable - written;
                if (text.Length > remaining) text = text[..remaining];

                PlushAnsiHelper.WriteSegment(new PlushLineSegment(text, seg.ForegroundColor, seg.Style));
                written += text.Length;
            }

            if (written < writable)
                Console.Write(new string(' ', writable - written));

            PlushAnsiHelper.Reset();
        }

        private void HideCursor()
        {
            try
            {
                if (_vtEnabled) Console.Write("\x1b[?25l");
                else Console.CursorVisible = false;
            }
            catch { }
        }

        private void ShowCursor()
        {
            try
            {
                if (_vtEnabled) Console.Write("\x1b[?25h");
                else Console.CursorVisible = true;
            }
            catch { }
        }
    }
}