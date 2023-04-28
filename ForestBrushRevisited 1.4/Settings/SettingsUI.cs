using ColossalFramework.UI;
using ForestBrushRevisited.GUI;
using ICities;
using System;
using TransferManagerCE.Settings;
using UnityEngine;

namespace ForestBrushRevisited.Settings
{
    internal class SettingsUI
    {
        private OptionsKeyBinding? optionKeys = null;
        private UIDropDown searchLogic;
        private UIDropDown newBrushBehaviour;

        public void OnSettingsUI(UIHelper helper)
        {
            try
            {
                // Title
                UIComponent pnlMain = (UIComponent)helper.self;
                UILabel txtTitle = AddDescription(pnlMain, "title", pnlMain, 1.0f, ForestBrushRevisitedMod.Title);
                txtTitle.textScale = 1.2f;

                // Add tabstrip.
                ExtUITabstrip tabStrip = ExtUITabstrip.Create(helper);
                UIHelper tabGeneral = tabStrip.AddTabPage(Localization.Get("tabGeneral"), true);
                UIHelper tabSorting = tabStrip.AddTabPage(Localization.Get("tabSorting"), true);
                UIHelper tabMainteanance = tabStrip.AddTabPage(Localization.Get("tabMaintenance"), true);

                SetupGeneralTab(tabGeneral);
                SetupSortingSearchingTab(tabSorting);
                SetupMaintenanceTab(tabMainteanance);
            }
            catch (Exception)
            {
                Debug.LogWarning("OnSettingsUI failure.");
            }
        }

        private void SetupGeneralTab(UIHelper helper)
        {
            UIHelper groupLocalisation = (UIHelper)helper.AddGroup(Localization.Get("GROUP_LOCALISATION"));
            groupLocalisation.AddDropdown(Localization.Get("dropdownLocalization"), Localization.GetLoadedLanguages(), Localization.GetLanguageIndexFromCode(ModSettings.Settings.PreferredLanguage), OnLocalizationDropDownChanged);

            UIHelper groupGeneral = helper.AddGroup(Localization.Get("tabGeneral")) as UIHelper;

            // Toolbar button
            groupGeneral.AddCheckbox("Add Button on Main Toolbar", ModSettings.Settings.MainToolbarButton, (b) =>
            {
                ModSettings.Settings.MainToolbarButton = b;
                ModSettings.Settings.Save();

                ForestBrush.Instance.UpdateToolbarButton();
            });

            // Interface
            UIHelper groupMainPanel = helper.AddGroup(Localization.Get("groupMainPanel")) as UIHelper;

            UIPanel panel = groupMainPanel.self as UIPanel;
            optionKeys = panel.gameObject.AddComponent<OptionsKeyBinding>();

            newBrushBehaviour = (UIDropDown)groupMainPanel.AddDropdown(Localization.Get("FOREST-BRUSH-OPTIONS-NEWBRUSH"),
                             new string[]
                             {
                                      Localization.Get("FOREST-BRUSH-OPTIONS-NEWBRUSH-CLEAR"),
                                      Localization.Get("FOREST-BRUSH-OPTIONS-NEWBRUSH-KEEP")
                             },
                             ModSettings.Settings.KeepTreesInNewBrush ? 1 : 0,
                             (index) =>
                             {
                                 ModSettings.Settings.KeepTreesInNewBrush = index == 1 ? true : false;
                                 ModSettings.Settings.Save();
                             });
            newBrushBehaviour.width = 500.0f;
            groupMainPanel.AddSpace(10);

            groupMainPanel.AddCheckbox(Localization.Get("FOREST-BRUSH-OPTIONS-SHOWMESHDATA"), ModSettings.Settings.ShowTreeMeshData, (b) =>
            {
                ModSettings.Settings.ShowTreeMeshData = b;
                ModSettings.Settings.Save();
            });

            // Behaviour
            UIHelper groupBehaviour = helper.AddGroup(Localization.Get("groupBehaviour")) as UIHelper;

            groupBehaviour.AddCheckbox(Localization.Get("FOREST-BRUSH-OPTIONS-CHARGE-MONEY"), ModSettings.Settings.ChargeMoney, (b) => { ModSettings.Settings.ChargeMoney = b; ModSettings.Settings.Save(); });
            groupBehaviour.AddSpace(10);

            groupBehaviour.AddCheckbox(Localization.Get("FOREST-BRUSH-OPTIONS-IGNORE-VANILLA"), ModSettings.Settings.IgnoreVanillaTrees, (b) =>
            {
                ModSettings.Settings.IgnoreVanillaTrees = b;
                ModSettings.Settings.Save();
                if (LoadingManager.instance.m_loadingComplete)
                {
                    ForestBrush.Instance.LoadTrees();
                    ForestBrushPanel.Instance.BrushEditSection.SetupFastlist();
                }
            });
            groupBehaviour.AddSpace(10);

            groupBehaviour.AddCheckbox(Localization.Get("FOREST-BRUSH-OPTIONS-SINGLETREE-EFFECT"), ModSettings.Settings.PlayEffect, (b) => { ModSettings.Settings.PlayEffect = b; ModSettings.Settings.Save(); });
            groupBehaviour.AddSpace(10);
        }

