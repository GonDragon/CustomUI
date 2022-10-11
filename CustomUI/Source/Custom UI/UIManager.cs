using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CustomUI
{
    public static class UIManager
    {

        public static Utility.MainButtonRenderMode mainButtonMode => Utility.MainButtonRenderMode.IconsAndText;
        public static bool editionModeEnabled = false;

        //public static float ExtendedBarHeight => ExtendedToolbar.Height;
        //public static float ExtendedBarWidth => ExtendedToolbar.Width;
        //public static float ResourceGap => (Settings.vanillaAnimals ? (Settings.TabsOnTop ? ExtendedToolbar.Height : 0f) : animalsRow.FinalY + 26f);

        //private static bool tabsOnTop = Settings.TabsOnTop;

        //private static Utility.VUIEhelper vuie;

        //public static Utility.VUIEhelper Helper
        //{
        //    get
        //    {
        //        if (vuie == null)
        //        {
        //            vuie = new Utility.VUIEhelper();
        //        }
        //        return vuie;
        //    }
        //}

        public static void Before_MainUIOnGUI()
        {
            //if (tabsOnTop != Settings.TabsOnTop)
            //{
            //    tabsOnTop = Settings.TabsOnTop;
            //    Find.ColonistBar.MarkColonistsDirty();
            //}
        }

        public static void MainUIOnGUI()
        {
            //if (Find.CurrentMap == null || WorldRendererUtility.WorldRenderedNow) return;
        }

        public static void After_MainUIOnGUI()
        {
        }
    }
}
