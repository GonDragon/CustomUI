using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace CustomUI.Patches
{
    [HarmonyPatch(typeof(GlobalControlsUtility))]
    internal class GlobalControlsUtilityPatches
    {
        private const float barHeight = 35f;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GlobalControlsUtility.DoPlaySettings)), HarmonyPriority(Priority.Low)]
        private static bool DoPlaySettingsPatch(WidgetRow rowVisibility, bool worldView, ref float curBaseY)
        {
            //if (Settings.TabsOnBottom) curBaseY -= UIManager.ExtendedBarHeight;
            //if (Settings.useDesignatorBar && !Settings.designationsOnLeft && !worldView) curBaseY -= 88f;
            //if (Settings.togglersOnTop) curBaseY += Widget.ExtendedToolbar.Height;

            curBaseY -= barHeight;

            if (Settings.togglersOnTop) curBaseY += barHeight;
            float borderGap = 4f;
            //float initialY = Settings.togglersOnTop ? (Settings.TabsOnTop ? UIManager.ExtendedBarHeight + borderGap : borderGap) : curBaseY;
            float initialY = Settings.togglersOnTop ? borderGap : curBaseY;
            rowVisibility.Init((float)UI.screenWidth - borderGap, initialY, Settings.togglersOnTop ? UIDirection.LeftThenDown : UIDirection.LeftThenUp, Settings.togglersOnTop ? 250f : 180f);
            Find.PlaySettings.DoPlaySettingsGlobalControls(rowVisibility, worldView);
            if (!Settings.togglersOnTop) curBaseY = rowVisibility.FinalY;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GlobalControlsUtility.DoTimespeedControls)), HarmonyPriority(Priority.Low)]
        private static bool DoTimespeedControls_Patch(ref float curBaseY)
        {
            if (Settings.vanillaControlSpeed) return true;
            curBaseY += 4f;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GlobalControlsUtility.DoDate)), HarmonyPriority(Priority.Low)]
        private static bool DoDate_Patch()
        {
            return Settings.vanillaDate;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GlobalControlsUtility.DoRealtimeClock)), HarmonyPriority(Priority.Low)]
        private static bool DoRealtimeClock_Patch(ref float curBaseY)
        {
            if (Settings.vanillaRealtime) return true;
            curBaseY += 4f;
            return false;
        }
    }
}
