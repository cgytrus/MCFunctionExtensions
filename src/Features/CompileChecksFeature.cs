using System;
using System.Collections.Generic;

namespace MCFunctionExtensions.Features {
    public class CompileChecksFeature : IFeature {
        private abstract class Check {
            public int line { get; }
            public bool success { get; protected set; }

            protected Check(int line) => this.line = line;
            
            public abstract void Run(Stack<Check> checks, string[] args);
            public abstract void Do(Stack<Check> checks, string[] args, string from);
        }

        private class EndCheck : Check {
            public const string Name = "end";

            public EndCheck(int line) : base(line) { }
            
            public override void Run(Stack<Check> checks, string[] args) {
                if(args.Length != 0) ThrowArgumentsCount(line);
                Do(checks, args, Name);
            }

            public override void Do(Stack<Check> checks, string[] args, string from) {
                if(!checks.TryPop(out _))
                    throw new FunctionExtensionErrorException(line + 1,
                        $"'{from}' check when no checks are running.");
            }
        }

        private class IfCheck : Check {
            public const string Name = "if";

            public IfCheck(int line) : base(line) { }

            public override void Run(Stack<Check> checks, string[] args) {
                if(args.Length < 2) ThrowArgumentsCount(line);
                Do(checks, args, Name);
                checks.Push(this);
            }

            public override void Do(Stack<Check> checks, string[] args, string from) {
                if(!ConstantsFeature.values.TryGetValue(args[0], out string value)) ThrowValueDoesNotExist(line);
                success = string.Join(' ', args[1..]) == value;
            }
        }

        private class ElseCheck : Check {
            public const string Name = "else";

            public ElseCheck(int line) : base(line) { }

            public override void Run(Stack<Check> checks, string[] args) {
                if(args.Length != 0) ThrowArgumentsCount(line);
                Do(checks, args, Name);
                checks.Push(this);
            }

            public override void Do(Stack<Check> checks, string[] args, string from) {
                if(!checks.TryPop(out Check check))
                    throw new FunctionExtensionErrorException(line + 1,
                        $"'{from}' check when no checks are running.");
                success = !check.success;
            }
        }

        private class ElseIfCheck : Check {
            public const string Name = "elseif";

            public ElseIfCheck(int line) : base(line) { }

            public override void Run(Stack<Check> checks, string[] args) {
                if(args.Length < 2) ThrowArgumentsCount(line);
                Do(checks, args, Name);
                checks.Push(this);
            }

            public override void Do(Stack<Check> checks, string[] args, string from) {
                ElseCheck elseCheck = new(line);
                elseCheck.Do(checks, args, Name);
                success = elseCheck.success;
                if(!success) return;
                
                IfCheck ifCheck = new(line);
                ifCheck.Do(checks, args, Name);
                success = ifCheck.success;
            }
        }
        
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options) {
            Stack<Check> checks = new();
            
            for(int i = 0; i < readLines.Count; i++) {
                string line = readLines[i];
                ProcessLine(newLines, line, checks, i);
            }

            if(checks.TryPeek(out Check check))
                throw new FunctionExtensionErrorException(check.line + 1, "Check not ended.");
        }

        private static void ProcessLine(ICollection<string> newLines, string line, Stack<Check> checks, int index) {
            const string checkPrefix = "const check";
            
            if(!line.StartsWith(checkPrefix, StringComparison.InvariantCulture)) {
                if(checks.Count <= 0 || checks.Peek().success) newLines.Add(line);
                return;
            }

            string[] args = line.Split(' ');
            if(args.Length < 3) ThrowArgumentsCount(index);
            args = args[2..];

            ProcessArgs(args, index, checks);
        }

        private static void ProcessArgs(string[] args, int index, Stack<Check> checks) {
            Check check = args[0] switch {
                EndCheck.Name => new EndCheck(index),
                IfCheck.Name => new IfCheck(index),
                ElseCheck.Name => new ElseCheck(index),
                ElseIfCheck.Name => new ElseIfCheck(index),
                _ => throw new FunctionExtensionErrorException(index + 1, $"Unknown check '{args[0]}'.")
            };
            check.Run(checks, args[1..]);
        }

        private static void ThrowArgumentsCount(int index) =>
            throw new FunctionExtensionErrorException(index + 1, "Wrong arguments count.");
        
        private static void ThrowValueDoesNotExist(int index) =>
            throw new FunctionExtensionErrorException(index + 1, "Value does not exist.");
    }
}
