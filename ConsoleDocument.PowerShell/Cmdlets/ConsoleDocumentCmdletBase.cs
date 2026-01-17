using System.Management.Automation;
using ConsoleDocumentSystem;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Base class for cmdlets that operate on a ConsoleDocument pipeline object.
    /// </summary>
    public abstract class ConsoleDocumentCmdletBase : PSCmdlet
    {
        /// <summary>
        /// The ConsoleDocument object to operate on.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        [ValidateNotNull]
        public ConsoleDocumentSystem.ConsoleDocument? Document { get; set; }

        /// <summary>
        /// Pass the document through the pipeline after processing.
        /// </summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Writes the document to the pipeline if PassThru is specified.
        /// </summary>
        protected void WriteDocumentIfPassThru()
        {
            if (PassThru.IsPresent && Document != null)
            {
                WriteObject(Document);
            }
        }
    }
}
