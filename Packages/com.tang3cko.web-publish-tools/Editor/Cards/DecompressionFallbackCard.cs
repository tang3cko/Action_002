using System;
using Tang3cko.WebPublishTools.Editor.Profiles;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor.Cards
{
    public sealed class DecompressionFallbackCard : ICheckCard
    {
        public VisualElement Build(IPublishProfile profile, Action onChanged)
        {
            var card = CardBuilder.CreateCard(
                "Decompression Fallback",
                "Embeds the JS decompressor in the loader. Off keeps the build small; On is a safety net when the server lacks the right Content-Encoding headers.");

            var current = PlayerSettings.WebGL.decompressionFallback;

            card.Add(CardBuilder.CreateBoolRow(current, profile.ExpectedDecompressionFallback, value =>
            {
                PlayerSettings.WebGL.decompressionFallback = value;
                AssetDatabase.SaveAssets();
                onChanged?.Invoke();
            }));

            return card;
        }
    }
}
