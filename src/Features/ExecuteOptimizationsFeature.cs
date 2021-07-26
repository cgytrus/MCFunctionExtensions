using System;
using System.Collections.Generic;

namespace MCFunctionExtensions.Features {
    public class ExecuteOptimizationsFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            const string executeRun = "execute run ";
            // ReSharper disable once ForCanBeConvertedToForeach
            for(int i = 0; i < readLines.Count; i++) {
                string line = readLines[i];
                while(line.StartsWith(executeRun, StringComparison.InvariantCulture))
                    line = line.Substring(executeRun.Length);
                newLines.Add(line);
            }
        }
    }
}
