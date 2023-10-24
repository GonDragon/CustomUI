using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CustomUI.Utility
{
    public class MainButtonProxy : IExposable
    {
        public bool visible;
        public int order;
        public bool minimized;
        public int toolbar;
        public string defName;
        public string iconPath;

        public MainButtonProxy()
        {
            this.visible = true;
            this.order = 0;
            this.minimized = false;
            this.toolbar = 0;
            this.defName = "";
            this.iconPath = "";
        }
        public MainButtonProxy(bool visible, int order, bool minimized, int toolbar, string defName, string iconPath)
        {
            this.visible = visible;
            this.order = order;
            this.minimized = minimized;
            this.toolbar = toolbar;
            this.defName = defName;
            this.iconPath = iconPath;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref visible, "isVisible", true);
            Scribe_Values.Look(ref order, "order", 0);
            Scribe_Values.Look(ref minimized, "minimized", false);
            Scribe_Values.Look(ref toolbar, "toolbar", 0);
            Scribe_Values.Look(ref defName, "defName", "");
            Scribe_Values.Look(ref iconPath, "iconPath", "");
        }
    }
}
