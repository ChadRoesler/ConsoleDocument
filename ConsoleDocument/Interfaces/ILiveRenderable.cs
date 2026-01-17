using ConsoleDocumentSystem.Models.Parts;
using ConsoleDocumentSystem.Models.Rendering;

namespace ConsoleDocumentSystem.Interfaces
{
    // Implemented by blocks that can be live-updated by the live renderer.
    public interface ILiveRenderable
    {
        // Called once after initial paint to capture the absolute anchor.
        void AttachAnchor(ConsoleBlockDimensions dims);

        // Called on each refresh tick to build the updated lines for the region.
        // Return the relative rows to update and the corresponding PlushLine.
        // completed: true when the block no longer needs refreshing.
        IReadOnlyList<(int relativeRow, PlushLine line)> BuildFrame(out bool completed);
    }
}