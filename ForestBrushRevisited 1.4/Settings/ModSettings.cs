using ColossalFramework.IO;
using ForestBrushRevisited.Persistence;
using ForestBrushRevisited.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Xml.Xsl;
using UnityEngine;

namespace ForestBrushRevisited
{
    [XmlRoot("ModSettings")] // Do not change this
    public class ModSettings
    {
        public static ModSettings Settings { get; private set; }

        private static readonly string SettingsFilePath = Path.Combine(DataLocation.localApplicationData, "ForestBrushRevisted.xml");
        private static readonly string OldModSettingsFilePath = Path.Combine(DataLocation.localApplicationData, "ForestBrush.xml");

        public string ModVersion { get; set; }

        public string PreferredLanguage { get; set; } = "System Default";

        public float PanelPosX { get; set; } = 200f;

        public float PanelPosY { get; set; } = 100f;

        public bool BrushShapesOpen { get; set; } = true;

        public bool BrushEditOpen { get; set; } = true;

        public bool BrushOptionsOpen { get; set; } = true;

        public bool ShowTreeMeshData { get; set; } = true;

        public TreeSorting Sorting { get; set; } = TreeSorting.Name;

        public SortingOrder SortingOrder { get; set; } = SortingOrder.Ascending;

        public FilterStyle FilterStyle { get; set; } = FilterStyle.AND;

        public Brush SelectedBrush { get; private set; }

        public bool KeepTreesInNewBrush { get; internal set; } = false;

        public bool IgnoreVanillaTrees { get; internal set; } = false;

        public bool ShowInfoTooltip { get; internal set; } = true;

        public bool PlayEffect { get; internal set; } = true;

        public bool ChargeMoney { get; internal set; } = true;

        public List<Brush> Brushes { get; set; } = new List<Brush>();

        public bool ShowBrushTrees { get; set; } = false;

        public bool MainToolbarButton { get; set; } = true;

        [XmlIgnore]
        public UnsavedKeyMapping ToggleTool = new UnsavedKeyMapping(
            "toggleTool",
            key: KeyCode.B, bCtrl: true, bShift: false, bAlt: true);

        [XmlElement("ToggleTool")]
        public Settings.XmlInputKey XMLTransferIssueHotKey
        {
            get => ToggleTool.XmlKey;
            set
            {
                ToggleTool.Key = value.Key;
                ToggleTool.Control = value.Control;
                ToggleTool.Shift = value.Shift;
                ToggleTool.Alt = value.Alt;
            }
        }

        public ModSettings()
        {
            Brushes = new List<Brush>();
        }

        public ModSettings(XmlSettings oldSettings)
        {
            PanelPosX = oldSettings.PanelPosX;
            PanelPosY = oldSettings.PanelPosY;
            BrushShapesOpen = oldSettings.BrushShapesOpen;
            BrushEditOpen = oldSettings.BrushEditOpen;
            BrushOptionsOpen = oldSettings.BrushOptionsOpen;
            ShowTreeMeshData = oldSettings.ShowTreeMeshData;
            Sorting = oldSettings.Sorting;
            SortingOrder = oldSettings.SortingOrder;
            FilterStyle = oldSettings.FilterStyle;
            Brushes = oldSettings.Brushes;
            SelectBrush(oldSettings.SelectedBrush);
            ToggleTool = new UnsavedKeyMapping(oldSettings.ToggleTool);
            KeepTreesInNewBrush = oldSettings.KeepTreesInNewBrush;
            IgnoreVanillaTrees = oldSettings.IgnoreVanillaTrees;
            ShowInfoTooltip = oldSettings.ShowInfoTooltip;
            PlayEffect = oldSettings.PlayEffect;
            ChargeMoney = oldSettings.ChargeMoney;
            ShowBrushTrees = oldSettings.ShowBrushTrees;
        }

        public static ModSettings Default()
        {
            ModSettings settings = new ModSettings();
            settings.CheckBrushes();

            return settings;
        }

        public void CheckBrushes()
        {
            if (Brushes.Count == 0) 
            {
                Brushes.Add(Brush.Default());
            }
            if (SelectedBrush == null && Brushes.Count > 0)
            {
                SelectedBrush = Brushes[0];
            }
        }

        public void SelectBrush(string brushName)
        {
            SelectedBrush = GetSelectedBrush(brushName);
        }

        public void SelectNextBestBrush()
        {
            this.SelectedBrush = GetNextBestBrush();
        }

        private Brush GetSelectedBrush(string brushName)
        {
            var brush = Brushes.Find(b => b.Name == brushName);
            if (brush != null)
            {
                return brush;
            }
            else
            {
                return GetNextBestBrush();
            }
        }

        private Brush GetNextBestBrush()
        {
            var first = Brushes.FirstOrDefault();
            if (first == null)
            {
                var newDefault = Brush.Default();
                Brushes.Add(newDefault);
                return newDefault;
            }
            else
            {
                return first;
            }
        }

        public void Save()
        {
            try
            {
                using (var sw = new StreamWriter(SettingsFilePath))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));

                    // Update the version info 
                    ModVersion = ForestBrushRevisitedMod.Version;

                    xmlSerializer.Serialize(sw, this);
                }
            }
            catch (Exception ex)
            {
                // Flag the error to display to user when level is loaded
                string sErrorMessage = $"Error saving settings file:\n{SettingsFilePath}\n{ex}";
                Debug.Log(sErrorMessage);
                Prompt.Info(Constants.ModName, sErrorMessage);
            }
        }

        public static void Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    using (var reader = new StreamReader(SettingsFilePath))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                        ModSettings? oSettings = xmlSerializer.Deserialize(reader) as ModSettings;
                        if (oSettings is not null)
                        {
                            oSettings.CheckBrushes();
                            Settings = oSettings;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // make a backup of the file so the user doesn't lose all their brushes
                    try
                    {
                        File.Copy(SettingsFilePath, SettingsFilePath + ".bad");
                    }
                    catch (Exception)
                    {
                    }

                    // Log the error
                    string sErrorMessage = "Error reading settings file:\n";
                    sErrorMessage += SettingsFilePath + "\n\n";
                    sErrorMessage += $"Your settings file has been renamed to:\n{SettingsFilePath}.bad\n\n";
                    sErrorMessage += "A new settings file will be created.";
                    Debug.Log(sErrorMessage, ex);
                }
            }
            else if (File.Exists(OldModSettingsFilePath))
            {
                // try to upgrade the old Forest Brush mod settings file.
                try
                {
                    Debug.Log($"Reading 'ForestBrush.xml' settings");
                    using (var reader = new StreamReader(OldModSettingsFilePath))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlSettings));
                        XmlSettings? oOldSettings = xmlSerializer.Deserialize(reader) as XmlSettings;
                        if (oOldSettings is not null)
                        {
                            // Convert old settings to new settings
                            ModSettings settings = new ModSettings(oOldSettings);
                            settings.CheckBrushes();

                            // Set new settings
                            Settings = settings;
                            Debug.Log($"Brush Count: {Settings.Brushes.Count}");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    Debug.Log("Error reading old 'ForestBrush.xml' file", ex);
                }
            }

            Settings = ModSettings.Default();
        }
    }
}
