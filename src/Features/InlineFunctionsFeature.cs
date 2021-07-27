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

        protected override bool IsBlockDeclaration(string line, out string trimmedLine) {
            if(!base.IsBlockDeclaration(line, out trimmedLine)) return false;
            string[] args = trimmedLine.Split(' ');
            if(args[^2] != "function") return false;
            return args[^1].Split(':').Length == 2;
        }

        private static string[] GetFunction(string trimmedLine) => trimmedLine.Split(' ')[^1].Split(':');
    }
}
