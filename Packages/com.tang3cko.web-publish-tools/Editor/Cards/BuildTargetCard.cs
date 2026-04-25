using System;
using Tang3cko.WebPublishTools.Editor.Profiles;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor.Cards
{
    public sealed class BuildTargetCard : ICheckCard
    {
        public VisualElement Build(IPublishProfile profile, Action onChanged)
        {
            var card = CardBuilder.CreateCard(
                "Build Target",
                "Active build platform. WebGL must be active to build for the web.");

            var current = EditorUserBuildSettings.activeBuildTarget;
            bool isWebGL = current == BuildTarget.WebGL;

            if (!profile.RequiresWebGLBuildTarget)
            {
                card.Add(CardBuilder.CreateStatusRow(CheckStatus.Info, $"Active: {current}"));
                return card;
            }

            var status = isWebGL ? CheckStatus.Match : CheckStatus.Mismatch;
            string label = isWebGL
                ? "Active: WebGL"
                : $"Active: {current} (WebGL required)";

            card.Add(CardBuilder.CreateStatusRow(status, label, "Switch to WebGL", () =>
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.WebGL, BuildTarget.WebGL);
                onChanged?.Invoke();
            }));

            if (!isWebGL)
            {
                card.Add(CardBuilder.CreateNoticeRow(
                    "Switching takes a while due to asset reimport."));
            }

            return card;
        }
    }
}
