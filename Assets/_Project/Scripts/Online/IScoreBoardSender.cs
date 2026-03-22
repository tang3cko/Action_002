using System.Threading;
using Cysharp.Threading.Tasks;

namespace Action002.Online
{
    public interface IScoreBoardSender
    {
        UniTask SendAsync(int boardId, float score, CancellationToken ct = default);
    }
}
