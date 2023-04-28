using System;
using System.Collections.Generic;
using System.Reflection;
using ForestBrushRevisited;
using HarmonyLib;

namespace ForestBrushRevisted.Patching
{
    public static class Patcher {
        public const string HarmonyId = "Sleepy.ForestBrushRevisted";

        private static bool s_patched = false;

        public static void PatchAll() 
        {
            if (!s_patched)
            {
#if DEBUG
                Debug.Log("Patching...");
#endif
                s_patched = true;
                var harmony = new Harmony(HarmonyId);

                List<Type> patchList = new List<Type>();

                // General patches
                patchList.Add(typeof(EscapePatch));

                // Perform the patching
                PatchAll(patchList);
            }
        }

        public static void PatchAll(List<Type> patchList)
        {
#if DEBUG           
            Debug.Log($"Patching:{patchList.Count} functions");
#endif
            var harmony = new Harmony(HarmonyId);

            foreach (var patchType in patchList)
            {
                Patch(harmony, patchType);
            }
        }

        public static void UnpatchAll() {
            if (s_patched)
            {
                var harmony = new Harmony(HarmonyId);
                harmony.UnpatchAll(HarmonyId);
                s_patched = false;
#if DEBUG
                Debug.Log("Unpatching...");
#endif
            }
        }

        public static void Patch(Type patchType)
        {
            Patch(new Harmony(HarmonyId), patchType);
        }

        public static void Unpatch(Type patchType, string sMethod)
        {
            Unpatch(new Harmony(HarmonyId), patchType, sMethod);
        }

        private static void Patch(Harmony harmony, Type patchType)
        {
#if DEBUG
            Debug.Log($"Patch:{patchType}");
#endif
            PatchClassProcessor processor = harmony.CreateClassProcessor(patchType);
            processor.Patch();
        }

        private static void Unpatch(Harmony harmony, Type patchType, string sMethod)
        {
#if DEBUG
            Debug.Log($"Unpatch:{patchType} Method:{sMethod}");
#endif
            MethodInfo info = AccessTools.Method(patchType, sMethod);
            harmony.Unpatch(info, HarmonyPatchType.All, HarmonyId);
        }
    }
}
