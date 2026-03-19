using UnityEngine;
using UnityEngine.UIElements;
using Tang3cko.ReactiveSO;

namespace Action002.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class TitleScreenController : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private VoidEventChannelSO onTitleStartSelected;

        private UIDocument uiDocument;
        private VisualElement rootVisualElement;
        private Button startButton;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (uiDocument == null) return;

            rootVisualElement = uiDocument.rootVisualElement;

            startButton = rootVisualElement.Q<Button>("StartButton");
            if (startButton == null)
            {
                Debug.LogError($"[{GetType().Name}] StartButton not found in UIDocument on {gameObject.name}.", this);
                return;
            }

            startButton.clicked += OnStartButtonClicked;
        }

        private void OnDisable()
        {
            if (startButton != null)
                startButton.clicked -= OnStartButtonClicked;
        }

        public void Show()
        {
            if (rootVisualElement != null)
                rootVisualElement.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (rootVisualElement != null)
                rootVisualElement.style.display = DisplayStyle.None;
        }

        private void OnStartButtonClicked()
        {
            onTitleStartSelected?.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GetComponent<UIDocument>() == null) Debug.LogWarning($"[{GetType().Name}] UIDocument component missing on {gameObject.name}.", this);
            if (onTitleStartSelected == null) Debug.LogWarning($"[{GetType().Name}] onTitleStartSelected not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
