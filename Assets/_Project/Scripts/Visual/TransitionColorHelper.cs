using UnityEngine;

namespace Action002.Visual
{
    /// <summary>
    /// Backward-compatible wrapper delegating to PolarityColors.
    /// Used by ScreenTransitionController for screen transition mask color.
    /// </summary>
    public static class TransitionColorHelper
    {
        public static readonly Color WhitePolarityColor = PolarityColors.WhiteForeground;

        public static Color GetColor(int polarity)
        {
            return PolarityColors.GetForeground(polarity);
        }
    }
}
