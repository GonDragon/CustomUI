using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CustomUI.Utility
{
    public class IndividualToolbar
    {
        public float fixedWidth = 0f;
        public int elasticElements = 0;
        public Rect inRect;
        //private Rect inRect = new Rect(0f, UI.screenHeight - Height, Width, Height);

        public IndividualToolbar(Rect rect)
        {
            inRect = rect;
        }

    }
}
