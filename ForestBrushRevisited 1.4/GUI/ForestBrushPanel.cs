using System;
using System.Collections.Generic;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Epic.OnlineServices.Lobby;
using UnityEngine;

namespace ForestBrushRevisited.GUI
{
    public class ForestBrushPanel : UIPanel
    {
        public static ForestBrushPanel? Instance = null;

        private UIPanel? m_mainPanel = null;
        internal TitleSection TitleSection;
        internal BrushSelectSection BrushSelectSection;
        internal BrushEditSection BrushEditSection;
        internal BrushOptionsSection BrushOptionsSection;
        internal BrushShapeSelector BrushShapeSelector;
        UIPanel layoutPanelSpace;

        public static void Init()
        {
            if (Instance is null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(ForestBrushPanel)) as ForestBrushPanel;
                if (Instance is null)
                {
                    Prompt.Info(ForestBrushRevisitedMod.Title, "Error creating Main Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();

            name = "ForestBrushPanel";
            width = 400f;
            height = 850f;
            canFocus = true;
            isInteractive = true;
            clipChildren = true;
            backgroundSprite = "SubcategoriesPanel";
            absolutePosition = new Vector3(ModSettings.Settings.PanelPosX, ModSettings.Settings.PanelPosY);
            opacity = 0.95f;
            playAudioEvents = true;
            eventVisibilityChanged += (c, e) =>
            {
                // TODO Update selection tool and main toolbar button as well
            };

            // Add title
            TitleSection = AddUIComponent<TitleSection>();

            m_mainPanel = AddUIComponent<UIPanel>();
            if (m_mainPanel != null )
            {
                Debug.Log($"TitleHeight: {TitleSection.height}");
                m_mainPanel.width = width;
                m_mainPanel.height = height - Constants.UITitleBarHeight;
                m_mainPanel.relativePosition = new Vector3(0, Constants.UITitleBarHeight);
                m_mainPanel.padding = new RectOffset(0, 0, 10, 0);
                //m_mainPanel.backgroundSprite = "InfoviewPanel";
                //m_mainPanel.color = Color.red;
                m_mainPanel.clipChildren = true;
                m_mainPanel.autoLayout = true;
                m_mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
                m_mainPanel.autoFitChildrenVertically = true;
                m_mainPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 10);
                m_mainPanel.atlas = ResourceLoader.Atlas;
                m_mainPanel.isInteractive = true;

                BrushSelectSection = m_mainPanel.AddUIComponent<BrushSelectSection>();
                BrushEditSection = m_mainPanel.AddUIComponent<BrushEditSection>();
                BrushOptionsSection = m_mainPanel.AddUIComponent<BrushOptionsSection>();
                BrushShapeSelector = m_mainPanel.AddUIComponent<BrushShapeSelector>();
                layoutPanelSpace = m_mainPanel.AddUIComponent<UIPanel>();
                layoutPanelSpace.size = new Vector2(width, 1);

                TitleSection.zOrder = 0;
                BrushSelectSection.zOrder = 1;
                BrushEditSection.zOrder = 2;
                BrushOptionsSection.zOrder = 3;
                BrushShapeSelector.zOrder = 4;
                layoutPanelSpace.zOrder = 5;

                m_mainPanel.eventSizeChanged += (c, e) =>
                {
                    height = m_mainPanel.height + Constants.UITitleBarHeight;
                };
            }

            LocaleManager.eventLocaleChanged += ForestBrushPanel_eventLocaleChanged;
            Hide();
        }

        public void ShowPanel()
        {
            Show();
            PlayClickSound(this);
        }

        public void HidePanel()
        {
            Hide();
            PlayClickSound(this);
        }

        public void Destroy()
        {
            Destroy(gameObject);
            Instance = null;
        }

        public bool IsDragging()
        {
            return TitleSection.m_bDragging;
        }

        public void SetTreeList(List<TreeInfo> treeList)
        {
            // Sort tree list
            treeList.Sort((t1, t2) => t1.CompareTo(t2, ModSettings.Settings.Sorting, ModSettings.Settings.SortingOrder));

            // Update list
            GUI.UIFastList list = BrushEditSection.TreesList;
            list.rowsData.m_buffer = treeList.ToArray();
            list.rowsData.m_size = treeList.Count;
            list.DisplayAt(0f);
        }

        public override void OnDestroy()
        {
            LocaleManager.eventLocaleChanged -= ForestBrushPanel_eventLocaleChanged;
            base.OnDestroy();
        }
        private void ForestBrushPanel_eventLocaleChanged()
        {
            BrushSelectSection.LocaleChanged();
            BrushEditSection.LocaleChanged();
            BrushOptionsSection.LocaleChanged();
        }

        internal void LoadBrush(Brush brush)
        {
            BrushSelectSection?.LoadBrush(brush);
            BrushEditSection?.LoadBrush(brush);
            BrushOptionsSection?.LoadBrush(brush);
            BrushShapeSelector?.LoadBrush(brush);
        }

        internal bool ToggleBrushEdit()
        {
            BrushEditSection.isVisible = !BrushEditSection.isVisible;
            return BrushEditSection.isVisible;
        }

        internal bool ToggleBrushOptions()
        {
            BrushOptionsSection.isVisible = !BrushOptionsSection.isVisible;
            return BrushOptionsSection.isVisible;
        }

        internal bool ToggleBrushShapes()
        {
            BrushShapeSelector.isVisible = !BrushShapeSelector.isVisible;
            return BrushShapeSelector.isVisible;
        }
    }
}
