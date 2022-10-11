using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CustomUI
{
    public class Settings : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
        }
    }

    public class SettingsWindow : Mod
    {
        public static Settings settings;

        public SettingsWindow(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }

        public override string SettingsCategory()
        {
            return CustomUI.Name;
        }

    }
}
