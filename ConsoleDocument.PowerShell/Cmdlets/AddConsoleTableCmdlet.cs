using System.Data;
using System.Management.Automation;
using ConsoleDocumentSystem.Models;
using ConsoleDocumentSystem.Models.Parts;

namespace ConsoleDocument.PowerShell.Cmdlets
{
    /// <summary>
    /// Adds a table block to a ConsoleDocument.
    /// </summary>
    /// <example>
    /// <code>
    /// $doc | Add-ConsoleTable -Title "Users" -Data $dataTable
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// $doc | Add-ConsoleTable -Title "Config" -Columns @("Category", "Setting", "Value") -Rows @(
    ///     @("Network", "Timeout", "30"),
    ///     @("Network", "Retries", "3"),
    ///     @("Display", "Theme", "Dark")
    /// )
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "ConsoleTable", DefaultParameterSetName = "FromRows")]
    [OutputType(typeof(ConsoleDocumentSystem.ConsoleDocument))]
    public class AddConsoleTableCmdlet : ConsoleDocumentCmdletBase
    {
        /// <summary>
        /// The title to display above the table.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Column names for the table.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FromRows", Position = 2)]
        [ValidateNotNull]
        public string[]? Columns { get; set; }

        /// <summary>
        /// Rows of data. Each row is an array of strings matching the column count.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FromRows", Position = 3)]
        [ValidateNotNull]
        public string[][]? Rows { get; set; }

        /// <summary>
        /// A DataTable to use as the table source.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FromDataTable", Position = 2)]
        [ValidateNotNull]
        public DataTable? Data { get; set; }

        /// <summary>
        /// An existing ConsoleTableHierarchy object.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FromHierarchy", Position = 2)]
        [ValidateNotNull]
        public ConsoleTableHierarchy? Hierarchy { get; set; }

        protected override void ProcessRecord()
        {
            if (Document == null) return;

            ConsoleTableHierarchy tableHierarchy;

            switch (ParameterSetName)
            {
                case "FromHierarchy":
                    tableHierarchy = Hierarchy!;
                    break;

                case "FromDataTable":
                    tableHierarchy = BuildFromDataTable(Data!);
                    break;

                case "FromRows":
                default:
                    tableHierarchy = BuildFromRows(Columns!, Rows!);
                    break;
            }

            var table = new ConsoleTable(Title, tableHierarchy);
            Document.Blocks.Add(table);

            WriteDocumentIfPassThru();
        }

        private static ConsoleTableHierarchy BuildFromRows(string[] columns, string[][] rows)
        {
            var hierarchy = new ConsoleTableHierarchy
            {
                Columns = [.. columns]
            };

            foreach (var row in rows)
            {
                if (row.Length != columns.Length)
                {
                    throw new ArgumentException($"Row has {row.Length} values but expected {columns.Length} columns.");
                }

                AddRowToHierarchy(hierarchy, row);
            }

            return hierarchy;
        }

        private static ConsoleTableHierarchy BuildFromDataTable(DataTable dataTable)
        {
            var hierarchy = new ConsoleTableHierarchy();

            // Extract column names
            foreach (DataColumn col in dataTable.Columns)
            {
                hierarchy.Columns.Add(col.ColumnName);
            }

            // Add rows
            foreach (DataRow row in dataTable.Rows)
            {
                var values = new string[dataTable.Columns.Count];
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    values[i] = row[i]?.ToString() ?? string.Empty;
                }
                AddRowToHierarchy(hierarchy, values);
            }

            return hierarchy;
        }

        private static void AddRowToHierarchy(ConsoleTableHierarchy hierarchy, string[] rowValues)
        {
            if (rowValues.Length < 2) return;

            var rootKey = rowValues[0];
            if (!hierarchy.RootNodes.TryGetValue(rootKey, out var currentNode))
            {
                currentNode = new ConsoleTableNode { Key = rootKey, Depth = 0 };
                hierarchy.RootNodes[rootKey] = currentNode;
            }

            // Navigate/create intermediate nodes
            for (int i = 1; i < rowValues.Length - 1; i++)
            {
                var key = rowValues[i];
                if (!currentNode.Children.TryGetValue(key, out var childNode))
                {
                    childNode = new ConsoleTableNode { Key = key, Depth = i };
                    currentNode.Children[key] = childNode;
                }
                currentNode = childNode;
            }

            // Add the final value as a leaf
            currentNode.Values.Add(rowValues[^1]);
        }
    }
}
