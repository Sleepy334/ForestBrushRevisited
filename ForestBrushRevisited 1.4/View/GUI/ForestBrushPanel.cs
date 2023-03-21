using System;
using System.Collections.Generic;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using UnityEngine;

namespace ForestBrushRevisited.GUI
{
    public class ForestBrushPanel : UIPanel
    {
        internal TitleSection TitleSection;
        internal BrushSelectSection BrushSelectSection;
        internal BrushEditSection BrushEditSection;
        internal BrushOptionsSection BrushOptionsSection;
        internal BrushShapeSelector BrushShapeSelector;
        UIPanel layoutPanelSpace;

        public override void Start()
        {
            base.Start();
            Setup();
            TitleSection = AddUIComponent<TitleSection>();
            BrushSelectSection = AddUIComponent<BrushSelectSection>();
            BrushEditSection = AddUIComponent<BrushEditSection>();
            BrushOptionsSection = AddUIComponent<BrushOptionsSection>();
            BrushShapeSelector = AddUIComponent<BrushShapeSelector>();
            layoutPanelSpace = AddUIComponent<UIPanel>();
            layoutPanelSpace.size = new Vector2(width, 1);

            TitleSection.zOrder = 0;
            BrushSelectSection.zOrder = 1;
            BrushEditSection.zOrder = 2;
            BrushOptionsSection.zOrder = 3;
            BrushShapeSelector.zOrder = 4;
            layoutPanelSpace.zOrder = 5;

            LocaleManager.eventLocaleChanged += ForestBrushPanel_eventLocaleChanged;
            Hide();
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

        private void Setup()
        {
            width = 400f;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoFitChildrenVertically = true;
            autoLayoutPadding = new RectOffset(0, 0, 0, 10);
            absolutePosition = new Vector3(ModSettings.Settings.PanelPosX, ModSettings.Settings.PanelPosY);
            atlas = ResourceLoader.Atlas;
            backgroundSprite = ResourceLoader.MenuPanel;
            isInteractive = true;
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
