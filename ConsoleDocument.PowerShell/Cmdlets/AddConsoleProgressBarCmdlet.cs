using System.Management.Automation;
using ConsoleDocument.PowerShell.Completers;
using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Structs;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Adds a progress bar block to a ConsoleDocument.
    /// </summary>
    /// <example>
    /// <code>
    /// # Simple static progress
    /// $doc | Add-ConsoleProgressBar -Title "Download" -Current 50 -Total 100 -Status "Downloading..."
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// # Dynamic progress with a task
    /// $progress = @{ Current = 0; Total = 100; Status = "Starting..." }
    /// $task = [System.Threading.Tasks.Task]::Run({
    ///     1..100 | ForEach-Object { 
    ///         $progress.Current = $_
    ///         $progress.Status = "Processing item $_"
    ///         Start-Sleep -Milliseconds 50
    ///     }
    /// })
    /// $doc | Add-ConsoleProgressBar -Title "Processing" -ProgressSource $progress -Task $task
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "ConsoleProgressBar", DefaultParameterSetName = "Static")]
    [OutputType(typeof(ConsoleDocumentSystem.ConsoleDocument))]
    public class AddConsoleProgressBarCmdlet : ConsoleDocumentCmdletBase
    {
        /// <summary>
        /// The title to display above the progress bar.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional description text below the title.
        /// </summary>
        [Parameter]
        public string? Description { get; set; }

        /// <summary>
        /// Current progress value (static progress mode).
        /// </summary>
        [Parameter(ParameterSetName = "Static", Position = 2)]
        public long Current { get; set; }

        /// <summary>
        /// Total progress value (static progress mode).
        /// </summary>
        [Parameter(ParameterSetName = "Static", Position = 3)]
        public long Total { get; set; } = 100;

        /// <summary>
        /// Status message to display (static progress mode).
        /// </summary>
        [Parameter(ParameterSetName = "Static")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// A hashtable or object with Current, Total, and Status properties that will be polled for progress updates.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Dynamic")]
        [ValidateNotNull]
        public PSObject? ProgressSource { get; set; }

        /// <summary>
        /// The task to monitor for completion (dynamic progress mode).
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Dynamic")]
        [ValidateNotNull]
        public System.Threading.Tasks.Task? Task { get; set; }

        /// <summary>
        /// Color for the filled portion of the progress bar.
        /// </summary>
        [Parameter]
        [ArgumentCompleter(typeof(PlushColorCompleter))]
        public PlushColor BarColor { get; set; } = PlushColor.Green;

        /// <summary>
        /// Color for the empty portion of the progress bar.
        /// </summary>
        [Parameter]
        [ArgumentCompleter(typeof(PlushColorCompleter))]
        public PlushColor EmptyColor { get; set; } = PlushColor.DarkGray;

        protected override void ProcessRecord()
        {
            if (Document == null) return;

            Func<ProgressState> progressProvider;
            System.Threading.Tasks.Task workTask;

            if (ParameterSetName == "Dynamic")
            {
                progressProvider = () => GetProgressFromSource(ProgressSource!);
                workTask = Task!;
            }
            else
            {
                // Static mode - create a completed task and fixed progress
                var staticState = new ProgressState(Current, Total, Status);
                progressProvider = () => staticState;
                workTask = System.Threading.Tasks.Task.CompletedTask;
            }

            var progressBar = new ConsoleProgressBar(
                Title,
                Document.VTEnabled,
                progressProvider,
                workTask,
                Description)
            {
                BarColor = BarColor,
                EmptyColor = EmptyColor,
                TextColor = Document.TextColor
            };

            Document.Blocks.Add(progressBar);

            WriteDocumentIfPassThru();
        }

        private static ProgressState GetProgressFromSource(PSObject source)
        {
            long current = 0;
            long total = 100;
            string status = string.Empty;

            // Try to get Current property
            var currentProp = source.Properties["Current"];
            if (currentProp?.Value != null)
            {
                current = Convert.ToInt64(currentProp.Value);
            }

            // Try to get Total property
            var totalProp = source.Properties["Total"];
            if (totalProp?.Value != null)
            {
                total = Convert.ToInt64(totalProp.Value);
            }

            // Try to get Status property
            var statusProp = source.Properties["Status"];
            if (statusProp?.Value != null)
            {
                status = statusProp.Value.ToString() ?? string.Empty;
            }

            return new ProgressState(current, total, status);
        }
    }
}
