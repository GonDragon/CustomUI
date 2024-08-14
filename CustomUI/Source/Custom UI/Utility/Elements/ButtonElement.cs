using CustomUI.Utility.Workers;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace CustomUI.Utility
{
    public class ButtonElement : ToolbarElement
    {
        private MainButtonDef mainButtonDef;

        private string _iconPath;
        private string _label;
        private bool _minimized;
        public bool hideLabel = false;
        public bool forceShow = false;
        private string _shortenedLabel;

        private float cachedLabelWidth = -1f;
        private float cachedShortenedLabelWidth = -1f;

        private Texture2D icon;
        public Texture2D Icon => icon;

        //public override bool Exist => (defName != "Inspect" && Def != null);
        public override bool Exist => (Def != null);

        public override MainButtonWorker Worker => ((MainButtonDef)Def).Worker;

        public override bool IsFixed => _minimized;
        public void RefreshIcon()
        {
            if (_iconPath != null) icon = ContentFinder<Texture2D>.Get(this._iconPath);
            else icon = null;
        }

        public ButtonElement(string defName)
        {
            this.defName = defName;
            _minimized = false;
        }

        public ButtonElement(MainButtonDef def)
        {
            this.defName = def.defName;
            mainButtonDef = def;
            Label = def.label;
            IconPath = def.iconPath;
            _minimized = def.minimized;
        }

        public ButtonElement()
        { } //Empty constructor to load from ExposeData

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _iconPath, "iconPath");
            Scribe_Values.Look(ref _label, "label");
            Scribe_Values.Look(ref _minimized, "minimized");
            Scribe_Values.Look(ref hideLabel, "hideLabel", false);
            Scribe_Values.Look(ref forceShow, "forceShow", false);
        }

        public string IconPath
        {
            get
            {
                return _iconPath;
            }

            set
            {
                _iconPath = value;
                RefreshIcon();
            }
        }

        public string Label
        {
            get
            {
                return _label;
            }

            set
            {
                _label = value.CapitalizeFirst();
                RefreshCache();
            }
        }

        public void RefreshCache()
        {
            this._shortenedLabel = _label.Shorten();
            GameFont font = Text.Font;
            Text.Font = GameFont.Small;
            cachedLabelWidth = Text.CalcSize(Label).x;
            cachedShortenedLabelWidth = Text.CalcSize(ShortenedLabel).x;
            Text.Font = font;
        }

        public float LabelWidth => cachedLabelWidth;

        public string ShortenedLabel => _shortenedLabel;

        public float ShortenedLabelWidth => cachedShortenedLabelWidth;

        public override void FixSize()
        {
            _minimized = true;
        }

        public override void UnfixSize()
        {
            _minimized = false;
        }

        public virtual Def Def
        {
            get
            {
                if (mainButtonDef == null) mainButtonDef = DefDatabase<MainButtonDef>.GetNamedSilentFail(this.defName);
                return mainButtonDef;
            }
        }
    }
}
