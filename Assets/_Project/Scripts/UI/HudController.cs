using UnityEngine;
using UnityEngine.UIElements;
using Action002.Core;
using Action002.Core.Flow;
using Tang3cko.ReactiveSO;

namespace Action002.UI
{
    public class HudController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Variables")]
        [SerializeField] private IntVariableSO playerHpVar;
        [SerializeField] private IntVariableSO scoreVar;
        [SerializeField] private IntVariableSO comboCountVar;
        [SerializeField] private FloatVariableSO spinGaugeVar;
        [SerializeField] private IntVariableSO playerLevelVar;

        [Header("Events (subscribe)")]
        [SerializeField] private IntEventChannelSO onGamePhaseChanged;
        [SerializeField] private IntEventChannelSO onPlayerHpChanged;
        [SerializeField] private IntEventChannelSO onScoreChanged;
        [SerializeField] private IntEventChannelSO onComboCountChanged;
        [SerializeField] private FloatEventChannelSO onSpinGaugeChanged;
        [SerializeField] private IntEventChannelSO onPlayerLevelUp;

        [Header("UI")]
        [SerializeField] private UIDocument uiDocument;

        private const string PIP_CLASS = "hud__hp-pip";
        private const string PIP_DEAD_CLASS = "hud__hp-pip--dead";

        private VisualElement hudRoot;
        private VisualElement[] hpPips;
        private Label scoreLabel;
        private Label comboLabel;
        private Label levelLabel;
        private VisualElement gaugeFill;

        private void Awake()
        {
            if (uiDocument == null)
            {
                Debug.LogError($"[HudController] uiDocument is null.", this);
                return;
            }

            var root = uiDocument.rootVisualElement;

            hudRoot = root.Q<VisualElement>("Hud");
            if (hudRoot == null)
                Debug.LogError("[HudController] VisualElement 'Hud' not found in UIDocument.", this);

            var hpGroup = root.Q<VisualElement>("HpGroup");
            if (hpGroup == null)
            {
                Debug.LogError("[HudController] VisualElement 'HpGroup' not found in UIDocument.", this);
            }
            else
            {
                int maxHp = gameConfig != null ? gameConfig.MaxHp : 5;
                hpGroup.Clear();
                hpPips = new VisualElement[maxHp];
                for (int i = 0; i < maxHp; i++)
                {
                    var pip = new VisualElement();
                    pip.AddToClassList(PIP_CLASS);
                    hpGroup.Add(pip);
                    hpPips[i] = pip;
                }
            }

            scoreLabel = root.Q<Label>("ScoreLabel");
            if (scoreLabel == null)
                Debug.LogError("[HudController] Label 'ScoreLabel' not found in UIDocument.", this);

            comboLabel = root.Q<Label>("ComboLabel");
            if (comboLabel == null)
                Debug.LogError("[HudController] Label 'ComboLabel' not found in UIDocument.", this);

            levelLabel = root.Q<Label>("LevelLabel");
            if (levelLabel == null)
                Debug.LogError("[HudController] Label 'LevelLabel' not found in UIDocument.", this);

            gaugeFill = root.Q<VisualElement>("GaugeFill");
            if (gaugeFill == null)
                Debug.LogError("[HudController] VisualElement 'GaugeFill' not found in UIDocument.", this);
        }

        private void OnEnable()
        {
            if (onGamePhaseChanged != null)
                onGamePhaseChanged.OnEventRaised += HandleGamePhaseChanged;
            if (onPlayerHpChanged != null)
                onPlayerHpChanged.OnEventRaised += OnHpChanged;
            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised += OnScoreChanged;
            if (onComboCountChanged != null)
                onComboCountChanged.OnEventRaised += OnComboChanged;
            if (onSpinGaugeChanged != null)
                onSpinGaugeChanged.OnEventRaised += OnGaugeChanged;
            if (onPlayerLevelUp != null)
                onPlayerLevelUp.OnEventRaised += OnLevelChanged;

            if (playerHpVar != null) OnHpChanged(playerHpVar.Value);
            if (scoreVar != null) OnScoreChanged(scoreVar.Value);
            if (comboCountVar != null) OnComboChanged(comboCountVar.Value);
            if (spinGaugeVar != null) OnGaugeChanged(spinGaugeVar.Value);
            if (playerLevelVar != null) OnLevelChanged(playerLevelVar.Value);

            Show();
        }

