using ColossalFramework.UI;
using ForestBrushRevisited.TranslationFramework;
using ICities;
using System;
using UnityEngine;

namespace ForestBrushRevisited.View
{
    internal class SettingsUI
    {
        private OptionsKeyBinding? optionKeys = null;
        private UIDropDown searchLogic;
        private UIDropDown newBrushBehaviour;

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                UIHelper group = helper.AddGroup(ForestBrushMod.Title) as UIHelper;

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
                                  (int)ModSettings.Settings.Sorting,
                                  (index) =>
                                  {
                                      ModSettings.Settings.Sorting = (TreeSorting)index; 
                                      ModSettings.SaveSettings();

                                      // Update list if loaded
                                      if (ForestBrushLoader.IsLoaded())
                                      {
                                          ForestBrush.Instance.UpdateTreeList();
                                      }
                                  });

                group.AddSpace(10);

                group.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING-ORDER"),
                                  new string[]
                                  {
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING-DESCENDING"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING-ASCENDING")
                                  },
                                  (int)ModSettings.Settings.SortingOrder,
                                  (index) =>
                                  {
                                      ModSettings.Settings.SortingOrder = (SortingOrder)index; 
                                      ModSettings.SaveSettings();

                                      // Update list if loaded
                                      if (ForestBrushLoader.IsLoaded())
                                      {
                                          ForestBrush.Instance.UpdateTreeList();
                                      }
                                  });

                group.AddSpace(10);

                searchLogic = (UIDropDown)group.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-FILTERING-LOGIC"),
                                  new string[]
                                  {
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-FILTERING-AND"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-FILTERING-OR"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-FILTERING-SIMPLE")
                                  },
                                  (int)ModSettings.Settings.FilterStyle,
                                  (index) =>
                                  {
                                      ModSettings.Settings.FilterStyle = (FilterStyle)index; 
                                      ModSettings.SaveSettings();

                                      // Update list if loaded
                                      if (ForestBrushLoader.IsLoaded())
                                      {
                                          ForestBrush.Instance.UpdateTreeList();
                                      }
                                  });
                searchLogic.width = 500.0f;

                group.AddSpace(10);

                newBrushBehaviour = (UIDropDown)group.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-NEWBRUSH"),
                                  new string[]
                                  {
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-NEWBRUSH-CLEAR"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-NEWBRUSH-KEEP")
                                  },
                                  ModSettings.Settings.KeepTreesInNewBrush ? 1 : 0,
                                  (index) =>
                                  {
                                      ModSettings.Settings.KeepTreesInNewBrush = index == 1 ? true : false; 
                                      ModSettings.SaveSettings();
                                  });
                newBrushBehaviour.width = 500.0f;

                group.AddSpace(10);

                group.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SHOWMESHDATA"), ModSettings.Settings.ShowTreeMeshData, (b) => 
                { 
                    ModSettings.Settings.ShowTreeMeshData = b; 
                    ModSettings.SaveSettings(); 
                });

                group.AddSpace(10);

                group.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-IGNORE-VANILLA"), ModSettings.Settings.IgnoreVanillaTrees, (b) =>
                {
                    ModSettings.Settings.IgnoreVanillaTrees = b;
                    ModSettings.SaveSettings();
                    if (LoadingManager.instance.m_loadingComplete)
                    {
                        ForestBrush.Instance.LoadTrees();
                        ForestBrush.Instance.ForestBrushPanel.BrushEditSection.SetupFastlist();
                    }
                });
                group.AddSpace(10);

                group.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SINGLETREE-EFFECT"), ModSettings.Settings.PlayEffect, (b) => { ModSettings.Settings.PlayEffect = b; ModSettings.SaveSettings(); });

                group.AddSpace(10);

                group.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-CHARGE-MONEY"), ModSettings.Settings.ChargeMoney, (b) => { ModSettings.Settings.ChargeMoney = b; ModSettings.SaveSettings(); });

                group.AddSpace(20);

                optionKeys = panel.gameObject.AddComponent<OptionsKeyBinding>();

                group.AddSpace(10);

                group.AddButton("Reset Panel Position", () =>
                {
                    ModSettings settings = ModSettings.Default();
                    ModSettings.Settings.PanelPosX = settings.PanelPosX;
                    ModSettings.Settings.PanelPosY = settings.PanelPosY;

                    // Move panel if created
                    if (ForestBrush.Instance != null && ForestBrush.Instance.ForestBrushPanel != null)
                    {
                        ForestBrush.Instance.ForestBrushPanel.absolutePosition = new Vector3(ModSettings.Settings.PanelPosX, ModSettings.Settings.PanelPosY);
                    }
                });
            }
            catch (Exception)
            {
                Debug.LogWarning("OnSettingsUI failure.");
            }
        }
    }
}
