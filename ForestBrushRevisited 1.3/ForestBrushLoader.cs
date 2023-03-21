using ICities;
using System.Collections.Generic;
using UnityEngine;

namespace ForestBrushRevisited
{
    public class ForestBrushLoader : LoadingExtensionBase
    {
        private static bool s_loaded = false;
        private static LoadMode s_gameMode;
        private static List<string> s_loadingErrorMessages = new List<string>();
        private static GameObject? s_forestBrushGameObject = null;

        public static bool IsLoaded() { return s_loaded; }

        private static bool ActiveInMode(LoadMode mode)
        {
            switch (mode)
            {
                case LoadMode.NewGame:
                case LoadMode.NewGameFromScenario:
                case LoadMode.LoadGame:
                    return true;

                case LoadMode.LoadMap:
                case LoadMode.NewMap:
                    return true;

                case LoadMode.NewTheme:
                case LoadMode.LoadTheme:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsGameMode()
        {
            return s_gameMode == LoadMode.NewGame ||
                s_gameMode == LoadMode.NewGameFromScenario ||
                s_gameMode == LoadMode.LoadGame;
        }
        public static bool IsThemeMode()
        {
            return s_gameMode == LoadMode.NewTheme ||
                s_gameMode == LoadMode.LoadTheme;
        }
        public static bool IsMapMode()
        {
            return s_gameMode == LoadMode.LoadMap ||
                s_gameMode == LoadMode.NewMap;
        }

        public static GameObject GameObject()
        {
            if (s_forestBrushGameObject == null)
            {
                s_forestBrushGameObject = new GameObject("ForestBrush");
            }
            return s_forestBrushGameObject;
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            // You can load a game while loaded in a game
            if (s_loaded)
            {
                UnloadBrush();
            }

            s_gameMode = mode;

            if (!s_loaded && ActiveInMode(mode))
            {
                ForestBrush.Instance.Initialize();
                s_loaded = true;
            }
        }

        public override void OnLevelUnloading()
        {
            UnloadBrush();
            base.OnLevelUnloading();
        }

        private void UnloadBrush()
        {
            if (s_loaded)
            {
                ForestBrush.Instance.CleanUp();

                if (IsMapMode() || IsThemeMode())
                {
                    if (ResourceLoader.Atlas != null)
                    {
                        UnityEngine.Resources.UnloadAsset(ResourceLoader.Atlas);
                    }
                }
                s_loaded = false;
            }
        }

        public static void DisplayWarning(string sMessage)
        {
            if (IsLoaded())
            {
                Prompt.Info(Constants.ModName, sMessage);
            }
            else
            {
                // Flag the error to display to user when level is loaded
                s_loadingErrorMessages.Add(sMessage);
            }
        }

        private static void DisplayLoadingWarnings()
        {
            // Display any errors encountered on loading
            if (s_loadingErrorMessages.Count > 0)
            {
                string sErrors = "";
                if (s_loadingErrorMessages.Count > 1)
                {
                    sErrors += "Errors detected during loading:\n\n";
                }

                foreach (string error in s_loadingErrorMessages)
                {
                    sErrors += error + "\n\n";
                }
                s_loadingErrorMessages.Clear();

                // Display errors
                Prompt.Info(Constants.ModName, sErrors);
            }
        }
    }
}
