using System;
using System.Collections.Generic;

namespace MCFunctionExtensions.Features {
    public class AnonymousFunctionsFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            const string functionCommand = "function";
            foreach(string line in readLines) ProcessLine(newLines, options, line, functionCommand);
        }

        private static void ProcessLine(ICollection<string> newLines, Options options, string line,
            string functionCommand) {
            bool isExecute = line.StartsWith("execute", StringComparison.InvariantCulture) ||
                             line.StartsWith("else", StringComparison.InvariantCulture);

            string useLine = line;
            if(isExecute) {
                int runIndex = line.IndexOf("run", StringComparison.InvariantCulture);
                if(runIndex >= 0) useLine = line[(runIndex + 4)..]; // +4 comes from "run" + a space
            }
            else if(line.StartsWith("define ", StringComparison.InvariantCulture)) useLine = line[7..];

            if(!useLine.StartsWith(functionCommand, StringComparison.InvariantCulture)) {
                newLines.Add(line);
                return;
            }

            int functionIdIndex = functionCommand.Length + 1;
            string function = functionIdIndex >= useLine.Length ? "" :
                useLine[functionIdIndex..].TrimEnd().TrimEnd('{');
            if(!string.IsNullOrWhiteSpace(function)) {
                newLines.Add(line);
                return;
            }

            bool isDeclaration = CodeBlocksFeature.IsBlockStart(line, out _);
            string functionId = isDeclaration ? Program.GenerateNextFunctionId(options.useNamespace, "anonymous") :
                Program.lastGeneratedFunctionId;
            newLines.Add(line.Replace(functionCommand, $"{functionCommand} {functionId}"));
        }
    }
}
