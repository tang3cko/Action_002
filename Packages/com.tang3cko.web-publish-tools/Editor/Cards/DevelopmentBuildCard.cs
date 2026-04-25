using System;
using Tang3cko.WebPublishTools.Editor.Profiles;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor.Cards
{
    public sealed class DevelopmentBuildCard : ICheckCard
    {
        public VisualElement Build(IPublishProfile profile, Action onChanged)
        {
            var card = CardBuilder.CreateCard(
                "Development Build",
                "Includes profiler and debug symbols. Disable for release uploads to keep the build small and fast.");

            var current = EditorUserBuildSettings.development;

            card.Add(CardBuilder.CreateBoolRow(current, profile.ExpectedDevelopmentBuild, value =>
            {
                EditorUserBuildSettings.development = value;
                onChanged?.Invoke();
            }));

            return card;
        }
    }
}
