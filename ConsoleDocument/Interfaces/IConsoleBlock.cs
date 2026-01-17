using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Interfaces
{
    public interface IConsoleBlock
    {
        public void Render(ConsoleDocument doc);
        public PlushBlock ConsolePlushBlock { get; }
    }
}
