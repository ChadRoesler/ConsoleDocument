namespace ConsoleDocumentSystem.Models.Parts
{
    public class ConsoleBlockDimensions(int Left, int Top, int Right, int Bottom)
    {
        public int LeftCoordinate => Left;
        public int TopCoordinate => Top;
        public int RightCoordinate => Right;
        public int BottomCoordinate => Bottom;

        public int Width => Right - Left + 1;
        public int Height => Bottom - Top + 1;

        public int RowAbs(int relative) => Top + relative;

        public (int left, int right) ClampToBuffer()
        {
            int maxRight = Math.Max(0, Console.BufferWidth - 1);
            int leftClamped = Math.Clamp(Left, 0, maxRight);
            int rightClamped = Math.Clamp(Right, 0, maxRight);
            return (leftClamped, rightClamped);
        }
    }
}
