using System.Management.Automation;
using ConsoleDocument.PowerShell.Completers;
using ConsoleDocumentSystem;
using ConsoleDocumentSystem.Enums;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Creates a new ConsoleDocument object for building rich console output.
    /// </summary>
    /// <example>
    /// <code>
    /// $doc = New-ConsoleDocument -Width 120 -EnableVT
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.New, "ConsoleDocument")]
    [OutputType(typeof(ConsoleDocumentSystem.ConsoleDocument))]
    public class NewConsoleDocumentCmdlet : PSCmdlet
    {
        /// <summary>
        /// The width of the console document in characters. Must be between 50 and 200.
        /// </summary>
        [Parameter(Position = 0)]
        [ValidateRange(50, 200)]
        public int Width { get; set; } = 100;

        /// <summary>
        /// Enable Virtual Terminal (VT/ANSI) sequences for rich formatting and colors.
        /// </summary>
        [Parameter]
        public SwitchParameter EnableVT { get; set; }

        /// <summary>
        /// The default text color for the document.
        /// </summary>
        [Parameter]
        [ArgumentCompleter(typeof(PlushColorCompleter))]
        public PlushColor TextColor { get; set; } = PlushColor.DefaultForeground;

        /// <summary>
        /// The color used for borders.
        /// </summary>
        [Parameter]
        [ArgumentCompleter(typeof(PlushColorCompleter))]
        public PlushColor BorderColor { get; set; } = PlushColor.DarkGray;

        /// <summary>
        /// The color used for tree node elements.
        /// </summary>
        [Parameter]
        [ArgumentCompleter(typeof(PlushColorCompleter))]
        public PlushColor TreeNodeColor { get; set; } = PlushColor.Green;

        /// <summary>
        /// The color used for bar graph elements.
        /// </summary>
        [Parameter]
        [ArgumentCompleter(typeof(PlushColorCompleter))]
        public PlushColor BarGraphColor { get; set; } = PlushColor.Yellow;

        /// <summary>
        /// The color used for progress bar elements.
        /// </summary>
        [Parameter]
        [ArgumentCompleter(typeof(PlushColorCompleter))]
        public PlushColor ProgressBarColor { get; set; } = PlushColor.Coral;

        protected override void ProcessRecord()
        {
            try
            {
                var doc = new ConsoleDocumentSystem.ConsoleDocument(Width, EnableVT.IsPresent)
                {
                    TextColor = TextColor,
                    BorderColor = BorderColor,
                    TreeNodeColor = TreeNodeColor,
                    BarGraphColor = BarGraphColor,
                    ProgressBarColor = ProgressBarColor
                };

                WriteObject(doc);
            }
            catch (ArgumentException ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "InvalidDocumentParameters",
                    ErrorCategory.InvalidArgument,
                    null));
            }
        }
    }
}
