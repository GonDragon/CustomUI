using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CustomUI.Utility.Workers
{
    public abstract class WidgetWorker
    {
        public float IconSize => CustomToolbar.Height - 9f;

        public Func<bool> Visible = () => { return true; };
        public virtual bool FixedWidth => false;
        public virtual float Width => 100f;

        public Rect DrawIcon(Texture2D icon, float curX, float curY, string tooltip = null)
        {
            Rect rect = new Rect(curX, curY, IconSize, IconSize);
            GUI.DrawTexture(rect, icon);
            if (!tooltip.NullOrEmpty())
                TooltipHandler.TipRegion(rect, (TipSignal)tooltip);
            return rect;
        }

        public virtual void Margins(ref Rect rect)
        {
            rect = rect.ContractedBy(CustomToolbar.margin);
        }

        public abstract void OnGUI(Rect rect);

        public virtual void OpenConfigWindow()
        { }

        public virtual void Padding(ref Rect rect)
        {
            rect = rect.ContractedBy(CustomToolbar.padding);
        }
    }
}
