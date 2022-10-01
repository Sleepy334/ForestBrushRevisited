﻿using ColossalFramework.UI;
using ForestBrush.Resources;
using ForestBrush.TranslationFramework;
using UnityEngine;

namespace ForestBrush.GUI
{
    public class TitleSection : UIPanel
    {
        UISprite icon;
        UIDragHandle dragHandle;
        UILabel titleLabel;
        UIButton closeButton;

        public override void Start()
        {
            base.Start();

            width = parent.width;
            height = Constants.UITitleBarHeight;

            icon = AddUIComponent<UISprite>();
            icon.atlas = ResourceLoader.ForestBrushAtlas;
            icon.spriteName = ResourceLoader.ForestBrushNormal;
            icon.relativePosition = new Vector3(5f, 5f);
            icon.size = new Vector2(28f, 32f);

            titleLabel = AddUIComponent<UILabel>();
            titleLabel.text = Translation.Instance.GetTranslation("FOREST-BRUSH-MODNAME") + " " + UserMod.Version;
            titleLabel.textScale = Constants.UITitleTextScale;
            titleLabel.relativePosition = new Vector3((width - titleLabel.width) / 2f, (Constants.UITitleBarHeight - titleLabel.height) / 2f);

            dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.size = new Vector2(width, Constants.UITitleBarHeight);
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = parent;
            dragHandle.eventMouseUp += DragHandle_eventMouseUp;

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
            dragHandle.eventMouseUp -= DragHandle_eventMouseUp;
            closeButton.eventClick -= CloseButton_eventClick;
            base.OnDestroy();
        }

        private void DragHandle_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam)
        {
            ForestBrush.Instance.ForestBrushPanel.ClampToScreen();
            SavePanelPosition();
        }

        private void CloseButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            ForestBrush.Instance.ToggleButton.SimulateClick();
        }

        public void SavePanelPosition()
        {
            UserMod.Settings.PanelPosX = parent.absolutePosition.x;
            UserMod.Settings.PanelPosY = parent.absolutePosition.y;
            UserMod.SaveSettings();
        }
    }
}
