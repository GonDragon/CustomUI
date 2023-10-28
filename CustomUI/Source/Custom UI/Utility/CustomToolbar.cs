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
        public static float Height => 35f; // PLACEHOLDER => Settings.Height
        public static float Width => UI.screenWidth;

        public static float interGap = 0;
        public static float padding = 2;
        public static float margin = 3;
        private const int minimizedWidth = 70;

        private static readonly List<MainButtonDef> allButtonsInOrder;
        private static readonly DragManager<MainButtonDef> manager;
        private static readonly List<int> buttonSizeCache;
        private static readonly List<int> buttonSizeCacheEditMode;
        private static int lastIndex = 0;

        private static readonly List<IndividualToolbar> toolbarList;

        private static bool topBar = false;
        private static bool bottomBar = true;

        public static bool TabsOnTop => topBar || UIManager.editionModeEnabled;
        public static bool TabsOnBottom => bottomBar || UIManager.editionModeEnabled;

        static CustomToolbar()
        {
            allButtonsInOrder = (List<MainButtonDef>)AccessTools.Field(typeof(MainButtonsRoot), "allButtonsInOrder").GetValue(Find.MainButtonsRoot);
            manager = new DragManager<MainButtonDef>((button, topLeft, width) => button.Worker.DoButton(new Rect(topLeft, new Vector2(width, Height))));
            buttonSizeCache = new List<int>();
            buttonSizeCacheEditMode = new List<int>();
            toolbarList = new List<IndividualToolbar>();

            IndividualToolbar bottomBar = new IndividualToolbar(new Rect(0f, UI.screenHeight - Height, Width, Height));
            IndividualToolbar topBar = new IndividualToolbar(new Rect(0f, 0f, Width, Height));

            toolbarList.Add(bottomBar);
            toolbarList.Add(topBar);

            Sync();
            OnChange();

        }

        public static void OnGui()
        {
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

                //if (!Settings.toolbarDefnames.Contains(button.defName))
                //{
                //    Settings.toolbarDefnames.Add(button.defName);
                //    Settings.toolbarValues.Add(0);
                //}
                int toolbar = Settings.toolbarValues[Settings.toolbarDefnames.IndexOf(button.defName)];

                if (!button.Worker.Visible) continue;

                Rect buttonRect = new Rect(curX[toolbar], toolbarList[toolbar].inRect.y, buttonSizeCache[i], Height);

                button.Worker.DoButton(buttonRect);

                curX[toolbar] += buttonSizeCache[i];
            }
        }

        public static void EditMode()
        {
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

                if (!Settings.toolbarDefnames.Contains(button.defName))
                {
                    Settings.toolbarDefnames.Add(button.defName);
                    Settings.toolbarValues.Add(0);
                }
                int toolbar = GetToolbar(button);

                //if (!button.Worker.Visible) continue;

                Rect buttonRect = new Rect(curX[toolbar], toolbarList[toolbar].inRect.y, buttonSizeCacheEditMode[i], Height);

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
                int originDraggingToolbar = GetToolbar(manager.Dragging.element);

                if (shouldDrawAtEnd)
                {
                    Rect draggedRect = new Rect(curX[originDraggingToolbar], (UI.screenHeight - Height), manager.Dragging.width, Height);
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

                    SetToolbar(dragButton.element, toolbar);

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

        // This function is awfull, a lot of repeated code. I need to rethink the whole thing.
        public static void OnChange()
        {
            topBar = false;
            bottomBar = false;

            allButtonsInOrder.SortBy(x => x.order);
            List<int> indexElasticWidth = new List<int>();
            List<int> indexElasticWidthEditMode = new List<int>();

            buttonSizeCache.Clear();
            buttonSizeCacheEditMode.Clear();
            foreach (IndividualToolbar toolbar in toolbarList)
            {
                toolbar.buttonsIndex.Clear();
                
                toolbar.elasticElements = 0;
                toolbar.fixedWidth = 0;

                toolbar.elasticElementsEditMode = 0;
                toolbar.fixedWidthEditMode = 0;
            }

            for (int i = 0; i < allButtonsInOrder.Count; ++i)
            {

                if (!Settings.toolbarDefnames.Contains(allButtonsInOrder[i].defName))
                {
                    Settings.toolbarDefnames.Add(allButtonsInOrder[i].defName);
                    Settings.toolbarValues.Add(0);
                }

                IndividualToolbar toolbar = toolbarList[GetToolbar(allButtonsInOrder[i])];
                toolbar.buttonsIndex.Add(i);

                if (!allButtonsInOrder[i].Worker.Visible)
                {
                    buttonSizeCache.Add(0);
                    if (!allButtonsInOrder[i].minimized)
                    {
                        buttonSizeCacheEditMode.Add(-1);
                        indexElasticWidthEditMode.Add(i);
                        toolbar.elasticElementsEditMode++;
                    }
                    else
                    {
                        buttonSizeCacheEditMode.Add(minimizedWidth);
                        toolbar.fixedWidthEditMode += minimizedWidth;
                    }

                    continue;
                }

                if (GetToolbar(allButtonsInOrder[i]) == 0) bottomBar = true;
                else topBar = true;

                // Cambiar para tomar en cuenta ancho fijo
                if (!allButtonsInOrder[i].minimized)
                {
                    buttonSizeCache.Add(-1);
                    indexElasticWidth.Add(i);                    
                    toolbar.elasticElements++;

                    buttonSizeCacheEditMode.Add(-1);
                    indexElasticWidthEditMode.Add(i);
                    toolbar.elasticElementsEditMode++;
                }
                else
                {
                    buttonSizeCache.Add(minimizedWidth);
                    toolbar.fixedWidth += minimizedWidth;

                    buttonSizeCacheEditMode.Add(minimizedWidth);
                    toolbar.fixedWidthEditMode += minimizedWidth;
                }
            }

            foreach (IndividualToolbar toolbar in toolbarList)
            {
                int elasticSpaceAvaible = (int)(Width - toolbar.fixedWidth);
                toolbar.elasticElementWidth = toolbar.elasticElements > 0 ? elasticSpaceAvaible / toolbar.elasticElements : 0;

                int elasticSpaceAvaibleEditMode = (int)(Width - toolbar.fixedWidthEditMode);
                toolbar.elasticElementWidthEditMode = toolbar.elasticElementsEditMode > 0 ? elasticSpaceAvaibleEditMode / toolbar.elasticElementsEditMode : 0;
            }

            foreach (int indexElastic in indexElasticWidth)
            {
                IndividualToolbar toolbar = toolbarList[GetToolbar(allButtonsInOrder[indexElastic])];
                buttonSizeCache[indexElastic] = toolbar.elasticElementWidth;

            }

            foreach (int indexElasticEditMode in indexElasticWidthEditMode)
            {
                IndividualToolbar toolbar = toolbarList[GetToolbar(allButtonsInOrder[indexElasticEditMode])];
                buttonSizeCacheEditMode[indexElasticEditMode] = toolbar.elasticElementWidthEditMode;
            }

            for (int i = 0; i < toolbarList.Count; ++i)
            {
                int lastIndex = allButtonsInOrder.FindLastIndex((Predicate<MainButtonDef>)(x => x.Worker.Visible && (GetToolbar(x) == i)));
                int lastIndexEditMode = allButtonsInOrder.FindLastIndex((Predicate<MainButtonDef>)(x => (GetToolbar(x) == i)));

                int allButtonsSize = 0;
                int allButtonsSizeEditMode = 0;
                if (lastIndex < 0 && lastIndexEditMode < 0) continue;

                foreach (int buttonIndex in toolbarList[i].buttonsIndex)
                {
                    allButtonsSize += buttonSizeCache[buttonIndex];
                    allButtonsSizeEditMode += buttonSizeCacheEditMode[buttonIndex];
                }

                if(lastIndex >= 0) buttonSizeCache[lastIndex] += UI.screenWidth - allButtonsSize;
                if (lastIndexEditMode >= 0) buttonSizeCacheEditMode[lastIndexEditMode] += UI.screenWidth - allButtonsSizeEditMode;

            }

            int minOrder = 0;
            allButtonsInOrder.Do((button) =>
            {
                button.order = minOrder;
                minOrder += 10;
            });

            Persist();
        }
        
        public static int GetToolbar(MainButtonDef buttonDef)
        {
            int toolbarIndexGet = Settings.toolbarDefnames.IndexOf(buttonDef.defName);
            return Settings.toolbarValues[toolbarIndexGet];
        }

        public static void SetToolbar(MainButtonDef buttonDef, int toolbar)
        {
            int toolbarIndexSet = Settings.toolbarDefnames.IndexOf(buttonDef.defName);
            Settings.toolbarValues[toolbarIndexSet] = toolbar;
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
                        proxy.toolbar = GetToolbar(buttonDef);
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
    }
}