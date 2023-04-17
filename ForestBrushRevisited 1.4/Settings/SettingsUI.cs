using ColossalFramework.UI;
using ForestBrushRevisited.GUI;
using ForestBrushRevisited.TranslationFramework;
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
                UILabel txtTitle = AddDescription(pnlMain, "title", pnlMain, 1.0f, ForestBrushMod.Title);
                txtTitle.textScale = 1.2f;

                // Add tabstrip.
                ExtUITabstrip tabStrip = ExtUITabstrip.Create(helper);
                UIHelper tabGeneral = tabStrip.AddTabPage("General", true);
                UIHelper tabSorting = tabStrip.AddTabPage("Sorting / Searching", true);
                UIHelper tabMainteanance = tabStrip.AddTabPage("Maintenance", true);

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
            UIHelper groupGeneral = helper.AddGroup("General") as UIHelper;

            // Toolbar button
            groupGeneral.AddCheckbox("Add Button on Main Toolbar", ModSettings.Settings.MainToolbarButton, (b) =>
            {
                ModSettings.Settings.MainToolbarButton = b;
                ModSettings.Settings.Save();

                ForestBrush.Instance.UpdateToolbarButton();
            });

            // Interface
            UIHelper groupMainPanel = helper.AddGroup("Main Panel") as UIHelper;

            UIPanel panel = groupMainPanel.self as UIPanel;
            optionKeys = panel.gameObject.AddComponent<OptionsKeyBinding>();

            newBrushBehaviour = (UIDropDown)groupMainPanel.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-NEWBRUSH"),
                             new string[]
                             {
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-NEWBRUSH-CLEAR"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-NEWBRUSH-KEEP")
                             },
                             ModSettings.Settings.KeepTreesInNewBrush ? 1 : 0,
                             (index) =>
                             {
                                 ModSettings.Settings.KeepTreesInNewBrush = index == 1 ? true : false;
                                 ModSettings.Settings.Save();
                             });
            newBrushBehaviour.width = 500.0f;
            groupMainPanel.AddSpace(10);

            groupMainPanel.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SHOWMESHDATA"), ModSettings.Settings.ShowTreeMeshData, (b) =>
            {
                ModSettings.Settings.ShowTreeMeshData = b;
                ModSettings.Settings.Save();
            });

            // Behaviour
            UIHelper groupBehaviour = helper.AddGroup("Behaviour") as UIHelper;

            groupBehaviour.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-CHARGE-MONEY"), ModSettings.Settings.ChargeMoney, (b) => { ModSettings.Settings.ChargeMoney = b; ModSettings.Settings.Save(); });
            groupBehaviour.AddSpace(10);

            groupBehaviour.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-IGNORE-VANILLA"), ModSettings.Settings.IgnoreVanillaTrees, (b) =>
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

            groupBehaviour.AddCheckbox(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SINGLETREE-EFFECT"), ModSettings.Settings.PlayEffect, (b) => { ModSettings.Settings.PlayEffect = b; ModSettings.Settings.Save(); });
            groupBehaviour.AddSpace(10);
        }

        private void SetupSortingSearchingTab(UIHelper helper)
        {
            UIHelper groupSorting = helper.AddGroup("Sorting") as UIHelper;

            groupSorting.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING"),
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
                                  ModSettings.Settings.Save();

                                  // Update list if loaded
                                  if (ForestBrushLoader.IsLoaded())
                                  {
                                      ForestBrush.Instance.UpdateTreeList();
                                  }
                              });

            groupSorting.AddSpace(10);

            groupSorting.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING-ORDER"),
                              new string[]
                              {
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING-DESCENDING"),
                                      Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-SORTING-ASCENDING")
                              },
                              (int)ModSettings.Settings.SortingOrder,
                              (index) =>
                              {
                                  ModSettings.Settings.SortingOrder = (SortingOrder)index;
                                  ModSettings.Settings.Save();

                                  // Update list if loaded
                                  if (ForestBrushLoader.IsLoaded())
                                  {
                                      ForestBrush.Instance.UpdateTreeList();
                                  }
                              });


            UIHelper groupSearching = helper.AddGroup("Searching") as UIHelper;
            searchLogic = (UIDropDown)groupSearching.AddDropdown(Translation.Instance.GetTranslation("FOREST-BRUSH-OPTIONS-FILTERING-LOGIC"),
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
                                  ModSettings.Settings.Save();

                                  // Update list if loaded
                                  if (ForestBrushLoader.IsLoaded())
                                  {
                                      ForestBrush.Instance.UpdateTreeList();
                                  }
                              });
            searchLogic.width = 500.0f;
        }

        private void SetupMaintenanceTab(UIHelper helper)
        {
            UIHelper groupMaintenance = helper.AddGroup("Maintenance") as UIHelper;

            groupMaintenance.AddButton("Reset Panel Position", () =>
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
