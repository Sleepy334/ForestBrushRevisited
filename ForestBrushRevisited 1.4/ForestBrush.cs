using System;
using System.Collections.Generic;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Packaging;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using ForestBrushRevisited.GUI;
using ForestBrushRevisited.TranslationFramework;
using UnityEngine;
using System.Linq;
using ForestBrushRevisited.SelectionTool;

namespace ForestBrushRevisited
{
    public class ForestBrush : MonoBehaviour
    {
        private static ForestBrush? s_Instance = null;

        public static ForestBrush Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = ForestBrushLoader.GameObject().AddComponent<ForestBrush>();
                }

                return s_Instance;
            }
        }

        public ForestBrushContainer? BrushContainer = new ForestBrushContainer();
        public MainToolbarButton? m_mainToolbarButton = null;

        private bool m_Initialized;
        internal bool Active => IsCurrentTreeContainer && ForestBrushPanel.Instance.isVisible;

        private static readonly string kEmptyContainer = "EmptyContainer";

        private static readonly string kMainToolbarSeparatorTemplate = "MainToolbarSeparator";

        private static readonly string kMainToolbarButtonTemplate = "MainToolbarButtonTemplate";

        public static readonly string kToggleButton = "ForestBrushRevisited";


        internal bool IsCurrentTreeContainer
        {
            get {
                return BrushContainer.Container != null &&
                    ToolsModifierControl.toolController.CurrentTool is TreeTool &&
                    ((TreeTool)ToolsModifierControl.toolController?.CurrentTool)?.m_prefab == BrushContainer.Container;
            }
        }

        public bool ShowBrushTrees
        {
            get 
            { 
                return ModSettings.Settings.ShowBrushTrees; 
            }

            set 
            {
                ModSettings.Settings.ShowBrushTrees = value;
                ModSettings.Settings.Save();

                // When we toggle brush tree mode we need to clear filters
                if (ForestBrushPanel.Instance != null)
                {
                    ForestBrushPanel.Instance.BrushEditSection.SearchTextField.text = "";
                    UpdateTreeList();
                }
            }
        }

        public void UpdateTreeList()
        {
            if (ForestBrushPanel.Instance != null)
            {
                ForestBrushPanel.Instance.BrushEditSection.SetupFastlist();
            }
        }

        public List<TreeInfo> GetAvailableTreesSorted()
        {
            if (ShowBrushTrees)
            {
                List<TreeInfo> allTrees = Trees.Values.ToList();

                List<TreeInfo> brushTrees = new List<TreeInfo>();
                foreach (TreeInfo tree in allTrees)
                {
                    if (ForestBrush.Instance.BrushContainer.Container.m_variations.Any(v => v.m_finalTree == tree))
                    {
                        brushTrees.Add(tree);
                    }
                }
                brushTrees.Sort((t1, t2) => t1.CompareTo(t2, ModSettings.Settings.Sorting, ModSettings.Settings.SortingOrder));
                return brushTrees;
            }
            else if (Trees != null)
            {
                List<TreeInfo> trees = Trees.Values.ToList();
                trees.Sort((t1, t2) => t1.CompareTo(t2, ModSettings.Settings.Sorting, ModSettings.Settings.SortingOrder));
                return trees;
            }
            else
            {
                return new List<TreeInfo>();
            }
        }

        public bool GetTreeInfo(string sTreeName, out TreeInfo? treeInfo)
        {
            if (Trees == null)
            {
                treeInfo = null;
                return false;
            }

            return Trees.TryGetValue(sTreeName, out treeInfo);
        }

        private Dictionary<string, TreeInfo> Trees { get; set; }

        public Dictionary<string, TreeMeshData> TreesMeshData { get; private set; }

        public Dictionary<string, string> TreeAuthors { get; private set; }

        public ForestTool Tool { get; private set; }

        private ToolBase m_lastTool;

        internal void Initialize()
        {
            LoadTrees();
            LoadTreeAuthors();

            if (ModSettings.Settings.MainToolbarButton || !DependencyUtils.IsUnifiedUIRunning())
            {
                m_mainToolbarButton = new MainToolbarButton();
                m_mainToolbarButton.AddToolButton();
            }

            ForestBrushPanel.Init();

            if (BrushContainer != null)
            {
                if (ModSettings.Settings.SelectedBrush != null)
                {
                    BrushContainer.UpdateTool(ModSettings.Settings.SelectedBrush.Name);
                }
                else
                {
                    Debug.Log("UserMod.Settings.SelectedBrush is null");
                }
            }
            else
            {
                Debug.Log("BrushContainer is null");
            }

            SetTutorialLocale();
            
            LocaleManager.eventLocaleChanged += SetTutorialLocale;
            Tool = ForestTool.AddSelectionTool();
            m_Initialized = true;
        }

        internal void CleanUp()
        {
            m_Initialized = false;

            LocaleManager.eventLocaleChanged -= SetTutorialLocale;
            ForestBrushPanel.Instance.Destroy();

            if (m_mainToolbarButton != null)
            {
                m_mainToolbarButton.Destroy();
            }

            TreesMeshData = null;
            Trees = null;

            // Clear instance
            s_Instance = null;
        }

        private ToggleButtonComponents CreateToggleButtonComponents(UITabstrip tabstrip)
        {
            SeparatorComponents preSeparatorComponents = CreateSeparatorComponents(tabstrip);

            GameObject tabStripPage = UITemplateManager.GetAsGameObject(kEmptyContainer);
            GameObject mainToolbarButtonTemplate = UITemplateManager.GetAsGameObject(kMainToolbarButtonTemplate);

            UIButton? toggleButton = tabstrip.AddTab(kToggleButton, mainToolbarButtonTemplate, tabStripPage, new Type[0]) as UIButton;
            if (toggleButton != null)
            {
                toggleButton.atlas = ResourceLoader.ForestBrushAtlas;
                toggleButton.tooltip = ForestBrushMod.Title;
                toggleButton.normalFgSprite = "ForestBrushNormal";
                toggleButton.disabledFgSprite = "ForestBrushDisabled";
                toggleButton.focusedFgSprite = "ForestBrushFocused";
                toggleButton.hoveredFgSprite = "ForestBrushHovered";
                toggleButton.pressedFgSprite = "ForestBrushPressed";

                toggleButton.normalBgSprite = "ToolbarIconGroup6Normal";
                toggleButton.disabledBgSprite = "ToolbarIconGroup6Disabled";
                toggleButton.focusedBgSprite = "ToolbarIconGroup6Focused";
                toggleButton.hoveredBgSprite = "ToolbarIconGroup6Hovered";
                toggleButton.pressedBgSprite = "ToolbarIconGroup6Pressed";
                toggleButton.parent.height = 1f;

                IncrementObjectIndex();
            }
            else
            {
                Debug.Log("toggleButton is null.");
            }

            SeparatorComponents postSeparatorComponents = CreateSeparatorComponents(tabstrip);

            return new ToggleButtonComponents(preSeparatorComponents, tabStripPage, mainToolbarButtonTemplate, toggleButton, postSeparatorComponents);
        }

        private void DestroyToggleButtonComponents(ToggleButtonComponents toggleButtonComponents)
        {
            DestroySeparatorComponents(toggleButtonComponents.PostSeparatorComponents);

            DecrementObjectIndex();

            Destroy(toggleButtonComponents.ToggleButton.gameObject);

            Destroy(toggleButtonComponents.MainToolbarButtonTemplate.gameObject);
            Destroy(toggleButtonComponents.TabStripPage.gameObject);

            DestroySeparatorComponents(toggleButtonComponents.PreSeparatorComponents);
        }

        private void SetTutorialLocale()
        {
            Locale locale = (Locale)typeof(LocaleManager).GetField("m_Locale", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(LocaleManager.instance);

            Locale.Key tutorialAdviserTitleKey = new Locale.Key
            {
                m_Identifier = "TUTORIAL_ADVISER_TITLE",
                m_Key = kToggleButton,
            };
            if (!locale.Exists(tutorialAdviserTitleKey))
            {
                locale.AddLocalizedString(tutorialAdviserTitleKey, Translation.Instance.GetTranslation("FOREST-BRUSH-MODNAME"));
            }

            Locale.Key tutorialAdviserKey = new Locale.Key
            {
                m_Identifier = "TUTORIAL_ADVISER",
                m_Key = kToggleButton
            };
            if (!locale.Exists(tutorialAdviserKey))
            {
                locale.AddLocalizedString(tutorialAdviserKey, "");
            }
        }

        private void IncrementObjectIndex()
        {
            FieldInfo m_ObjectIndex = typeof(MainToolbar).GetField("m_ObjectIndex", BindingFlags.Instance | BindingFlags.NonPublic);
            m_ObjectIndex.SetValue(ToolsModifierControl.mainToolbar, (int)m_ObjectIndex.GetValue(ToolsModifierControl.mainToolbar) + 1);
        }

        private void DecrementObjectIndex()
        {
            FieldInfo m_ObjectIndex = typeof(MainToolbar).GetField("m_ObjectIndex", BindingFlags.Instance | BindingFlags.NonPublic);
            m_ObjectIndex.SetValue(ToolsModifierControl.mainToolbar, (int)m_ObjectIndex.GetValue(ToolsModifierControl.mainToolbar) - 1);
        }

        protected SeparatorComponents CreateSeparatorComponents(UITabstrip strip)
        {
            GameObject mainToolbarSeparatorTemplate = UITemplateManager.GetAsGameObject(kMainToolbarSeparatorTemplate);
            GameObject emptyContainer = UITemplateManager.GetAsGameObject(kEmptyContainer);
            UIComponent separatorTab = strip.AddTab("Separator", mainToolbarSeparatorTemplate, emptyContainer, new Type[0]);
            separatorTab.width *= 0.5f;
            separatorTab.isEnabled = false;
            IncrementObjectIndex();
            return new SeparatorComponents(mainToolbarSeparatorTemplate, emptyContainer, separatorTab);
        }

        protected void DestroySeparatorComponents(SeparatorComponents separatorComponents)
        {
            DecrementObjectIndex();
            Destroy(separatorComponents.SeparatorTab.gameObject);
            Destroy(separatorComponents.EmptyContainer.gameObject);
            Destroy(separatorComponents.MainToolbarSeparatorTemplate.gameObject);
        }

        public void ShowPanel()
        {
            // Select tool
            m_lastTool = ToolsModifierControl.toolController.CurrentTool;
            ToolsModifierControl.SetTool<ForestTool>();

            // Toggle main toolbar button
            if (m_mainToolbarButton != null)
            {
                m_mainToolbarButton.Enable();
            }

            // Show panel
            ForestBrushPanel.Instance.ShowPanel();
        }

        public void HidePanel()
        {
            // Hide panel
            if (ForestBrushPanel.Instance.isVisible)
            {
                // Toggle main toolbar button
                if (m_mainToolbarButton != null)
                {
                    m_mainToolbarButton.Disable();
                }

                // Just select default tool.
                ToolsModifierControl.SetTool<DefaultTool>();

                // Show panel
                ForestBrushPanel.Instance.HidePanel();
            }
        }

        public void TogglePanel()
        {
            if (ForestBrushPanel.Instance.isVisible)
            {
                HidePanel();
            }
            else
            {
                ShowPanel();
            }
        }

        public void UpdateToolbarButton()
        {
            if (m_Initialized)
            {
                if (ModSettings.Settings.MainToolbarButton || !DependencyUtils.IsUnifiedUIRunning())
                {
                    if (m_mainToolbarButton == null)
                    {
                        m_mainToolbarButton = new MainToolbarButton();
                    }

                    m_mainToolbarButton.ShowButton();
                }
                else
                {
                    m_mainToolbarButton.HideButton();
                    m_mainToolbarButton = null;
                }
            }
        }

        internal void LoadTrees()
        {
            Trees = new Dictionary<string, TreeInfo>();
            TreesMeshData = new Dictionary<string, TreeMeshData>();
            var treeCount = PrefabCollection<TreeInfo>.LoadedCount();
            for (uint i = 0; i < treeCount; i++)
            {
                var tree = PrefabCollection<TreeInfo>.GetLoaded(i);
                if (tree == null || tree == BrushContainer.Container || (ModSettings.Settings != null && ModSettings.Settings.IgnoreVanillaTrees && !tree.m_isCustomContent)) continue;
                if (tree.m_availableIn != ItemClass.Availability.All)
                {
                    tree.m_availableIn = ItemClass.Availability.All;
                }

                if (tree.m_Atlas == null || string.IsNullOrEmpty(tree.m_Thumbnail) || tree.m_Thumbnail.IsNullOrWhiteSpace()) ImageUtils.CreateThumbnailAtlas(GetName(tree), tree);

                Trees.Add(tree.name, tree);
                TreesMeshData.Add(tree.name, new TreeMeshData(tree));
            }
        }

        private void LoadTreeAuthors()
        {
            TreeAuthors = new Dictionary<string, string>();
            foreach (Package.Asset current in PackageManager.FilterAssets(new Package.AssetType[] { UserAssetType.CustomAssetMetaData }))
            {
                PublishedFileId id = current.package.GetPublishedFileID();
                string publishedFileId = string.Concat(id.AsUInt64);
                if (!TreeAuthors.ContainsKey(publishedFileId) && !current.package.packageAuthor.IsNullOrWhiteSpace())
                {
                    if (ulong.TryParse(current.package.packageAuthor.Substring("steamid:".Length), out ulong authorID))
                    {
                        string author = new Friend(new UserID(authorID)).personaName;
                        TreeAuthors.Add(publishedFileId, author);
                    }
                }
            }
        }

        public static string GetName(PrefabInfo prefab)
        {
            string name = prefab.name;
            if (name.EndsWith("_Data"))
            {
                name = name.Substring(0, name.LastIndexOf("_Data"));
            }
            return name;
        }

        public void SetBrush(string id)
        {
            Tool.SetBrush(id);
        }

        public Dictionary<string, Texture2D> GetBrushBitmaps()
        {
            return Tool.GetBrushes();
        }

        public void OnGUI()
        {
            try
            {
                if (m_Initialized && !UIView.HasModalInput() &&
                    (!UIView.HasInputFocus() || (UIView.activeComponent != null)))
                {
                    Event e = Event.current;

                    if (ModSettings.Settings.ToggleTool.IsPressed(e))
                    {
                        TogglePanel();
                    }
                }
                
            }
            catch (Exception)
            {
                Debug.LogWarning("OnGUI failed.");
            }
        }
    }
}
