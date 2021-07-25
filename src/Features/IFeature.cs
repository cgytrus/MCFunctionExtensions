using System.Collections.Generic;

namespace MCFunctionExtensions.Features {
    public interface IFeature {
        public void Use(IReadOnlyList<string> readLines, List<string> newLines, Options options);
    }
}
