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
        private static int lastIndex = 0;

        //TODO - Convertir en lista de structs, para múltiples barras.
        private static float fixedWidth = 0f;
        private static int elasticElements = 0;
        private static Rect inRect = new Rect(0f, UI.screenHeight - Height, Width, Height);

        private static readonly List<IndividualToolbar> toolbarList;

        static CustomToolbar()
        {
            allButtonsInOrder = (List<MainButtonDef>)AccessTools.Field(typeof(MainButtonsRoot), "allButtonsInOrder").GetValue(Find.MainButtonsRoot);
            manager = new DragManager<MainButtonDef>((button, topLeft, width) => button.Worker.DoButton(new Rect(topLeft, new Vector2(width, Height))));
            buttonSizeCache = new List<int>();
            toolbarList = new List<IndividualToolbar>();

            IndividualToolbar bottomBar = new IndividualToolbar(new Rect(0f, UI.screenHeight - Height, Width, Height));
            IndividualToolbar topBar = new IndividualToolbar(new Rect(0f, Height, Width, Height));

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

            for (int index = 0; index < allButtonsInOrder.Count; ++index)
            {
                MainButtonDef button = allButtonsInOrder[index];

                if (!Settings.toolbarDefnames.Contains(button.defName))
                {
                    Settings.toolbarDefnames.Add(button.defName);
                    Settings.toolbarValues.Add(0);
                }
                int toolbar = Settings.toolbarValues[Settings.toolbarDefnames.IndexOf(button.defName)];

                if (!button.Worker.Visible) continue;

                Rect buttonRect = new Rect(curX[toolbar], (UI.screenHeight - Height), buttonSizeCache[index], Height);

                button.Worker.DoButton(buttonRect);

                curX[toolbar] += buttonSizeCache[index];
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
            for (int index = 0; index < allButtonsInOrder.Count; ++index)
            {
                MainButtonDef button = allButtonsInOrder[index];

                if (!Settings.toolbarDefnames.Contains(button.defName))
                {
                    Settings.toolbarDefnames.Add(button.defName);
                    Settings.toolbarValues.Add(0);
                }
                int toolbar = Settings.toolbarValues[Settings.toolbarDefnames.IndexOf(button.defName)];

                if (!button.Worker.Visible) continue;

                Rect buttonRect = new Rect(curX[toolbar], (UI.screenHeight - Height), buttonSizeCache[index], Height);

                //* Edit Mode ONLY - Once per Button *//

                bool mouseOverButton = Mouse.IsOver(buttonRect);
                bool mouseOverBar = Mouse.IsOver(inRect);

                if (manager.DraggingNow)
                {
                    if (manager.Dragging.index == index)
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
                        replaceIndex = index;

                        buttonRect.x += manager.Dragging.width;
                        curX[toolbar] += manager.Dragging.width;
                    }
                }

                if (mouseOverButton && Input.GetMouseButtonDown(1))
                {
                    CustomUI.Log("Edit");
                    Event.current.Use();
                }
                if (manager.TryStartDrag(button, buttonRect, index)) CustomUI.Log("TryDrag");

                DrawWithConfigIcon(button, buttonRect);

                curX[toolbar] += buttonSizeCache[index];
            }

            //* Edit Mode ONLY - Just Once *//
            if (manager.DraggingNow)
            {
                int toolbar = Settings.toolbarValues[Settings.toolbarDefnames.IndexOf(manager.Dragging.element.defName)];

                if (shouldDrawAtEnd)
                {
                    Rect draggedRect = new Rect(curX[toolbar], (UI.screenHeight - Height), manager.Dragging.width, Height);
                    DrawWithConfigIcon(manager.Dragging.element, draggedRect);
                    replaceIndex = lastIndex;
                    isLastIndex = true;
                }

                manager.DropLocation(inRect, null, dragButton =>
                {
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

                    OnChange();
                    return true;
                });

                manager.DragDropOnGUI((dragButton) => CustomUI.Log($"Stop Drag For {dragButton.defName}"), !Mouse.IsOver(inRect));
            }
        }

        public static void OnChange()
        {
            allButtonsInOrder.SortBy(x => x.order);
            List<int> indexElasticWidth = new List<int>();
            buttonSizeCache.Clear();
            elasticElements = 0;
            fixedWidth = 0;

            for (int index = 0; index < allButtonsInOrder.Count; ++index)
            {
                if (!allButtonsInOrder[index].Worker.Visible)
                {
                    buttonSizeCache.Add(0);
                    continue;
                }

                // Cambiar para tomar en cuenta ancho fijo
                if (!allButtonsInOrder[index].minimized)
                {
                    buttonSizeCache.Add(-1);
                    indexElasticWidth.Add(index);
                    elasticElements++;
                }
                else
                {
                    buttonSizeCache.Add(minimizedWidth);
                    fixedWidth += minimizedWidth;
                }
            }

            // Algo asi probablemente sirva con multiples barras
            //if (Mouse.IsOver(inRect) && manager.DraggingNow)
            //{
            //    // Cambiar para tomar en cuenta ancho fijo
            //    if (!manager.Dragging.element.minimized) elasticElements++;
            //    else fixedWidth += minimizedWidth;
            //}

            int elasticSpaceAvaible = (int)(Width - fixedWidth);
            int elasticElementWidth = elasticSpaceAvaible / elasticElements;

            foreach (int index in indexElasticWidth)
            {
                buttonSizeCache[index] = elasticElementWidth;
            }

            lastIndex = allButtonsInOrder.FindLastIndex((Predicate<MainButtonDef>)(x => x.Worker.Visible));
            int allButtonsSize = 0;
            buttonSizeCache.Do((size) => allButtonsSize += size);

            buttonSizeCache[lastIndex] += UI.screenWidth - allButtonsSize;

            int minOrder = 0;
            allButtonsInOrder.Do((button) =>
            {
                button.order = minOrder;
                minOrder += 10;
            });

            Persist();
        }

        public static void DrawWithConfigIcon(MainButtonDef button, Rect space)
        {
            if (Widgets.ButtonInvisible(space, false))
            {
                //Open Window
                Find.WindowStack.Add(new Windows.EditMainButton_Window(button));
            }

            button.Worker.DoButton(space);
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
                        proxy.toolbar = 0; // TODO - Multiple Toolbars
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
            //
            MainIconDef testIconDef = DefDatabase<MainIconDef>.AllDefs.First();
            //

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

                        // TODO - multiple toolbars
                    }
                }
            }
        }
    }
}