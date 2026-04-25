using System;
using Tang3cko.WebPublishTools.Editor.Profiles;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor.Cards
{
    public sealed class CompressionCard : ICheckCard
    {
        public VisualElement Build(IPublishProfile profile, Action onChanged)
        {
            var card = CardBuilder.CreateCard(
                "Compression Format",
                "Compression for the build files. unityroom requires Gzip; Brotli is unsupported there.");

            var current = PlayerSettings.WebGL.compressionFormat;

            card.Add(CardBuilder.CreateEnumRow(current, profile.ExpectedCompression, value =>
            {
                PlayerSettings.WebGL.compressionFormat = value;
                AssetDatabase.SaveAssets();
                onChanged?.Invoke();
            }));

            return card;
        }
    }
}
