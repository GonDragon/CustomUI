using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace CustomUI.Utility
{
    public class ElementManager : IExposable
    {
        public static List<string> toolbarDefnames = new List<string>();
        public static List<int> toolbarValues = new List<int>();
        public static List<int> toolbarFixedWidths = new List<int>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref toolbarDefnames, "toolbarDefnames", LookMode.Value);
            Scribe_Collections.Look(ref toolbarValues, "toolbarValues", LookMode.Value);
            Scribe_Collections.Look(ref toolbarFixedWidths, "toolbarFixedWidths", LookMode.Value);
        }

        public void AddDef(string defName)
        {
            toolbarDefnames.Add(defName);
            toolbarValues.Add(0);
            toolbarFixedWidths.Add(-1);
        }

        public bool DefExist(string defName)
        {
            return toolbarDefnames.Contains(defName);
        }

        public int GetToolbar(string defName)
        {
            return toolbarValues[toolbarDefnames.IndexOf(defName)];
        }

        public void SetToolbar(string defName, int toolbar)
        {
            toolbarValues[toolbarDefnames.IndexOf(defName)] = toolbar;
        }

        public int GetFixedSize(string defName)
        {
            return toolbarFixedWidths[toolbarDefnames.IndexOf(defName)];
        }

        public void SetFixedSize(string defName, int size)
        {
            toolbarFixedWidths[toolbarDefnames.IndexOf(defName)] = size;
        }

        public bool HasFixedSize(string defName)
        {
            return toolbarFixedWidths[toolbarDefnames.IndexOf(defName)] > 0;
        }
    }
}
