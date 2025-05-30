using ForestBrushRevisited.GUI;
using UnifiedUI.Helpers;
using UnityEngine;

namespace ForestBrushRevisited
{
    public class UnifiedUIButton
    {
        private static UUICustomButton? s_button = null;
        private static UnifiedUIButton? s_instance = null;

        // ----------------------------------------------------------------------------------------
        public static UnifiedUIButton Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new UnifiedUIButton();
                }

                return s_instance;
            }
        }

        public static bool Exists
        {
            get
            {
                return s_instance != null;
            }
        }

        public bool HasBeenAdded()
        {
            return s_button != null;
        }

        public static void Add()
        {
            if (s_button is null && ModSettings.Settings.AddUnifiedUIButton && DependencyUtils.IsUnifiedUIRunning())
            {
                Texture2D? icon = TextureResources.LoadDllResource("ForestBrushNormal.png", 32, 32);
                if (icon is null)
                {
                    Debug.Log("Failed to load icon from resources");
                    return;
                }

                s_button = UUIHelpers.RegisterCustomButton(ForestBrushRevisitedMod.Title, null, ForestBrushRevisitedMod.Title, icon, OnToggle, null, null);
            }
        }

        public static void Remove()
        {
            if (s_button is not null)
            {
                UUIHelpers.Destroy(s_button.Button);
                s_button = null;
            }
        }

        public static void OnToggle(bool bToggle)
        {
            ForestBrushPanel.Instance.TogglePanel();
        }

        public static void OnUnifiedToolbarButtonChanged()
        {
            if (ModSettings.Settings.AddUnifiedUIButton)
            {
                Add();
            }
            else
            {
                Remove();
            }
        }

        public void Enable()
        {
            if (s_button != null && !s_button.Button.enabled)
            {
                s_button.Button.SimulateClick();
            }
        }

        public void Disable()
        {
            if (s_button != null && s_button.Button.enabled)
            {
                s_button.Button.SimulateClick();
            }
        }
    }
}