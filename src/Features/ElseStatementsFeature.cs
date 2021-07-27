using System;
using System.Collections.Generic;

namespace MCFunctionExtensions.Features {
    public class ElseStatementsFeature : IFeature {
        private const string PlayerName = "ElseStatement";
        
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            for(int i = 0; i < readLines.Count; i++) {
                string prevLine = i > 0 ? readLines[i - 1] : null;
                string line = readLines[i];
                string nextLine = i < readLines.Count - 1 ? readLines[i + 1] : null;

                if(line.StartsWith("else", StringComparison.InvariantCulture))
                    newLines.AddRange(ParseElseStatement(line, options, prevLine, i));
                else if(!line.StartsWith("execute", StringComparison.InvariantCulture) || nextLine == null ||
                        !nextLine.StartsWith("else", StringComparison.InvariantCulture)) newLines.Add(line);
            }
        }
        
        private static IEnumerable<string> ParseElseStatement(string line, Options options, string prevLine, int i) {
            if(string.IsNullOrWhiteSpace(prevLine))
                throw new FunctionExtensionErrorException(0, "'else' statement is placed on the first line.");
            
            bool isPrevExecute = prevLine.StartsWith("execute", StringComparison.InvariantCulture);
            bool isPrevElse = prevLine.StartsWith("else", StringComparison.InvariantCulture);

            if(!isPrevExecute && !isPrevElse) throw new FunctionExtensionErrorException(i + 1,
                "'else' statement doesn't have an execute or another 'else' statement before it.");

            List<string> lines = new(3);

            if(isPrevExecute) {
                AppendScoreboardSetLine(prevLine, options, i);
                lines.Add($"scoreboard objectives add {Program.InternalName} dummy");
                lines.Add($"scoreboard players reset {PlayerName} {Program.InternalName}");
                lines.Add($"execute {prevLine.Substring(8)}");
            }

            AppendScoreboardSetLine(line, options, i);
            string elseStart = $"execute unless score {PlayerName} {Program.InternalName} matches 0..";
            lines.Add($"{elseStart} {line.Substring(5)}");

            return lines;
        }

        private static string GetExecuteRunFunction(string line, int lineIndex) {
            int runIndex = line.IndexOf("run", StringComparison.InvariantCulture);
            if(runIndex < 0)
                throw new FunctionExtensionErrorException(lineIndex + 1, "Conditional statements require 'run'.");
            int runCommandIndex = runIndex + 4;
            if(runCommandIndex >= line.Length ||
               !line[runCommandIndex..].StartsWith("function", StringComparison.InvariantCulture))
                throw new FunctionExtensionErrorException(lineIndex + 1,
                    "Conditional expressions only support 'run function'.");
            return line[(runCommandIndex + "function ".Length)..];
        }

        private static void AppendScoreboardSetLine(string line, Options options, int lineIndex) {
            string functionId = GetExecuteRunFunction(line, lineIndex);
            if(!Program.PostCompileAppend(functionId, options,
                new string[] { "", $"scoreboard players set {PlayerName} {Program.InternalName} 0" }))
                throw new FunctionExtensionErrorException(lineIndex + 1,
                    "Tried using a function that is not in the current namespace in a conditional statement.");
        }
    }
}
