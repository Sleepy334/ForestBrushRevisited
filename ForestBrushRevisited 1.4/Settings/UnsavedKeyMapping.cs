﻿using ColossalFramework;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using TransferManagerCE.Settings;
using UnityEngine;

namespace ForestBrushRevisited.Settings
{
    public class UnsavedKeyMapping : SavedInputKey
    {
        public UnsavedKeyMapping(string keyName, KeyCode key, bool bCtrl, bool bShift, bool bAlt)
            : base(keyName, "ModName", key, bCtrl, bShift, bAlt, autoUpdate: false)
        {
            m_Synced = true;
        }

        public UnsavedKeyMapping(string keyName, InputKey key)
            : base(keyName, "ModName", key, autoUpdate: false)
        {
            m_Synced = true;
        }

        public UnsavedKeyMapping(XmlInputKey xmlKey)
            : base(xmlKey.Name, "ModName", xmlKey.Key, control: xmlKey.Control, shift: xmlKey.Shift, alt: xmlKey.Alt, autoUpdate: false)
        {
            m_Synced = true;
        }

        public XmlInputKey XmlKey
        {
            get => new XmlInputKey(name, Key, Control, Shift, Alt);
        }
    }
}
