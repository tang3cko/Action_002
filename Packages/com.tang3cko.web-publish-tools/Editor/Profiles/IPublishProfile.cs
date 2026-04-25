using UnityEditor;
using UnityEditor.Build;

namespace Tang3cko.WebPublishTools.Editor.Profiles
{
    public interface IPublishProfile
    {
        string Id { get; }
        string DisplayName { get; }
        string DocumentationUrl { get; }
        string Notes { get; }

        bool RequiresWebGLBuildTarget { get; }
        WebGLCompressionFormat? ExpectedCompression { get; }
        bool? ExpectedDecompressionFallback { get; }
        bool? ExpectedDevelopmentBuild { get; }
        bool? ExpectedDataCaching { get; }
        WebGLExceptionSupport? ExpectedExceptionSupport { get; }
        ManagedStrippingLevel? ExpectedStrippingLevel { get; }
    }
}
