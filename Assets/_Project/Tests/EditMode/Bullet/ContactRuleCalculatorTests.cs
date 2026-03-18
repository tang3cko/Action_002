using NUnit.Framework;
using Action002.Bullet.Logic;
using Action002.Core;

namespace Action002.Tests.Bullet
{
    public class ContactRuleCalculatorTests
    {
        [Test]
        public void Resolve_SamePolarity_ReturnsSamePolarity()
        {
            var result = ContactRuleCalculator.Resolve(Polarity.White, (byte)Polarity.White);
            Assert.That(result, Is.EqualTo(ContactResult.SamePolarity));
        }

        [Test]
        public void Resolve_OppositePolarity_ReturnsOppositePolarity()
        {
            var result = ContactRuleCalculator.Resolve(Polarity.White, (byte)Polarity.Black);
            Assert.That(result, Is.EqualTo(ContactResult.OppositePolarity));
        }

        [Test]
        public void Resolve_BlackPlayerWhiteEnemy_ReturnsOppositePolarity()
        {
            var result = ContactRuleCalculator.Resolve(Polarity.Black, (byte)Polarity.White);
            Assert.That(result, Is.EqualTo(ContactResult.OppositePolarity));
        }

        [Test]
        public void Resolve_BlackPlayerBlackEnemy_ReturnsSamePolarity()
        {
            var result = ContactRuleCalculator.Resolve(Polarity.Black, (byte)Polarity.Black);
            Assert.That(result, Is.EqualTo(ContactResult.SamePolarity));
        }

        [Test]
        public void IsScoringContact_SamePolarityAndFirstContact_ReturnsTrue()
        {
            Assert.That(
                ContactRuleCalculator.IsScoringContact(ContactResult.SamePolarity, isFirstContact: true),
                Is.True);
        }

        [Test]
        public void IsScoringContact_SamePolarityAndNotFirstContact_ReturnsFalse()
        {
            Assert.That(
                ContactRuleCalculator.IsScoringContact(ContactResult.SamePolarity, isFirstContact: false),
                Is.False);
        }

        [Test]
        public void IsScoringContact_OppositePolarity_ReturnsFalse()
        {
            Assert.That(
                ContactRuleCalculator.IsScoringContact(ContactResult.OppositePolarity, isFirstContact: true),
                Is.False);
        }

        [Test]
        public void IsDamageContact_OppositePolarity_ReturnsTrue()
        {
            Assert.That(
                ContactRuleCalculator.IsDamageContact(ContactResult.OppositePolarity),
                Is.True);
        }

        [Test]
        public void IsDamageContact_SamePolarity_ReturnsFalse()
        {
            Assert.That(
                ContactRuleCalculator.IsDamageContact(ContactResult.SamePolarity),
                Is.False);
        }
    }
}
