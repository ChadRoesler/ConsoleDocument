using System.Management.Automation;
using ConsoleDocumentSystem.Helpers;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Stops the live region renderer, completing any active progress bars or live updates.
    /// </summary>
    /// <example>
    /// <code>
    /// # Stop live rendering after progress completes
    /// Stop-ConsoleLiveRegion
    /// </code>
    /// </example>
    [Cmdlet(VerbsLifecycle.Stop, "ConsoleLiveRegion")]
    [OutputType(typeof(void))]
    public class StopConsoleLiveRegionCmdlet : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                LiveRegionRenderer.StopAsync().GetAwaiter().GetResult();
                WriteVerbose("Live region renderer stopped.");
            }
            catch (Exception ex)
            {
                WriteWarning($"Error stopping live region renderer: {ex.Message}");
            }
        }
    }
}
