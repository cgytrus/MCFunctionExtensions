using System;
using System.Collections.Generic;
using System.IO;

using CommandLine;

using MCFunctionExtensions.Features;

namespace MCFunctionExtensions {
    [Flags]
    public enum Feature {
        None = 0,
        All = 0b1111111,
        ElseStatements = 1,
        SelfNamespace = 1 << 1,
        InlineFunctions = 1 << 2,
        ForLoops = 1 << 3,
        ExecuteOptimizations = 1 << 4,
        RecursiveExecuteCompilation = 1 << 5,
        Constants = 1 << 6
    }
    
    internal static class Program {
        public const string InternalObjectiveName = "mcfunctionext";
        
        private static readonly IReadOnlyDictionary<Feature, IFeature> features =
            new Dictionary<Feature, IFeature> {
                { Feature.Constants, new ConstantsFeature() },
                { Feature.SelfNamespace, new SelfNamespaceFeature() },
                { Feature.InlineFunctions, new InlineFunctionsFeature() },
                { Feature.ElseStatements, new ElseStatementsFeature() },
                { Feature.ForLoops, new ForLoopsFeature() },
                { Feature.RecursiveExecuteCompilation, new RecursiveExecuteCompilationFeature() },
                { Feature.ExecuteOptimizations, new ExecuteOptimizationsFeature() }
            };
        
        private static void Main(string[] args) {
            Options options = null;
            Parser.Default.ParseArguments<Options>(args).WithParsed(opts => options = opts);
            if(options == null) return;

            if(options.functionsPath == null) {
                options.useNamespace ??= Path.GetFileName(Directory.GetCurrentDirectory());
                options.functionsPath = Path.Join("data", options.useNamespace, "functions");
            }
            else options.useNamespace ??= Path.GetFileName(Path.GetDirectoryName(options.functionsPath));

            string[] filePaths = Directory.GetFiles(options.functionsPath);
            foreach(string filePath in filePaths)
                if(Path.GetExtension(filePath) == ".extmcfunction")
                    File.WriteAllLines(Path.ChangeExtension(filePath, ".mcfunction"),
                        CompileFunction(options, File.ReadAllLines(filePath)));
        }

        public static IEnumerable<string> CompileFunction(Options options, IEnumerable<string> lines) {
            List<string> newLines = new(lines);

            foreach((Feature featureEnum, IFeature feature) in features) {
                if((options.feature & featureEnum) == 0) continue;
                IReadOnlyList<string> readLines = new List<string>(newLines);
                newLines.Clear();
                feature.Use(readLines, newLines, options);
            }

            return newLines;
        }
    }
}
