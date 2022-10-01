﻿using ColossalFramework;
using UnityEngine;

namespace ForestBrush.GUI
{
    //Class by SamsamTS
    public class PreviewRenderer : MonoBehaviour
    {
        private Camera m_camera;
        private float m_rotation = 120f;
        private float m_zoom = 4f;
        
        void Awake()
        {
            m_camera = new GameObject("Camera").AddComponent<Camera>();
            m_camera.transform.SetParent(transform);
            m_camera.backgroundColor = new Color(0, 0, 0, 0);
            m_camera.fieldOfView = 30f;
            m_camera.nearClipPlane = 1f;
            m_camera.farClipPlane = 1000f;
            m_camera.allowHDR = true;
            m_camera.enabled = false;
            m_camera.targetTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            m_camera.pixelRect = new Rect(0f, 0f, 512, 512);
            m_camera.clearFlags = CameraClearFlags.Color;
        }

        public Vector2 size
        {
            get { return new Vector2(m_camera.targetTexture.width, m_camera.targetTexture.height); }
            set
            {
                if (size != value)
                {
                    m_camera.targetTexture = new RenderTexture((int)value.x, (int)value.y, 24, RenderTextureFormat.ARGB32);
                    m_camera.pixelRect = new Rect(0f, 0f, value.x, value.y);
                }
            }
        }

        public Mesh mesh;
        public Material material;

        public RenderTexture texture
        {
            get { return m_camera.targetTexture; }
        }

        public float cameraRotation
        {
            get { return m_rotation; }
            set { m_rotation = value % 360f; }
        }

        public float zoom
        {
            get { return m_zoom; }
            set
            {
                m_zoom = Mathf.Clamp(value, 0.5f, 5f);
            }
        }

        public void Render()
        {
            if (mesh == null) return;

            InfoManager infoManager = Singleton<InfoManager>.instance;
            InfoManager.InfoMode currentMod = infoManager.CurrentMode;
            InfoManager.SubInfoMode currentSubMod = infoManager.CurrentSubMode; ;
            infoManager.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
            infoManager.UpdateInfoMode();

            Light sunLight = DayNightProperties.instance.sunLightSource;
            float lightIntensity = sunLight.intensity;
            Color lightColor = sunLight.color;
            Vector3 lightAngles = sunLight.transform.eulerAngles;

            sunLight.intensity = 2f;
            sunLight.color = Color.white;
            sunLight.transform.eulerAngles = new Vector3(50, 210, 70);

            Light mainLight = RenderManager.instance.MainLight;
            RenderManager.instance.MainLight = sunLight;

            if (mainLight == DayNightProperties.instance.moonLightSource)
            {
                DayNightProperties.instance.sunLightSource.enabled = true;
                DayNightProperties.instance.moonLightSource.enabled = false;
            }

            float magnitude = mesh.bounds.extents.magnitude;
            float num = magnitude + 16f;
            float num2 = magnitude * m_zoom;

            m_camera.transform.position = Vector3.forward * num2;
            m_camera.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
            m_camera.nearClipPlane = Mathf.Max(num2 - num * 1.5f, 0.01f);
            m_camera.farClipPlane = num2 + num * 1.5f;

            Quaternion rotation = Quaternion.Euler(-40f, 180f, 0f) * Quaternion.Euler(0f, m_rotation, 0f);
            Vector3 position = rotation * -mesh.bounds.center;
            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

            Graphics.DrawMesh(mesh, matrix, material, 0, m_camera, 0, null, true, true);
            m_camera.RenderWithShader(material.shader, "");

            sunLight.intensity = lightIntensity;
            sunLight.color = lightColor;
            sunLight.transform.eulerAngles = lightAngles;

            RenderManager.instance.MainLight = mainLight;

            if (mainLight == DayNightProperties.instance.moonLightSource)
            {
                DayNightProperties.instance.sunLightSource.enabled = false;
                DayNightProperties.instance.moonLightSource.enabled = true;
            }

            infoManager.SetCurrentMode(currentMod, currentSubMod);
            infoManager.UpdateInfoMode();
        }
    }
}
