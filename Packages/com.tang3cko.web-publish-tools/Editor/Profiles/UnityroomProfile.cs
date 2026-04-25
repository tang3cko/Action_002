using UnityEditor;
using UnityEditor.Build;

namespace Tang3cko.WebPublishTools.Editor.Profiles
{
    public sealed class UnityroomProfile : IPublishProfile
    {
        public string Id => "unityroom";
        public string DisplayName => "unityroom";
        public string DocumentationUrl => "https://help.unityroom.com/build-settings";

        public string Notes =>
            "unityroom requires Gzip compression. After building, upload the four files in the Build folder " +
            "(.loader.js, .data.gz, .framework.js.gz, .wasm.gz). Folder and file names must be alphanumeric only.";

        public bool RequiresWebGLBuildTarget => true;
        public WebGLCompressionFormat? ExpectedCompression => WebGLCompressionFormat.Gzip;
        public bool? ExpectedDecompressionFallback => false;
        public bool? ExpectedDevelopmentBuild => false;
        public bool? ExpectedDataCaching => false;
        public WebGLExceptionSupport? ExpectedExceptionSupport => WebGLExceptionSupport.None;
        public ManagedStrippingLevel? ExpectedStrippingLevel => ManagedStrippingLevel.High;
    }
}
