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

        //TODO - Convertir en lista de structs, para múltiples barras.
        private static float fixedWidth = 0f;
        private static int elasticElements = 0;
        private static Rect inRect = new Rect(0f,UI.screenHeight - Height, Width, Height);


        static CustomToolbar()
        {
            allButtonsInOrder = (List<MainButtonDef>)AccessTools.Field(typeof(MainButtonsRoot),"allButtonsInOrder").GetValue(Find.MainButtonsRoot);
            manager = new DragManager<MainButtonDef>((button, topLeft, width) => button.Worker.DoButton(new Rect(topLeft, new Vector2(width, Height))));

            OnChange();
        }

        public static void OnGui()
        {
            int elasticSpaceAvaible = (int)(Width - fixedWidth);
            int elasticElementWidth = elasticSpaceAvaible / elasticElements;

            GUI.color = Color.white;
            int lastIndex = allButtonsInOrder.FindLastIndex((Predicate<MainButtonDef>)(x => x.Worker.Visible));
            int curX = 0;
            int draggedWidth = 0;
            for (int index = 0; index < allButtonsInOrder.Count; ++index)
            {
                MainButtonDef button = allButtonsInOrder[index];

                if (!button.Worker.Visible) continue;

                int width = (int)(button.minimized ? minimizedWidth : elasticElementWidth);
                if (index == lastIndex)
                    width = UI.screenWidth - curX;

                Rect buttonRect = new Rect(curX, (UI.screenHeight - Height), width, Height);

                //* Edit Mode ONLY - Once per Button *//
                if (UIManager.editionModeEnabled)
                {
                    bool mouseOverButton = Mouse.IsOver(buttonRect);
                    if (manager.Dragging == button) 
                    {
                        draggedWidth = width;
                        if (!Mouse.IsOver(inRect))
                        {
                            int offset = draggedWidth;
                            buttonRect.x += offset;
                            curX += offset;
                        }                        
                        continue;
                    }
                    if (mouseOverButton)
                    {
                        if (manager.DraggingNow)
                        {
                            manager.mouseoverIdx = index;

                            DrawWithConfigIcon(manager.Dragging, buttonRect);

                            int offset = manager.Dragging.minimized ? minimizedWidth : elasticElementWidth;
                            buttonRect.x += offset;
                            curX += offset;
                        }
                    }
                    

                    if (Mouse.IsOver(buttonRect) && Input.GetMouseButtonDown(1))
                    {
                        CustomUI.Log("Edit");
                        Event.current.Use();
                    }
                    if (manager.TryStartDrag(button, buttonRect)) CustomUI.Log("TryDrag");

                    DrawWithConfigIcon(button, buttonRect);

                    manager.DropLocation(inRect, null, dragButton =>
                    {
                        CustomUI.Log($"MouseoverID: {manager.mouseoverIdx} - Botton: {dragButton.defName}");
                        return true;
                    });


                } else
                {
                    button.Worker.DoButton(buttonRect);
                }

                curX += width;
            }

            //* Edit Mode ONLY - Just Once *//
            if (UIManager.editionModeEnabled)
            {
                if (manager.DraggingNow)
                {
                    manager.DragDropOnGUI((dragButton) => CustomUI.Log($"Stop Drag For {dragButton.defName}"), !Mouse.IsOver(inRect), draggedWidth);
                }
            }
        }

        public static void OnChange()
        {
            elasticElements = 0;
            fixedWidth = 0;

            foreach(MainButtonDef button in allButtonsInOrder)
            {
                if (!button.Worker.Visible) continue;
                
                // Cambiar para tomar en cuenta ancho fijo
                if (!button.minimized) elasticElements++;
                else fixedWidth += minimizedWidth;
            }

            // Si está sobre una de las barras
            if (Mouse.IsOver(inRect) && manager.DraggingNow)
            {
                // Cambiar para tomar en cuenta ancho fijo
                if (!manager.Dragging.minimized) elasticElements++;
                else fixedWidth += minimizedWidth;
            }
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
