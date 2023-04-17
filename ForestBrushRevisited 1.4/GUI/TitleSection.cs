using ColossalFramework.UI;
using ForestBrushRevisited.TranslationFramework;
using UnityEngine;

namespace ForestBrushRevisited.GUI
{
    public class TitleSection : UIPanel
    {
        public bool m_bDragging = false;
        UISprite icon;
        UIDragHandle m_dragHandle;
        UILabel titleLabel;
        UIButton closeButton;

        public override void Start()
        {
            base.Start();

            width = parent.width;
            height = Constants.UITitleBarHeight;
            relativePosition = Vector3.zero;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "ButtonMenuDisabled";

            icon = AddUIComponent<UISprite>();
            icon.atlas = ResourceLoader.ForestBrushAtlas;
            icon.spriteName = ResourceLoader.ForestBrushNormal;
            icon.relativePosition = new Vector3(5f, 5f);
            icon.size = new Vector2(28f, 32f);

            titleLabel = AddUIComponent<UILabel>();
            titleLabel.text = ForestBrushMod.Title;
            titleLabel.textScale = Constants.UITitleTextScale;
            titleLabel.relativePosition = new Vector3((width - titleLabel.width) / 2f, (Constants.UITitleBarHeight - titleLabel.height) / 2f);

            m_dragHandle = AddUIComponent<UIDragHandle>();
            m_dragHandle.size = new Vector2(width, Constants.UITitleBarHeight);
            m_dragHandle.relativePosition = Vector3.zero;
            m_dragHandle.target = parent;
            m_dragHandle.eventMouseUp += DragHandle_eventMouseUp;
            m_dragHandle.eventDragStart += (c, e) => 
            { 
                m_bDragging = true;
            };
            
            closeButton = AddUIComponent<UIButton>();
            closeButton.atlas = ResourceLoader.Atlas;
            closeButton.size = new Vector2(20f, 20f);
            closeButton.relativePosition = new Vector3(width - closeButton.width - Constants.UISpacing, Constants.UISpacing);
            closeButton.normalBgSprite = ResourceLoader.DeleteLineButton;
            closeButton.hoveredBgSprite = ResourceLoader.DeleteLineButtonHovered;
            closeButton.pressedBgSprite = ResourceLoader.DeleteLineButtonPressed;
            closeButton.eventClick += CloseButton_eventClick;
        }

        public override void OnDestroy()
        {
            m_dragHandle.eventMouseUp -= DragHandle_eventMouseUp;
            closeButton.eventClick -= CloseButton_eventClick;
            base.OnDestroy();
        }

        private void DragHandle_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_bDragging = false;
            ForestBrushPanel.Instance.ClampToScreen();
            SavePanelPosition();
        }

        private void CloseButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ForestBrush.Instance.HidePanel();
        }

        public void SavePanelPosition()
        {
            ModSettings.Settings.PanelPosX = parent.absolutePosition.x;
            ModSettings.Settings.PanelPosY = parent.absolutePosition.y;
            ModSettings.Settings.Save();
        }
    }
}
