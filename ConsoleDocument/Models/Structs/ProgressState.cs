namespace ConsoleDocumentSystem.Models.Structs
{
    public readonly record struct ProgressState(long Current, long Total, string Status);
}
