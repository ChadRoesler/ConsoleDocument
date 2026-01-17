using System.Management.Automation;
using ConsoleDocumentSystem.Helpers;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Renders a ConsoleDocument to the console output.
    /// </summary>
    /// <example>
    /// <code>
    /// $doc | Write-ConsoleDocument
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// # Full pipeline example
    /// New-ConsoleDocument -Width 120 -EnableVT |
    ///     Add-ConsoleTable -Title "Data" -Columns @("A", "B") -Rows @(@("1", "2")) |
    ///     Write-ConsoleDocument
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommunications.Write, "ConsoleDocument")]
    [OutputType(typeof(void))]
    public class WriteConsoleDocumentCmdlet : ConsoleDocumentCmdletBase
    {
        /// <summary>
        /// Wait for live regions (like progress bars) to complete before returning.
        /// </summary>
        [Parameter]
        public SwitchParameter Wait { get; set; }

        protected override void ProcessRecord()
        {
            if (Document == null) return;

            try
            {
                Document.Render();

                if (Wait.IsPresent)
                {
                    // Wait for all live regions to complete
                    LiveRegionRenderer.StopAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "RenderError",
                    ErrorCategory.WriteError,
                    Document));
            }
        }

        protected override void StopProcessing()
        {
            // Handle Ctrl+C gracefully
            try
            {
                LiveRegionRenderer.StopAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore errors during stop
            }
            base.StopProcessing();
        }
    }
}
