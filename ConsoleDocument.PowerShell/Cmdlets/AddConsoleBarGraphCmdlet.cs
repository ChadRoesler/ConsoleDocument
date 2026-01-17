using System.Management.Automation;
using ConsoleDocumentSystem.Enums;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Parts;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Adds a horizontal bar graph block to a ConsoleDocument.
    /// </summary>
    /// <example>
    /// <code>
    /// $data = @(
    ///     @{ Text = "Apples"; Value = 150 },
    ///     @{ Text = "Oranges"; Value = 90 },
    ///     @{ Text = "Bananas"; Value = 200 }
    /// )
    /// $doc | Add-ConsoleBarGraph -Title "Fruit Sales" -Segments $data
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// # With custom colors
    /// $data = @(
    ///     @{ Text = "Success"; Value = 85; Color = "Green" },
    ///     @{ Text = "Warning"; Value = 10; Color = "Yellow" },
    ///     @{ Text = "Error"; Value = 5; Color = "Red" }
    /// )
    /// $doc | Add-ConsoleBarGraph -Title "Status Distribution" -Segments $data
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "ConsoleBarGraph")]
    [OutputType(typeof(ConsoleDocumentSystem.ConsoleDocument))]
    public class AddConsoleBarGraphCmdlet : ConsoleDocumentCmdletBase
    {
        /// <summary>
        /// The title to display above the bar graph.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The data segments for the bar graph. Each segment should have Text and Value properties, and optionally a Color property.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2)]
        [ValidateNotNull]
        public PSObject[]? Segments { get; set; }

        protected override void ProcessRecord()
        {
            if (Document == null || Segments == null) return;

            var graphSegments = new List<ConsoleGraphSegment>();

            foreach (var segment in Segments)
            {
                graphSegments.Add(ConvertToGraphSegment(segment));
            }

            var barGraph = new ConsoleBarGraph(Title, graphSegments);
            Document.Blocks.Add(barGraph);

            WriteDocumentIfPassThru();
        }

        private static ConsoleGraphSegment ConvertToGraphSegment(PSObject psObject)
        {
            // If it's already a ConsoleGraphSegment, return it
            if (psObject.BaseObject is ConsoleGraphSegment existing)
            {
                return existing;
            }

            string text = psObject.Properties["Text"]?.Value?.ToString() ?? "Item";
            int value = 0;

            var valueProp = psObject.Properties["Value"];
            if (valueProp?.Value != null)
            {
                value = Convert.ToInt32(valueProp.Value);
            }

            // Check for optional color
            var colorProp = psObject.Properties["Color"];
            if (colorProp?.Value != null)
            {
                var colorValue = colorProp.Value;
                PlushColor color;

                if (colorValue is PlushColor pc)
                {
                    color = pc;
                }
                else if (Enum.TryParse<PlushColor>(colorValue.ToString(), true, out var parsedColor))
                {
                    color = parsedColor;
                }
                else
                {
                    color = PlushColor.Yellow; // Default
                }

                return new ConsoleGraphSegment(text, color, value);
            }

            return new ConsoleGraphSegment(text, value);
        }
    }
}
