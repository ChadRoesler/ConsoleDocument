namespace ConsoleDocumentSystem.Models.Parts
{
    public class ConsoleNode
    {
        public ConsoleNode(string text)
        {
            Text = text;
            ConsoleNodes = [];
        }
        public ConsoleNode(string text, List<ConsoleNode> nodes)
        {
            Text = text;
            ConsoleNodes = nodes;
        }
        public List<ConsoleNode> ConsoleNodes { get; set; }
        public string Text { get; set; }
    }
}
