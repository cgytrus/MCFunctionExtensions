using System.Collections.Generic;

namespace MCFunctionExtensions.Features {
    public abstract class CodeBlocksFeature : IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            List<string> inlineFunctionLines = new();
            for(int i = 0; i < readLines.Count; i++) {
                string line = readLines[i];

                ProcessLine(ref i, line, readLines, newLines, options, inlineFunctionLines);
            }
        }
        
        private void ProcessLine(ref int index, string line, IReadOnlyList<string> readLines,
            ICollection<string> newLines, Options options, ICollection<string> inlineLines) {
            int startLine;
            int inlineDepth;
            try {
                if(!IsInlineDeclaration(line, out string trimmedLine)) {
                    newLines.Add(line);
                    return;
                }

                newLines.Add(line.TrimEnd('{', ' '));
                startLine = index;
                inlineDepth = 0;
                inlineLines.Clear();
                index++;
                while(index < readLines.Count) {
                    line = readLines[index++].TrimStart();

                    if(IsInlineDeclaration(line, out _)) inlineDepth++;
                    if(IsInlineEnd(line)) {
                        if(inlineDepth <= 0) {
                            BlockEnd(options, inlineLines, trimmedLine);
                            index--;
                            break;
                        }

                        inlineDepth--;
                    }

                    inlineLines.Add(line);
                }
            }
            catch(FunctionExtensionErrorException ex) {
                throw new FunctionExtensionErrorException(index + 1, ex.Message, ex);
            }

            if(inlineDepth > 0) throw new FunctionExtensionErrorException(startLine + 1, "Braces not closed.");
        }

        protected abstract void BlockEnd(Options options, IEnumerable<string> inlineLines, string trimmedLine);

        protected abstract bool IsInlineDeclaration(string line, out string trimmedLine);

        protected virtual bool IsInlineEnd(string line) => line.Trim() == "}";
    }
}
