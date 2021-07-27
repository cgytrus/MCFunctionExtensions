using System;
using System.Collections.Generic;

namespace MCFunctionExtensions.Features {
    public class AnonymousFunctionsFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            const string functionCommand = "function";
            foreach(string line in readLines) {
                bool isExecute = line.StartsWith("execute", StringComparison.InvariantCulture) ||
                                 line.StartsWith("else", StringComparison.InvariantCulture);

                string useLine = line;
                if(isExecute) {
                    int runIndex = line.IndexOf("run", StringComparison.InvariantCulture);
                    if(runIndex >= 0) useLine = line[(runIndex + 4)..]; // +4 comes from "run" + a space
                }
                
                if(!useLine.StartsWith(functionCommand, StringComparison.InvariantCulture)) {
                    newLines.Add(line);
                    continue;
                }

                int functionIdIndex = functionCommand.Length + 1;
                string function = functionIdIndex >= line.Length ? "" : useLine[functionIdIndex..].TrimEnd().Trim('{');
                if(!string.IsNullOrWhiteSpace(function)) {
                    newLines.Add(line);
                    continue;
                }

                string functionId = Program.GetGeneratedFunctionId(options.useNamespace, "anonymous");
                newLines.Add(line.Replace(functionCommand, $"{functionCommand} {functionId}"));
            }
        }
    }
}
