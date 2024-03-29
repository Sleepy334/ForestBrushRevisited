﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ForestBrushRevisited.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnifiedUI.Helpers;
using UnityEngine;

namespace ForestBrushRevisited
{
    public class ForestTool : ToolBase
    {
        private static readonly string kCursorInfoNormalColor = "<color #87d3ff>";
        private static readonly string kCursorInfoCloseColorTag = "</color>";
        private float Angle = ToolsModifierControl.cameraController.m_currentAngle.x;
        private bool AxisChanged;
        private float MouseRayLength;
        private bool MouseLeftDown;
        private bool MouseRightDown;
        private bool MouseRayValid;
        private ToolErrors Errors;
        private Ray MouseRay;
        private Vector3 CachedPosition;
        private Vector3 MousePosition;
        private Vector3 BrushPosition;
        private Randomizer Randomizer;
        private static bool s_bLoadingTool = false;
        private UIComponent? m_button = null;
        private float Size => Options.Size;
        private float Strength => Options.Strength;
        private float Density => Options.Density;
        private int TreeCount
        {
            get
            {
                return ForestBrush.Instance.BrushContainer.Container == null || ForestBrush.Instance.BrushContainer.Container.m_variations == null ? 0 : ForestBrush.Instance.BrushContainer.Container.m_variations.Length;
            }
        }

        private Brush.BrushOptions Options { get => ModSettings.Settings.SelectedBrush.Options; set => ModSettings.Settings.SelectedBrush.Options = value; }

        private bool ShiftDown => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        private bool AltDown => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        private bool CtrlDown => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                              || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);

        private bool Painting => MouseLeftDown && !CtrlDown && !AltDown && !MouseRightDown;
        private bool Deleting => MouseRightDown && !CtrlDown && !AltDown && !MouseLeftDown;
        private bool DensityOrRotation => CtrlDown && !ShiftDown && !AltDown && !MouseLeftDown;
        private bool SelectiveDelete => MouseRightDown && ShiftDown && !CtrlDown && !AltDown && !MouseLeftDown;
        private bool SizeAndStrength => AltDown && !CtrlDown && !ShiftDown && !MouseLeftDown && !MouseRightDown;
        private float[] BrushData;

        public int ID_Angle { get; private set; }
        public int ID_BrushTex { get; private set; }
        public int ID_BrushWS { get; private set; }
        public int ID_Color1 { get; private set; }
        public int ID_Color2 { get; private set; }

        private int RandomModifier { get; set; } = 1;

        public Material BrushMaterial { get; private set; }
        private Mesh BoxMesh { get; set; }
        private Dictionary<string, Texture2D> Brushes { get; set; }
        private Shader Shader => ResourceLoader.Shader;

        public Texture2D BrushTexture { get; set; }

