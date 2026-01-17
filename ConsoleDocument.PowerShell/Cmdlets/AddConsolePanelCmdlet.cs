using System.Management.Automation;
using ConsoleDocumentSystem.Models;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Adds a panel block with a title and body text to a ConsoleDocument.
    /// </summary>
    /// <example>
    /// <code>
    /// $doc | Add-ConsolePanel -Title "Instructions" -Text "Follow these steps carefully..."
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "ConsolePanel")]
    [OutputType(typeof(ConsoleDocumentSystem.ConsoleDocument))]
    public class AddConsolePanelCmdlet : ConsoleDocumentCmdletBase
    {
        /// <summary>
        /// The title to display above the panel content.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The body text of the panel. Long text will be wrapped automatically.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2)]
        [ValidateNotNull]
        public string Text { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            if (Document == null) return;

            var panel = new ConsolePanel(Title, Text);
            Document.Blocks.Add(panel);

            WriteDocumentIfPassThru();
        }
    }
}
