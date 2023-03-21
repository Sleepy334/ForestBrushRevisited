using ForestBrushRevisited.TranslationFramework;
using ForestBrushRevisited.View;
using ICities;

namespace ForestBrushRevisited
{
    public class ForestBrushMod : IUserMod
    {
        public static string Version = "v1.4";

#if TEST_RELEASE || TEST_DEBUG
        private static string Edition => " TEST";
#else
        private static string Edition => "";
#endif

#if DEBUG
        private static string Config => " [DEBUG]";
#else
        private static string Config => "";
#endif

        public static string Title => $"{Translation.Instance.GetTranslation("FOREST-BRUSH-MODNAME")} {Version}{Edition}{Config}";
        public string Description => Translation.Instance.GetTranslation("FOREST-BRUSH-MODDESCRIPTION");
        public string Name => $"{Constants.ModName} {Version}{Edition}{Config}";

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
