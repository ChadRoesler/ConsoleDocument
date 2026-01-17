namespace ConsoleDocumentSystem.Models.Parts
{
    public class ConsoleTableNode
    {
        public string? Key { get; set; }                             // The value for this column
        public int Depth { get; set; }                               // Column index (0..n-1)

        // Child nodes (next column values)
        public Dictionary<string, ConsoleTableNode> Children { get; set; } = new();

        // Leaf values (only used at the last column)
        public List<string?> Values { get; set; } = new();

        // Convenience: check if this node is terminal
        public bool IsLeaf => Children.Count == 0 && Values.Count > 0;
    }
}