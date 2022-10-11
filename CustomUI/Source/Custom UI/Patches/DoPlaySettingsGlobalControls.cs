using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace CustomUI.Patches
{
    internal class DoPlaySettingsGlobalControlsPatch
    {
        public static void Postfix(WidgetRow row, bool worldView)
        {
            if (worldView)
            {
            }
            else
            {
                row.ToggleableIcon(ref UIManager.editionModeEnabled, Textures.toggleIcon, "CustomUI.Playsetting.Tooltip".Translate(), SoundDefOf.Mouseover_ButtonToggle);
                //CheckKeyBindingToggle(KeybindingDefOf.ToggleElectricGrid, ref CustomPowerOverlay.enabled);
            }
        }
    }
}
