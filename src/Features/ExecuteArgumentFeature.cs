using System;
using System.Collections.Generic;

namespace MCFunctionExtensions.Features {
    public abstract class ExecuteArgumentFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            for(int i = 0; i < readLines.Count; i++) {
                string line = readLines[i];
                string[] args = line.Split(' ');

                ProcessLine(newLines, line, args, i);
            }
        }
        
        private void ProcessLine(ICollection<string> newLines, string line, IReadOnlyList<string> args,
            int lineIndex) {
            if(!line.StartsWith("execute", StringComparison.InvariantCulture)) {
                newLines.Add(line);
                return;
            }

            bool containsArg = false;
            for(int j = 0; j < args.Count; j++) {
                if(args[j] == "run") break;
                containsArg |= ProcessArg(newLines, args, j, lineIndex);
            }

            if(!containsArg) newLines.Add(line);
        }

        protected abstract bool ProcessArg(ICollection<string> newLines, IReadOnlyList<string> args, int i, int line);
    }
}
