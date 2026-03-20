using UnityEngine;
using UnityEngine.UIElements;
using Action002.Core.Flow;
using Tang3cko.ReactiveSO;

namespace Action002.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ResultScreenController : MonoBehaviour
    {
        [Header("Events (subscribe)")]
        [SerializeField] private IntEventChannelSO onGamePhaseChanged;

        [Header("Variables (read)")]
        [SerializeField] private IntVariableSO scoreVar;
        [SerializeField] private IntVariableSO resultTypeVar;

        [Header("Events (publish)")]
        [SerializeField] private VoidEventChannelSO onResultRetrySelected;
        [SerializeField] private VoidEventChannelSO onResultBackToTitleSelected;

        private UIDocument uiDocument;
        private VisualElement resultScreenRoot;
        private Label resultTypeLabel;
        private Label resultLabel;
        private Label scoreLabel;
        private Button retryButton;
        private Button titleButton;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (uiDocument == null) return;

            var root = uiDocument.rootVisualElement;
            resultScreenRoot = root.Q<VisualElement>("ResultScreen");
            if (resultScreenRoot == null)
            {
                Debug.LogError($"[{GetType().Name}] ResultScreen panel not found in UIDocument on {gameObject.name}.", this);
                return;
            }

            retryButton = resultScreenRoot.Q<Button>("RetryButton");
            if (retryButton == null)
                Debug.LogError($"[{GetType().Name}] RetryButton not found in UIDocument on {gameObject.name}.", this);

            titleButton = resultScreenRoot.Q<Button>("TitleButton");
            if (titleButton == null)
                Debug.LogError($"[{GetType().Name}] TitleButton not found in UIDocument on {gameObject.name}.", this);

            resultTypeLabel = resultScreenRoot.Q<Label>("ResultTypeLabel");
            if (resultTypeLabel == null)
                Debug.LogError($"[{GetType().Name}] ResultTypeLabel not found in UIDocument on {gameObject.name}.", this);

            resultLabel = resultScreenRoot.Q<Label>("ResultTitleLabel");
            if (resultLabel == null)
                Debug.LogError($"[{GetType().Name}] ResultTitleLabel not found in UIDocument on {gameObject.name}.", this);

            scoreLabel = resultScreenRoot.Q<Label>("ResultScoreLabel");
            if (scoreLabel == null)
                Debug.LogError($"[{GetType().Name}] ResultScoreLabel not found in UIDocument on {gameObject.name}.", this);

            if (retryButton != null)
                retryButton.clicked += OnRetryClicked;

            if (titleButton != null)
                titleButton.clicked += OnTitleClicked;

            if (onGamePhaseChanged != null)
                onGamePhaseChanged.OnEventRaised += HandleGamePhaseChanged;

            Show();
        }

        private void OnDisable()
        {
            if (retryButton != null)
                retryButton.clicked -= OnRetryClicked;

            if (titleButton != null)
                titleButton.clicked -= OnTitleClicked;

            if (onGamePhaseChanged != null)
                onGamePhaseChanged.OnEventRaised -= HandleGamePhaseChanged;
        }

        private void HandleGamePhaseChanged(int phase)
        {
            if (phase == (int)GamePhase.Result)
                Show();
            else
                Hide();
        }

        private void Show()
        {
            if (resultScreenRoot == null) return;

            resultScreenRoot.style.display = DisplayStyle.Flex;

            if (resultTypeVar == null)
                Debug.LogError($"[{GetType().Name}] resultTypeVar is null on {gameObject.name}. Defaulting to GameOver.", this);

            int resultType = resultTypeVar != null ? resultTypeVar.Value : (int)GameResultType.GameOver;
            bool isClear = resultType == (int)GameResultType.Clear;

            if (!isClear && resultType != (int)GameResultType.GameOver)
                Debug.LogWarning($"[{GetType().Name}] Unknown resultType={resultType} on {gameObject.name}. Defaulting to GameOver.", this);

            resultScreenRoot.RemoveFromClassList("result-screen--gameover");
            resultScreenRoot.RemoveFromClassList("result-screen--clear");
            resultScreenRoot.AddToClassList(isClear ? "result-screen--clear" : "result-screen--gameover");

            if (resultTypeLabel != null)
                resultTypeLabel.text = isClear ? "STAGE CLEAR" : "GAME OVER";

            if (resultLabel != null)
                resultLabel.text = isClear ? "RESONANCE HELD" : "POLARITY COLLAPSED";

            if (scoreLabel != null)
                scoreLabel.text = scoreVar != null ? scoreVar.Value.ToString() : "0";
        }

        private void Hide()
        {
            if (resultScreenRoot == null) return;

            resultScreenRoot.style.display = DisplayStyle.None;
            resultScreenRoot.RemoveFromClassList("result-screen--gameover");
            resultScreenRoot.RemoveFromClassList("result-screen--clear");
        }

        private void OnRetryClicked()
        {
            onResultRetrySelected?.RaiseEvent();
        }

        private void OnTitleClicked()
        {
            onResultBackToTitleSelected?.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GetComponent<UIDocument>() == null) Debug.LogWarning($"[{GetType().Name}] UIDocument component missing on {gameObject.name}.", this);
            if (onGamePhaseChanged == null) Debug.LogWarning($"[{GetType().Name}] onGamePhaseChanged not assigned on {gameObject.name}.", this);
            if (scoreVar == null) Debug.LogWarning($"[{GetType().Name}] scoreVar not assigned on {gameObject.name}.", this);
            if (resultTypeVar == null) Debug.LogWarning($"[{GetType().Name}] resultTypeVar not assigned on {gameObject.name}.", this);
            if (onResultRetrySelected == null) Debug.LogWarning($"[{GetType().Name}] onResultRetrySelected not assigned on {gameObject.name}.", this);
            if (onResultBackToTitleSelected == null) Debug.LogWarning($"[{GetType().Name}] onResultBackToTitleSelected not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
