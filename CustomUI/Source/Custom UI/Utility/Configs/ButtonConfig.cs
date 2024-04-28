using RimWorld;
using UnityEngine;
using Verse;

namespace CustomUI.Utility
{
    public class ButtonConfig : IExposable
    {
        private MainButtonDef mainButtonDef;
        public string defName;

        private string _iconPath;
        private string _label;
        public bool minimized;
        public bool hideLabel = false;
        public bool forceShow = false;
        private string _shortenedLabel;

        private float cachedLabelWidth = -1f;
        private float cachedShortenedLabelWidth = -1f;

        private Texture2D icon;
        public Texture2D Icon => icon;
        public void RefreshIcon()
        {
            if (_iconPath != null) icon = ContentFinder<Texture2D>.Get(this._iconPath);
            else icon = null;
        }

        public ButtonConfig(string defName)
        {
            this.defName = defName;
            minimized = false;
        }

        public ButtonConfig(MainButtonDef def)
        {
            this.defName = def.defName;
            mainButtonDef = def;
            Label = def.label;
            IconPath = def.iconPath;
            minimized = def.minimized;
        }

        public ButtonConfig()
        { } //Empty constructor to load from ExposeData

        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref _iconPath, "iconPath");
            Scribe_Values.Look(ref _label, "label");
            Scribe_Values.Look(ref minimized, "minimized");
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
