using System;
using System.Collections.Generic;

namespace MCFunctionExtensions.Features {
    public class ConstantsFeature : IFeature {
        private static readonly Dictionary<(string, string), string> constants = new();
        
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            string context = "";
            foreach(string line in readLines) ProcessLine(newLines, line, ref context);
        }
        
        private static void ProcessLine(ICollection<string> newLines, string line, ref string context) {
            if(!line.StartsWith(";", StringComparison.InvariantCulture)) {
                newLines.Add(PopulateLine(line, context));
                return;
            }
            
            string[] args = line.Split(';');

            string actualLine = null;
            string currConstName = null;
            string savedContext = context;
            for(int i = 1; i < args.Length; i++) {
                string arg = args[i];
                if(currConstName is not null) {
                    constants[(context, currConstName)] = arg;
                    currConstName = null;
                }
                else if(arg.StartsWith("#=", StringComparison.InvariantCulture)) context = arg.Substring(2);
                else if(arg.StartsWith("#!=", StringComparison.InvariantCulture)) currConstName = arg.Substring(3);
                else actualLine = PopulateLine(arg, context);
            }

            if(actualLine is null) return;
            newLines.Add(actualLine);
            context = savedContext;
        }

        private static string PopulateLine(string line, string context) {
            foreach(((string constContext, string constName), string constValue) in constants) {
                if(constContext != context) continue;
                line = line.Replace("#!" + constName, constValue);
            }

            return line;
        }
    }
}
