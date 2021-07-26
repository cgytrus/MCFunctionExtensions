using System;
using System.Collections.Generic;
using System.IO;

namespace MCFunctionExtensions.Features {
    public class InlineFunctionsFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            List<string> inlineFunctionLines = new();
            for(int i = 0; i < readLines.Count; i++) {
                string line = readLines[i];

                ProcessLine(ref i, line, readLines, newLines, options, inlineFunctionLines);
            }
        }
        
        private static void ProcessLine(ref int index, string line, IReadOnlyList<string> readLines,
            ICollection<string> newLines, Options options, ICollection<string> inlineFunctionLines) {
            int startLine;
            int inlineDepth;
            try {
                string functionPath =
                    ParseInlineFunctionDeclaration(options.functionsPath, options.useNamespace, line);
                if(functionPath == null) {
                    newLines.Add(line);
                    return;
                }

                newLines.Add(line.TrimEnd('{', ' '));
                startLine = index;
                inlineDepth = 0;
                inlineFunctionLines.Clear();
                index++;
                while(index < readLines.Count) {
                    line = readLines[index++].TrimStart();

                    if(IsInlineFunctionDeclaration(line, out string _, out string[] _)) inlineDepth++;
                    if(line == "}") {
                        if(inlineDepth <= 0) {
                            File.WriteAllLines(functionPath, Program.CompileFunction(options, inlineFunctionLines));
                            index--;
                            break;
                        }

                        inlineDepth--;
                    }

                    inlineFunctionLines.Add(line);
                }
            }
            catch(FunctionExtensionErrorException ex) {
                throw new FunctionExtensionErrorException(index + 1, ex.Message, ex);
            }

            if(inlineDepth > 0) throw new FunctionExtensionErrorException(startLine + 1, "Braces not closed.");
        }

        private static bool IsInlineFunctionDeclaration(string line, out string trimmedLine, out string[] function) {
            trimmedLine = line.TrimEnd();
            function = null;
            
            if(!trimmedLine.EndsWith("{", StringComparison.InvariantCulture)) return false;
            
            trimmedLine = trimmedLine.TrimEnd('{', ' ');
            string[] args = trimmedLine.Split(' ');
            
            if(args[^2] != "function") return false;
            
            function = args[^1].Split(':');
            return function.Length == 2;
        }
        
        private static string ParseInlineFunctionDeclaration(string directory, string currNamespace, string line) {
            if(!IsInlineFunctionDeclaration(line, out _, out string[] function)) return null;
            if(function[0] != currNamespace)
                throw new FunctionExtensionErrorException(0, "Tried declaring an inline function not in the current namespace.");
            string functionName = function[1];

            return Path.Join(directory, $"{functionName}.mcfunction");
        }
    }
}
