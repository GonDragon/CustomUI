using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CustomUI.Utility
{
    public class IndividualToolbar
    {
        public Rect inRect;

        public bool empty = true;

        public float fixedWidth = 0f;
        private float fixedWidthEditMode = 0f;

        public int elasticElements = 0;
        private int elasticElementsEditMode = 0;

        public List<MainButtonDef> buttons = new List<MainButtonDef>();
        public List<int> buttonIndexes = new List<int>();
        public List<int> buttonSizes = new List<int>();
        public List<int> buttonSizesEditMode = new List<int>();

        private const int minimizedWidth = 70;

        public IndividualToolbar(Rect rect)
        {
            inRect = rect;
        }

        public void Initialize()
        {
            empty = true;

            buttons.Clear();
            buttonIndexes.Clear();
            buttonSizes.Clear();
            buttonSizesEditMode.Clear();

            elasticElements = 0;
            elasticElementsEditMode = 0;
            fixedWidth = 0f;
            fixedWidthEditMode = 0f;
        }

        public void AddButton(MainButtonDef button, int index)
        {
            buttons.Add(button);
            buttonIndexes.Add(index);
        }

        public void GetSizes(List<int> buttonSizeCache, List<int> buttonSizeCacheEditMode)
        {
            CalculateSizes();

            for (int i = 0; i < buttons.Count; i++)
            {
                buttonSizeCache[buttonIndexes[i]] = buttonSizes[i];
                buttonSizeCacheEditMode[buttonIndexes[i]] = buttonSizesEditMode[i];
            }
        }

        private void CalculateSizes()
        {
            foreach (MainButtonDef button in buttons)
            {
                if (!button.Worker.Visible)
                {
                    if (!button.minimized)
                    {
                        elasticElementsEditMode++;
                        buttonSizesEditMode.Add(-1);
                    } else
                    {
                        fixedWidthEditMode += minimizedWidth;
                        buttonSizesEditMode.Add(minimizedWidth);
                    }

                    buttonSizes.Add(0);
                }
                else
                {
                    if (!button.minimized)
                    {
                        elasticElements++;
                        elasticElementsEditMode++;
                        buttonSizes.Add(-1);
                        buttonSizesEditMode.Add(-1);
                    }
                    else
                    {
                        fixedWidth += minimizedWidth;
                        fixedWidthEditMode += minimizedWidth;
                        buttonSizes.Add(minimizedWidth);
                        buttonSizesEditMode.Add(minimizedWidth);
                    }
                }
            }

            int elasticSize = elasticElements > 0 ? (int)(UIManager.Width - fixedWidth) / elasticElements : 0;
            int elasticSizeEditMode = elasticElementsEditMode > 0 ? (int)(UIManager.Width - fixedWidthEditMode) / elasticElementsEditMode : 0;

            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttonSizes[i] < 0) { buttonSizes[i] = elasticSize; }

                if (buttonSizesEditMode[i] < 0) { buttonSizesEditMode[i] = elasticSizeEditMode; }
            }

            int lastIndex = buttonSizes.FindLastIndex(x => x > 0);
            int lastIndexEditMode = buttonSizesEditMode.FindLastIndex(x => x > 0);

            int allButtonSize = buttonSizes.Sum();
            int allButtonSizeEditMode = buttonSizesEditMode.Sum();

            if (lastIndex >= 0) { buttonSizes[lastIndex] += (int)(UIManager.Width - allButtonSize); }
            if (lastIndexEditMode >= 0) { buttonSizesEditMode[lastIndexEditMode] += (int)(UIManager.Width - allButtonSizeEditMode); }

            if (allButtonSize > 0) empty = false;

        }

    }
}
