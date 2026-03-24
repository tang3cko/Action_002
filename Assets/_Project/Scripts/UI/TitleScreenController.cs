using UnityEngine;
using UnityEngine.UIElements;
using Action002.Core.Flow;
using Tang3cko.ReactiveSO;

namespace Action002.UI
{
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(AudioSource))]
    public class TitleScreenController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private AudioSource audioSource;

        [Header("Settings")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField, Range(0f, 1f)] private float clickVolume = 0.5f;

        [Header("Events (subscribe)")]
        [SerializeField] private IntEventChannelSO onGamePhaseChanged;

        [Header("Events (publish)")]
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
            startButton.RegisterCallback<NavigationSubmitEvent>(OnStartButtonSubmit);

            if (onGamePhaseChanged != null)
                onGamePhaseChanged.OnEventRaised += HandleGamePhaseChanged;

            Show();
        }

        private void OnDisable()
        {
            if (startButton != null)
            {
                startButton.UnregisterCallback<PointerUpEvent>(OnStartButtonPointerUp);
                startButton.UnregisterCallback<NavigationSubmitEvent>(OnStartButtonSubmit);
            }

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

            if (startButton != null)
                startButton.Focus();
        }

        private void Hide()
        {
            if (titleScreenRoot != null)
                titleScreenRoot.style.display = DisplayStyle.None;
        }

        private void PlayClickSFX()
        {
            if (audioSource != null && clickClip != null)
                audioSource.PlayOneShot(clickClip, clickVolume);
        }

        private void OnStartButtonPointerUp(PointerUpEvent evt)
        {
            PlayClickSFX();
            onTitleStartSelected?.RaiseEvent();
        }

        private void OnStartButtonSubmit(NavigationSubmitEvent evt)
        {
            PlayClickSFX();
            onTitleStartSelected?.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GetComponent<UIDocument>() == null) Debug.LogWarning($"[{GetType().Name}] UIDocument component missing on {gameObject.name}.", this);
            if (onGamePhaseChanged == null) Debug.LogWarning($"[{GetType().Name}] onGamePhaseChanged not assigned on {gameObject.name}.", this);
            if (onTitleStartSelected == null) Debug.LogWarning($"[{GetType().Name}] onTitleStartSelected not assigned on {gameObject.name}.", this);

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (audioSource == null) Debug.LogWarning($"[{GetType().Name}] audioSource not assigned on {gameObject.name}.", this);
            if (clickClip == null) Debug.LogWarning($"[{GetType().Name}] clickClip not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
