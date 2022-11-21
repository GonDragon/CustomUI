using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using HarmonyLib;
using RimWorld;
using System;

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
        private static Rect inRect = new Rect(0f,UI.screenHeight - Height, Width, Height);


        static CustomToolbar()
        {
            allButtonsInOrder = (List<MainButtonDef>)AccessTools.Field(typeof(MainButtonsRoot),"allButtonsInOrder").GetValue(Find.MainButtonsRoot);
            manager = new DragManager<MainButtonDef>((button, topLeft, width) => button.Worker.DoButton(new Rect(topLeft, new Vector2(width, Height))));
            buttonSizeCache = new List<int>();

            OnChange();
        }

        public static void OnGui()
        {

            GUI.color = Color.white;
            int curX = 0;
            bool shouldDrawAtEnd = true;
            int replaceIndex = 0;
            bool isLastIndex = false;
            for (int index = 0; index < allButtonsInOrder.Count; ++index)
            {
                MainButtonDef button = allButtonsInOrder[index];

                if (!button.Worker.Visible) continue;

                Rect buttonRect = new Rect(curX, (UI.screenHeight - Height), buttonSizeCache[index], Height);

                //* Edit Mode ONLY - Once per Button *//
                if (UIManager.editionModeEnabled)
                {
                    bool mouseOverButton = Mouse.IsOver(buttonRect);
                    bool mouseOverBar = Mouse.IsOver(inRect);

                    if(manager.DraggingNow)
                    {
                        if (manager.Dragging.index == index)
                        {
                            if(!mouseOverBar)
                            {
                                int offset = manager.Dragging.width;
                                buttonRect.x += offset;
                                curX += offset;
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
                            curX += manager.Dragging.width;
                        }
                    }                    

                    if (mouseOverButton && Input.GetMouseButtonDown(1))
                    {
                        CustomUI.Log("Edit");
                        Event.current.Use();
                    }
                    if (manager.TryStartDrag(button, buttonRect, index)) CustomUI.Log("TryDrag");

                    DrawWithConfigIcon(button, buttonRect);

                } else
                {
                    button.Worker.DoButton(buttonRect);
                }

                curX += buttonSizeCache[index];
            }

            //* Edit Mode ONLY - Just Once *//
            if (UIManager.editionModeEnabled)
            {

                if (manager.DraggingNow)
                {
                    if (shouldDrawAtEnd)
                    {
                        Rect draggedRect = new Rect(curX, (UI.screenHeight - Height), manager.Dragging.width, Height);
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
                        }
                        CustomUI.Log($"Dragged Index: {dragButton.index} - Displaced Index: {replaceIndex}");
                        CustomUI.Warning($"DraggedButton: {dragButton.element.defName} - DisplacedButton: {allButtonsInOrder[replaceIndex]}");
                        return true;
                    });

                    manager.DragDropOnGUI((dragButton) => CustomUI.Log($"Stop Drag For {dragButton.defName}"), !Mouse.IsOver(inRect));
                }
            }
        }

        public static void OnChange()
        {
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
        }

        public static void DrawWithConfigIcon(MainButtonDef button, Rect space)
        {
            button.Worker.DoButton(space);
            GUI.BeginGroup(space);

            float configSizef = space.height - 8f;
            Rect configSpace = new Rect(space.width - configSizef - 4f, 4f, configSizef, configSizef);
            GUI.DrawTexture(configSpace, Textures.configIcon);

            GUI.EndGroup();
        }
    }
}
