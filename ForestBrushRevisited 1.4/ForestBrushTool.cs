﻿using ColossalFramework.UI;
using ForestBrushRevisited.GUI;
using ForestBrushRevisited.TranslationFramework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ForestBrushRevisited
{
    public class ForestBrushTool : MonoBehaviour
    {
        private ProbabilityCalculator probabilityCalculator;

        public Brush Brush => ModSettings.Settings.SelectedBrush;

        internal List<Tree> Trees = new List<Tree>();

        internal List<TreeInfo> TreeInfos { get; set; } = new List<TreeInfo>();

        private TreeInfo Container { get; set; } = ForestBrush.Instance.Container;

        public List<Brush> Brushes => ModSettings.Settings.Brushes;

        void Awake() {
            probabilityCalculator = new ProbabilityCalculator();
        }

        public void UpdateTool(string brushName) {
            ModSettings.Settings.SelectBrush(brushName);

            TreeInfos.Clear();
            Trees.Clear();
            foreach (var tree in Brush.Trees) 
            {
                if (ForestBrush.Instance.GetTreeInfo(tree.Name, out TreeInfo? treeInfo))
                {
                    if (treeInfo != null)
                    {
                        TreeInfos.Add(treeInfo);
                        Trees.Add(tree);
                    }
                }
            }

            Container = CreateBrushPrefab(Trees);

            ForestBrush.Instance.ForestBrushPanel.LoadBrush(Brush);

            ModSettings.SaveSettings();
        }

        private void Add(TreeInfo tree) {
            if (!TreeInfos.Contains(tree)) TreeInfos.Add(tree);
            if (Brush.Trees.Find(t => t.Name == tree.name) == null) {
                Brush.Add(tree);
                Trees.Add(new Tree(tree));
            }
        }

        private void Remove(TreeInfo treeInfo) {
            if (!TreeInfos.Contains(treeInfo)) return;
            TreeInfos.Remove(treeInfo);
            Brush.Remove(treeInfo);
            Tree tree = Trees.Find(t => t.Name == treeInfo.name);
            if (tree == null) return;
            Trees.Remove(tree);
        }

        public void RemoveAll() {
            foreach (TreeInfo tree in ForestBrush.Instance.ForestBrushPanel.BrushEditSection.TreesList.rowsData) {
                Remove(tree);
            }
            foreach (TreeItemRow item in ForestBrush.Instance.ForestBrushPanel.BrushEditSection.TreesList.rows) {
                item?.ToggleCheckbox(false);
            }
        }

        private void AddAll() {
            foreach (TreeInfo tree in ForestBrush.Instance.ForestBrushPanel.BrushEditSection.TreesList.rowsData) {
                if (TreeInfos.Count == 100) {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(
                         Translation.Instance.GetTranslation("FOREST-BRUSH-MODAL-LIMITREACHED-TITLE"),
                         Translation.Instance.GetTranslation("FOREST-BRUSH-MODAL-LIMITREACHED-MESSAGE-ALL"),
                         false);
                    break;
                }
                Add(tree);
            }

            IUIFastListRow[] itemBuffer = ForestBrush.Instance.ForestBrushPanel.BrushEditSection.TreesList.rows.m_buffer;

            for (int i = 0; i < itemBuffer.Length; i++) {
                TreeItemRow treeItem = itemBuffer[i] as TreeItemRow;
                if (TreeInfos.Contains(treeItem.Prefab)) treeItem.ToggleCheckbox(true);
            }
        }

        public void New(string brushName) {
            if (Brushes.Find(b => b.Name == brushName) == null)
            {
                Brush brush = Brush.Default();

                brush.Name = brushName;

                if (ModSettings.Settings.KeepTreesInNewBrush)
                {
                    foreach (var tree in Trees)
                    {
                        brush.Trees.Add(tree);
                    }
                }

                Brushes.Add(brush);

                UpdateTool(brushName);
            }
            else
            {
                Debug.LogError("Error creating new brush. Brush already exists. This shouldn't happen, please contact the mod author.");
            }
        }

        internal void DeleteCurrent() {
            Brushes.Remove(Brush);
            ModSettings.Settings.SelectNextBestBrush();
            ForestBrush.Instance.ForestBrushPanel.BrushSelectSection.UpdateDropDown();
            string nextBrush = ForestBrush.Instance.ForestBrushPanel.BrushSelectSection.SelectBrushDropDown.items.Length <= 0 ? Constants.NewBrushName :
                ForestBrush.Instance.ForestBrushPanel.BrushSelectSection.SelectBrushDropDown.selectedValue;
            UpdateTool(nextBrush);
            ModSettings.SaveSettings();
        }

        public TreeInfo CreateBrushPrefab(List<Tree> trees) {
            var variations = new TreeInfo.Variation[TreeInfos.Count];
            if (TreeInfos.Count == 0) {
                Container.m_variations = variations;
                return Container;
            }
            var probabilities = probabilityCalculator.Calculate(trees);
            for (int i = 0; i < probabilities.Count; i++) {
                var variation = new TreeInfo.Variation();
                variation.m_tree = variation.m_finalTree = TreeInfos[i];
                variation.m_probability = probabilities[i].FloorProbability;
                //TODO not sure if going over the index is a good idea.
                //The calculate method should preserve indizes, but still its kinda risky.
                //alternative to use probabilities.Find(x => x.Name == TreeInfos[i].name); but that is slower
                //another alternative is to return a Dictionary from the Calculate method, but that makes the Calculate method a little more awkward.

                variations[i] = variation;
            }
            Container.m_variations = variations;
            return Container;
        }

        public void UpdateBrushPrefabProbabilities() {
            if (TreeInfos.Count == 0) return;
            var probabilities = probabilityCalculator.Calculate(Trees);
            for (int i = 0; i < probabilities.Count; i++) {
                var variation = Container.m_variations[i];
                variation.m_probability = probabilities[i].FloorProbability;
            }
        }

        internal void UpdateTreeList(TreeInfo treeInfo, bool value, bool updateAll) 
        {    
            if (updateAll) 
            {
                if (value)
                {
                    AddAll();
                }
                else
                {
                    RemoveAll();
                }
            }
            else
            {
                if (value)
                {
                    Add(treeInfo);
                }
                else 
                { 
                    Remove(treeInfo); 
                }
            }
            Container = CreateBrushPrefab(Trees);
            ModSettings.SaveSettings();
        }
    }
}