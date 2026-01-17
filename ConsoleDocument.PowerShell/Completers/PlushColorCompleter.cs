using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Language;
using ConsoleDocumentSystem.Enums;

namespace ConsoleDocument.PowerShell.Completers
{
    /// <summary>
    /// Provides tab completion for PlushColor enum values.
    /// </summary>
    public class PlushColorCompleter : IArgumentCompleter
    {
        public IEnumerable<CompletionResult> CompleteArgument(
            string commandName,
            string parameterName,
            string wordToComplete,
            CommandAst commandAst,
            IDictionary fakeBoundParameters)
        {
            var colorNames = Enum.GetNames(typeof(PlushColor));

            foreach (var colorName in colorNames)
            {
                if (string.IsNullOrEmpty(wordToComplete) ||
                    colorName.StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase))
                {
                    yield return new CompletionResult(
                        colorName,
                        colorName,
                        CompletionResultType.ParameterValue,
                        $"PlushColor: {colorName}");
                }
            }
        }
    }
}
