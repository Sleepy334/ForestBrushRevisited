using ForestBrushRevisited.Settings;
using ICities;

namespace ForestBrushRevisited
{
    public class ForestBrushRevisitedMod : IUserMod
    {
        public static string Version = "1.4.7";

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

        public static string Title => $"{Localization.Get("FOREST-BRUSH-MODNAME")} v{Version}{Edition}{Config}";
        public string Description => Localization.Get("FOREST-BRUSH-MODDESCRIPTION");
        public string Name => $"{Constants.ModName} v{Version}{Edition}{Config}";

        public void OnEnabled()
        {
            ModSettings.Load();
        }

        public void OnDisabled()
        {
            ModSettings.Settings.Save();
        }

        public void OnSettingsUI(UIHelper helper)
        {
            SettingsUI settingsUI = new SettingsUI();
            settingsUI.OnSettingsUI(helper);
        }
    }
}
