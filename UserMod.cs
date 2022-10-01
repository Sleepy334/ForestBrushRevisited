using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ForestBrush.Persistence;
using ForestBrush.Redirection;
using ForestBrush.TranslationFramework;
using HarmonyLib;
using ICities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ForestBrush
{
    public class UserMod : LoadingExtensionBase, IUserMod
    {
        public static string Version = "v1.3.3";
        public static string Title => "Forest Brush " + Version;

        public static XmlPersistenceService XmlPersistenceService { get; private set; }
        private static GameObject? s_forestBrushGameObject = null;

        public static Settings Settings { get; private set; }
        public static LoadMode LoadMode;
        private static bool modInstalled = false;
        private OptionsKeyBinding optionKeys;

        private UIDropDown searchLogic;
        private UIDropDown newBrushBehaviour;
        //private HarmonyInstance Harmony { get; set; }
        private const string HarmonyID = "Sleepy.forestbrush";

        public static bool IsLoaded() { return modInstalled; }

        public static GameObject GameObject()
        {
            if (s_forestBrushGameObject == null)
            {
                s_forestBrushGameObject = new GameObject("ForestBrush");
            }
            return s_forestBrushGameObject;
        }

        public string Name => Constants.ModName;

        public string Description => Translation.Instance.GetTranslation("FOREST-BRUSH-MODDESCRIPTION");

        public static bool IsModEnabled(ulong publishedFileID, string modName) {
            foreach (var plugin in PluginManager.instance.GetPluginsInfo()) {
                if (plugin.publishedFileID.AsUInt64 == publishedFileID
                    || plugin.name == modName) {
                    return plugin.isEnabled;
                }
            }
            return false;
        }

        public void OnEnabled()
        {
            InstallOutOfGameDependencies();

            if (LoadingManager.exists && LoadingManager.instance.m_loadingComplete)
            {
                InstallMod();
            }
        }

        public void OnDisabled()
        {
            if (LoadingManager.exists && LoadingManager.instance.m_loadingComplete)
            {
                UninstallMod();
            }

            UninstallOutOfGameDependencies();
            Unpatch();
        }

        public static bool IsGame = LoadMode == LoadMode.LoadGame || LoadMode == LoadMode.NewGame || LoadMode == LoadMode.NewGameFromScenario;
        public static bool IsMap = LoadMode == LoadMode.LoadMap || LoadMode == LoadMode.NewMap;
        public static bool IsTheme = LoadMode == LoadMode.NewTheme || LoadMode == LoadMode.LoadTheme;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            LoadMode = mode;
            if (IsGame || IsMap || IsTheme)
            {
                InstallMod();
            }
        }

        public override void OnLevelUnloading()
        {
            if (modInstalled)
            {
                UninstallMod();
            }

            base.OnLevelUnloading();
        }

        void InstallOutOfGameDependencies()
        {
            if (GameSettings.FindSettingsFileByName(Constants.ModName) == null) {
                GameSettings.AddSettingsFile(new SettingsFile { fileName = Constants.ModName });
            }
            XmlPersistenceService = new XmlPersistenceService();
            Settings = XmlPersistenceService.Load();
        }

        void UninstallOutOfGameDependencies()
        {
            XmlPersistenceService = null;
            Settings = null;
            var settingFiles = (Dictionary<string, SettingsFile>)typeof(GameSettings).GetField("m_SettingsFiles", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(GameSettings.instance);
            settingFiles.Remove(Constants.ModName);
        }

        private void Patch() {
            if (DependencyUtilities.IsHarmonyRunning() && !IsModEnabled(593588108UL, "Prop & Tree Anarchy"))
            {
                var harmony = new Harmony(HarmonyID);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }

            if (!IsModEnabled(1873351912UL, "Tree Precision")) {
                Redirector<PositionDetour>.Deploy();
            }
        }

        private void Unpatch() {
            Redirector<PositionDetour>.Revert();
            var harmony = new Harmony(HarmonyID);
            harmony.UnpatchAll(HarmonyID);
        }

        void InstallMod()
        {
            if (s_forestBrushGameObject != null)
            {
                UninstallMod();
                s_forestBrushGameObject = null;
            }

            if (!IsGame && !IsMap && !IsTheme)
            {
                return;
            }

            if (DependencyUtilities.IsHarmonyRunning())
            {
                Patch();
            }
            else
            {
                string sMessage = "Mod Dependency Error:\r\n";
                sMessage += "\r\n";
                sMessage += "Harmony not found.\r\n";
                sMessage += "\r\n";
                sMessage += "Mod disabled until dependencies resolved, please subscribe to Harmony.";
                Prompt.Info(Title, sMessage);
                return;
            }

            if (IsMap || IsTheme)
            {
                Resources.ResourceLoader.Atlas = Resources.ResourceLoader.Atlas;
            }
            Resources.ResourceLoader.ForestBrushAtlas = Resources.ResourceLoader.ForestBrushAtlas;

            ForestBrush.Instance.Initialize();

            modInstalled = true;
        }

        void UninstallMod()
        {
            ForestBrush.Instance.CleanUp();

            if (IsMap || IsTheme)
            {
                if(Resources.ResourceLoader.Atlas != null) UnityEngine.Resources.UnloadAsset(Resources.ResourceLoader.Atlas);
            }
            if (s_forestBrushGameObject != null)
            {
                UnityEngine.Object.Destroy(s_forestBrushGameObject);
                s_forestBrushGameObject = null;
            }
            modInstalled = false;
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                UIHelper group = helper.AddGroup(Name) as UIHelper;

                UIPanel panel = group.self as UIPanel;

                group.AddSpace(10);

                group.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING"), 
                                  new string[] 
                                  {
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-DATA-NAME"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-DATA-AUTHOR"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-DATA-TEXTURE"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-DATA-TRIANGLES")
                                  }, 
                                  (int)Settings.Sorting, 
                                  (index) => 
                                  {
                                      Settings.Sorting = (TreeSorting)index; SaveSettings();
                                  });

                group.AddSpace(10);

                group.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING-ORDER"),
                                  new string[]
                                  {
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING-DESCENDING"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING-ASCENDING")
                                  },
                                  (int)Settings.SortingOrder,
                                  (index) =>
                                  {
                                      Settings.SortingOrder = (SortingOrder)index; SaveSettings();
                                  });

                group.AddSpace(10);

                searchLogic = (UIDropDown)group.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-FILTERING-LOGIC"),
                                  new string[]
                                  {
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-FILTERING-AND"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-FILTERING-OR"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-FILTERING-SIMPLE")
                                  },
                                  (int)Settings.FilterStyle,
                                  (index) =>
                                  {
                                      Settings.FilterStyle = (FilterStyle)index; SaveSettings();
                                  });
                searchLogic.width = 500.0f;

                group.AddSpace(10);

                newBrushBehaviour = (UIDropDown)group.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-NEWBRUSH"),
                                  new string[]
                                  {
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-NEWBRUSH-CLEAR"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-NEWBRUSH-KEEP")
                                  },
                                  Settings.KeepTreesInNewBrush ? 1 : 0,
                                  (index) =>
                                  {
                                      Settings.KeepTreesInNewBrush = index == 1 ? true : false; SaveSettings();
                                  });
                newBrushBehaviour.width = 500.0f;

                group.AddSpace(10);

                group.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SHOWMESHDATA"), Settings.ShowTreeMeshData, (b) => { Settings.ShowTreeMeshData = b; SaveSettings(); });

                group.AddSpace(10);

                group.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-IGNORE-VANILLA"), Settings.IgnoreVanillaTrees, (b) => 
                {
                    Settings.IgnoreVanillaTrees = b;
                    SaveSettings();
                    if (LoadingManager.instance.m_loadingComplete)
                    {
                        ForestBrush.Instance.LoadTrees();
                        ForestBrush.Instance.ForestBrushPanel.BrushEditSection.SetupFastlist();
                    }
                });
                group.AddSpace(10);

                group.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SINGLETREE-EFFECT"), Settings.PlayEffect, (b) => { Settings.PlayEffect = b; SaveSettings(); }); 
                
                group.AddSpace(10);

                group.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-CHARGE-MONEY"), Settings.ChargeMoney, (b) => { Settings.ChargeMoney = b; SaveSettings(); });

                group.AddSpace(20);

                optionKeys = panel.gameObject.AddComponent<OptionsKeyBinding>();

                group.AddSpace(10); 
            }
            catch (Exception)
            {
                Debug.LogWarning("OnSettingsUI failure.");
            }
        }

        public static void SaveSettings()
        {
            XmlPersistenceService.Save(Settings);
        }
    }
}
