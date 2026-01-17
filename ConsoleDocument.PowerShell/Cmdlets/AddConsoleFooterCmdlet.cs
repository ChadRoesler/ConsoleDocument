using System.Management.Automation;
using ConsoleDocumentSystem.Models;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Adds a decorative footer block to a ConsoleDocument.
    /// </summary>
    /// <example>
    /// <code>
    /// $doc | Add-ConsoleFooter -Text "End of Report"
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "ConsoleFooter")]
    [OutputType(typeof(ConsoleDocumentSystem.ConsoleDocument))]
    public class AddConsoleFooterCmdlet : ConsoleDocumentCmdletBase
    {
        /// <summary>
        /// The text to display in the footer.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string Text { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            if (Document == null) return;

            var footer = new ConsoleFooter(Text);
            Document.Blocks.Add(footer);

            WriteDocumentIfPassThru();
        }
    }
}
