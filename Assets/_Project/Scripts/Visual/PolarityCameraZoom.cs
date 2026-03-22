using UnityEngine;
using LitMotion;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class PolarityCameraZoom : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onPolarityChanged;

        [Header("Settings")]
        [SerializeField] private float zoomAmount = 0.3f;
        [SerializeField] private float zoomInDuration = 0.05f;
        [SerializeField] private float zoomOutDuration = 0.1f;

        private Camera targetCamera;
        private float originalOrthographicSize;
        private MotionHandle zoomHandle;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            originalOrthographicSize = targetCamera.orthographicSize;
        }

        private void OnEnable()
        {
            if (onPolarityChanged == null)
            {
                return;
            }

            onPolarityChanged.OnEventRaised += HandlePolarityChanged;
        }

        private void OnDisable()
        {
            CancelZoom();

            if (onPolarityChanged == null)
            {
                return;
            }

            onPolarityChanged.OnEventRaised -= HandlePolarityChanged;
        }

        private void HandlePolarityChanged(int polarity)
        {
            CancelZoom();

            float zoomedSize = originalOrthographicSize - zoomAmount;

            zoomHandle = LMotion.Create(originalOrthographicSize, zoomedSize, zoomInDuration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() =>
                {
                    zoomHandle = LMotion.Create(zoomedSize, originalOrthographicSize, zoomOutDuration)
                        .WithEase(Ease.OutQuad)
                        .WithOnComplete(() =>
                        {
                            targetCamera.orthographicSize = originalOrthographicSize;
                        })
                        .Bind(size =>
                        {
                            targetCamera.orthographicSize = size;
                        });
                })
                .Bind(size =>
                {
                    targetCamera.orthographicSize = size;
                });
        }

        private void CancelZoom()
        {
            if (zoomHandle.IsActive())
            {
                zoomHandle.Cancel();
                targetCamera.orthographicSize = originalOrthographicSize;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onPolarityChanged == null) Debug.LogWarning($"[{GetType().Name}] onPolarityChanged not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
