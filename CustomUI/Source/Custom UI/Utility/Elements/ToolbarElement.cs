using Verse;
using CustomUI.Utility.Workers;
using System;
using RimWorld;

namespace CustomUI.Utility
{    public class ToolbarElement : IExposable
    {
        public virtual bool Configurable => false;

        public virtual bool Repeatable => false;

        public virtual bool IsFixed => false;

        public virtual int FixedSize => 70; // Default is equal to minimized icon

        public virtual bool Exist => true;

        public virtual MainButtonWorker Worker => throw new NotImplementedException();

        public virtual bool Equivalent(ToolbarElement other) => this.GetType() == other.GetType();

        public virtual string SettingLabel => "undefined";

        public string defName;
        public bool visible;
        public int order;
        public string iconPath;
        public int toolbar;

        public virtual void FixSize()
        { }

        public virtual void UnfixSize()
        { }

        public virtual void Reset()
        { }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref visible, "isVisible", true);
            Scribe_Values.Look(ref order, "order", 0);
            Scribe_Values.Look(ref toolbar, "toolbar", 0);
            Scribe_Values.Look(ref defName, "defName", "");
            Scribe_Values.Look(ref iconPath, "iconPath", "");
        }
    }
}