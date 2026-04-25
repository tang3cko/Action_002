using System;
using Tang3cko.WebPublishTools.Editor.Profiles;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor.Cards
{
    public sealed class ExceptionSupportCard : ICheckCard
    {
        public VisualElement Build(IPublishProfile profile, Action onChanged)
        {
            var card = CardBuilder.CreateCard(
                "Exception Support",
                "Runtime exception handling. None produces the smallest binary; higher levels help debugging at the cost of size and speed.");

            var current = PlayerSettings.WebGL.exceptionSupport;

            card.Add(CardBuilder.CreateEnumRow(current, profile.ExpectedExceptionSupport, value =>
            {
                PlayerSettings.WebGL.exceptionSupport = value;
                AssetDatabase.SaveAssets();
                onChanged?.Invoke();
            }));

            return card;
        }
    }
}
