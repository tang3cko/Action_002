using UnityEngine;
using UnityEngine.UIElements;
using Action002.Core;
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

        [Header("Events")]
        [SerializeField] private IntEventChannelSO onPlayerHpChanged;
        [SerializeField] private IntEventChannelSO onScoreChanged;
        [SerializeField] private IntEventChannelSO onComboCountChanged;
        [SerializeField] private FloatEventChannelSO onSpinGaugeChanged;

        [Header("UI")]
        [SerializeField] private UIDocument uiDocument;

        private const string PipClass = "hud__hp-pip";
        private const string PipDeadClass = "hud__hp-pip--dead";

        private VisualElement[] hpPips;
        private Label scoreLabel;
        private Label comboLabel;
        private VisualElement gaugeFill;
        private VisualElement gaugeTrack;

        private void Start()
        {
            if (uiDocument == null) return;

            var root = uiDocument.rootVisualElement;
            var hpGroup = root.Q("HpGroup");
            if (hpGroup == null) return;

            int maxHp = gameConfig != null ? gameConfig.MaxHp : 5;
            hpGroup.Clear();
            hpPips = new VisualElement[maxHp];
            for (int i = 0; i < maxHp; i++)
            {
                var pip = new VisualElement();
                pip.AddToClassList(PipClass);
                hpGroup.Add(pip);
                hpPips[i] = pip;
            }

            scoreLabel = root.Q<Label>("ScoreLabel");
            comboLabel = root.Q<Label>("ComboLabel");
            gaugeTrack = root.Q("GaugeTrack");
            gaugeFill = root.Q("GaugeFill");

            if (onPlayerHpChanged != null)
                onPlayerHpChanged.OnEventRaised += OnHpChanged;
            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised += OnScoreChanged;
            if (onComboCountChanged != null)
                onComboCountChanged.OnEventRaised += OnComboChanged;
            if (onSpinGaugeChanged != null)
                onSpinGaugeChanged.OnEventRaised += OnGaugeChanged;

            if (playerHpVar != null) OnHpChanged(playerHpVar.Value);
            if (scoreVar != null) OnScoreChanged(scoreVar.Value);
            if (comboCountVar != null) OnComboChanged(comboCountVar.Value);
            if (spinGaugeVar != null) OnGaugeChanged(spinGaugeVar.Value);
        }

        private void OnDestroy()
        {
            if (onPlayerHpChanged != null)
                onPlayerHpChanged.OnEventRaised -= OnHpChanged;
            if (onScoreChanged != null)
                onScoreChanged.OnEventRaised -= OnScoreChanged;
            if (onComboCountChanged != null)
                onComboCountChanged.OnEventRaised -= OnComboChanged;
            if (onSpinGaugeChanged != null)
                onSpinGaugeChanged.OnEventRaised -= OnGaugeChanged;
        }

        private void OnHpChanged(int hp)
        {
            if (hpPips == null) return;
            for (int i = 0; i < hpPips.Length; i++)
            {
                if (hpPips[i] == null) continue;
                if (i < hp)
                    hpPips[i].RemoveFromClassList(PipDeadClass);
                else
                    hpPips[i].AddToClassList(PipDeadClass);
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

        private void OnGaugeChanged(float gauge)
        {
            if (gaugeFill != null)
                gaugeFill.style.width = new Length(Mathf.Clamp01(gauge) * 100f, LengthUnit.Percent);
        }
    }
}
