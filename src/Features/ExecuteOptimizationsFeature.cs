using System;
using System.Collections.Generic;
using System.Linq;

namespace MCFunctionExtensions.Features {
    public class ExecuteOptimizationsFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            const string executeRun = "execute run ";
            newLines.AddRange(readLines.Select(line => line.StartsWith(executeRun, StringComparison.InvariantCulture) ? line.Substring(executeRun.Length) : line));
        }
    }
}
