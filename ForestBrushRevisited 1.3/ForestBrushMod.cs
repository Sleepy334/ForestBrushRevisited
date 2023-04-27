using ForestBrushRevisited.TranslationFramework;
using ForestBrushRevisited.View;
using ICities;

namespace ForestBrushRevisited
{
    public class ForestBrushMod : IUserMod
    {
        public static string Version = "v1.3.16";
        public static string Title => Translation.Instance.GetTranslation("FOREST-BRUSH-MODNAME") + " " + Version;
        public string Description => Translation.Instance.GetTranslation("FOREST-BRUSH-MODDESCRIPTION");
        public string Name => Constants.ModName;

        public void OnEnabled()
        {
            ModSettings.LoadSettings();
        }

        public void OnDisabled()
        {
            ModSettings.ReleaseSettings();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            SettingsUI settingsUI = new SettingsUI();
            settingsUI.OnSettingsUI(helper);
        }
    }
}
