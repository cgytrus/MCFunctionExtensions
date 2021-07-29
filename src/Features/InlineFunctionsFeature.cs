using System;
using System.Collections.Generic;
using System.IO;

namespace MCFunctionExtensions.Features {
    public class InlineFunctionsFeature : CodeBlocksFeature {
        protected override void BlockEnd(Options options, IEnumerable<string> inlineLines, string trimmedLine) {
            string[] functionId = GetFunction(trimmedLine);
            if(functionId[0] != options.useNamespace)
                throw new FunctionExtensionErrorException(0,
                    "Tried declaring an inline function not in the current namespace.");
            string functionPath = Program.GetFunctionPath(options.functionsPath, functionId[1]);
            Directory.CreateDirectory(Path.GetDirectoryName(functionPath) ?? string.Empty);
            File.WriteAllLines(functionPath, Program.CompileFunction(options, inlineLines));
        }

        protected override void AddOriginalLine(string trimmedLine, ICollection<string> newLines) {
            int keywordIndex = trimmedLine.IndexOf("define ", StringComparison.InvariantCulture);
            switch(keywordIndex) {
                case >= 1: base.AddOriginalLine(trimmedLine[..(keywordIndex - 1)], newLines);
                    break;
                case < 0: base.AddOriginalLine(trimmedLine, newLines);
                    break;
            }
        }

        protected override bool IsBlockDeclaration(string line, int index, out string trimmedLine) {
            if(!base.IsBlockDeclaration(line, index, out trimmedLine)) return false;
            string[] args = trimmedLine.Split(' ');
            if(args[^2] != "function") return false;
            return args[^1].Split(':').Length == 2;
        }

        private static string[] GetFunction(string trimmedLine) => trimmedLine.Split(' ')[^1].Split(':');
    }
}
