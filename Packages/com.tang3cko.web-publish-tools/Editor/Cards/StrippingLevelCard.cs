using System;
using Tang3cko.WebPublishTools.Editor.Profiles;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor.Cards
{
    public sealed class StrippingLevelCard : ICheckCard
    {
        public VisualElement Build(IPublishProfile profile, Action onChanged)
        {
            var card = CardBuilder.CreateCard(
                "Managed Stripping Level",
                "Removes unused managed code from the build. High shrinks size most but may break reflection-heavy code.");

            var current = PlayerSettings.GetManagedStrippingLevel(NamedBuildTarget.WebGL);

            card.Add(CardBuilder.CreateEnumRow(current, profile.ExpectedStrippingLevel, value =>
            {
                PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, value);
                AssetDatabase.SaveAssets();
                onChanged?.Invoke();
            }));

            return card;
        }
    }
}
