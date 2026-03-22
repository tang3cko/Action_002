using UnityEngine;
using Tang3cko.ReactiveSO;

namespace Action002.Core.Save
{
    public class RunSessionStatsCollector : MonoBehaviour
    {
        [Header("Events (subscribe)")]
        [SerializeField] private IntEventChannelSO onEnemyKilled;
        [SerializeField] private FloatEventChannelSO onComboIncremented;

        [Header("Variables (read)")]
        [SerializeField] private IntVariableSO comboCountVar;

        [Header("Variables (write)")]
        [SerializeField] private IntVariableSO maxComboVar;
        [SerializeField] private IntVariableSO runKillCountVar;
        [SerializeField] private IntVariableSO runAbsorptionCountVar;

        private readonly RunSessionStatsCalculator calculator = new RunSessionStatsCalculator();

        private void OnEnable()
        {
            calculator.Reset();
            if (onEnemyKilled != null)
                onEnemyKilled.OnEventRaised += HandleEnemyKilled;
            if (onComboIncremented != null)
                onComboIncremented.OnEventRaised += HandleComboIncremented;
        }

        private void OnDisable()
        {
            if (onEnemyKilled != null)
                onEnemyKilled.OnEventRaised -= HandleEnemyKilled;
            if (onComboIncremented != null)
                onComboIncremented.OnEventRaised -= HandleComboIncremented;
        }

        private void HandleEnemyKilled(int enemyPolarity)
        {
            calculator.RecordKill();
            if (runKillCountVar != null)
                runKillCountVar.Value = calculator.KillCount;
        }

        private void HandleComboIncremented(float scoreValue)
        {
            int currentCombo = comboCountVar != null ? comboCountVar.Value : 0;
            calculator.RecordAbsorption(currentCombo);

            if (runAbsorptionCountVar != null)
                runAbsorptionCountVar.Value = calculator.AbsorptionCount;
            if (maxComboVar != null)
                maxComboVar.Value = calculator.MaxCombo;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onEnemyKilled == null) Debug.LogWarning($"[{GetType().Name}] onEnemyKilled not assigned on {gameObject.name}.", this);
            if (onComboIncremented == null) Debug.LogWarning($"[{GetType().Name}] onComboIncremented not assigned on {gameObject.name}.", this);
            if (comboCountVar == null) Debug.LogWarning($"[{GetType().Name}] comboCountVar not assigned on {gameObject.name}.", this);
            if (maxComboVar == null) Debug.LogWarning($"[{GetType().Name}] maxComboVar not assigned on {gameObject.name}.", this);
            if (runKillCountVar == null) Debug.LogWarning($"[{GetType().Name}] runKillCountVar not assigned on {gameObject.name}.", this);
            if (runAbsorptionCountVar == null) Debug.LogWarning($"[{GetType().Name}] runAbsorptionCountVar not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
