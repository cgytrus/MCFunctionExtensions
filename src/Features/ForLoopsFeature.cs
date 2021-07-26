using System;
using System.Collections.Generic;
using System.Text;

namespace MCFunctionExtensions.Features {
    public class ForLoopsFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            for(int i = 0; i < readLines.Count; i++) {
                string line = readLines[i];
                string[] args = line.Split(' ');

                ProcessLine(newLines, line, args, i);
            }
        }
        
        private static void ProcessLine(ICollection<string> newLines, string line, IReadOnlyList<string> args,
            int lineIndex) {
            if(!line.StartsWith("execute", StringComparison.InvariantCulture)) {
                newLines.Add(line);
                return;
            }

            bool containsFor = false;
            for(int j = 0; j < args.Count; j++) {
                if(args[j] == "run") break;
                containsFor |= ProcessArg(newLines, args, j, lineIndex);
            }

            if(!containsFor) newLines.Add(line);
        }

        private static bool ProcessArg(ICollection<string> newLines, IReadOnlyList<string> args, int i, int line) {
            string arg = args[i];
            string nextArg = i < args.Count - 1 ? args[i + 1] : null;
            if(arg != "for") return false;

            if(nextArg == null) throw new FunctionExtensionErrorException(line + 1, "'for' argument placed last.");
            if(!int.TryParse(nextArg, out int repeatTimes))
                throw new FunctionExtensionErrorException(line + 1, "'for' must be an integer");

            for(int k = 0; k < repeatTimes; k++) {
                StringBuilder reconstructedLine = ReconstructLine(args);
                newLines.Add(reconstructedLine.ToString());
            }

            return true;
        }

        private static StringBuilder ReconstructLine(IReadOnlyList<string> args) {
            StringBuilder reconstructedLine = new();
            bool end = false;
            for(int i = 0; i < args.Count; i++) {
                string arg = args[i];
                if(arg == "run") end = true;
                else if(!end && arg == "for") {
                    i++;
                    continue;
                }

                reconstructedLine.Append(arg);
                if(i < args.Count - 1) reconstructedLine.Append(' ');
            }

            return reconstructedLine;
        }
    }
}
