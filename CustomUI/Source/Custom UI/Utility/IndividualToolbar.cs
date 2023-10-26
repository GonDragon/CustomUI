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
        public int elasticElementWidth = 0;
        public Rect inRect;
        public List<int> buttonsIndex = new List<int>();

        // public float fixedWidthEditMode = 0f;
        // public int elasticElementsEditMode = 0;
        // public int elasticElementWidthEditMode = 0;

        public IndividualToolbar(Rect rect)
        {
            inRect = rect;
        }

    }
}
