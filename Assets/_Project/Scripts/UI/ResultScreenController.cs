using UnityEngine;
using UnityEngine.UIElements;
using Action002.Core.Flow;
using Tang3cko.ReactiveSO;

namespace Action002.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ResultScreenController : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private VoidEventChannelSO onResultRetrySelected;
        [SerializeField] private VoidEventChannelSO onResultBackToTitleSelected;

        private UIDocument uiDocument;
        private VisualElement root;
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

            root = uiDocument.rootVisualElement;

            retryButton = root.Q<Button>("RetryButton");
            if (retryButton == null)
                Debug.LogError($"[{GetType().Name}] RetryButton not found in UIDocument on {gameObject.name}.", this);

            titleButton = root.Q<Button>("TitleButton");
            if (titleButton == null)
                Debug.LogError($"[{GetType().Name}] TitleButton not found in UIDocument on {gameObject.name}.", this);

            resultLabel = root.Q<Label>("ResultLabel");
            if (resultLabel == null)
                Debug.LogError($"[{GetType().Name}] ResultLabel not found in UIDocument on {gameObject.name}.", this);

            scoreLabel = root.Q<Label>("ScoreLabel");
            if (scoreLabel == null)
                Debug.LogError($"[{GetType().Name}] ScoreLabel not found in UIDocument on {gameObject.name}.", this);

            if (retryButton != null)
                retryButton.clicked += OnRetryClicked;

            if (titleButton != null)
                titleButton.clicked += OnTitleClicked;

            Hide();
        }

        private void OnDisable()
        {
            if (retryButton != null)
                retryButton.clicked -= OnRetryClicked;

            if (titleButton != null)
                titleButton.clicked -= OnTitleClicked;
        }

        public void Show(GameResultType resultType, int score)
        {
            if (root != null)
                root.style.display = DisplayStyle.Flex;

            if (resultLabel != null)
            {
                resultLabel.text = resultType switch
                {
                    GameResultType.Clear => "STAGE CLEAR",
                    GameResultType.GameOver => "GAME OVER",
                    _ => ""
                };
            }

            if (scoreLabel != null)
                scoreLabel.text = score.ToString();
        }

        public void Hide()
        {
            if (root != null)
                root.style.display = DisplayStyle.None;
        }

        private void OnRetryClicked()
        {
            if (onResultRetrySelected != null)
                onResultRetrySelected.RaiseEvent();
        }

        private void OnTitleClicked()
        {
            if (onResultBackToTitleSelected != null)
                onResultBackToTitleSelected.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GetComponent<UIDocument>() == null) Debug.LogWarning($"[{GetType().Name}] UIDocument component missing on {gameObject.name}.", this);
            if (onResultRetrySelected == null) Debug.LogWarning($"[{GetType().Name}] onResultRetrySelected not assigned on {gameObject.name}.", this);
            if (onResultBackToTitleSelected == null) Debug.LogWarning($"[{GetType().Name}] onResultBackToTitleSelected not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
