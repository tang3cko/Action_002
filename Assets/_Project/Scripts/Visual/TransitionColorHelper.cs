using UnityEngine;

namespace Action002.Visual
{
    /// <summary>
    /// Pure utility class for computing screen transition colors based on polarity.
    /// Extracted from ScreenTransitionController for testability.
    /// </summary>
    public static class TransitionColorHelper
    {
        public static readonly Color WhitePolarityColor = new Color(0.878f, 0.878f, 1f, 1f);
        public static readonly Color BlackPolarityColor = new Color(0.15f, 0.15f, 0.25f, 1f);

        /// <summary>
        /// Returns the transition mask color for the given polarity value.
        /// 0 = White polarity, anything else = Black polarity.
        /// </summary>
        public static Color GetColor(int polarity)
        {
            return polarity == 0 ? WhitePolarityColor : BlackPolarityColor;
        }
    }
}