        private void OnDisable()
        {
            if (onGamePhaseChanged != null)
                onGamePhaseChanged.OnEventRaised -= HandleGamePhaseChanged;
            if (onPlayerHpChanged != null)
                onPlayerHpChanged.OnEventRaised -= OnHpChanged;
            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised -= OnScoreChanged;
            if (onComboCountChanged != null)
                onComboCountChanged.OnEventRaised -= OnComboChanged;
            if (onSpinGaugeChanged != null)
                onSpinGaugeChanged.OnEventRaised -= OnGaugeChanged;
            if (onPlayerLevelUp != null)
                onPlayerLevelUp.OnEventRaised -= OnLevelChanged;
        }

        private void HandleGamePhaseChanged(int phase)
        {
            if (phase == (int)GamePhase.Stage || phase == (int)GamePhase.Boss)
                Show();
            else
                Hide();
        }

        private void Show()
        {
            if (hudRoot != null)
                hudRoot.style.display = DisplayStyle.Flex;
        }

        private void Hide()
        {
            if (hudRoot != null)
                hudRoot.style.display = DisplayStyle.None;
        }

        private void OnHpChanged(int hp)
        {
            if (hpPips == null) return;
            for (int i = 0; i < hpPips.Length; i++)
            {
                if (hpPips[i] == null) continue;
                if (i < hp)
                    hpPips[i].RemoveFromClassList(PIP_DEAD_CLASS);
                else
                    hpPips[i].AddToClassList(PIP_DEAD_CLASS);
            }
        }

        private void OnScoreChanged(int score)
        {
            if (scoreLabel != null) scoreLabel.text = score.ToString();
        }

        private void OnComboChanged(int combo)
        {
            if (comboLabel != null)
                comboLabel.text = combo > 1 ? $"x{combo}" : "";
        }

        private void OnLevelChanged(int level)
        {
            if (levelLabel != null)
                levelLabel.text = $"Lv.{level}";
        }

        private void OnGaugeChanged(float gauge)
        {
            if (gaugeFill != null)
                gaugeFill.style.width = new Length(Mathf.Clamp01(gauge) * 100f, LengthUnit.Percent);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameConfig == null)
                Debug.LogWarning("[HudController] gameConfig is not assigned.", this);
            if (playerHpVar == null)
                Debug.LogWarning("[HudController] playerHpVar is not assigned.", this);
            if (scoreVar == null)
                Debug.LogWarning("[HudController] scoreVar is not assigned.", this);
            if (comboCountVar == null)
                Debug.LogWarning("[HudController] comboCountVar is not assigned.", this);
            if (spinGaugeVar == null)
                Debug.LogWarning("[HudController] spinGaugeVar is not assigned.", this);
            if (onGamePhaseChanged == null)
                Debug.LogWarning("[HudController] onGamePhaseChanged is not assigned.", this);
            if (onPlayerHpChanged == null)
                Debug.LogWarning("[HudController] onPlayerHpChanged is not assigned.", this);
            if (onScoreChanged == null)
                Debug.LogWarning("[HudController] onScoreChanged is not assigned.", this);
            if (onComboCountChanged == null)
                Debug.LogWarning("[HudController] onComboCountChanged is not assigned.", this);
            if (onSpinGaugeChanged == null)
                Debug.LogWarning("[HudController] onSpinGaugeChanged is not assigned.", this);
            if (playerLevelVar == null)
                Debug.LogWarning("[HudController] playerLevelVar is not assigned.", this);
            if (onPlayerLevelUp == null)
                Debug.LogWarning("[HudController] onPlayerLevelUp is not assigned.", this);
            if (uiDocument == null)
                Debug.LogWarning("[HudController] uiDocument is not assigned.", this);
        }
#endif
    }
}
