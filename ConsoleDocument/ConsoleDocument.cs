using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.Helpers;
using ConsoleDocumentSystem.Interfaces;
using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem
{
    public class ConsoleDocument
    {
        public ConsoleDocument(int width, bool enableVT)
        {
            if (width < 50)
                throw new ArgumentException("Width must be at least 50 characters.");
            if (width > 200)
                throw new ArgumentException("Width must not exceed 200 characters.");
            Width = width;
            if (enableVT)
                VTEnabled = VT.Enable();
            PlushGlobalPalette.SetDefaults();
        }

        public int Width { get; set; } = 100;
        public bool VTEnabled { get; set; }
        public List<IConsoleBlock> Blocks { get; set; } = [];
        public PlushColor TextColor { get; set; } = PlushColor.DefaultForeground;
        public PlushColor RootGlyphColor { get; set; } = PlushColor.Cyan;
        public PlushColor BorderColor { get; set; } = PlushColor.DarkGray;
        public PlushColor TreeNodeColor { get; set; } = PlushColor.Green;
        public PlushColor BarGraphColor { get; set; } = PlushColor.Yellow;
        public PlushColor ProgressBarColor { get; set; } = PlushColor.Coral;
        public bool AlternateBarGraphColors { get; set; } = false;
        public List<PlushColor> OutlineColors { get; set; } = [PlushColor.DarkBlue, PlushColor.DarkGreen, PlushColor.DarkRed];

        public void Render()
        {
            // Split blocks into static and live
            var liveBlocks = new List<(ILiveRenderable Live, IConsoleBlock Block)>();
            foreach (var block in Blocks)
            {
                block.Render(this);

                if (block is ILiveRenderable live)
                {
                    liveBlocks.Add((live, block));
                }
                else
                {
                    // Static block: render and write immediately
                    PlushSmartWriter.WritePlushBlockTracked(block.ConsolePlushBlock, Width);
                }
            }

            if (liveBlocks.Count == 0)
                return;

            // Reserve a single contiguous live region at the bottom (Spectre-like)
            int anchorTop = Console.CursorTop;
            int left = 0;
            int right = Math.Min(Console.BufferWidth - 1, Math.Max(0, Width - 1));

            // Compute total height and per-block dims inside this region
            int totalLiveHeight = 0;
            var dimsPerLive = new List<(ILiveRenderable Live, ConsoleBlockDimensions Dims, IConsoleBlock Block)>(liveBlocks.Count);
            foreach (var (live, block) in liveBlocks)
            {
                int height = Math.Max(1, block.ConsolePlushBlock.Block.Count);
                var dims = new ConsoleBlockDimensions(
                    left,
                    anchorTop + totalLiveHeight,
                    right,
                    anchorTop + totalLiveHeight + height - 1
                );
                dimsPerLive.Add((live, dims, block));
                totalLiveHeight += height;
            }

            // Actually reserve the rows so anchors remain stable
            for (int i = 0; i < totalLiveHeight; i++)
                Console.WriteLine();

            // Initial paint of the live region and registration
            foreach (var (live, dims, block) in dimsPerLive)
            {
                live.AttachAnchor(dims);

                for (int rel = 0; rel < block.ConsolePlushBlock.Block.Count; rel++)
                {
                    int rowTop = dims.RowAbs(rel);
                    WriteLineAt(block.ConsolePlushBlock.Block[rel], dims.LeftCoordinate, dims.Width, rowTop);
                }
            }

            LiveRegionRenderer.StartIfNeeded(VTEnabled);

            // Place caret after the live region
            int afterRow = Math.Min(Math.Max(0, Console.BufferHeight - 1), Math.Max(0, anchorTop + totalLiveHeight));
            Console.SetCursorPosition(0, afterRow);
        }

        // Writes a line (list of segments) at a fixed row/col, clamped to width
        private static void WriteLineAt(PlushLine line, int left, int width, int rowTop)
        {
            width = Math.Max(0, width);
            if (width == 0) return;

            int bufferWidth = Math.Max(1, Console.BufferWidth);
            int leftClamped = Math.Clamp(left, 0, bufferWidth - 1);
            int rightClamped = Math.Clamp(left + width - 1, 0, bufferWidth - 1);
            int writable = Math.Max(0, rightClamped - leftClamped + 1);
            if (writable == 0) return;

            int topClamped = Math.Clamp(rowTop, 0, Math.Max(0, Console.BufferHeight - 1));
            Console.SetCursorPosition(leftClamped, topClamped);

            int written = 0;
            foreach (var seg in line.Line)
            {
                if (written >= writable) break;

                string text = seg.Text;
                int remaining = writable - written;
                if (text.Length > remaining)
                    text = text[..remaining];

                // write trimmed segment
                PlushAnsiHelper.WriteSegment(new PlushLineSegment(text, seg.ForegroundColor, seg.Style));
                written += text.Length;
            }

            if (written < writable)
                Console.Write(new string(' ', writable - written));

            PlushAnsiHelper.Reset();
        }

        // New: full-screen live with optional scrolling (Up/Down/PageUp/PageDown)
        public async Task RenderFullScreenLiveAsync(int refreshMs = 100, CancellationToken cancellationToken = default)
        {
            var session = new FullScreenLiveSession(this, Blocks, VTEnabled, refreshMs);
            await session.RunAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}