using UnityEngine;
using UnityEngine.UIElements;
using Action002.Core.Flow;
using Tang3cko.ReactiveSO;

namespace Action002.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class TitleScreenController : MonoBehaviour
    {
        [Header("Events (subscribe)")]
        [SerializeField] private IntEventChannelSO onGamePhaseChanged;

        [Header("Events (publish)")]
        [SerializeField] private Vector2EventChannelSO onTitleStartTransitionOriginSelected;
        [SerializeField] private VoidEventChannelSO onTitleStartSelected;

        private UIDocument uiDocument;
        private VisualElement titleScreenRoot;
        private Button startButton;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (uiDocument == null) return;

            var root = uiDocument.rootVisualElement;
            titleScreenRoot = root.Q<VisualElement>("TitleScreen");
            if (titleScreenRoot == null)
            {
                Debug.LogError($"[{GetType().Name}] TitleScreen panel not found in UIDocument on {gameObject.name}.", this);
                return;
            }

            startButton = titleScreenRoot.Q<Button>("TitleStartButton");
            if (startButton == null)
            {
                Debug.LogError($"[{GetType().Name}] TitleStartButton not found in UIDocument on {gameObject.name}.", this);
                return;
            }

            startButton.RegisterCallback<PointerUpEvent>(OnStartButtonPointerUp);

            if (onGamePhaseChanged != null)
                onGamePhaseChanged.OnEventRaised += HandleGamePhaseChanged;

            Show();
        }

        private void OnDisable()
        {
            if (startButton != null)
                startButton.UnregisterCallback<PointerUpEvent>(OnStartButtonPointerUp);

            if (onGamePhaseChanged != null)
                onGamePhaseChanged.OnEventRaised -= HandleGamePhaseChanged;
        }

        private void HandleGamePhaseChanged(int phase)
        {
            if (phase == (int)GamePhase.Title)
                Show();
            else
                Hide();
        }

        private void Show()
        {
            if (titleScreenRoot != null)
                titleScreenRoot.style.display = DisplayStyle.Flex;
        }

        private void Hide()
        {
            if (titleScreenRoot != null)
                titleScreenRoot.style.display = DisplayStyle.None;
        }

        private void OnStartButtonPointerUp(PointerUpEvent evt)
        {
            Vector2 screenPosition = new Vector2(evt.position.x, Screen.height - evt.position.y);
            onTitleStartTransitionOriginSelected?.RaiseEvent(screenPosition);
            onTitleStartSelected?.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GetComponent<UIDocument>() == null) Debug.LogWarning($"[{GetType().Name}] UIDocument component missing on {gameObject.name}.", this);
            if (onGamePhaseChanged == null) Debug.LogWarning($"[{GetType().Name}] onGamePhaseChanged not assigned on {gameObject.name}.", this);
            if (onTitleStartTransitionOriginSelected == null) Debug.LogWarning($"[{GetType().Name}] onTitleStartTransitionOriginSelected not assigned on {gameObject.name}.", this);
            if (onTitleStartSelected == null) Debug.LogWarning($"[{GetType().Name}] onTitleStartSelected not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
