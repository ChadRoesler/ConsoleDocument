using ConsoleDocumentSystem.Models.Parts;
using System.Data;

namespace ConsoleDocumentSystem.ExtensionMethods
{
    public static class DataTableExtensions
    {
        public static ConsoleTableHierarchy ToConsoleTableHierarchy(this DataTable table)
        {
            var hierarchy = new ConsoleTableHierarchy
            {
                Columns = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList()
            };

            foreach (DataRow row in table.Rows)
            {
                var current = hierarchy.RootNodes;
                ConsoleTableNode? parent = null;

                for (int col = 0; col < table.Columns.Count; col++)
                {
                    string key = row[col]?.ToString() ?? "";

                    if (col == table.Columns.Count - 1)
                    {
                        // Last column → store as leaf
                        if (parent != null)
                            parent.Values.Add(key);
                    }
                    else
                    {
                        // Not last column → ensure node exists
                        if (!current.TryGetValue(key, out var node))
                        {
                            node = new ConsoleTableNode
                            {
                                Key = key,
                                Depth = col
                            };
                            current[key] = node;
                        }
                        parent = node;
                        current = node.Children;
                    }
                }
            }

            return hierarchy;
        }

    }
}
