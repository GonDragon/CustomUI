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

        public List<ToolbarElement> elements = new List<ToolbarElement>();
        private List<ToolbarElement> _existingElements = new List<ToolbarElement>();
        public List<int> elementIndexes = new List<int>();
        public List<int> elementSizes = new List<int>();
        public List<int> elementSizesEditMode = new List<int>();

        public IndividualToolbar(Rect rect)
        {
            inRect = rect;
        }

        public void Initialize()
        {
            empty = true;

            elements.Clear();
            elementIndexes.Clear();
            elementSizes.Clear();
            elementSizesEditMode.Clear();

            elasticElements = 0;
            elasticElementsEditMode = 0;
            fixedWidth = 0f;
            fixedWidthEditMode = 0f;
        }

        public void AddButton(ToolbarElement element, int index)
        {
            elements.Add(element);
            elementIndexes.Add(index);
        }

        public void GetSizes(List<int> buttonSizeCache, List<int> buttonSizeCacheEditMode)
        {
            _existingElements.Clear();
            _existingElements.AddRange(elements.Where(x => x.Exist));
            CustomUI.ErrorOnce($"Elementos existentes: {_existingElements.Count}", "ExistingElements_Count");

            CalculateSizes();


            for (int i = 0; i < _existingElements.Count; i++)
            {
                buttonSizeCache[elementIndexes[i]] = elementSizes[i];
                buttonSizeCacheEditMode[elementIndexes[i]] = elementSizesEditMode[i];
            }
        }

        private void CalculateSizes()
        {
            CustomUI.ErrorOnce("Inicio de CalculateSizes", "CalculateSizes_Start");

            foreach (ToolbarElement element in _existingElements)
            {
                if (!element.Worker.Visible)
                {
                    if (element.IsFixed)
                    {
                        fixedWidthEditMode += element.FixedSize;
                        elementSizesEditMode.Add(element.FixedSize);
                        CustomUI.ErrorOnce($"Elemento fijo no visible: {element.defName}, Tamaño: {element.FixedSize}", $"FixedInvisible_{element.defName}");
                    }
                    //else if (Settings.elementManager.HasFixedSize(element.defName))
                    //{
                    //    int fixedSize = Settings.elementManager.GetFixedSize(element.defName);
                    //    fixedWidthEditMode += fixedSize;
                    //    elementSizesEditMode.Add(fixedSize);
                    //}
                    else
                    {
                        elasticElementsEditMode++;
                        elementSizesEditMode.Add(-1);
                        CustomUI.ErrorOnce($"Elemento elástico no visible: {element.defName}", $"ElasticInvisible_{element.defName}");
                    }

                    elementSizes.Add(0);
                }
                else
                {
                    if (element.IsFixed)
                    {
                        fixedWidth += element.FixedSize;
                        fixedWidthEditMode += element.FixedSize;
                        elementSizes.Add(element.FixedSize);
                        elementSizesEditMode.Add(element.FixedSize);
                        CustomUI.ErrorOnce($"Elemento fijo visible: {element.defName}, Tamaño: {element.FixedSize}", $"FixedVisible_{element.defName}");
                    }
                    else if (Settings.elementManager.HasFixedSize(element.defName))
                    {
                        int fixedSize = Settings.elementManager.GetFixedSize(element.defName);

                        fixedWidth += fixedSize;
                        fixedWidthEditMode += fixedSize;

                        elementSizes.Add(fixedSize);
                        elementSizesEditMode.Add(fixedSize);
                        CustomUI.ErrorOnce($"Elemento con tamaño fijo en Settings: {element.defName}, Tamaño: {fixedSize}", $"FixedSizeSettings_{element.defName}");
                    }
                    else
                    {
                        elasticElements++;
                        elasticElementsEditMode++;
                        elementSizes.Add(-1);
                        elementSizesEditMode.Add(-1);
                        CustomUI.ErrorOnce($"Elemento elástico visible: {element.defName}", $"ElasticVisible_{element.defName}");
                    }
                }
            }

            int elasticSize = elasticElements > 0 ? (int)(UIManager.Width - fixedWidth) / elasticElements : 0;
            int elasticSizeEditMode = elasticElementsEditMode > 0 ? (int)(UIManager.Width - fixedWidthEditMode) / elasticElementsEditMode : 0;
            CustomUI.ErrorOnce($"Tamaño elástico: {elasticSize}, Tamaño elástico en modo edición: {elasticSizeEditMode}", "ElasticSizes");

            for (int i = 0; i < _existingElements.Count; i++)
            {
                if (elementSizes[i] < 0) { elementSizes[i] = elasticSize; }

                if (elementSizesEditMode[i] < 0) { elementSizesEditMode[i] = elasticSizeEditMode; }

                CustomUI.ErrorOnce($"Asignando tamaño elástico: Elemento {i}, Tamaño: {elementSizes[i]}, Tamaño en modo edición: {elementSizesEditMode[i]}", $"AssignElasticSize_{i}");
            }

            int lastIndex = elementSizes.FindLastIndex(x => x > 0);
            int lastIndexEditMode = elementSizesEditMode.FindLastIndex(x => x > 0);

            int allButtonSize = elementSizes.Sum();
            int allButtonSizeEditMode = elementSizesEditMode.Sum();

            if (lastIndex >= 0) { elementSizes[lastIndex] += (int)(UIManager.Width - allButtonSize); }
            if (lastIndexEditMode >= 0) { elementSizesEditMode[lastIndexEditMode] += (int)(UIManager.Width - allButtonSizeEditMode); }

            CustomUI.ErrorOnce($"Último índice: {lastIndex}, Último índice en modo edición: {lastIndexEditMode}", "LastIndex");
            CustomUI.ErrorOnce($"Tamaño total de botones: {allButtonSize}, Tamaño total de botones en modo edición: {allButtonSizeEditMode}", "TotalButtonSize");

            if (allButtonSize > 0) empty = false;

            CustomUI.ErrorOnce("","");

        }

    }
}
