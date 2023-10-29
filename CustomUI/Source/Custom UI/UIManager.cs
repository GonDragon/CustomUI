using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CustomUI
{
    public static class UIManager
    {

        public static Utility.MainButtonRenderMode mainButtonMode => Utility.MainButtonRenderMode.IconsAndText;

        public static float Height => 35f; // PLACEHOLDER => Settings.Height
        public static float Width => UI.screenWidth;

        public static bool topBar = false;
        public static bool bottomBar = true;

        public static bool editionModeEnabled = false;
        public static bool previousMode = false;

        public static bool TabsOnTop => topBar || editionModeEnabled;
        public static bool TabsOnBottom => bottomBar || editionModeEnabled;

        public static void CheckForChanges()
        {
            if (editionModeEnabled != previousMode)
            {
                previousMode = editionModeEnabled;
                Find.ColonistBar.MarkColonistsDirty();
            }
        }
        //public static float ResourceGap => (Settings.vanillaAnimals ? (Settings.TabsOnTop ? ExtendedToolbar.Height : 0f) : animalsRow.FinalY + 26f);
        public static float ResourceGap => (TabsOnTop ? Height : 0f);


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
