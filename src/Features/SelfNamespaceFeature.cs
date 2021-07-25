using System.Collections.Generic;
using System.Linq;

namespace MCFunctionExtensions.Features {
    public class SelfNamespaceFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) => newLines.AddRange(
            readLines.Select(useLine =>
                useLine.Replace("function :", $"function {options.useNamespace}:")
                    .Replace("function #:", $"function #{options.useNamespace}:")));
    }
}
