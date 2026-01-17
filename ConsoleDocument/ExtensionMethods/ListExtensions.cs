using ConsoleDocumentSystem.Models.Parts;

namespace ConsoleDocumentSystem.ExtensionMethods
{
    public static class ListExtensions
    {
        /// <summary>
        /// Adds a new item to the list if it does not already exist.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="list">The list to which the item will be added.</param>
        /// <param name="item">The item to add.</param>
        public static List<string> Texts(this List<ConsoleGraphSegment> list)
        {
            return list.Select(x => x.Text).ToList();
        }

        public static List<int> Values(this List<ConsoleGraphSegment> list)
        {
            return list.Select(x => x.Value).ToList();
        }
    }
}
