using UnityEngine;
using LitMotion;
using Tang3cko.ReactiveSO;

namespace Action002.Visual
{
    public class CameraShake : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onPlayerDamaged;

        [Header("Settings")]
        [SerializeField] private float duration = 0.15f;
        [SerializeField] private float magnitude = 0.1f;

        private Vector3 originalPosition;
        private MotionHandle shakeHandle;

        private void Awake()
        {
            originalPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            if (onPlayerDamaged == null)
            {
                return;
            }

            onPlayerDamaged.OnEventRaised += HandlePlayerDamaged;
        }

        private void OnDisable()
        {
            CancelShake();

            if (onPlayerDamaged == null)
            {
                return;
            }

            onPlayerDamaged.OnEventRaised -= HandlePlayerDamaged;
        }

        private void HandlePlayerDamaged()
        {
            CancelShake();
            originalPosition = transform.localPosition;

            shakeHandle = LMotion.Create(magnitude, 0f, duration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() =>
                {
                    transform.localPosition = originalPosition;
                })
                .Bind(currentMagnitude =>
                {
                    float x = Random.Range(-1f, 1f) * currentMagnitude;
                    float y = Random.Range(-1f, 1f) * currentMagnitude;
                    transform.localPosition = originalPosition + new Vector3(x, y, 0f);
                });
        }

        private void CancelShake()
        {
            if (shakeHandle.IsActive())
            {
                shakeHandle.Cancel();
                transform.localPosition = originalPosition;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onPlayerDamaged == null) Debug.LogWarning($"[{GetType().Name}] onPlayerDamaged not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
