using UnityEngine;

namespace Action002.Visual
{
    public static class DiamondTextureGenerator
    {
        public static Texture2D Create(int resolution = 64)
        {
            var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            int totalPixels = resolution * resolution;
            var pixels = new Color32[totalPixels];
            float center = resolution * 0.5f;

            for (int i = 0; i < totalPixels; i++)
            {
                float x = (i % resolution) + 0.5f;
                float y = (i / resolution) + 0.5f;
                float dx = Mathf.Abs(x - center);
                float dy = Mathf.Abs(y - center);
                float dist = (dx + dy) / center;

                byte alpha = dist > 1f ? (byte)0
                    : dist < 0.9f ? (byte)255
                    : (byte)(255f * (1f - (dist - 0.9f) / 0.1f));

                pixels[i] = new Color32(255, 255, 255, alpha);
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }
    }
}
