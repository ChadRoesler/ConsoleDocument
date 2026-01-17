using System.Management.Automation;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Parts;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Adds an outline block (hierarchical bullet list) to a ConsoleDocument.
    /// </summary>
    /// <example>
    /// <code>
    /// $outline = @(
    ///     @{ Text = "Introduction"; Children = @(
    ///         @{ Text = "Background" },
    ///         @{ Text = "Objectives" }
    ///     )},
    ///     @{ Text = "Methods" },
    ///     @{ Text = "Results" }
    /// )
    /// $doc | Add-ConsoleOutline -Title "Report Structure" -Nodes $outline
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "ConsoleOutline")]
    [OutputType(typeof(ConsoleDocumentSystem.ConsoleDocument))]
    public class AddConsoleOutlineCmdlet : ConsoleDocumentCmdletBase
    {
        /// <summary>
        /// The title to display above the outline.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The nodes of the outline. Can be ConsoleNode objects or hashtables with Text and Children properties.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2)]
        [ValidateNotNull]
        public PSObject[]? Nodes { get; set; }

        protected override void ProcessRecord()
        {
            if (Document == null || Nodes == null) return;

            var consoleNodes = new List<ConsoleNode>();
            foreach (var node in Nodes)
            {
                consoleNodes.Add(ConvertToConsoleNode(node));
            }

            var outline = new ConsoleOutline(Title, consoleNodes);
            Document.Blocks.Add(outline);

            WriteDocumentIfPassThru();
        }

        private static ConsoleNode ConvertToConsoleNode(PSObject psObject)
        {
            // If it's already a ConsoleNode, return it
            if (psObject.BaseObject is ConsoleNode existingNode)
            {
                return existingNode;
            }

            // Extract Text property
            string text = psObject.Properties["Text"]?.Value?.ToString() ?? "Node";

            var node = new ConsoleNode(text);

            // Extract Children property if present
            var childrenProp = psObject.Properties["Children"];
            if (childrenProp?.Value != null)
            {
                var children = childrenProp.Value;

                if (children is object[] array)
                {
                    foreach (var child in array)
                    {
                        if (child != null)
                        {
                            var childPsObject = child is PSObject pso ? pso : PSObject.AsPSObject(child);
                            node.ConsoleNodes.Add(ConvertToConsoleNode(childPsObject));
                        }
                    }
                }
                else if (children is System.Collections.IEnumerable enumerable)
                {
                    foreach (var child in enumerable)
                    {
                        if (child != null)
                        {
                            var childPsObject = child is PSObject pso ? pso : PSObject.AsPSObject(child);
                            node.ConsoleNodes.Add(ConvertToConsoleNode(childPsObject));
                        }
                    }
                }
            }

            return node;
        }
    }
}
