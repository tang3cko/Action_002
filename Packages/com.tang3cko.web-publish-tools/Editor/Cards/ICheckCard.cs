using System;
using Tang3cko.WebPublishTools.Editor.Profiles;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor.Cards
{
    public interface ICheckCard
    {
        VisualElement Build(IPublishProfile profile, Action onChanged);
    }
}
