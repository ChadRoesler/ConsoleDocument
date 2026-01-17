using System.Management.Automation;
using ConsoleDocumentSystem.Models;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Adds a separator block to a ConsoleDocument, optionally with centered text.
    /// </summary>
    /// <example>
    /// <code>
    /// # Simple separator line
    /// $doc | Add-ConsoleSeparator
    /// 
    /// # Separator with label
    /// $doc | Add-ConsoleSeparator -Text "Section 2"
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "ConsoleSeparator")]
    [OutputType(typeof(ConsoleDocumentSystem.ConsoleDocument))]
    public class AddConsoleSeparatorCmdlet : ConsoleDocumentCmdletBase
    {
        /// <summary>
        /// Optional text to display centered in the separator.
        /// </summary>
        [Parameter(Position = 1)]
        public string Text { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            if (Document == null) return;

            var separator = new ConsoleSeperator(Text);
            Document.Blocks.Add(separator);

            WriteDocumentIfPassThru();
        }
    }
}
