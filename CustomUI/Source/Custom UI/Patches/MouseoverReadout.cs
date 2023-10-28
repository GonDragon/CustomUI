using HarmonyLib;
using UnityEngine;
using Verse;

namespace CustomUI.Patches
{
    [HarmonyPatch(typeof(MouseoverReadout))]
    internal class MouseoverReadoutPatches
    {
        private const float barHeight = 35f;

        [HarmonyPatch(nameof(MouseoverReadout.MouseoverReadoutOnGUI))]
        private static bool Prefix(out bool __state)
        {
            if (!Settings.vanillaReadout)
            {
                __state = false;
                return false;
            }
            __state = true;

            float deltaH = Utility.CustomToolbar.TabsOnBottom ? 0 : barHeight;

            GUI.BeginGroup(new Rect(0f, 0f + deltaH, UI.screenWidth, UI.screenHeight - deltaH));
            //GUI.BeginGroup(new Rect(0f, 0f, UI.screenWidth, UI.screenHeight));
            return true;
        }

        [HarmonyPatch(nameof(MouseoverReadout.MouseoverReadoutOnGUI))]
        private static void Postfix(bool __state)
        {
            if (__state)
            {
                GUI.EndGroup();
            }
        }
    }
}