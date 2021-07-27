using System;
using System.Collections.Generic;
using System.Text;

namespace MCFunctionExtensions.Features {
    public class ConstantsFeature : IFeature {
        private static readonly Dictionary<string, string> constants = new();
        
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            for(int i = 0; i < readLines.Count; i++) {
                string line = readLines[i];
                ProcessLine(newLines, line, i);
            }
        }
        
        private static void ProcessLine(ICollection<string> newLines, string line, int index) {
            const string prefix = "const value";
            
            if(!line.StartsWith(prefix, StringComparison.InvariantCulture)) {
                IEnumerable<string> populatedLines = PopulateLines(line, index);
                if(populatedLines is null) newLines.Add(PopulateLine(line));
                else
                    foreach(string populatedLine in populatedLines)
                        newLines.Add(populatedLine);
                return;
            }
            
            string[] args = line[(prefix.Length + 1)..].Split(' ');
            string constName = args[0];
            
            StringBuilder builder = new();
            for(int i = 1; i < args.Length; i++) {
                builder.Append(args[i]);
                if(i < args.Length - 1) builder.Append(' ');
            }
            
            constants[constName] = builder.ToString();
        }

        private static string PopulateLine(string line) {
            foreach((string name, string value) in constants)
                line = line.Replace("#!" + name, value);

            return line;
        }

        private static IEnumerable<string> PopulateLines(string line, int index) {
            string[] splitLine = line.Split(' ');
            
            foreach((string name, (string[] args, IList<string> lines)) in CustomCommandsFeature.commands) {
                IList<string> newLines = new List<string>(lines);
                if(!TryPopulateLinesWithCustomCommand(index, splitLine, name, args, newLines)) continue;
                return newLines;
            }

            return null;
        }

        private static bool TryPopulateLinesWithCustomCommand(int index, string[] splitLine, string name,
            IReadOnlyList<string> args, IList<string> lines) {
            if(splitLine[0] != name) return false;
            
            if(splitLine.Length - 1 < args.Count) {
                string expectedArguments = $"expected {args.Count}, got {splitLine.Length - 1}";
                throw new FunctionExtensionErrorException(index + 1,
                    $"Low number of arguments for function '{name}': {expectedArguments}.");
            }

            string[] passedArgs = new string[args.Count];
            for(int i = 0; i < args.Count; i++) {
                if(i < args.Count - 1) passedArgs[i] = splitLine[i + 1];
                else passedArgs[i] = string.Join(' ', splitLine[(i + 1)..]);
            }

            for(int i = 0; i < lines.Count; i++)
                for(int j = 0; j < args.Count; j++)
                    lines[i] = lines[i].Replace("#$" + args[j], passedArgs[j]);
            
            return true;
        }
    }
}
