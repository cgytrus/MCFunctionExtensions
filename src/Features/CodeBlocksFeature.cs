﻿using System.Collections.Generic;

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
                if(!IsInlineDeclaration(line, out string trimmedDeclaration)) {
                    newLines.Add(line);
                    return;
                }

                newLines.Add(line.TrimEnd('{', ' '));
                startLine = index;
                inlineDepth = 0;
                inlineLines.Clear();
                index++;
                while(index < readLines.Count)
                    if(ProcessInlineLine(readLines, inlineLines, options, trimmedDeclaration, ref index,
                        ref inlineDepth))
                        break;
            }
            catch(FunctionExtensionErrorException ex) {
                throw new FunctionExtensionErrorException(index + 1, ex.Message, ex);
            }

            if(inlineDepth > 0) throw new FunctionExtensionErrorException(startLine + 1, "Braces not closed.");
        }
        
        private bool ProcessInlineLine(IReadOnlyList<string> readLines, ICollection<string> inlineLines, Options options,
            string trimmedDeclaration, ref int index, ref int inlineDepth) {
            string line = readLines[index++].TrimStart();

            if(IsInlineDeclaration(line, out _)) inlineDepth++;
            if(IsInlineEnd(line)) {
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

        protected abstract bool IsInlineDeclaration(string line, out string trimmedLine);

        protected virtual bool IsInlineEnd(string line) => line.Trim() == "}";
    }
}
