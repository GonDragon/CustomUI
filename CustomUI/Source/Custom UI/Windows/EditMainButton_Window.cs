using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;
using CustomUI.Utility;
using HarmonyLib;

namespace CustomUI.Windows
{
    public class EditMainButton_Window : Window
    {
        internal readonly MainButtonDef buttonDef;

        internal string originalLabel;
        internal bool originalMinimized;
        internal bool originalVisible;
        internal string originalIconPath;

        internal string tempLabel;
        internal bool tempMinimized;
        internal bool tempVisible;

        private Vector2 scrollPos;

        private float viewHeight;
        private const int IconSize = 40;
        private const int IconPadding = 5;
        private const int IconMargin = 5;
        private const int ColorSize = 22;
        private const int ColorPadding = 2;
        private static readonly Vector2 ButSize = new Vector2(150f, 38f);
        private static readonly float EditFieldHeight = 30f;
        private static readonly float ResetButtonWidth = 60f;
        private static readonly Regex ValidSymbolRegex = new Regex("^[\\p{L}0-9 '\\-]*$");
        private const int MaxSymbolLength = 40;
        private static readonly Dictionary<string, Texture2D> cacheIcons = new Dictionary<string, Texture2D>();
        private static List<string> cacheIconsPath;

        private bool isFixedSize = false;
        private float placeholderFixedSize = -1f;

        public override Vector2 InitialSize => new Vector2(350f, 600f);
        public new bool doCloseX = false;

        public EditMainButton_Window(MainButtonDef buttonDef)
        {
            this.buttonDef = buttonDef;
            
            this.originalLabel = buttonDef.label;
            this.originalVisible = buttonDef.buttonVisible;
            this.originalMinimized = buttonDef.minimized;
            this.originalIconPath = buttonDef.iconPath;

            this.tempLabel = buttonDef.label;
            this.tempVisible = buttonDef.buttonVisible;
            this.tempMinimized = buttonDef.minimized;

            absorbInputAroundWindow = true;
        }

        public override void OnAcceptKeyPressed()
        {
            TryAccept();
            CustomToolbar.Persist();
            Event.current.Use();
        }

        public override void DoWindowContents(Rect rect)
        {
            Rect inRect = rect;
            inRect.height -= Window.CloseButSize.y;
            Verse.Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, rect.width, 35f), "CustomUI.Windows.EditButton".Translate());
            Verse.Text.Font = GameFont.Small;
            string defaultLabel = (string)"Default".Translate();
            inRect.yMin += 45f;
            float curY = inRect.y;
            float iconX = (float)Math.Floor(inRect.width / 2) - 44f;
            if (buttonDef.Icon != null) GUI.DrawTexture(new Rect(rect.x + iconX, curY + 5f, 88f, 88f), (Texture)buttonDef.Icon);
            curY += 93f;
            float x = inRect.x + inRect.width / 3f;
            float width = (float)((double)inRect.xMax - (double)x - (double)EditMainButton_Window.ResetButtonWidth - 10.0);
            float labelY = curY;
            Widgets.Label(inRect.x, ref labelY, inRect.width, (string)"Name".Translate());
            buttonDef.label = Widgets.TextField(new Rect(x, curY, width, EditMainButton_Window.EditFieldHeight), buttonDef.label, 40, EditMainButton_Window.ValidSymbolRegex);

            curY += (EditMainButton_Window.EditFieldHeight + 10f);

            Rect minimizedRect = new Rect(inRect.x, curY, width * 2 - Widgets.CheckboxSize - 11f, EditMainButton_Window.EditFieldHeight);
            Rect defaultMinimizedRect = new Rect(minimizedRect.xMax + 10f, curY, EditMainButton_Window.ResetButtonWidth, EditMainButton_Window.EditFieldHeight);
            Widgets.CheckboxLabeled(minimizedRect, "CustomUI.Windows.Minimize".Translate(), ref buttonDef.minimized);

            curY += (EditMainButton_Window.EditFieldHeight + 10f);

            Rect hidelabelRect = new Rect(inRect.x, curY, width * 2 - Widgets.CheckboxSize - 11f, EditMainButton_Window.EditFieldHeight);
            Widgets.CheckboxLabeled(hidelabelRect, "Visible", ref buttonDef.buttonVisible);

            curY += (EditMainButton_Window.EditFieldHeight + 10f);

            Rect fixedSizeRect = new Rect(inRect.x, curY, width * 2 - Widgets.CheckboxSize - 11f, EditMainButton_Window.EditFieldHeight);
            Widgets.CheckboxLabeled(fixedSizeRect, "Fixed Size", ref isFixedSize);
            if (isFixedSize)
            {
                curY += (EditMainButton_Window.EditFieldHeight + 10f);

                Rect fixedSizeSliderRect = new Rect(inRect.x, curY, width * 2 - Widgets.CheckboxSize - 11f, EditMainButton_Window.EditFieldHeight);
                Widgets.HorizontalSlider(fixedSizeSliderRect, ref placeholderFixedSize, new FloatRange(50f, 300f), "Size", 1f);

            }

            curY += (EditMainButton_Window.EditFieldHeight + 10f);

