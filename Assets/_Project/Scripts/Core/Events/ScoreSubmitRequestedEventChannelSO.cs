using Tang3cko.ReactiveSO;
using UnityEngine;

namespace Action002.Core.Events
{
    [CreateAssetMenu(
        fileName = "OnScoreSubmitRequested",
        menuName = "Action002/Events/ScoreSubmitRequested")]
    public class ScoreSubmitRequestedEventChannelSO : EventChannelSO<ScoreSubmitRequest>
    {
    }
}
