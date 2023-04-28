﻿using ForestBrushRevisited;
using System;
using System.Diagnostics;
using System.Reflection;

namespace ForestBrushRevisited
{
    public static class Debug
    {
        public static void Log(string sText)
        {
            UnityEngine.Debug.Log($"[{Constants.ModName}] {GetCallingFunction()}: {sText}");
        }

        public static void LogWarning(string sText)
        {
            UnityEngine.Debug.LogWarning($"[{Constants.ModName}] {GetCallingFunction()}: {sText}");
        }

        public static void LogError(string sText)
        {
            UnityEngine.Debug.LogError($"[{Constants.ModName}] {GetCallingFunction()}: {sText}");
        }

        public static void Log(Exception ex)
        {
            LogError("");
            UnityEngine.Debug.LogException(ex);
        }

        public static void Log(string sText, Exception ex)
        {
            LogError(sText);
            UnityEngine.Debug.LogException(ex);
            if (ex.InnerException is not null)
            {
                UnityEngine.Debug.LogException(ex.InnerException);
            }
        }

        public static string GetCallingFunction()
        {
            StackTrace stackTrace = new StackTrace();
            if (stackTrace.FrameCount >= 3)
            {
                StackFrame frame = stackTrace.GetFrame(2);
                MethodBase method = frame.GetMethod();
                var Class = method.ReflectedType;
                var Namespace = Class.Namespace;
                return Namespace + "." + Class.Name + "." + method.Name;
            }
            return "Unknown";
        }
    }
}
