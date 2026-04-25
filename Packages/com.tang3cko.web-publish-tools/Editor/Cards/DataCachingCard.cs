using System;
using Tang3cko.WebPublishTools.Editor.Profiles;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor.Cards
{
    public sealed class DataCachingCard : ICheckCard
    {
        public VisualElement Build(IPublishProfile profile, Action onChanged)
        {
            var card = CardBuilder.CreateCard(
                "Data Caching",
                "Caches the .data file in IndexedDB. Off avoids users seeing stale builds after you re-upload.");

            var current = PlayerSettings.WebGL.dataCaching;

            card.Add(CardBuilder.CreateBoolRow(current, profile.ExpectedDataCaching, value =>
            {
                PlayerSettings.WebGL.dataCaching = value;
                AssetDatabase.SaveAssets();
                onChanged?.Invoke();
            }));

            return card;
        }
    }
}