            Rect iconRect = new Rect(inRect.x, curY, width * 2 - Widgets.CheckboxSize - 11f, EditMainButton_Window.EditFieldHeight);
            Rect defaulticonRect = new Rect(minimizedRect.xMax + 10f, curY, EditMainButton_Window.ResetButtonWidth, EditMainButton_Window.EditFieldHeight);
            Widgets.Label(iconRect, "Icon".Translate());

            curY += (EditMainButton_Window.EditFieldHeight + 10f);

            Rect iconSelectorRect = inRect;
            iconSelectorRect.yMax -= 4f;
            iconSelectorRect.yMin = curY;

            DoIconSelector(iconSelectorRect);

            CheckForChanges();
            if (Widgets.ButtonText(new Rect(0.0f, rect.height - EditMainButton_Window.ButSize.y, EditMainButton_Window.ButSize.x, EditMainButton_Window.ButSize.y), (string)"Cancel".Translate()))
            {
                buttonDef.buttonVisible = this.originalVisible;
                buttonDef.minimized = this.originalMinimized;
                buttonDef.label = this.originalLabel;
                buttonDef.iconPath = this.originalIconPath;
                CustomToolbar.OnChange();
                Traverse.Create(buttonDef).Field("cachedLabelCap").SetValue((TaggedString)(string)null);
                Traverse.Create(buttonDef).Field("icon").SetValue(null);

                Close();
            }
            if (!Widgets.ButtonText(new Rect(InitialSize.x - EditMainButton_Window.ButSize.x - (this.Margin * 2), rect.height - EditMainButton_Window.ButSize.y, EditMainButton_Window.ButSize.x, EditMainButton_Window.ButSize.y), (string)"DoneButton".Translate()))
            {
                return;
            }
            TryAccept();
        }

        private void DoIconSelector(Rect mainRect)
        {
            int num1 = 50;
            Rect viewRect = new Rect(0.0f, 0.0f, mainRect.width - 16f, viewHeight);
            Widgets.BeginScrollView(mainRect, ref scrollPos, viewRect);
            IEnumerable<string> allPaths = GetAvaibleIcons();
            int num2 = Mathf.FloorToInt(viewRect.width / (float)(num1 + 5));
            int num3 = allPaths.Count();
            int num4 = 0;
            foreach (string iconPath in allPaths)
            {
                if (iconPath != null && !cacheIcons.ContainsKey(iconPath)) cacheIcons.Add(iconPath, ContentFinder<Texture2D>.Get(iconPath));
                int num5 = num4 / num2;
                int num6 = num4 % num2;
                int num7 = num4 >= num3 - num3 % num2 ? num3 % num2 : num2;
                Rect rect = new Rect((float)(((double)viewRect.width - (double)(num7 * num1) - (double)((num7 - 1) * 5)) / 2.0) + (float)(num6 * num1) + (float)(num6 * 5), (float)(num5 * num1 + num5 * 5), (float)num1, (float)num1);
                Widgets.DrawLightHighlight(rect);
                Widgets.DrawHighlightIfMouseover(rect);
                if (iconPath == buttonDef.iconPath)
                    Widgets.DrawBox(rect);
                if (iconPath != null && cacheIcons[iconPath] != null) GUI.DrawTexture(new Rect(rect.x + 5f, rect.y + 5f, 40f, 40f), cacheIcons[iconPath]);
                GUI.color = Color.white;
                if (Widgets.ButtonInvisible(rect))
                {
                    buttonDef.iconPath = iconPath;
                    Traverse.Create(buttonDef).Field("icon").SetValue(null);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
                viewHeight = Mathf.Max(viewHeight, rect.yMax);
                ++num4;
            }
            GUI.color = Color.white;
            Widgets.EndScrollView();
        }

        public static void InitializeIconsPathCache()
        {
            if (cacheIconsPath == null)
            {
                cacheIconsPath = new List<string>();
                foreach (MainButtonDef button in DefDatabase<MainButtonDef>.AllDefs)
                {
                    if (button.iconPath != null) cacheIconsPath.Add(button.iconPath);
                }
                foreach (MainIconDef icon in DefDatabase<MainIconDef>.AllDefs) cacheIconsPath.Add(icon.path);
            }
        }

        private IEnumerable<string> GetAvaibleIcons()
        {
            InitializeIconsPathCache();
            yield return null;
            foreach (string path in cacheIconsPath) yield return path;
        }

        private void TryAccept()
        {
            if (!buttonDef.label.NullOrEmpty())
                buttonDef.label = buttonDef.label.Trim();
                Traverse.Create(buttonDef).Field("cachedLabelCap").SetValue((TaggedString)(string)null);
            Close();
        }

        private void CheckForChanges()
        {
            if (this.tempLabel != buttonDef.label)
            {
                this.tempLabel = buttonDef.label;
                Traverse.Create(buttonDef).Field("cachedLabelCap").SetValue((TaggedString)(string)null);

            }
            if (this.tempVisible != buttonDef.buttonVisible)
            {
                this.tempVisible = buttonDef.buttonVisible;
                CustomToolbar.OnChange();
            }
            if (this.tempMinimized != buttonDef.minimized)
            {
                this.tempMinimized = buttonDef.minimized;
                CustomToolbar.OnChange();
            }
        }
    }
}
