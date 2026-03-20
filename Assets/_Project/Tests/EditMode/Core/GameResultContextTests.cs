using NUnit.Framework;
using Action002.Core.Flow;

namespace Action002.Tests.Core
{
    public class GameResultContextTests
    {
        // ── Default values ──

        [Test]
        public void Fields_DefaultValues_ResultTypeIsClear()
        {
            var result = default(GameResultContext);

            Assert.That(result.ResultType, Is.EqualTo(GameResultType.Clear));
        }

        [Test]
        public void Fields_DefaultValues_FinalScoreIsZero()
        {
            var result = default(GameResultContext);

            Assert.That(result.FinalScore, Is.EqualTo(0));
        }

        // ── Constructor / assign and read ──

        [Test]
        public void Fields_AssignAndRead_ClearResultType()
        {
            var result = new GameResultContext(GameResultType.Clear, 100);

            Assert.That(result.ResultType, Is.EqualTo(GameResultType.Clear));
        }

        [Test]
        public void Fields_AssignAndRead_GameOverResultType()
        {
            var result = new GameResultContext(GameResultType.GameOver, 50);

            Assert.That(result.ResultType, Is.EqualTo(GameResultType.GameOver));
        }

        [Test]
        public void Fields_AssignAndRead_FinalScore()
        {
            var result = new GameResultContext(GameResultType.Clear, 9999);

            Assert.That(result.FinalScore, Is.EqualTo(9999));
        }

        [Test]
        public void Fields_AssignAndRead_ZeroScore()
        {
            var result = new GameResultContext(GameResultType.GameOver, 0);

            Assert.That(result.FinalScore, Is.EqualTo(0));
        }

        [Test]
        public void Fields_AssignAndRead_NegativeScore()
        {
            var result = new GameResultContext(GameResultType.Clear, -100);

            Assert.That(result.FinalScore, Is.EqualTo(-100));
        }

        [Test]
        public void Fields_AssignAndRead_MaxScore()
        {
            var result = new GameResultContext(GameResultType.Clear, int.MaxValue);

            Assert.That(result.FinalScore, Is.EqualTo(int.MaxValue));
        }

        // ── GameResultType enum values ──

        [Test]
        public void GameResultType_ClearValue()
        {
            Assert.That((byte)GameResultType.Clear, Is.EqualTo(0));
        }

        [Test]
        public void GameResultType_GameOverValue()
        {
            Assert.That((byte)GameResultType.GameOver, Is.EqualTo(1));
        }
    }
}
