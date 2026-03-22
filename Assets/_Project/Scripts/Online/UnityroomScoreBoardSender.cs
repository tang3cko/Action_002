using System.Threading;
using Cysharp.Threading.Tasks;
using Unityroom.Client;

namespace Action002.Online
{
    internal sealed class UnityroomScoreBoardSender : IScoreBoardSender
    {
        private readonly UnityroomClient client;

        public UnityroomScoreBoardSender(UnityroomClient client)
        {
            this.client = client;
        }

        public async UniTask SendAsync(int boardId, float score, CancellationToken ct = default)
        {
            await client.Scoreboards.SendAsync(
                new SendScoreRequest { ScoreboardId = boardId, Score = score },
                ct);
        }
    }
}
