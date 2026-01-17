using System.Collections.Concurrent;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Helpers
{
    public static class LiveRegionRenderer
    {
        private static readonly object _sync = new();
        private static CancellationTokenSource? _cts;
        private static Task? _loop;
        private static readonly ConcurrentDictionary<ILiveRenderable, ConsoleBlockDimensions> _participants = new();
        private static bool _vtEnabled;

        public static void Register(ILiveRenderable live, ConsoleBlockDimensions dims)
        {
            live.AttachAnchor(dims);
            _participants[live] = dims;
        }

        public static void StartIfNeeded(bool vtEnabled, int refreshMs = 100)
        {
            lock (_sync)
            {
                if (_loop != null) return;
                _vtEnabled = vtEnabled;
                _cts = new CancellationTokenSource();
                _loop = Task.Run(() => LoopAsync(refreshMs, _cts.Token));
            }
        }

        public static async Task StopAsync()
        {
            lock (_sync)
            {
                _cts?.Cancel();
            }
            if (_loop != null)
            {
                try { await _loop.ConfigureAwait(false); } catch { /* ignore */ }
            }
            lock (_sync)
            {
                _cts?.Dispose();
                _cts = null;
                _loop = null;
                _participants.Clear();
            }

            // Ensure the caret ends after the last repaint and on a fresh line (avoid prompt collision).
            try
            {
                int lastBottom = Math.Max(0, Console.CursorTop);
                Console.SetCursorPosition(0, Math.Min(Console.BufferHeight - 1, lastBottom + 1));
                Console.WriteLine();
            }
            catch { /* ignore */ }
        }

        private static async Task LoopAsync(int refreshMs, CancellationToken ct)
        {
            try
            {
                HideCursor();
                while (!ct.IsCancellationRequested)
                {
                    bool anyActive = false;

                    // Stable top-to-bottom updates
                    foreach (var kvp in _participants.OrderBy(p => p.Value.TopCoordinate).ToArray())
                    {
                        var live = kvp.Key;
                        var dims = kvp.Value;

                        var frames = live.BuildFrame(out bool completed);
                        if (!completed) anyActive = true;

                        foreach (var (rel, line) in frames)
                        {
                            int rowTop = dims.RowAbs(rel);
                            RewriteLineClamped(line, dims, rowTop);
                        }
                    }

                    if (!anyActive) break;
                    await Task.Delay(refreshMs, ct).ConfigureAwait(false);
                }
            }
            catch { /* swallow */ }
            finally
            {
                ShowCursor();
            }
        }

        private static void RewriteLineClamped(PlushLine line, ConsoleBlockDimensions dims, int rowTop)
        {
            // Clamp left/right to current buffer width.
            var (leftClamped, rightClamped) = dims.ClampToBuffer();
            int width = Math.Max(0, rightClamped - leftClamped + 1);
            if (width <= 0) return;

            TrySetCursor(leftClamped, rowTop);

            int written = 0;
            foreach (var segment in line.Line)
            {
                PlushAnsiHelper.WriteSegment(segment);
                written += segment.Text.Length;
                if (written >= width) break; // don’t overflow the line
            }
            if (written < width)
                Console.Write(new string(' ', width - written));

            PlushAnsiHelper.Reset();
        }

        private static void TrySetCursor(int left, int top)
        {
            left = Math.Clamp(left, 0, Math.Max(0, Console.BufferWidth - 1));
            top = Math.Clamp(top, 0, Math.Max(0, Console.BufferHeight - 1));
            Console.SetCursorPosition(left, top);
        }

        private static void HideCursor()
        {
            try
            {
                if (_vtEnabled)
                    Console.Write("\x1b[?25l");
                else
                    Console.CursorVisible = false;
            }
            catch { }
        }

        private static void ShowCursor()
        {
            try
            {
                if (_vtEnabled)
                    Console.Write("\x1b[?25h");
                else
                    Console.CursorVisible = true;
            }
            catch { }
        }
    }
}