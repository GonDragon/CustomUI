using CustomUI.Utility;
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
        public static bool togglersOnTop = false;

        public static bool vanillaReadout = true;
        public static bool vanillaControlSpeed = true;
        public static bool vanillaDate = true;
        public static bool vanillaRealtime = true;
        public static bool vanillaWeather = true;
        public static bool vanillaTemperature = true;

        public static List<MainButtonProxy> mainButtonProxies = new List<MainButtonProxy>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref togglersOnTop, "togglersOnTop", true);

            Scribe_Values.Look(ref vanillaReadout, "vanillaReadout", false);
            Scribe_Values.Look(ref vanillaControlSpeed, "vanillaControlSpeed", false);
            Scribe_Values.Look(ref vanillaDate, "vanillaDate", false);
            Scribe_Values.Look(ref vanillaRealtime, "vanillaRealtime", false);
            Scribe_Values.Look(ref vanillaWeather, "vanillaWeather", false);
            Scribe_Values.Look(ref vanillaTemperature, "vanillaTemperature", false);

            Scribe_Collections.Look(ref mainButtonProxies, "buttons" ,LookMode.Deep);

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
            float columnWidth = inRect.width / 2;
            float heigth = inRect.height;
            Rect column1 = new Rect(columnWidth - columnWidth / 2, inRect.y, columnWidth, heigth).ContractedBy(2f);
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(column1);
            listingStandard.CheckboxLabeled("CustomUI.Setting.togglersOnTop".Translate(), ref Settings.togglersOnTop, "CustomUI.Setting.togglersOnTop.Description".Translate());

            listingStandard.CheckboxLabeled("CustomUI.Setting.vanillaReadout".Translate(), ref Settings.vanillaReadout, "CustomUI.Setting.vanillaReadout.Description".Translate());
            listingStandard.CheckboxLabeled("CustomUI.Setting.vanillaRealtime".Translate(), ref Settings.vanillaRealtime, "CustomUI.Setting.vanillaRealtime.Description".Translate());
            listingStandard.CheckboxLabeled("CustomUI.Setting.vanillaWeather".Translate(), ref Settings.vanillaWeather, "CustomUI.Setting.vanillaWeather.Description".Translate());
            listingStandard.CheckboxLabeled("CustomUI.Setting.vanillaTemperature".Translate(), ref Settings.vanillaTemperature, "CustomUI.Setting.vanillaTemperature.Description".Translate());

            listingStandard.CheckboxLabeled("CustomUI.Setting.vanillaDate".Translate(), ref Settings.vanillaDate, "CustomUI.Setting.vanillaDate.Description".Translate());
            listingStandard.CheckboxLabeled("CustomUI.Setting.vanillaControlSpeed".Translate(), ref Settings.vanillaControlSpeed, "CustomUI.Setting.vanillaControlSpeed.Description".Translate());

            listingStandard.End();

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
