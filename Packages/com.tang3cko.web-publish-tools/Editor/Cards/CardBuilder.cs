using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor.Cards
{
    internal static class CardBuilder
    {
        public static VisualElement CreateCard(string title, string subtitle = null)
        {
            var card = new VisualElement();
            card.AddToClassList("web-publish-tools__card");

            var header = new VisualElement();
            header.AddToClassList("web-publish-tools__card-header");

            var titleLabel = new Label(title);
            titleLabel.AddToClassList("web-publish-tools__card-title");
            header.Add(titleLabel);

            card.Add(header);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var subtitleLabel = new Label(subtitle);
                subtitleLabel.AddToClassList("web-publish-tools__card-subtitle");
                card.Add(subtitleLabel);
            }

            return card;
        }

        public static VisualElement CreateStatusRow(
            CheckStatus status,
            string label,
            string applyText = null,
            Action applyAction = null)
        {
            var row = CreateRow(status);

            var labelElement = new Label(label);
            labelElement.AddToClassList("web-publish-tools__row-label");
            row.Add(labelElement);

            if (status == CheckStatus.Mismatch && applyAction != null && !string.IsNullOrEmpty(applyText))
            {
                var applyButton = new Button(applyAction) { text = applyText };
                applyButton.AddToClassList("web-publish-tools__row-button");
                row.Add(applyButton);
            }

            return row;
        }

        public static VisualElement CreateNoticeRow(string text)
        {
            var label = new Label(text);
            label.AddToClassList("web-publish-tools__notice");
            return label;
        }

        public static VisualElement CreateEnumRow<TEnum>(
            TEnum currentValue,
            TEnum? expectedValue,
            Action<TEnum> onApply)
            where TEnum : struct, Enum
        {
            var status = ResolveStatus(currentValue, expectedValue);
            var row = CreateRow(status);

            var values = ((TEnum[])Enum.GetValues(typeof(TEnum)))
                .Where(v => !IsObsolete(v))
                .ToArray();
            var labels = values.Select(v => v.ToString()).ToList();

            var dropdown = new DropdownField(labels, currentValue.ToString());
            dropdown.AddToClassList("web-publish-tools__row-control");
            dropdown.RegisterValueChangedCallback(evt =>
            {
                if (Enum.TryParse(evt.newValue, out TEnum parsed))
                {
                    onApply(parsed);
                }
            });
            row.Add(dropdown);

            AppendShortcut(row, expectedValue, currentValue, onApply);

            return row;
        }

        public static VisualElement CreateBoolRow(
            bool currentValue,
            bool? expectedValue,
            Action<bool> onApply)
        {
            var status = ResolveStatus(currentValue, expectedValue);
            var row = CreateRow(status);

            var toggle = new Toggle { value = currentValue };
            toggle.AddToClassList("web-publish-tools__row-control");
            toggle.AddToClassList("web-publish-tools__row-control--toggle");
            toggle.RegisterValueChangedCallback(evt => onApply(evt.newValue));
            row.Add(toggle);

            if (expectedValue.HasValue && currentValue != expectedValue.Value)
            {
                var expected = expectedValue.Value;
                var shortcut = new Button(() => onApply(expected))
                {
                    text = $"Set {(expected ? "On" : "Off")}",
                };
                shortcut.AddToClassList("web-publish-tools__row-button");
                row.Add(shortcut);
            }

            return row;
        }

        private static VisualElement CreateRow(CheckStatus status)
        {
            var row = new VisualElement();
            row.AddToClassList("web-publish-tools__row");

            var icon = new Label(StatusIcon(status));
            icon.AddToClassList(StatusIconClass(status));
            icon.AddToClassList("web-publish-tools__row-icon");
            row.Add(icon);

            return row;
        }

        private static void AppendShortcut<TEnum>(
            VisualElement row,
            TEnum? expectedValue,
            TEnum currentValue,
            Action<TEnum> onApply)
            where TEnum : struct, Enum
        {
            if (!expectedValue.HasValue) return;
            if (EqualityComparer<TEnum>.Default.Equals(currentValue, expectedValue.Value)) return;

            var expected = expectedValue.Value;
            var shortcut = new Button(() => onApply(expected)) { text = $"Set {expected}" };
            shortcut.AddToClassList("web-publish-tools__row-button");
            row.Add(shortcut);
        }

        public static CheckStatus ResolveStatus<TEnum>(TEnum current, TEnum? expected)
            where TEnum : struct, Enum
        {
            if (!expected.HasValue) return CheckStatus.Info;
            return EqualityComparer<TEnum>.Default.Equals(current, expected.Value)
                ? CheckStatus.Match
                : CheckStatus.Mismatch;
        }

        public static CheckStatus ResolveStatus(bool current, bool? expected)
        {
            if (!expected.HasValue) return CheckStatus.Info;
            return current == expected.Value ? CheckStatus.Match : CheckStatus.Mismatch;
        }

        private static string StatusIcon(CheckStatus status) => status switch
        {
            CheckStatus.Match => "\u2713",
            CheckStatus.Mismatch => "\u2717",
            CheckStatus.Info => "\u25CB",
            _ => "\u25CB",
        };

        private static string StatusIconClass(CheckStatus status) => status switch
        {
            CheckStatus.Match => "web-publish-tools__icon--ok",
            CheckStatus.Mismatch => "web-publish-tools__icon--ng",
            CheckStatus.Info => "web-publish-tools__icon--info",
            _ => "web-publish-tools__icon--info",
        };

        private static bool IsObsolete<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString());
            if (member.Length == 0) return false;
            return Attribute.IsDefined(member[0], typeof(ObsoleteAttribute));
        }
    }

    internal enum CheckStatus
    {
        Match,
        Mismatch,
        Info,
    }
}
