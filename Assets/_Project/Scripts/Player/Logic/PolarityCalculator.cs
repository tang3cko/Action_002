using Action002.Core;

namespace Action002.Player.Logic
{
    public static class PolarityCalculator
    {
        public static Polarity Toggle(Polarity current)
        {
            return current == Polarity.White ? Polarity.Black : Polarity.White;
        }

        public static bool IsSamePolarity(Polarity player, byte bulletPolarity)
        {
            return (byte)player == bulletPolarity;
        }
    }
}
