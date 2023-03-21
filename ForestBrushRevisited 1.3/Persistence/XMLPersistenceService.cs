using ColossalFramework;
using ColossalFramework.IO;
using System;
using System.IO;
using System.Xml.Serialization;

namespace ForestBrushRevisited.Persistence
{
    public class XmlPersistenceService
    {
        private readonly string configurationPath = Path.Combine(DataLocation.localApplicationData, "ForestBrush.xml");
        private readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlSettings));

        public void Save(ModSettings settings)
        {
            var xmlSettings = new XmlSettings()
            {
                PanelPosX = settings.PanelPosX,
                PanelPosY = settings.PanelPosY,
                BrushShapesOpen = settings.BrushShapesOpen,
                BrushEditOpen = settings.BrushEditOpen,
                BrushOptionsOpen = settings.BrushOptionsOpen,
                ShowTreeMeshData = settings.ShowTreeMeshData,
                Sorting = settings.Sorting,
                SortingOrder = settings.SortingOrder,
                FilterStyle = settings.FilterStyle,
                Brushes = settings.Brushes,
                SelectedBrush = settings.SelectedBrush.Name,
                ToggleTool = GetXmlInputKey(settings.ToggleTool),
                KeepTreesInNewBrush = settings.KeepTreesInNewBrush,
                IgnoreVanillaTrees = settings.IgnoreVanillaTrees,
                ShowInfoTooltip = settings.ShowInfoTooltip,
                PlayEffect = settings.PlayEffect,
                ChargeMoney = settings.ChargeMoney,
                ShowBrushTrees = settings.ShowBrushTrees
            };

            try
            {
                using (var sw = new StreamWriter(configurationPath))
                {
                    xmlSerializer.Serialize(sw, xmlSettings);
                }
            }
            catch (Exception ex)
            {
                // Flag the error to display to user when level is loaded
                string sErrorMessage = $"Error saving settings file:\n{configurationPath}\n{Debug.ToString(ex)}";
                Debug.Log(sErrorMessage);
                Prompt.Info(Constants.ModName, sErrorMessage);
            }
        }

        public ModSettings Load()
        {
            if (File.Exists(configurationPath))
            {
                XmlSettings xmlSettings;

                try
                {
                    using (var sr = new StreamReader(configurationPath))
                    {
                        xmlSettings = (XmlSettings)xmlSerializer.Deserialize(sr);
                    }

                    return new ModSettings(
                        xmlSettings.PanelPosX,
                        xmlSettings.PanelPosY,
                        xmlSettings.BrushShapesOpen,
                        xmlSettings.BrushEditOpen,
                        xmlSettings.BrushOptionsOpen,
                        xmlSettings.ShowTreeMeshData,
                        xmlSettings.Sorting,
                        xmlSettings.SortingOrder,
                        xmlSettings.FilterStyle,
                        xmlSettings.Brushes,
                        xmlSettings.SelectedBrush,
                        GetSavedInputKey(xmlSettings.ToggleTool),
                        xmlSettings.KeepTreesInNewBrush,
                        xmlSettings.IgnoreVanillaTrees,
                        xmlSettings.ShowInfoTooltip,
                        xmlSettings.PlayEffect,
                        xmlSettings.ChargeMoney,
                        xmlSettings.ShowBrushTrees
                    );
                }
                catch (Exception ex)
                {
                    // make a backup of the file so the user doesn't lose all their brushes
                    try
                    {
                        File.Copy(configurationPath, configurationPath + ".bad");
                    }
                    catch (Exception)
                    { 
                    }

                    // Display explanatory message to user
                    string sErrorMessage = "Error reading settings file:\n";
                    sErrorMessage += configurationPath + "\n\n";
                    sErrorMessage += $"Your settings file has been renamed to:\n{configurationPath}.bad\n\n";
                    sErrorMessage += "A new settings file will be created.";
                    Prompt.Info(Constants.ModName, sErrorMessage);

                    // Log the error
                    Debug.Log(Debug.ToString(ex));
                }                
            }

            return ModSettings.Default();
        }

        private XmlInputKey GetXmlInputKey(SavedInputKey savedInputKey)
        {
            return new XmlInputKey()
            {
                Name = savedInputKey.name,
                Key = savedInputKey.Key,
                Control = savedInputKey.Control,
                Alt = savedInputKey.Alt,
                Shift = savedInputKey.Shift,
            };
        }

        private SavedInputKey GetSavedInputKey(XmlInputKey xmlInputKey)
        {
            return new SavedInputKey(xmlInputKey.Name, Constants.ModName, xmlInputKey.Key, xmlInputKey.Control, xmlInputKey.Shift, xmlInputKey.Alt, true);
        }
    }
}
