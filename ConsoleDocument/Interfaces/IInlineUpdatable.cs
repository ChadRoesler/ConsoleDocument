using ConsoleDocumentSystem.Models.Parts;

namespace ConsoleDocumentSystem.Interfaces
{
    public interface IInlineUpdatable
    {
        // Called once, right after the block has been written to the console.
        void OnAfterFirstWrite(ConsoleBlockDimensions dims);
    }
}