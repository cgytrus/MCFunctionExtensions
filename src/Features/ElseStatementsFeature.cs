using System;
using System.Collections.Generic;

namespace MCFunctionExtensions.Features {
    public class ElseStatementsFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            for(int i = 0; i < readLines.Count; i++) {
                string prevLine = i > 0 ? readLines[i - 1] : null;
                string line = readLines[i];
                string nextLine = i < readLines.Count - 1 ? readLines[i + 1] : null;

                if(line.StartsWith("else", StringComparison.InvariantCulture)) {
                    try {
                        newLines.AddRange(ParseElseStatement(options.useNamespace, line, prevLine, nextLine));
                    }
                    catch(FunctionExtensionErrorException ex) {
                        throw new FunctionExtensionErrorException(i + 1, ex.Message, ex);
                    }
                }
                else if(!line.StartsWith("execute", StringComparison.InvariantCulture) || nextLine == null ||
                        !nextLine.StartsWith("else", StringComparison.InvariantCulture)) newLines.Add(line);
            }
        }
        
        private static IEnumerable<string> ParseElseStatement(string funcNamespace, string line, string prevLine,
            string nextLine) {
            if(string.IsNullOrWhiteSpace(prevLine))
                throw new FunctionExtensionErrorException(0, "'else' statement is placed on the first line.");
            
            bool isPrevExecute = prevLine.StartsWith("execute", StringComparison.InvariantCulture);
            bool isPrevElse = prevLine.StartsWith("else", StringComparison.InvariantCulture);

            if(!isPrevExecute && !isPrevElse) throw new FunctionExtensionErrorException(0,
                "'else' statement doesn't have an execute or another 'else' statement before it.");

            List<string> lines = new(3);
            
            bool isNextElse = nextLine?.StartsWith("else", StringComparison.InvariantCulture) ?? false;

            string storageTarget = $"{funcNamespace}:mcfunctionext";
            const string storagePath = "ElseStatement";

            string storePart = $"store success storage {storageTarget} {storagePath} byte 1";

            if(isPrevExecute) {
                lines.Add($"data modify storage {storageTarget} {storagePath} set value 0b");
                lines.Add($"execute {storePart} {prevLine.Substring(8)}");
            }
            
            string endStore = isNextElse ? $"run execute {storePart} " : "";
            lines.Add($"execute if data storage {storageTarget} {{{storagePath}:0b}} {endStore}{line.Substring(5)}");

            return lines;
        }
    }
}
