using System;
using System.Collections.Generic;
using System.Linq;
using Tang3cko.WebPublishTools.Editor.Profiles;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor.Cards
{
    public sealed class ResolutionCard : ICheckCard
    {
        private static readonly ResolutionPreset[] presets = new[]
        {
            new ResolutionPreset("640 x 360 (Landscape)", 640, 360),
            new ResolutionPreset("854 x 480 (Landscape)", 854, 480),
            new ResolutionPreset("960 x 540 (Landscape)", 960, 540),
            new ResolutionPreset("1280 x 720 (Landscape)", 1280, 720),
            new ResolutionPreset("1920 x 1080 (Landscape)", 1920, 1080),
            new ResolutionPreset("360 x 640 (Portrait)", 360, 640),
            new ResolutionPreset("540 x 960 (Portrait)", 540, 960),
            new ResolutionPreset("1080 x 1920 (Portrait)", 1080, 1920),
            new ResolutionPreset("720 x 720 (Square)", 720, 720),
        };

        private const string CustomLabel = "Custom...";

        public VisualElement Build(IPublishProfile profile, Action onChanged)
        {
            var card = CardBuilder.CreateCard(
                "Resolution",
                "Default canvas size for the WebGL player. Pick a preset that matches the host platform's display.");

            int currentWidth = PlayerSettings.defaultWebScreenWidth;
            int currentHeight = PlayerSettings.defaultWebScreenHeight;

            card.Add(CardBuilder.CreateStatusRow(
                CheckStatus.Info,
                $"Current: {currentWidth} x {currentHeight}"));

            var dropdownChoices = presets.Select(p => p.Label).Append(CustomLabel).ToList();
            string initialChoice = MatchPresetLabel(currentWidth, currentHeight) ?? CustomLabel;

            var dropdown = new DropdownField("Preset", dropdownChoices, initialChoice);
            dropdown.AddToClassList("web-publish-tools__dropdown");
            card.Add(dropdown);

            var customRow = new VisualElement();
            customRow.AddToClassList("web-publish-tools__custom-row");

            var widthField = new IntegerField("Width") { value = currentWidth };
            widthField.AddToClassList("web-publish-tools__custom-field");
            var heightField = new IntegerField("Height") { value = currentHeight };
            heightField.AddToClassList("web-publish-tools__custom-field");

            customRow.Add(widthField);
            customRow.Add(heightField);
            card.Add(customRow);

            customRow.style.display = initialChoice == CustomLabel
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            dropdown.RegisterValueChangedCallback(evt =>
            {
                customRow.style.display = evt.newValue == CustomLabel
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

                var preset = presets.FirstOrDefault(p => p.Label == evt.newValue);
                if (preset != null)
                {
                    widthField.value = preset.Width;
                    heightField.value = preset.Height;
                }
            });

            var applyButton = new Button(() =>
            {
                int w = Math.Max(1, widthField.value);
                int h = Math.Max(1, heightField.value);
                PlayerSettings.defaultWebScreenWidth = w;
                PlayerSettings.defaultWebScreenHeight = h;
                AssetDatabase.SaveAssets();
                onChanged?.Invoke();
            }) { text = "Apply" };
            applyButton.AddToClassList("web-publish-tools__card-button");
            card.Add(applyButton);

            return card;
        }

        private static string MatchPresetLabel(int width, int height)
        {
            return presets.FirstOrDefault(p => p.Width == width && p.Height == height)?.Label;
        }

        private sealed class ResolutionPreset
        {
            public string Label { get; }
            public int Width { get; }
            public int Height { get; }

            public ResolutionPreset(string label, int width, int height)
            {
                Label = label;
                Width = width;
                Height = height;
            }
        }
    }
}