        private void SetupSortingSearchingTab(UIHelper helper)
        {
            UIHelper groupSorting = helper.AddGroup(Localization.Get("groupSorting")) as UIHelper;

            groupSorting.AddDropdown(Localization.Get("FOREST-BRUSH-OPTIONS-SORTING"),
                              new string[]
                              {
                                      Localization.Get("FOREST-BRUSH-DATA-NAME"),
                                      Localization.Get("FOREST-BRUSH-DATA-AUTHOR"),
                                      Localization.Get("FOREST-BRUSH-DATA-TEXTURE"),
                                      Localization.Get("FOREST-BRUSH-DATA-TRIANGLES")
                              },
                              (int)ModSettings.Settings.Sorting,
                              (index) =>
                              {
                                  ModSettings.Settings.Sorting = (TreeSorting)index;
                                  ModSettings.Settings.Save();

                                  // Update list if loaded
                                  if (ForestBrushRevisitedLoader.IsLoaded())
                                  {
                                      ForestBrush.Instance.UpdateTreeList();
                                  }
                              });

            groupSorting.AddSpace(10);

            groupSorting.AddDropdown(Localization.Get("FOREST-BRUSH-OPTIONS-SORTING-ORDER"),
                              new string[]
                              {
                                      Localization.Get("FOREST-BRUSH-OPTIONS-SORTING-DESCENDING"),
                                      Localization.Get("FOREST-BRUSH-OPTIONS-SORTING-ASCENDING")
                              },
                              (int)ModSettings.Settings.SortingOrder,
                              (index) =>
                              {
                                  ModSettings.Settings.SortingOrder = (SortingOrder)index;
                                  ModSettings.Settings.Save();

                                  // Update list if loaded
                                  if (ForestBrushRevisitedLoader.IsLoaded())
                                  {
                                      ForestBrush.Instance.UpdateTreeList();
                                  }
                              });


            UIHelper groupSearching = helper.AddGroup(Localization.Get("groupSearching")) as UIHelper;
            searchLogic = (UIDropDown)groupSearching.AddDropdown(Localization.Get("FOREST-BRUSH-OPTIONS-FILTERING-LOGIC"),
                              new string[]
                              {
                                      Localization.Get("FOREST-BRUSH-OPTIONS-FILTERING-AND"),
                                      Localization.Get("FOREST-BRUSH-OPTIONS-FILTERING-OR"),
                                      Localization.Get("FOREST-BRUSH-OPTIONS-FILTERING-SIMPLE")
                              },
                              (int)ModSettings.Settings.FilterStyle,
                              (index) =>
                              {
                                  ModSettings.Settings.FilterStyle = (FilterStyle)index;
                                  ModSettings.Settings.Save();

                                  // Update list if loaded
                                  if (ForestBrushRevisitedLoader.IsLoaded())
                                  {
                                      ForestBrush.Instance.UpdateTreeList();
                                  }
                              });
            searchLogic.width = 500.0f;
        }

        private void SetupMaintenanceTab(UIHelper helper)
        {
            UIHelper groupMaintenance = helper.AddGroup(Localization.Get("groupMaintenance")) as UIHelper;

            groupMaintenance.AddButton(Localization.Get("buttonResetPanelPosition"), () =>
            {
                // Move panel if created
                if (ForestBrush.Instance != null)
                {
                    ForestBrushPanel? panel = ForestBrushPanel.Instance;
                    if (panel is not null)
                    {
                        // Center panel to screen
                        panel.CenterTo(null);

                        // Save new positions
                        ModSettings.Settings.PanelPosX = panel.absolutePosition.x;
                        ModSettings.Settings.PanelPosY = panel.absolutePosition.y;
                        ModSettings.Settings.Save();
                    }
                }
            });
        }

        public void OnLocalizationDropDownChanged(int value)
        {
            ModSettings.Settings.PreferredLanguage = Localization.GetLoadedCodes()[value];
            ModSettings.Settings.Save();
        }

        /* 
         * Code adapted from PropAnarchy under MIT license
         */
        private static readonly Color32 m_greyColor = new Color32(0xe6, 0xe6, 0xe6, 0xee);
        private static UILabel AddDescription(UIHelper parent, string name, float fontScale, string text)
        {
            return AddDescription(parent.self as UIPanel, name, parent.self as UIPanel, fontScale, text);
        }
        private static UILabel AddDescription(UIComponent panel, string name, UIComponent alignTo, float fontScale, string text)
        {
            UILabel desc = panel.AddUIComponent<UILabel>();
            desc.name = name;
            desc.width = panel.width - 80;
            desc.wordWrap = true;
            desc.autoHeight = true;
            desc.textScale = fontScale;
            desc.textColor = m_greyColor;
            desc.text = text;
            desc.relativePosition = new Vector3(alignTo.relativePosition.x + 26f, alignTo.relativePosition.y + alignTo.height + 10);
            return desc;
        }
    }
}
