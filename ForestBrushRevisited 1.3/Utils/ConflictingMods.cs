using ColossalFramework.Plugins;
using System.Reflection;

namespace ForestBrushRevisited
{
    public class ConflictingMods
    {
        public static bool ConflictingModsFound()
        {
            string sConflictingMods = "";
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin != null && plugin.isEnabled)
                {
                    foreach (Assembly assembly in plugin.GetAssemblies())
                    {
                        switch (assembly.GetName().Name)
                        {
                            case "ForestBrush":
                                {
                                    sConflictingMods += "Forest Brush\r\n";
                                    break;
                                }
                            default:
                                {
                                    //Debug.Log("Assembly: " + assembly.GetName().Name);
                                    break;
                                }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(sConflictingMods))
            {
                return false;
            }
            else
            {
                string sMessage = "Conflicting Mods Found:\r\n";
                sMessage += "\r\n";
                sMessage += sConflictingMods;
                sMessage += "\r\n";
                sMessage += "Mod disabled until conflicts resolved, please remove these mods.";
                Prompt.Info(Constants.ModName, sMessage);
                return true;
            }
        }
    }
}