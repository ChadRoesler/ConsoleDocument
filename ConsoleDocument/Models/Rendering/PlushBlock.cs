namespace ConsoleDocumentSystem.Models.Rendering
{
    public class PlushBlock
    {
        public List<PlushLine> Block { get; set; } = [];
        public void Clear()
        {
            Block.Clear();
        }
    }
}
