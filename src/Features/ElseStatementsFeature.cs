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

            const string playerName = "ElseStatement";

            string storePart = $"execute store success score {playerName} {Program.InternalObjectiveName}";

            if(isPrevExecute) {
                lines.Add($"scoreboard objectives add {Program.InternalObjectiveName} dummy");
                lines.Add($"scoreboard players set {playerName} {Program.InternalObjectiveName} 0");
                lines.Add($"execute {storePart} {prevLine.Substring(8)}");
            }

            string elseStart = $"execute if score {playerName} {Program.InternalObjectiveName} matches 0";
            string endStore = isNextElse ? $"run execute {storePart} " : "";
            lines.Add($"{elseStart} {endStore}{line.Substring(5)}");

            return lines;
        }
    }
}
