using System;
using UnityEngine;
using Verse;

namespace CustomUI.Utility
{
    public class DragManager<T>
    {
        private readonly Action<T, Vector2, int> drawDragged;

        private T active;

        private Vector2 dragOffset;

        private Vector3 lastClickPos;

#if DEBUG
        [TweakValue("DragManager - DragStartDistanceSquared", 0.0f, 100f)]
#endif
        public static float DragStartDistanceSquared = 20f;

        //PLACEHOLDER
        public int mouseoverIdx = 0;

        public bool DraggingNow => Dragging.element != null;
        public DragElement<T> Dragging = new DragElement<T>();
        public DragManager(Action<T, Vector2, int> drawDragged)
        {
            this.drawDragged = drawDragged;
        }

        public bool TryStartDrag(T draggee, Rect rect)
        {
            if (DraggingNow)
            {
                return false;
            }

            if (Mouse.IsOver(rect) && Input.GetMouseButtonDown(0))
            {
                lastClickPos = Input.mousePosition;
                active = draggee;
            }

            if (Input.GetMouseButtonUp(0))
            {
                active = default;
            }

            if (Input.GetMouseButton(0) && (lastClickPos - Input.mousePosition).sqrMagnitude > DragStartDistanceSquared && object.Equals(draggee, active))
            {
                Dragging.element = draggee;
                Dragging.width = (int) rect.width;
                dragOffset = rect.position - UI.MousePositionOnUIInverted;
                return true;
            }

            return false;
        }

        public void DropLocation(Rect rect, Action<T> onOver, Func<T, bool> onDrop)
        {
            if (Mouse.IsOver(rect) && Dragging.element != null)
            {
                onOver?.Invoke(Dragging.element);
                if (!Input.GetMouseButton(0) && onDrop(Dragging.element))
                {
                    dragOffset = Vector2.zero;
                    Dragging = default;
                }
            }
        }

        public void DragDropOnGUI(Action<T> onDragStop, bool shouldDraw)
        {
            if (Dragging.element != null)
            {
                if (Input.GetMouseButton(0))
                {
                    if(shouldDraw) drawDragged(Dragging.element, dragOffset + UI.MousePositionOnUIInverted, Dragging.width);
                    return;
                }

                onDragStop(Dragging.element);
                dragOffset = Vector2.zero;
                Dragging = default;
            }
        }
    }

    public struct DragElement<T>
    {
        public int pos;
        public T element;
        public int width;
    }
}
