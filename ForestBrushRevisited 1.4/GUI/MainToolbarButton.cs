using ColossalFramework.UI;
using System;
using System.Reflection;
using UnityEngine;

namespace ForestBrushRevisited.GUI
{
    public class MainToolbarButton
    {
        private ToggleButtonComponents? m_toggleButtonComponents = null;
        private static readonly string kEmptyContainer = "EmptyContainer";
        private static readonly string kMainToolbarSeparatorTemplate = "MainToolbarSeparator";
        private static readonly string kMainToolbarButtonTemplate = "MainToolbarButtonTemplate";
        private static readonly string kToggleButton = "ForestBrushRevisited";

        public UIButton ToggleButton => m_toggleButtonComponents.ToggleButton;

        public bool IsButtonAdded()
        {
            return m_toggleButtonComponents != null;
        }

        public void AddToolButton()
        {
            UITabstrip? tabstrip = GetMainToolStrip();
            if (tabstrip != null)
            {
                m_toggleButtonComponents = CreateToggleButtonComponents(tabstrip);

                if (m_toggleButtonComponents != null)
                {
                    tabstrip.eventSelectedIndexChanged += OnSelectedIndexChanged;
                }
                else
                {
                    Debug.LogError("AddToolButton - Failed to create toolbar button.");
                }
            }
        }

        

        public void Enable()
        {
            if (m_toggleButtonComponents != null)
            {
                UITabstrip toolStrip = GetMainToolStrip();
                if (toolStrip != null)
                {
                    toolStrip.selectedIndex = FindButtonIndex();
                    m_toggleButtonComponents.ToggleButton.Focus();
                }
            }
        }

        public void Disable()
        {
            if (m_toggleButtonComponents != null)
            {
                UITabstrip toolStrip = GetMainToolStrip();
                if (toolStrip != null)
                {
                    toolStrip.closeButton.SimulateClick();
                    m_toggleButtonComponents.ToggleButton.Unfocus();
                }
            }
        }

        public void ShowButton()
        {
            if (m_toggleButtonComponents == null)
            {
                AddToolButton();
            }
            else
            {
                m_toggleButtonComponents.ToggleButton.isVisible = true;
            }
        }

        public void HideButton()
        {
            if (m_toggleButtonComponents != null)
            {
                m_toggleButtonComponents.ToggleButton.isVisible = false;

                // Seem to need to destroy tab as well
                Destroy();
            }
        }

        private UITabstrip? GetMainToolStrip()
        {
            return ToolsModifierControl.mainToolbar.component as UITabstrip;
            //return UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip");
        }

        public void Destroy()
        {
            // Remove event handler
            if (m_toggleButtonComponents != null)
            {
                GetMainToolStrip().eventSelectedIndexChanged -= OnSelectedIndexChanged;
            }
            
            DestroyToggleButtonComponents(m_toggleButtonComponents);
            m_toggleButtonComponents = null;
        }

        public void OnSelectedIndexChanged(UIComponent oComponent, int iSelectedIndex)
        {
            UITabstrip toolStrip = GetMainToolStrip();
            if (toolStrip != null && toolStrip.isVisible)
            {
                if (IsToolbarButton(iSelectedIndex))
                {
                    ForestBrush.Instance.ShowPanel();
                }
                else
                {
                    ForestBrush.Instance.HidePanel();
                }
            }
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

        private void DestroyToggleButtonComponents(ToggleButtonComponents toggleButtonComponents)
        {
            DestroySeparatorComponents(toggleButtonComponents.PostSeparatorComponents);

            DecrementObjectIndex();

            UnityEngine.Object.Destroy(toggleButtonComponents.ToggleButton.gameObject);
            UnityEngine.Object.Destroy(toggleButtonComponents.MainToolbarButtonTemplate.gameObject);
            UnityEngine.Object.Destroy(toggleButtonComponents.TabStripPage.gameObject);

            DestroySeparatorComponents(toggleButtonComponents.PreSeparatorComponents);
        }

        private void DestroySeparatorComponents(SeparatorComponents separatorComponents)
        {
            DecrementObjectIndex();
            UnityEngine.Object.Destroy(separatorComponents.SeparatorTab.gameObject);
            UnityEngine.Object.Destroy(separatorComponents.EmptyContainer.gameObject);
            UnityEngine.Object.Destroy(separatorComponents.MainToolbarSeparatorTemplate.gameObject);
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

        private bool IsToolbarButton(int iIndex)
        {
            if (iIndex != -1)
            {
                UITabstrip? toolStrip = GetMainToolStrip();
                if (toolStrip != null)
                {
                    return toolStrip.tabs[iIndex].name.Contains(kToggleButton);
                }
            }

            return false;
        }

        private int FindButtonIndex()
        {
            UITabstrip? toolStrip = GetMainToolStrip();
            if (toolStrip != null)
            {
                // Start from the end as our button will be added close to the end of the tabs
                for (int i = toolStrip.tabs.Count - 1; i >= 0; --i)
                {
                    UIComponent tab = toolStrip.tabs[i];
                    if (tab.name.Contains(kToggleButton))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
    }
}
