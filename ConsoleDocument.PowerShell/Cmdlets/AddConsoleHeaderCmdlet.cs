using System.Management.Automation;
using ConsoleDocumentSystem.Models;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Adds a decorative header block to a ConsoleDocument.
    /// </summary>
    /// <example>
    /// <code>
    /// $doc | Add-ConsoleHeader -Text "My Application"
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "ConsoleHeader")]
    [OutputType(typeof(ConsoleDocumentSystem.ConsoleDocument))]
    public class AddConsoleHeaderCmdlet : ConsoleDocumentCmdletBase
    {
        /// <summary>
        /// The text to display in the header.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string Text { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            if (Document == null) return;

            var header = new ConsoleHeader(Text);
            Document.Blocks.Add(header);

            WriteDocumentIfPassThru();
        }
    }
}
