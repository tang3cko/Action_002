using UnityEngine;
using Action002.Boss.Data;
using Action002.Core;
using Action002.Visual;

namespace Action002.Boss.Rendering
{
    public class BossRenderer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer whiteGuardianRenderer;
        [SerializeField] private SpriteRenderer blackGuardianRenderer;
        [SerializeField] private SpriteRenderer magatamaRenderer;

        public void ShowEntity(BossEntityId id)
        {
            var renderer = GetRenderer(id);
            if (renderer != null)
                renderer.gameObject.SetActive(true);
        }

        public void HideEntity(BossEntityId id)
        {
            var renderer = GetRenderer(id);
            if (renderer != null)
                renderer.gameObject.SetActive(false);
        }

        public void UpdatePosition(BossEntityId id, float x, float y)
        {
            var renderer = GetRenderer(id);
            if (renderer != null)
                renderer.transform.position = new Vector3(x, y, 0f);
        }

        public void UpdatePolarity(BossEntityId id, byte polarity)
        {
            var renderer = GetRenderer(id);
            if (renderer != null)
                renderer.color = PolarityColors.GetForeground((Polarity)polarity);
        }

        public void PlayMergeAnimation()
        {
            // TODO: coroutine/animator for merge visual
        }

        public void HideAll()
        {
            if (whiteGuardianRenderer != null)
                whiteGuardianRenderer.gameObject.SetActive(false);
            if (blackGuardianRenderer != null)
                blackGuardianRenderer.gameObject.SetActive(false);
            if (magatamaRenderer != null)
                magatamaRenderer.gameObject.SetActive(false);
        }

        private SpriteRenderer GetRenderer(BossEntityId id)
        {
            return id switch
            {
                BossEntityId.WhiteGuardian => whiteGuardianRenderer,
                BossEntityId.BlackGuardian => blackGuardianRenderer,
                BossEntityId.Magatama => magatamaRenderer,
                _ => null,
            };
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (whiteGuardianRenderer == null) Debug.LogWarning($"[{GetType().Name}] whiteGuardianRenderer not assigned on {gameObject.name}.", this);
            if (blackGuardianRenderer == null) Debug.LogWarning($"[{GetType().Name}] blackGuardianRenderer not assigned on {gameObject.name}.", this);
            if (magatamaRenderer == null) Debug.LogWarning($"[{GetType().Name}] magatamaRenderer not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
