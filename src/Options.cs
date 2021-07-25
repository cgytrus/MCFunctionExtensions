using CommandLine;

namespace MCFunctionExtensions {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Options {
        [Option('s', "src", MetaValue = "STRING",
            HelpText = "Path to the functions folder. Gets determined automatically if not set.")]
        public string functionsPath { get; set; }
        
        [Option('n', "namespace", Required = false, MetaValue = "STRING",
            HelpText = "The namespace of the datapack. Gets determined automatically if not set.")]
        public string useNamespace { get; set; }

        [Option('f', "features", Required = false, MetaValue = "ENUM", Default = Feature.All,
            HelpText = "Sets the enabled features of the compiler.")]
        public Feature feature { get; set; }
    }
}
