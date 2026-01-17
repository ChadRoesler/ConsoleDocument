using System.Management.Automation;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Parts;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Adds a tree diagram block to a ConsoleDocument.
    /// </summary>
    /// <example>
    /// <code>
    /// # Create tree nodes using hashtables
    /// $root = @{
    ///     Text = "Root"
    ///     Children = @(
    ///         @{ Text = "Child 1"; Children = @(@{ Text = "Grandchild" }) },
    ///         @{ Text = "Child 2" }
    ///     )
    /// }
    /// $doc | Add-ConsoleTreeDiagram -Title "File Structure" -RootNode $root
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "ConsoleTreeDiagram")]
    [OutputType(typeof(ConsoleDocumentSystem.ConsoleDocument))]
    public class AddConsoleTreeDiagramCmdlet : ConsoleDocumentCmdletBase
    {
        /// <summary>
        /// The title to display above the tree.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The root node of the tree. Can be a ConsoleNode object or a hashtable with Text and Children properties.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "FromObject")]
        [ValidateNotNull]
        public PSObject? RootNode { get; set; }

        /// <summary>
        /// An existing ConsoleNode object to use as the root.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "FromConsoleNode")]
        [ValidateNotNull]
        public ConsoleNode? Node { get; set; }

        protected override void ProcessRecord()
        {
            if (Document == null) return;

            ConsoleNode rootNode;

            if (ParameterSetName == "FromConsoleNode")
            {
                rootNode = Node!;
            }
            else
            {
                rootNode = ConvertToConsoleNode(RootNode!);
            }

            var tree = new ConsoleTreeDiagram(Title, rootNode);
            Document.Blocks.Add(tree);

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
