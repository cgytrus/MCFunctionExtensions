using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using CommandLine;

using MCFunctionExtensions.Features;

namespace MCFunctionExtensions {
    [Flags]
    public enum Feature {
        None = 0,
        All = 0b1111111111,
        ElseStatements = 1,
        SelfNamespace = 1 << 1,
        InlineFunctions = 1 << 2,
        ForLoops = 1 << 3,
        ExecuteOptimizations = 1 << 4,
        RecursiveExecuteCompilation = 1 << 5,
        Constants = 1 << 6,
        AnonymousFunctions = 1 << 7,
        CustomCommands = 1 << 8,
        CompileChecks = 1 << 9
    }
    
    internal static class Program {
        public const string InternalName = "mcfunctionext";

        public const string SourceExtension = ".extmcfunction";
        public const string LibraryExtension = ".extmclibrary";
        
        private static readonly Dictionary<string, List<string>> postCompileAppend = new();
        private static ulong _globalFunctionId;

        private static readonly IReadOnlyDictionary<Feature, IFeature> features =
            new Dictionary<Feature, IFeature> {
                { Feature.CustomCommands, new CustomCommandsFeature() },
                { Feature.Constants, new ConstantsFeature() },
                { Feature.CompileChecks, new CompileChecksFeature() },
                { Feature.SelfNamespace, new SelfNamespaceFeature() },
                { Feature.AnonymousFunctions, new AnonymousFunctionsFeature() },
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

            if(options.libraries.Length > 0) IncludeLibraries(options);
            else IncludeAllLibraries(options);
            CompileSource(options);
        }

        private static void IncludeAllLibraries(Options sourceOptions) {
            string[] filePaths = Directory.GetFiles(sourceOptions.functionsPath);
            foreach(string filePath in filePaths)
                if(Path.GetExtension(filePath) == LibraryExtension)
                    IncludeLibrary(sourceOptions.functionsPath, sourceOptions.useNamespace, sourceOptions.features,
                        filePath);
        }

        private static void IncludeLibraries(Options sourceOptions) {
            foreach(string library in sourceOptions.libraries)
                IncludeLibrary(sourceOptions.functionsPath, sourceOptions.useNamespace, sourceOptions.features,
                    library);
        }

        private static void IncludeLibrary(string sourcePath, string sourceNamespace, Feature features, string path) {
            string name = Path.GetFileNameWithoutExtension(path);

            Options options = new() {
                functionsPath = Path.Join(sourcePath, "libraries", name),
                useNamespace = sourceNamespace,
                features = features
            };
            IncludeLibrary(options, path);
        }

        private static void IncludeLibrary(Options options, string path) {
            string newPath = Path.Join(options.functionsPath,
                Path.ChangeExtension(Path.GetFileName(path), ".mcfunction"));
            Directory.CreateDirectory(Path.GetDirectoryName(newPath) ?? string.Empty);
            File.WriteAllLines(newPath, CompileFunction(options, File.ReadAllLines(path)));
        }

        private static void CompileSource(Options options) {
            string[] filePaths = Directory.GetFiles(options.functionsPath);
            foreach(string filePath in filePaths)
                if(Path.GetExtension(filePath) == SourceExtension)
                    File.WriteAllLines(Path.ChangeExtension(filePath, ".mcfunction"),
                        CompileFunction(options, File.ReadAllLines(filePath)));
            
            ExecutePostCompileAppend();
        }

        private static void ExecutePostCompileAppend() {
            foreach((string path, List<string> lines) in postCompileAppend) File.AppendAllLines(path, lines);
        }

        public static IEnumerable<string> CompileFunction(Options options, IEnumerable<string> lines) {
            List<string> newLines = new(lines);

            foreach((Feature featureEnum, IFeature feature) in features) {
                if((options.features & featureEnum) == 0) continue;
                IReadOnlyList<string> readLines = new List<string>(newLines);
                newLines.Clear();
                feature.Use(readLines, newLines, options);
            }

            return newLines;
        }

        public static bool PostCompileAppend(string functionId, Options options, IEnumerable<string> lines) {
            string[] splitFunctionId = functionId.Split(':');
            if(splitFunctionId[0] != options.useNamespace) return false;
            string path = GetFunctionPath(options.functionsPath, splitFunctionId[1]);
            if(!postCompileAppend.ContainsKey(functionId)) postCompileAppend[path] = new List<string>();
            postCompileAppend[path].AddRange(lines);
            return true;
        }

        public static string GetGeneratedFunctionId(string useNamespace, string featureName) =>
            $"{useNamespace}:z__{InternalName}/{featureName}/generated_{_globalFunctionId++.ToString(CultureInfo.InvariantCulture)}";

        public static string GetFunctionPath(string directory, string functionName) =>
            Path.Join(directory, $"{Path.Join(functionName.Split('/'))}.mcfunction");
    }
}
