﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace CustomUI.Utility
{
    internal static class Helper
    {
        public const float MainButtonWorker_IconSize = 32;
        public const float MainButtonWorker_BaseIconMargin = 32;
        public const float ReferenceWidth = 1920;
        public const float ReferenceWidth_IconMargin = ReferenceWidth / 12;
        private const float MinWidthFractionForIconAndLabel = 1f / 14;

        public static bool CanDrawIconAndLabel(float rectWidth, MainButtonDef def) => UIManager.mainButtonMode == MainButtonRenderMode.IconsAndText &&
            !def.minimized && def.Icon != null && !def.label.NullOrEmpty() && (rectWidth / ReferenceWidth >= MinWidthFractionForIconAndLabel * Prefs.UIScale);

        public static float CalculatedMainButtonIconMargin(float rectWidth) => MainButtonWorker_BaseIconMargin * (rectWidth / ReferenceWidth_IconMargin) / Prefs.UIScale;



    }
}
