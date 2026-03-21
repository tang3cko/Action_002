using UnityEngine;
using Action002.Core;

namespace Action002.Visual
{
    /// <summary>
    /// Central color constants for polarity-based visuals.
    /// All polarity colors must be referenced from here.
    /// </summary>
    public static class PolarityColors
    {
        /// <summary>Background color for White polarity (dark navy).</summary>
        public static readonly Color WhiteBackground = new Color(0.102f, 0.102f, 0.180f, 1f); // #1A1A2E

        /// <summary>Background color for Black polarity (warm off-white).</summary>
        public static readonly Color BlackBackground = new Color(0.941f, 0.933f, 0.902f, 1f); // #F0EEE6

        /// <summary>Sprite/foreground color for White polarity.</summary>
        public static readonly Color WhiteForeground = new Color(0.941f, 0.933f, 0.902f, 1f); // #F0EEE6

        /// <summary>Sprite/foreground color for Black polarity.</summary>
        public static readonly Color BlackForeground = new Color(0.102f, 0.102f, 0.180f, 1f); // #1A1A2E

        public static Color GetBackground(int polarity)
        {
            return polarity == (int)Polarity.White ? WhiteBackground : BlackBackground;
        }

        public static Color GetBackground(Polarity polarity)
        {
            return polarity == Polarity.White ? WhiteBackground : BlackBackground;
        }

        public static Color GetForeground(int polarity)
        {
            return polarity == (int)Polarity.White ? WhiteForeground : BlackForeground;
        }

        public static Color GetForeground(Polarity polarity)
        {
            return polarity == Polarity.White ? WhiteForeground : BlackForeground;
        }
    }

    /// <summary>
    /// Backward-compatible wrapper delegating to PolarityColors.
    /// </summary>
    public static class TransitionColorHelper
    {
        public static readonly Color WhitePolarityColor = PolarityColors.WhiteForeground;
        public static readonly Color BlackPolarityColor = PolarityColors.BlackForeground;

        public static Color GetColor(int polarity)
        {
            return PolarityColors.GetForeground(polarity);
        }
    }
}
