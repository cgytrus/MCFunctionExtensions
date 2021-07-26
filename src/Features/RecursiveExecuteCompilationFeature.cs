using System;
using System.Collections.Generic;
using System.Linq;

namespace MCFunctionExtensions.Features {
    public class RecursiveExecuteCompilationFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            foreach(string line in readLines) {
                if(!line.StartsWith("execute", StringComparison.InvariantCulture)) {
                    newLines.Add(line);
                    continue;
                }

                int runIndex = line.IndexOf("run", StringComparison.InvariantCulture);
                if(runIndex < 0) {
                    newLines.Add(line);
                    continue;
                }
                runIndex += 4;

                string preCommand = line.Substring(0, runIndex);
                string command = line.Substring(runIndex);
                
                IEnumerable<string> compiledLines = Program.CompileFunction(options, new[] { command });
                newLines.AddRange(compiledLines.Select(compiledLine => preCommand + compiledLine));
            }
        }
    }
}
