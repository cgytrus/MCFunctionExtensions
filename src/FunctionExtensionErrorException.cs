using System;
using System.Globalization;

namespace MCFunctionExtensions {
    public class FunctionExtensionErrorException : Exception {
        public FunctionExtensionErrorException(int line, string message, Exception innerException = null) : base(
            $"{(line > 0 ? $"Error on line {line.ToString(CultureInfo.InvariantCulture)}: " : "")}{message}",
            innerException) { }
    }
}
