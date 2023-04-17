using ColossalFramework;
using ColossalFramework.IO;
using ForestBrushRevisited.Persistence;
using ForestBrushRevisited.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace ForestBrushRevisited
{
    public class ModSettings
    {
        public static ModSettings Settings { get; private set; }
        private static readonly string configurationPath = Path.Combine(DataLocation.localApplicationData, "ForestBrush.xml");

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
                using (var sw = new StreamWriter(configurationPath))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    xmlSerializer.Serialize(sw, this);
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

        public static void Load()
        {
            if (File.Exists(configurationPath))
            {
                try
                {
                    using (var reader = new StreamReader(configurationPath))
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

            Settings = ModSettings.Default();
        }
    }
}
