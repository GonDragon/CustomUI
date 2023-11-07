using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CustomUI.Utility
{
    public static class CustomToolbar
    {
        public static float interGap = 0;
        public static float padding = 2;
        public static float margin = 3;

        private static readonly List<MainButtonDef> allButtonsInOrder;
        private static readonly DragManager<MainButtonDef> manager;
        private static readonly List<int> buttonSizeCache;
        private static readonly List<int> buttonSizeCacheEditMode;
        private static int lastIndex = 0;

        private static readonly List<IndividualToolbar> toolbarList;

        static CustomToolbar()
        {
            allButtonsInOrder = (List<MainButtonDef>)AccessTools.Field(typeof(MainButtonsRoot), "allButtonsInOrder").GetValue(Find.MainButtonsRoot);
            manager = new DragManager<MainButtonDef>((button, topLeft, width) => button.Worker.DoButton(new Rect(topLeft, new Vector2(width, UIManager.Height))));
            buttonSizeCache = new List<int>();
            buttonSizeCacheEditMode = new List<int>();
            toolbarList = new List<IndividualToolbar>();

            IndividualToolbar bottomBar = new IndividualToolbar(new Rect(0f, UI.screenHeight - UIManager.Height, UIManager.Width, UIManager.Height));
            IndividualToolbar topBar = new IndividualToolbar(new Rect(0f, 0f, UIManager.Width, UIManager.Height));

            toolbarList.Add(bottomBar);
            toolbarList.Add(topBar);

            Sync();
            OnChange();

        }

        public static void OnGui()
        {
            UIManager.CheckForChanges();

            if (UIManager.editionModeEnabled)
            {
                EditMode();
            }
            else
            {
                NormalMode();
            }
        }

        public static void NormalMode()
        {
            GUI.color = Color.white;
            List<int> curX = new List<int>();

            for (int i = 0; i < toolbarList.Count; i++)
            {
                curX.Add(0);
            }

            for (int i = 0; i < allButtonsInOrder.Count; ++i)
            {
                MainButtonDef button = allButtonsInOrder[i];

                int toolbar = Settings.buttonManager.GetToolbar(button.defName);

                if (!button.Worker.Visible) continue;

                Rect buttonRect = new Rect(curX[toolbar], toolbarList[toolbar].inRect.y, buttonSizeCache[i], UIManager.Height);

                button.Worker.DoButton(buttonRect);

                curX[toolbar] += buttonSizeCache[i];
            }
        }

        public static void EditMode()
        {
            foreach(IndividualToolbar toolbar in toolbarList)
            {
                DrawTransparentRect(toolbar.inRect, 0.3f);
            }

            GUI.color = Color.white;

            List<int> curX = new List<int>();

            for (int i = 0; i < toolbarList.Count; i++)
            {
                curX.Add(0);
            }

            bool shouldDrawAtEnd = true;
            int replaceIndex = 0;
            bool isLastIndex = false;
            for (int i = 0; i < allButtonsInOrder.Count; ++i)
            {
                MainButtonDef button = allButtonsInOrder[i];

                //if (!Settings.toolbarDefnames.Contains(button.defName))
                //{
                //    Settings.toolbarDefnames.Add(button.defName);
                //    Settings.toolbarValues.Add(0);
                //}
                int toolbar = Settings.buttonManager.GetToolbar(button.defName);

                //if (!button.Worker.Visible) continue;

                Rect buttonRect = new Rect(curX[toolbar], toolbarList[toolbar].inRect.y, buttonSizeCacheEditMode[i], UIManager.Height);

                //* Edit Mode ONLY - Once per Button *//

                bool mouseOverButton = Mouse.IsOver(buttonRect);
                bool mouseOverBar = IsMouseOverBar();

                if (manager.DraggingNow)
                {
                    if (manager.Dragging.index == i)
                    {
                        if (!mouseOverBar)
                        {
                            int offset = manager.Dragging.width;
                            buttonRect.x += offset;
                            curX[toolbar] += offset;
                        }
                        continue;
                    }

                    if (mouseOverButton)
                    {
                        Rect draggedRect = new Rect(buttonRect.x, buttonRect.y, manager.Dragging.width, buttonRect.height);
                        DrawWithConfigIcon(manager.Dragging.element, draggedRect);
                        shouldDrawAtEnd = false;
                        replaceIndex = i;

                        buttonRect.x += manager.Dragging.width;
                        curX[toolbar] += manager.Dragging.width;
                    }
                }

                if (manager.TryStartDrag(button, buttonRect, i)) CustomUI.Log("TryDrag");

                DrawWithConfigIcon(button, buttonRect);

                curX[toolbar] += buttonSizeCacheEditMode[i];
            }

            //* Edit Mode ONLY - Just Once *//
            if (manager.DraggingNow)
            {
                int originDraggingToolbar = Settings.buttonManager.GetToolbar(manager.Dragging.element.defName);

                if (shouldDrawAtEnd)
                {
                    Rect draggedRect = new Rect(curX[originDraggingToolbar], (UI.screenHeight - UIManager.Height), manager.Dragging.width, UIManager.Height);
                    DrawWithConfigIcon(manager.Dragging.element, draggedRect);
                    replaceIndex = lastIndex;
                    isLastIndex = true;
                }

                Func<DragElement<MainButtonDef>, int, bool> onDropFunction = (dragButton, toolbar) => {
                    if (replaceIndex > dragButton.index && !isLastIndex)
                    {
                        replaceIndex--;
                        while (!allButtonsInOrder[replaceIndex].Worker.Visible)
                        {
                            replaceIndex--;
                        }
                        dragButton.element.order = allButtonsInOrder[replaceIndex].order + 1;
                    }
                    else
                    {
                        dragButton.element.order = allButtonsInOrder[replaceIndex].order - 1;
                    }

                    Settings.buttonManager.SetToolbar(dragButton.element.defName, toolbar);

                    OnChange();
                    return true;
                };

                for (int i = 0; i < toolbarList.Count; i++)
                {
                    manager.DropLocation(toolbarList[i].inRect, null, onDropFunction, i);
                }

                //manager.DragDropOnGUI((dragButton) => CustomUI.Log($"Stop Drag For {dragButton.defName}"), !Mouse.IsOver(inRect));
            }
        }

        public static bool IsMouseOverBar()
        {
            bool isMouseOverBar = false;

            foreach (IndividualToolbar toolbar in toolbarList)
            {
                isMouseOverBar = isMouseOverBar || Mouse.IsOver(toolbar.inRect);
            }

            return isMouseOverBar;
        }

        public static void OnChange()
        {
            allButtonsInOrder.SortBy(x => x.order);

            buttonSizeCache.Clear();
            buttonSizeCacheEditMode.Clear();

            foreach (IndividualToolbar toolbar in toolbarList)
            {
                toolbar.Initialize();
            }

            for (int i = 0; i < allButtonsInOrder.Count; ++i)
            {
                string defName = allButtonsInOrder[i].defName;

                if (!Settings.buttonManager.DefExist(defName)) Settings.buttonManager.AddDef(defName);

                IndividualToolbar toolbar = toolbarList[Settings.buttonManager.GetToolbar(defName)];

                toolbar.AddButton(allButtonsInOrder[i], i);
                buttonSizeCache.Add(0);
                buttonSizeCacheEditMode.Add(0);
            }

            foreach (IndividualToolbar toolbar in toolbarList)
            {
                toolbar.GetSizes(buttonSizeCache, buttonSizeCacheEditMode);
            }

            UIManager.bottomBar = !toolbarList[0].empty;
            UIManager.topBar = !toolbarList[1].empty;

            int order = 0;
            allButtonsInOrder.Do((button) =>
            {
                button.order = order;
                order += 10;
            });

            Find.ColonistBar.MarkColonistsDirty();
            Persist();
        }

        public static void DrawWithConfigIcon(MainButtonDef button, Rect space)
        {
            if (Widgets.ButtonInvisible(space, false))
            {
                //Open Window
                Find.WindowStack.Add(new Windows.EditMainButton_Window(button));
            }

            Color originalColor = GUI.color;
            if(!button.Worker.Visible) GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.5f);
            button.Worker.DoButton(space);
            GUI.color = originalColor;

            GUI.BeginGroup(space);

            float configSizef = space.height - 8f;
            Rect configSpace = new Rect(space.width - configSizef - 4f, 4f, configSizef, configSizef);
            GUI.DrawTexture(configSpace, Textures.configIcon);

            GUI.EndGroup();
        }

        public static void Persist()
        {
            List<MainButtonProxy> buttonsToAdd = new List<MainButtonProxy>();
            foreach (MainButtonDef buttonDef in allButtonsInOrder)
            {
                bool found = false;
                foreach (MainButtonProxy proxy in Settings.mainButtonProxies)
                {
                    if (proxy.defName == buttonDef.defName)
                    {
                        proxy.visible = buttonDef.buttonVisible;
                        proxy.toolbar = Settings.buttonManager.GetToolbar(buttonDef.defName);
                        proxy.order = buttonDef.order;
                        proxy.minimized = buttonDef.minimized;
                        proxy.iconPath = buttonDef.iconPath;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    MainButtonProxy newButton = new MainButtonProxy(buttonDef.buttonVisible, buttonDef.order, buttonDef.minimized, 0, buttonDef.defName, buttonDef.iconPath); // TODO - Multiple Toolbars
                    buttonsToAdd.Add(newButton);
                }
            }

            foreach (MainButtonProxy button in buttonsToAdd)
            {
                Settings.mainButtonProxies.Add(button);
            }

            SettingsWindow.settings.Write();
        }

        public static void Sync()
        {
            if (Settings.mainButtonProxies == null)
            {
                Settings.mainButtonProxies = new List<MainButtonProxy>();
            }
            foreach (MainButtonProxy proxy in Settings.mainButtonProxies)
            {
                foreach (MainButtonDef buttonDef in allButtonsInOrder)
                {
                    if (buttonDef.defName == proxy.defName)
                    {
                        buttonDef.buttonVisible = proxy.visible;
                        buttonDef.order = proxy.order;
                        buttonDef.minimized = proxy.minimized;
                        buttonDef.iconPath = proxy.iconPath;
                        Traverse.Create(buttonDef).Field("icon").SetValue(null);
                    }
                }
            }
        }

        private static void DrawTransparentRect(Rect rect, float opacity)
        {
            Color prevColor = GUI.color;
            GUI.color = new Color(0.3f, 0.3f, 0.3f, opacity);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = prevColor;
        }

    }
}