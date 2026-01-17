using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Helpers
{
    public static class PlushSmartWriter
    {
        public static void WritePlushBlock(PlushBlock block, bool newline = true)
        {
            foreach (var line in block.Block)
            {
                foreach (var seg in line.Line)
                    PlushAnsiHelper.WriteSegment(seg);

                if (newline)
                    Console.WriteLine();
            }

            PlushAnsiHelper.Reset();
        }

        public static ConsoleBlockDimensions WritePlushBlockTracked(PlushBlock block, int docWidth, bool newline = true)
        {
            int startTop = Console.CursorTop;
            int startLeft = Console.CursorLeft;

            WritePlushBlock(block, newline);

            int lineCount = block.Block.Count;
            if (lineCount <= 0)
                lineCount = 1;

            int bottom = startTop + (lineCount - 1);
            int bufferRight = Math.Max(0, Console.BufferWidth - 1);
            int desiredRight = docWidth > 0 ? startLeft + docWidth - 1 : bufferRight;
            int right = Math.Clamp(desiredRight, startLeft, bufferRight);

            return new ConsoleBlockDimensions(startLeft, startTop, right, bottom);
        }
    }
}
