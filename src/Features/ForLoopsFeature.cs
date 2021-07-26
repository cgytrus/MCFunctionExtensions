using System.Collections.Generic;
using System.Text;

namespace MCFunctionExtensions.Features {
    public class ForLoopsFeature : ExecuteArgumentFeature {
        protected override bool ProcessArg(ICollection<string> newLines, IReadOnlyList<string> args, int i, int line) {
            string arg = args[i];
            string nextArg = i < args.Count - 1 ? args[i + 1] : null;
            if(arg != "for") return false;

            if(nextArg == null) throw new FunctionExtensionErrorException(line + 1, "'for' argument placed last.");
            if(!int.TryParse(nextArg, out int repeatTimes))
                throw new FunctionExtensionErrorException(line + 1, "'for' must be an integer");

            string reconstructedLine = ReconstructLine(args).ToString();
            for(int k = 0; k < repeatTimes; k++) newLines.Add(reconstructedLine);

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
