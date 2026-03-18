using UnityEngine;

namespace Action002.Core
{
    /// <summary>
    /// Initializes frame rate settings before any scene loads.
    /// Uses vSync for hardware-based synchronization (recommended for WebGL).
    /// </summary>
    public static class FpsBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // vSyncCount = 1: Sync to display refresh rate (typically 60Hz)
            QualitySettings.vSyncCount = 1;

            // Fallback for platforms that don't support vSync
            Application.targetFrameRate = 60;
        }
    }
}
