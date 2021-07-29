using System;
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
                if(!IsBlockDeclaration(line, index, out string trimmedDeclaration)) {
                    newLines.Add(line);
                    return;
                }

                AddOriginalLine(trimmedDeclaration, newLines);
                startLine = index;
                inlineDepth = 0;
                inlineLines.Clear();
                index++;
                while(index < readLines.Count)
                    if(ProcessBlockLine(readLines, inlineLines, options, trimmedDeclaration, ref index,
                        ref inlineDepth))
                        break;
            }
            catch(FunctionExtensionErrorException ex) {
                throw new FunctionExtensionErrorException(index + 1, ex.Message, ex);
            }

            if(inlineDepth > 0) throw new FunctionExtensionErrorException(startLine + 1, "Braces not closed.");
        }

        private bool ProcessBlockLine(IReadOnlyList<string> readLines, ICollection<string> inlineLines, Options options,
            string trimmedDeclaration, ref int index, ref int inlineDepth) {
            string line = readLines[index++].TrimStart();

            if(IsBlockStart(line, out _)) inlineDepth++;
            if(IsBlockEnd(line)) {
                if(inlineDepth <= 0) {
                    BlockEnd(options, inlineLines, trimmedDeclaration);
                    index--;
                    return true;
                }

                inlineDepth--;
            }

            inlineLines.Add(line);
            return false;
        }

        protected abstract void BlockEnd(Options options, IEnumerable<string> inlineLines, string trimmedLine);

        protected virtual void AddOriginalLine(string trimmedLine, ICollection<string> newLines) =>
            newLines.Add(trimmedLine);

        protected virtual bool IsBlockDeclaration(string line, int index, out string trimmedLine) =>
            IsBlockStart(line, out trimmedLine);

        protected virtual bool IsBlockEnd(string line) => line.Trim() == "}";

        public static bool IsBlockStart(string line, out string trimmedLine) {
            trimmedLine = line.TrimEnd();
            bool isBlockDeclaration = trimmedLine.EndsWith("{", StringComparison.InvariantCulture);
            trimmedLine = trimmedLine.TrimEnd('{').TrimEnd();
            return isBlockDeclaration;
        }
    }
}
