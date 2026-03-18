using Action002.Core;

namespace Action002.Bullet.Logic
{
    public enum ContactResult : byte
    {
        SamePolarity = 0,
        OppositePolarity = 1,
    }

    public static class ContactRuleCalculator
    {
        public static ContactResult Resolve(Polarity playerPolarity, byte enemyPolarity)
        {
            return (byte)playerPolarity == enemyPolarity
                ? ContactResult.SamePolarity
                : ContactResult.OppositePolarity;
        }

        public static bool IsScoringContact(ContactResult result, bool isFirstContact)
        {
            return result == ContactResult.SamePolarity && isFirstContact;
        }

        public static bool IsDamageContact(ContactResult result)
        {
            return result == ContactResult.OppositePolarity;
        }
    }
}
