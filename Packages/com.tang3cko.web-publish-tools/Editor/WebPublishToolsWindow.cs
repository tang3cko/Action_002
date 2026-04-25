using System.Collections.Generic;
using System.Linq;
using Tang3cko.WebPublishTools.Editor.Cards;
using Tang3cko.WebPublishTools.Editor.Profiles;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tang3cko.WebPublishTools.Editor
{
    public sealed class WebPublishToolsWindow : EditorWindow
    {
        private const string SelectedProfilePrefsKey = "Tang3cko.WebPublishTools.SelectedProfile";

        private static readonly ICheckCard[] cards = new ICheckCard[]
        {
            new BuildTargetCard(),
            new CompressionCard(),
            new DecompressionFallbackCard(),
            new DevelopmentBuildCard(),
            new DataCachingCard(),
            new ExceptionSupportCard(),
            new StrippingLevelCard(),
            new ResolutionCard(),
        };

        private IPublishProfile activeProfile;

        [MenuItem("Window/Web Publish Tools")]
        public static void ShowWindow()
        {
            var window = GetWindow<WebPublishToolsWindow>();
            window.titleContent = new GUIContent("Web Publish Tools");
            window.minSize = new Vector2(480, 560);
        }

        private void CreateGUI()
        {
            string savedId = EditorPrefs.GetString(SelectedProfilePrefsKey, PublishProfileRegistry.All[0].Id);
            activeProfile = PublishProfileRegistry.GetById(savedId);

            BuildLayout();
        }

        private void OnFocus()
        {
            if (rootVisualElement.childCount > 0)
            {
                Rebuild();
            }
        }

        private void BuildLayout()
        {
            var root = rootVisualElement;
            root.Clear();

            var styleSheet = LoadStyleSheet();
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            root.AddToClassList("web-publish-tools__root");

            root.Add(BuildToolbar());
            root.Add(BuildAboutSection());
            root.Add(BuildDivider());

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.AddToClassList("web-publish-tools__scroll");
            scrollView.Add(BuildCards());
            root.Add(scrollView);
        }

        private void Rebuild() => BuildLayout();

        private VisualElement BuildToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.AddToClassList("web-publish-tools__toolbar");

            var refreshButton = new Button(Rebuild) { text = "Refresh" };
            refreshButton.AddToClassList("web-publish-tools__toolbar-button");
            toolbar.Add(refreshButton);

            if (!string.IsNullOrEmpty(activeProfile.DocumentationUrl))
            {
                var docButton = new Button(() => Application.OpenURL(activeProfile.DocumentationUrl))
                {
                    text = "Docs",
                };
                docButton.AddToClassList("web-publish-tools__toolbar-button");
                toolbar.Add(docButton);
            }

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            var profileLabel = new Label("Profile");
            profileLabel.AddToClassList("web-publish-tools__profile-label");
            toolbar.Add(profileLabel);

            var choices = PublishProfileRegistry.All.Select(p => p.DisplayName).ToList();
            var dropdown = new DropdownField(choices, activeProfile.DisplayName);
            dropdown.AddToClassList("web-publish-tools__profile-dropdown");
            dropdown.RegisterValueChangedCallback(evt =>
            {
                var profile = PublishProfileRegistry.All.FirstOrDefault(p => p.DisplayName == evt.newValue);
                if (profile == null || profile.Id == activeProfile.Id) return;

                activeProfile = profile;
                EditorPrefs.SetString(SelectedProfilePrefsKey, profile.Id);
                Rebuild();
            });
            toolbar.Add(dropdown);

            return toolbar;
        }

        private VisualElement BuildAboutSection()
        {
            var section = new VisualElement();
            section.AddToClassList("web-publish-tools__about");

            var title = new Label($"Profile: {activeProfile.DisplayName}");
            title.AddToClassList("web-publish-tools__about-title");
            section.Add(title);

            if (!string.IsNullOrEmpty(activeProfile.Notes))
            {
                var notes = new Label(activeProfile.Notes);
                notes.AddToClassList("web-publish-tools__about-body");
                section.Add(notes);
            }

            return section;
        }

        private static VisualElement BuildDivider()
        {
            var line = new VisualElement();
            line.AddToClassList("web-publish-tools__divider");
            return line;
        }

        private VisualElement BuildCards()
        {
            var container = new VisualElement();
            foreach (var card in cards)
            {
                container.Add(card.Build(activeProfile, Rebuild));
            }
            return container;
        }

        private static StyleSheet LoadStyleSheet()
        {
            var guids = AssetDatabase.FindAssets("WebPublishToolsWindow t:StyleSheet");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("WebPublishToolsWindow.uss"))
                {
                    return AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                }
            }
            return null;
        }
    }
}
