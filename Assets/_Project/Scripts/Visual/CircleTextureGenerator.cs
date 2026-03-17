using UnityEngine;

namespace Action002.Visual
{
    public static class CircleTextureGenerator
    {
        public static Texture2D Create(int resolution = 64)
        {
            var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            int totalPixels = resolution * resolution;
            var pixels = new Color32[totalPixels];
            float center = resolution * 0.5f;
            float radiusSq = center * center;
            float edgeStart = (center - 1.5f) * (center - 1.5f);

            for (int i = 0; i < totalPixels; i++)
            {
                float dx = (i % resolution) + 0.5f - center;
                float dy = (i / resolution) + 0.5f - center;
                float distSq = dx * dx + dy * dy;

                byte alpha = distSq > radiusSq ? (byte)0
                    : distSq < edgeStart ? (byte)255
                    : (byte)(255f * (1f - (Mathf.Sqrt(distSq) - center + 1.5f) / 1.5f));

                pixels[i] = new Color32(255, 255, 255, alpha);
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }
    }
}
