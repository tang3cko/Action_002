using System;
using NUnit.Framework;
using Action002.Core.Flow;

namespace Action002.Tests.Core
{
    public class GamePhaseTests
    {
        [Test]
        public void GamePhase_HasExpectedValues()
        {
            var values = (GamePhase[])Enum.GetValues(typeof(GamePhase));
            Assert.That(values.Length, Is.EqualTo(6));
        }

        [Test]
        public void GamePhase_Boot_IsZero()
        {
            Assert.That((byte)GamePhase.Boot, Is.EqualTo(0));
        }

        [TestCase(GamePhase.Boot)]
        [TestCase(GamePhase.Tutorial)]
        [TestCase(GamePhase.Title)]
        [TestCase(GamePhase.Stage)]
        [TestCase(GamePhase.Boss)]
        [TestCase(GamePhase.Result)]
        public void GamePhase_AllValues_AreDefined(GamePhase phase)
        {
            Assert.That(Enum.IsDefined(typeof(GamePhase), phase), Is.True);
        }
    }
}
