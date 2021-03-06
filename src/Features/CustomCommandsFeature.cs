using System;
using System.Collections.Generic;
using System.Linq;

namespace MCFunctionExtensions.Features {
    public class CustomCommandsFeature : CodeBlocksFeature {
        public static Dictionary<string, (string[], IList<string>)> commands { get; } = new();
        
        protected override void BlockEnd(Options options, IEnumerable<string> inlineLines, string trimmedLine) {
            string[] args = trimmedLine.Split(' ');
            commands[args[0]] = (args[1..], inlineLines.ToList());
        }

        protected override void AddOriginalLine(string trimmedLine, ICollection<string> newLines) { }

        protected override bool IsBlockDeclaration(string line, int index, out string trimmedLine) {
            const string prefix = "const function";
            if(!base.IsBlockDeclaration(line, index, out trimmedLine) ||
               !trimmedLine.StartsWith(prefix, StringComparison.InvariantCulture)) return false;
            trimmedLine = prefix.Length + 1 >= trimmedLine.Length ? "" : trimmedLine[(prefix.Length + 1)..];
            string name = trimmedLine.Split(' ')[0];
            if(commands.ContainsKey(name))
                throw new FunctionExtensionErrorException(index + 1, $"Function {name} already defined.");
            return true;
        }
    }
}
