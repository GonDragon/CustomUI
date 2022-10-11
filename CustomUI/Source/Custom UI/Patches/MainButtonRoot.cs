using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace CustomUI.Patches
{
    //[HarmonyPatch(typeof(MainButtonsRoot), "MainButtonsOnGUI")]
    //internal class MainButtonRoot_Patches
    //{
    //    public static bool Prefix(List<MainButtonDef> ___allButtonsInOrder)
    //    {

    //        float num1 = 0.0f;
    //        for (int index = 0; index < ___allButtonsInOrder.Count; ++index)
    //        {
    //            if (___allButtonsInOrder[index].Worker.Visible)
    //                num1 += ___allButtonsInOrder[index].minimized ? 0.5f : 1f;
    //        }
    //        GUI.color = Color.white;
    //        int num2 = (int)((double)UI.screenWidth / (double)num1);
    //        int num3 = num2 / 2;
    //        int lastIndex = ___allButtonsInOrder.FindLastIndex((Predicate<MainButtonDef>)(x => x.Worker.Visible));
    //        int x1 = 0;
    //        for (int index = 0; index < ___allButtonsInOrder.Count; ++index)
    //        {
    //            if (___allButtonsInOrder[index].Worker.Visible)
    //            {
    //                int width = ___allButtonsInOrder[index].minimized ? num3 : num2;
    //                if (index == lastIndex)
    //                    width = UI.screenWidth - x1;
    //                Rect rect = new Rect((float)x1, (float)(UI.screenHeight - 35), (float)width, 36f);
    //                ___allButtonsInOrder[index].Worker.DoButton(rect);
    //                if(UIManager.editionModeEnabled) DrawConfigIcon(rect);
    //                x1 += width;
    //            }
    //        }

    //        return false;
    //    }

    //    public static void DrawConfigIcon(Rect space)
    //    {
    //        GUI.BeginGroup(space);

    //        float configSizef = space.height - 8f;
    //        Rect configSpace = new Rect(space.width - configSizef - 4f, 4f, configSizef, configSizef);
    //        //bool clicked = Widgets.ButtonImage(configSpace, Textures.configIcon);
    //        GUI.DrawTexture(configSpace, Textures.configIcon);
    //        //bool clicked = Widgets.ButtonInvisible(configSpace,true);

    //        GUI.EndGroup();
    //    }

    //}

    [HarmonyPatch(typeof(MainButtonsRoot), "MainButtonsOnGUI")]
    internal class MainButtonsRoot_TranspilerPatch
    {
        private static readonly MethodInfo vanilla_DoButtons = AccessTools.Method(typeof(MainButtonsRoot), "DoButtons");
        private static MethodInfo uini_DoButtons = AccessTools.Method(typeof(Utility.CustomToolbar), "OnGui");

        private static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {

            bool found = false;
            bool patched = false;
            CodeInstruction temp = new CodeInstruction(OpCodes.Nop);

            foreach (CodeInstruction code in instructions)
            {
                if (!patched)
                {
                    if (!found)
                    {
                        if (code.opcode == OpCodes.Ldarg_0)
                        {
                            temp = code;
                            found = true;
                            continue;
                        }
                    }
                    else
                    {
                        if (code.opcode == OpCodes.Call && (MethodInfo)code.operand == vanilla_DoButtons)
                        {
                            code.operand = uini_DoButtons;
                            code.labels = temp.labels;
                            patched = true;
                        }
                        else
                        {
                            found = false;
                            yield return temp;

                        }
                    }

                }
                yield return code;
            }
        }
    }
}
