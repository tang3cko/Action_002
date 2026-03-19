using NUnit.Framework;
using Action002.Core.Flow;

namespace Action002.Tests.Core
{
    public class GameResultContextTests
    {
        [Test]
        public void Constructor_Clear_StoresResultType()
        {
            var ctx = new GameResultContext(GameResultType.Clear, 100);
            Assert.That(ctx.ResultType, Is.EqualTo(GameResultType.Clear));
        }

        [Test]
        public void Constructor_GameOver_StoresResultType()
        {
            var ctx = new GameResultContext(GameResultType.GameOver, 50);
            Assert.That(ctx.ResultType, Is.EqualTo(GameResultType.GameOver));
        }

        [Test]
        public void Constructor_StoresFinalScore()
        {
            var ctx = new GameResultContext(GameResultType.Clear, 12345);
            Assert.That(ctx.FinalScore, Is.EqualTo(12345));
        }

        [Test]
        public void Constructor_ZeroScore_StoresZero()
        {
            var ctx = new GameResultContext(GameResultType.GameOver, 0);
            Assert.That(ctx.FinalScore, Is.EqualTo(0));
        }
    }
}