        public static ForestTool AddSelectionTool()
        {
            ForestTool? tool = null;
            try
            {
                s_bLoadingTool = true;

                // Has tool already been loaded
                foreach (ToolBase addedTool in ToolsModifierControl.toolController.Tools)
                {
                    if (addedTool is ForestTool)
                    {
                        tool = (ForestTool) addedTool;
                        break;
                    }
                }

                // Otherwise create it
                if (tool == null)
                {
                    tool = ToolsModifierControl.toolController.gameObject.AddComponent<ForestTool>();
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Selection tool failed to load: {e}");
            }
            finally
            {
                s_bLoadingTool = false;
            }
            return tool;
        }

        protected override void Awake() {
            base.Awake();
            enabled = false;
            BoxMesh = RenderManager.instance.OverlayEffect.GetType().GetField("m_boxMesh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(RenderManager.instance.OverlayEffect) as Mesh;
            BrushData = new float[128 * 128];
            Brushes = ResourceLoader.LoadBrushTextures();
            ID_BrushTex = Shader.PropertyToID("_BrushTex");
            ID_BrushWS = Shader.PropertyToID("_BrushWS");
            ID_Angle = Shader.PropertyToID("_Angle");
            ID_Color1 = Shader.PropertyToID("_TerrainBrushColor1");
            ID_Color2 = Shader.PropertyToID("_TerrainBrushColor2");
            BrushMaterial = new Material(ToolsModifierControl.toolController.m_brushMaterial) { shader = Shader };
            Randomizer = new Randomizer((int)DateTime.Now.Ticks);

            FieldInfo fieldInfo = typeof(ToolController).GetField("m_tools", BindingFlags.Instance | BindingFlags.NonPublic);
            ToolBase[]? tools = (ToolBase[])fieldInfo.GetValue(ToolsModifierControl.toolController);
            if (tools != null)
            {
                // Add 
                Dictionary<Type, ToolBase>? dictionary = (Dictionary<Type, ToolBase>)typeof(ToolsModifierControl).GetField("m_Tools", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                if (dictionary != null)
                {
                    // Add or update tools dictionary
                    if (!dictionary.ContainsKey(typeof(ForestTool)))
                    {
                        // Add reference
                        ToolsModifierControl.SetTool<DefaultTool>();
                        int initialLength = tools.Length;
                        Array.Resize(ref tools, initialLength + 1);
                        dictionary.Add(typeof(ForestTool), this);
                        tools[initialLength] = this;
                    }
                    else
                    {
                        // Update reference
                        dictionary[typeof(ForestTool)] = this;

                        // Tool must already exist
                        int iIndex = -1;
                        foreach (ToolBase existing in tools)
                        {
                            if (existing is ForestBrush)
                            {
                                break;
                            }
                            iIndex++;
                        }

                        // Check we found it
                        if (iIndex >= 0) 
                        {
                            // Update tool reference
                            tools[iIndex] = this;
                        }
                    }

                    fieldInfo.SetValue(ToolsModifierControl.toolController, tools);
                    ToolsModifierControl.SetTool<DefaultTool>();
                }
            }

            if (DependencyUtils.IsUnifiedUIRunning())
            {
                Texture2D? icon = TextureResources.LoadDllResource("ForestBrushNormal.png", 32, 32);
                if (icon is null)
                {
                    Debug.LogError("Failed to load icon from resources");
                    return;
                }

                m_button = UUIHelpers.RegisterToolButton(
                    name: "ForestBrushRevisited",
                    groupName: null,
                    tooltip: ForestBrushRevisitedMod.Title,
                    tool: this,
                    icon: icon,
                    hotkeys: new UUIHotKeys { ActivationKey = ModSettings.Settings.ToggleTool });
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            try 
            {
                FieldInfo fieldInfo = typeof(ToolController).GetField("m_tools", BindingFlags.Instance | BindingFlags.NonPublic);
                List<ToolBase> tools = ((ToolBase[])fieldInfo.GetValue(ToolsModifierControl.toolController)).ToList();
                tools.Remove(this);
                fieldInfo.SetValue(ToolsModifierControl.toolController, tools.ToArray());
                Dictionary<Type, ToolBase> dictionary = (Dictionary<Type, ToolBase>)typeof(ToolsModifierControl).GetField("m_Tools", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                dictionary.Remove(typeof(ForestTool));
                if (BrushMaterial != null) 
                {
                    Destroy(BrushMaterial);
                    BrushMaterial = null;
                }
            } 
            catch (Exception exception) 
            {
                Debug.LogWarning($"Exception caught: {exception}");
            }
        }

        protected override void OnEnable() {
            base.OnEnable();

            ForestBrush.Instance.ShowPanel();

            if (!s_bLoadingTool)
            {
                this.m_toolController.ClearColliding();
                ToolBase.cursorInfoLabel.textAlignment = UIHorizontalAlignment.Left;
                SetBrush(Brushes.FirstOrDefault(b => b.Key == Options.BitmapID).Value);
                ClampAngle();
            }
        }

        protected override void OnDisable() {
            base.OnDisable();

            ForestBrush.Instance.HidePanel();

            SetBrush(string.Empty);
            MouseLeftDown = false;
            MouseRightDown = false;
            MouseRayValid = false;
            ToolBase.cursorInfoLabel.textAlignment = UIHorizontalAlignment.Center;
        }

        public Dictionary<string, Texture2D> GetBrushes() 
        {
            return Brushes;
        }

        public void SetBrush(string id) 
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!Brushes.TryGetValue(id, out Texture2D brush)) return;
            SetBrush(brush);
            Options.BitmapID = id;
            ModSettings.Settings.Save();
        }

        private void SetBrush(Texture2D brush) 
        {
            BrushTexture = brush;
            if (brush == null) return;
            for (var i = 0; i < 128; i++) {
                for (var j = 0; j < 128; j++) {
                    BrushData[i * 128 + j] = brush.GetPixel(j, i).a;
                }
            }
        }

        private bool IsInsideUI()
        {
            if (m_toolController.IsInsideUI)
            {
                if (UIInput.hoveredComponent != null)
                {
                    // We seem to get back an invisible hoveredComponent where the road menu appears
                    // ignore this so we can still use the brush in this area
                    return UIInput.hoveredComponent.name != ForestBrush.kToggleButton + "Panel";
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        protected override void OnToolGUI(Event e) 
        {
            const int iLeftMouseButton = 0;
            const int iRightMouseButton = 1;

            switch (e.type)
            {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    {
                        if (IsInsideUI() || (ForestBrush.Instance != null && ForestBrushPanel.Instance.IsDragging()))
                        {
                            return;
                        }

                        if (e.button == iLeftMouseButton)
                        {
                            MouseLeftDown = true;

                            // Place single tree if neeeded
                            PlaceSingleTree(e.type == EventType.MouseDrag);
                        }
                        else if (e.button == iRightMouseButton)
                        {
                            MouseRightDown = true;
                            AxisChanged = false;

                            // Cycle through trees in selected brush
                            if (Size == 1.0f && TreeCount > 0)
                            {
                                RandomModifier += 1;
                            }
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (e.button == iLeftMouseButton)
                        {
                            MouseLeftDown = false;
                        }
                        else if (e.button == iRightMouseButton)
                        {
                            if (!AxisChanged && DensityOrRotation)
                            {
                                Rotate45();
                            }

                            MouseRightDown = false;
                        }
                        AxisChanged = false;
                        break;
                    }
            } 
        }

        void PlaceSingleTree(bool bDragging)
        {
            if (Size == 1.0f && TreeCount > 0)
            {
                // Apply strength to placing of trees
                // Holding SHIFT sets strength to 100%
                int iStrength = ShiftDown ? 100 : (int)Mathf.Clamp(Strength * 100.0f, 1.0f, 100.0f);
                int iRandom = SimulationManager.instance.m_randomizer.Int32(100U);

                if (!bDragging || iRandom < iStrength)
                {
                    // Create tree
                    SimulationManager.instance.AddAction(() => CreateTree(MousePosition));
                }
            }
        }

        void CreateTree(Vector3 position)
        {
            if (Errors == ToolErrors.None)
            {
                int iSeed = TreeManager.instance.m_treeCount + RandomModifier;
                Randomizer random = new Randomizer(iSeed);
                TreeInfo treeInfo = ForestBrush.Instance.BrushContainer.Container.GetVariation(ref random);

                bool success = true;
                bool needMoney = ModSettings.Settings.ChargeMoney && (ToolManager.instance.m_properties.m_mode & ItemClass.Availability.Game) != 0;
                if (needMoney)
                {
                    int cost = treeInfo.GetConstructionCost();
                    success = (cost == 0 || cost == EconomyManager.instance.FetchResource(EconomyManager.Resource.Construction, cost, treeInfo.m_class));
                }
                if (success)
                {
                    if (TreeManager.instance.CreateTree(out uint tree, ref Randomizer, treeInfo, position, true) && ModSettings.Settings.PlayEffect)
                    {
                        TreeTool.DispatchPlacementEffect(position, false);
                    }
                }
            }
        }

        public override ToolErrors GetErrors() {
            return Errors;
        }

        protected override void OnToolUpdate() {
            if (ForestBrush.Instance.BrushContainer.Container is null) return;
            if (MouseRayValid) {
                if (ModSettings.Settings.ShowInfoTooltip && (CtrlDown || AltDown)) {
                    string density = Options.AutoDensity ? "Auto" : string.Concat(Math.Round((16 - Options.Density) * 6.451612903225806f, 1, MidpointRounding.AwayFromZero), "%");
                    string text = $"Trees: {ForestBrush.Instance.BrushContainer.Container.m_variations.Length}\nSize: {Options.Size}\nStrength: { Math.Round(Options.Strength * 100, 1) + "%"}\nDensity: {density}";
                    ShowInfo(true, text);
                } else if (Size == 1.0f) {
                    if (TreeCount > 0) {
                        Randomizer tmp = new Randomizer(TreeManager.instance.m_treeCount + RandomModifier);
                        int cost = ForestBrush.Instance.BrushContainer.Container.GetVariation(ref tmp).GetConstructionCost();
                        if (ModSettings.Settings.ChargeMoney && cost != 0) {
                            string text = StringUtils.SafeFormat(Locale.Get(LocaleID.TOOL_CONSTRUCTION_COST), cost / 100);
                            ShowToolInfo(true, text, MousePosition);
                        } else {
                            ShowToolInfo(true, null, MousePosition);
                        }
                    } else {
                        string text = Localization.Get("FOREST-BRUSH-TOOLERROR-BRUSHEMPTY");
                        ShowToolInfo(true, text, MousePosition);
                    }
                } else ShowToolInfo(false, null, CachedPosition);
            } else ShowToolInfo(false, null, CachedPosition);
            if ((DensityOrRotation || SizeAndStrength)) {
                float axisX = Input.GetAxis("Mouse X");
                float axisY = Input.GetAxis("Mouse Y");
                if (axisX != 0 || axisY != 0) {
                    if (Mathf.Abs(axisX) > Mathf.Abs(axisY)) {
                        AxisChanged = true;
                        if (DensityOrRotation) {
                            DeltaAngle(axisX * 10.0f);
                        } else if (SizeAndStrength) {
                            Options.Size = Mathf.Clamp((float)Math.Round(Options.Size + axisX * (Tweaker.MaxSize / 50.0f), 1), 1.0f, Tweaker.MaxSize);
                        }
                    } else if (Mathf.Abs(axisY) > Mathf.Abs(axisX)) {
                        AxisChanged = true;
                        if (DensityOrRotation) {
                            Options.Density = Mathf.Clamp(Options.Density - axisY, 0.5f, 16.0f);
                        } else if (SizeAndStrength) {
                            Options.Strength = Mathf.Clamp(Options.Strength + axisY * 0.1f, 0.01f, 1.0f);
                        }
                    }
                }
            }

            ForestBrushPanel.Instance.BrushOptionsSection.UpdateBindings(Options);
        }

        protected void ShowInfo(bool show, string text) {
            if (ToolBase.cursorInfoLabel == null) {
                return;
            }
            if (!string.IsNullOrEmpty(text) && show) {
                text = kCursorInfoNormalColor + text + kCursorInfoCloseColorTag;
                ToolBase.cursorInfoLabel.isVisible = true;
                UIView uiview = ToolBase.cursorInfoLabel.GetUIView();
                Vector2 vector = (!(ToolBase.fullscreenContainer != null)) ? uiview.GetScreenResolution() : ToolBase.fullscreenContainer.size;
                Vector3 relativePosition = ForestBrushPanel.Instance.absolutePosition + new Vector3(410.0f, 0.0f);
                ToolBase.cursorInfoLabel.text = text;
                if (relativePosition.x < 0f) {
                    relativePosition.x = 0f;
                }
                if (relativePosition.y < 0f) {
                    relativePosition.y = 0f;
                }
                if (relativePosition.x + ToolBase.cursorInfoLabel.width > vector.x) {
                    relativePosition.x = vector.x - ToolBase.cursorInfoLabel.width;
                }
                if (relativePosition.y + ToolBase.cursorInfoLabel.height > vector.y) {
                    relativePosition.y = vector.y - ToolBase.cursorInfoLabel.height;
                }
                ToolBase.cursorInfoLabel.relativePosition = relativePosition;
            } else {
                ToolBase.cursorInfoLabel.isVisible = false;
            }
        }

        protected override void OnToolLateUpdate() {
            MouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            MouseRayLength = Camera.main.farClipPlane;
            MouseRayValid =  (!IsInsideUI() && Cursor.visible);
            CachedPosition = MousePosition;
            BrushPosition = MousePosition;
        }

        public override void SimulationStep() {
            if (ForestBrush.Instance.BrushContainer.Container is null)
            {
                return;
            }

            ulong[] collidingSegmentBuffer;
            ulong[] collidingBuildingBuffer;
            m_toolController.BeginColliding(out collidingSegmentBuffer, out collidingBuildingBuffer);
            try {
                RaycastInput input = new RaycastInput(MouseRay, MouseRayLength);
                if (RayCast(input, out RaycastOutput output)) {
                    MousePosition = output.m_hitPos;
                    if (Size > 1.0f) 
                    {
                        if (MouseLeftDown != MouseRightDown) ApplyBrush();
                    } 
                    else 
                    {
                        Randomizer tmp = Randomizer;
                        Randomizer tmp2 = new Randomizer(TreeManager.instance.m_treeCount + RandomModifier);
                        uint item = TreeManager.instance.m_trees.NextFreeItem(ref tmp);
                        TreeInfo treeInfo = ForestBrush.Instance.BrushContainer.Container.GetVariation(ref tmp2);
                        ToolErrors errors = TreeTool.CheckPlacementErrors(treeInfo, output.m_hitPos, output.m_currentEditObject, item, collidingSegmentBuffer, collidingBuildingBuffer);
                        bool needMoney = ModSettings.Settings.ChargeMoney && (ToolManager.instance.m_properties.m_mode & ItemClass.Availability.Game) != 0;
                        if (needMoney) 
                        {
                            int cost = treeInfo.GetConstructionCost();
                            if (cost != 0) 
                            {
                                if (cost != EconomyManager.instance.PeekResource(EconomyManager.Resource.Construction, cost)) 
                                {
                                    errors |= ToolErrors.NotEnoughMoney;
                                }
                            }
                        }
                        if (!TreeManager.instance.CheckLimits())
                        {
                            errors |= ToolErrors.TooManyObjects;
                        }
                        MousePosition = output.m_hitPos;
                        Errors = errors;
                    }
                } 
                else 
                {
                    Errors = ToolErrors.RaycastFailed;
                }
            } 
            finally 
            {
                m_toolController.EndColliding();
            }
        }

        private void ApplyBrush() {
            if (ForestBrush.Instance.BrushContainer.Container is null)
            {
                return;
            }
            if (Painting && TreeCount > 0)
            {
                AddTreesImpl();
            }
            else if (Deleting && !AxisChanged)
            {
                RemoveTreesImpl();
            }
        }

        private void AddTreesImpl() {
            int batchSize = (int)Size * Tweaker.SizeMultiplier + Tweaker.SizeAddend;
            for (int i = 0; i < batchSize; i++) {
                float brushRadius = Size / 2;
                Vector2 randomPosition = UnityEngine.Random.insideUnitCircle;
                Vector3 treePosition = MousePosition + new Vector3(randomPosition.x, 0f, randomPosition.y) * Size;


                var distance = treePosition - MousePosition;
                var distanceRotated = Quaternion.Euler(0, Angle, 0) * distance;

                float brushZ = (distanceRotated.z + brushRadius) / Size * 128.0f - 0.5f;
                int z0 = Mathf.Clamp(Mathf.FloorToInt(brushZ), 0, 127);
                int z1 = Mathf.Clamp(Mathf.CeilToInt(brushZ), 0, 127);

                float brushX = (distanceRotated.x + brushRadius) / Size * 128.0f - 0.5f;
                int x0 = Mathf.Clamp(Mathf.FloorToInt(brushX), 0, 127);
                int x1 = Mathf.Clamp(Mathf.CeilToInt(brushX), 0, 127);

                float brush00 = BrushData[z0 * 128 + x0];
                float brush10 = BrushData[z0 * 128 + x1];
                float brush01 = BrushData[z1 * 128 + x0];
                float brush11 = BrushData[z1 * 128 + x1];

                float brush0 = brush00 + (brush10 - brush00) * (brushX - x0);
                float brush1 = brush01 + (brush11 - brush01) * (brushX - x0);
                float brush = brush0 + (brush1 - brush0) * (brushZ - z0);

                int change = (int)(Strength * (brush * 1.2f - 0.2f) * 10000.0f);

                if (Randomizer.Int32(10000) < change) {
                    TreeInfo treeInfo = ForestBrush.Instance.BrushContainer.Container.GetVariation(ref Randomizer);

                    treePosition.y = Singleton<TerrainManager>.instance.SampleDetailHeight(treePosition, out float f, out float f2);
                    float spacing = Options.AutoDensity ? treeInfo.m_generatedInfo.m_size.x * Tweaker.SpacingFactor : Density;
                    Randomizer tempRandomizer = Randomizer;
                    uint item = TreeManager.instance.m_trees.NextFreeItem(ref tempRandomizer);
                    Randomizer treeRandomizer = new Randomizer(item);
                    float scale = treeInfo.m_minScale + (float)treeRandomizer.Int32(10000u) * (treeInfo.m_maxScale - treeInfo.m_minScale) * 0.0001f;
                    float height = treeInfo.m_generatedInfo.m_size.y * scale;
                    float clearance = Tweaker.Clearance;
                    Vector2 treePosition2 = VectorUtils.XZ(treePosition);
                    Quad2 clearanceQuad = default(Quad2);
                    clearanceQuad.a = treePosition2 + new Vector2(-clearance, -clearance);
                    clearanceQuad.b = treePosition2 + new Vector2(-clearance, clearance);
                    clearanceQuad.c = treePosition2 + new Vector2(clearance, clearance);
                    clearanceQuad.d = treePosition2 + new Vector2(clearance, -clearance);
                    Quad2 spacingQuad = default(Quad2);
                    spacingQuad.a = treePosition2 + new Vector2(-spacing, -spacing);
                    spacingQuad.b = treePosition2 + new Vector2(-spacing, spacing);
                    spacingQuad.c = treePosition2 + new Vector2(spacing, spacing);
                    spacingQuad.d = treePosition2 + new Vector2(spacing, -spacing);
                    float minY = MousePosition.y;
                    float maxY = MousePosition.y + height;
                    ItemClass.CollisionType collisionType = ItemClass.CollisionType.Terrain;

                    if (PropManager.instance.OverlapQuad(clearanceQuad, minY, maxY, collisionType, 0, 0) && !ShiftDown) continue;
                    if (TreeManager.instance.OverlapQuad(spacingQuad, minY, maxY, collisionType, 0, 0)) continue;
                    if (NetManager.instance.OverlapQuad(clearanceQuad, minY, maxY, collisionType, treeInfo.m_class.m_layer, 0, 0, 0)) continue;
                    if (BuildingManager.instance.OverlapQuad(clearanceQuad, minY, maxY, collisionType, treeInfo.m_class.m_layer, 0, 0, 0)) continue;
                    if (TerrainManager.instance.HasWater(treePosition2) && !ShiftDown) continue;
                    int noiseScale = Randomizer.Int32(16);
                    float str2Rnd = UnityEngine.Random.Range(0.0f, Tweaker.MaxRandomRange);
                    if (Mathf.PerlinNoise(treePosition.x * noiseScale, treePosition.y * noiseScale) > 0.5 && str2Rnd < Strength * Tweaker.StrengthMultiplier) {
                        if (Singleton<TreeManager>.instance.CreateTree(out uint num25, ref Randomizer, treeInfo, treePosition, false)) {
                            if (ModSettings.Settings.ChargeMoney) EconomyManager.instance.FetchResource(EconomyManager.Resource.Construction, 1000, treeInfo.m_class);
                        }
                    }
                }
            }
        }

        private void RemoveTreesImpl() {
            float brushRadius = Size / 2;
            float cellSize = TreeManager.TREEGRID_CELL_SIZE;
            int resolution = TreeManager.TREEGRID_RESOLUTION;
            TreeInstance[] trees = TreeManager.instance.m_trees.m_buffer;
            uint[] treeGrid = TreeManager.instance.m_treeGrid;
            float strength = Strength;
            Vector3 position = MousePosition;

            int minX = Mathf.Max((int)((position.x - Size) / cellSize + resolution * 0.5f), 0);
            int minZ = Mathf.Max((int)((position.z - Size) / cellSize + resolution * 0.5f), 0);
            int maxX = Mathf.Min((int)((position.x + Size) / cellSize + resolution * 0.5f), resolution - 1);
            int maxZ = Mathf.Min((int)((position.z + Size) / cellSize + resolution * 0.5f), resolution - 1);

            for (int z = minZ; z <= maxZ; ++z) {
                for (int x = minX; x <= maxX; ++x) {
                    uint treeIndex = treeGrid[z * resolution + x];

                    while (treeIndex != 0) {
                        uint next = trees[treeIndex].m_nextGridTree;

                        Vector3 treePosition = TreeManager.instance.m_trees.m_buffer[treeIndex].Position;

                        var distance = treePosition - position;
                        var distanceRotated = Quaternion.Euler(0, Angle, 0) * distance;

                        float brushZ = (distanceRotated.z + brushRadius) / Size * 128.0f - 0.5f;
                        int z0 = Mathf.Clamp(Mathf.FloorToInt(brushZ), 0, 127);
                        int z1 = Mathf.Clamp(Mathf.CeilToInt(brushZ), 0, 127);

                        float brushX = (distanceRotated.x + brushRadius) / Size * 128.0f - 0.5f;
                        int x0 = Mathf.Clamp(Mathf.FloorToInt(brushX), 0, 127);
                        int x1 = Mathf.Clamp(Mathf.CeilToInt(brushX), 0, 127);

                        float brush00 = BrushData[z0 * 128 + x0];
                        float brush10 = BrushData[z0 * 128 + x1];
                        float brush01 = BrushData[z1 * 128 + x0];
                        float brush11 = BrushData[z1 * 128 + x1];

                        float brush0 = brush00 + (brush10 - brush00) * (brushX - x0);
                        float brush1 = brush01 + (brush11 - brush01) * (brushX - x0);
                        float brush = brush0 + (brush1 - brush0) * (brushZ - z0);

                        int change = (int)(strength * (brush * 1.2f - 0.2f) * 10000.0f);

                        if (Randomizer.Int32(10000) < change) {
                            var noiseScale = Randomizer.Int32(Tweaker.NoiseScale);
                            var strengthToRandom = UnityEngine.Random.Range(0.0f, Tweaker.MaxRandomRange);
                            TreeInfo treeInfo = TreeManager.instance.m_trees.m_buffer[treeIndex].Info;
                            if ((SelectiveDelete && ForestBrush.Instance.BrushContainer.TreeInfos.Contains(treeInfo))
                            || !SelectiveDelete) {
                                Vector2 xzTreePosition = VectorUtils.XZ(treePosition);
                                Vector2 xzMousePosition = VectorUtils.XZ(MousePosition);
                                if ((xzMousePosition - xzTreePosition).sqrMagnitude <= Size * Size) TreeManager.instance.ReleaseTree(treeIndex);
                            }
                        }
                        treeIndex = next;
                    }
                }
            }
        }

        public override void RenderGeometry(RenderManager.CameraInfo cameraInfo) {
            if (!MouseRayValid || ForestBrush.Instance.BrushContainer.Container is null) {
                base.RenderGeometry(cameraInfo);
                return;
            }

            Randomizer tmp = new Randomizer(TreeManager.instance.m_treeCount + RandomModifier);
            TreeInfo info = ForestBrush.Instance.BrushContainer.Container.GetVariation(ref tmp);
            if (!(info is null) && Size == 1.0f && Errors == ToolErrors.None && TreeCount > 0) {
                Randomizer tmp2 = Randomizer;
                uint item = TreeManager.instance.m_trees.NextFreeItem(ref tmp2);

                Randomizer r = new Randomizer(item);
                float scale = info.m_minScale + r.Int32(10000) * (info.m_maxScale - info.m_minScale) * 0.0001f;
                float brightness = info.m_minBrightness + r.Int32(10000) * (info.m_maxBrightness - info.m_minBrightness) * 0.0001f;

                TreeInstance.RenderInstance(null, info, MousePosition, scale, brightness, RenderManager.DefaultColorLocation, false);
            }

            base.RenderGeometry(cameraInfo);
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            if (!MouseRayValid)
            {
                return;
            }

            if (ForestBrush.Instance.BrushContainer.Container is null)
            {
                Debug.LogError("ForestBrush.Instance.Container is null");
                return;
            }

            Randomizer tmp = new Randomizer(TreeManager.instance.m_treeCount + RandomModifier);
            TreeInfo info = ForestBrush.Instance.BrushContainer.Container.GetVariation(ref tmp);

            if (info is not null && Size == 1.0f) 
            {
                RenderSingleTree(cameraInfo, info);
            } 
            else 
            {
                RenderBrush(cameraInfo);
            }
        }

        private void RenderSingleTree(RenderManager.CameraInfo cameraInfo, TreeInfo info)
        {
            Color color = TreeCount == 0 ? m_toolController.m_errorColorInfo : GetToolColor(false, Errors != ToolErrors.None);
            m_toolController.RenderColliding(cameraInfo, color, color, color, color, 0, 0);

            Randomizer tmp2 = Randomizer;
            uint item = TreeManager.instance.m_trees.NextFreeItem(ref tmp2);

            Randomizer r = new Randomizer(item);
            float scale = info.m_minScale + r.Int32(10000) * (info.m_maxScale - info.m_minScale) * 0.0001f;
            TreeTool.RenderOverlay(cameraInfo, info, MousePosition, scale, color);
        }

        private void RenderBrush(RenderManager.CameraInfo cameraInfo) 
        {
            if (BrushTexture != null) 
            {
                Vector4 position = BrushPosition;
                position.w = Size;
                BrushMaterial.SetTexture(ID_BrushTex, BrushTexture);
                BrushMaterial.SetVector(this.ID_BrushWS, position);
                BrushMaterial.SetFloat(ID_Angle, Angle);

                Vector3 center = new Vector3(BrushPosition.x, 512f, BrushPosition.z);
                Vector3 size = new Vector3(Size, 1224f, Size);
                Bounds bounds = new Bounds(center, size * 1.5f);

                // Render brush
                Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
                RenderManager.instance.OverlayEffect.DrawEffect(cameraInfo, BrushMaterial, 0, bounds);
            }
            else
            {
                Debug.LogError("BrushTexture is null");
            }
        }

        private void Rotate45() {
            Angle = Mathf.Round(Angle / 45f - 1f) * 45f;
            ClampAngle();
        }

        private void ClampAngle() {
            if (Angle < 0f) 
            {
                Angle += 360f;
            }
            if (Angle >= 360f) 
            {
                Angle -= 360f;
            }
        }

        private void DeltaAngle(float delta) {
            Angle += delta;
            ClampAngle();
        }

        public class Tweaks
        {
            public int SizeAddend;
            public int SizeMultiplier;
            public uint NoiseScale;
            public float NoiseThreshold;
            public float MaxRandomRange;
            public float Clearance;
            public float SingleTreeClearance;
            public float SpacingFactor;
            public float StrengthMultiplier;
            public float _maxSize;
            public float MaxSize {
                get {
                    return _maxSize;
                }
                set {
                    _maxSize = value;
                    ForestBrushPanel.Instance.BrushOptionsSection.sizeSlider.maxValue = value;
                }
            }
        }

        public Tweaks Tweaker = new Tweaks() {
            SizeAddend = 10,
            SizeMultiplier = 7,
            NoiseScale = 16,
            NoiseThreshold = 0.5f,
            MaxRandomRange = 4f,
            Clearance = 4.5f,
            SingleTreeClearance = 0.3f,
            SpacingFactor = 0.6f,
            StrengthMultiplier = 10,
            _maxSize = 1000f
        };
    }
}
