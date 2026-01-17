namespace ConsoleDocumentSystem.Models.Parts
{
    public class ConsoleTableHierarchy
    {
        public List<string> Columns { get; set; } = new();          // Column names
        public Dictionary<string, ConsoleTableNode> RootNodes { get; set; } = new();  // Root-level groups
    }
}