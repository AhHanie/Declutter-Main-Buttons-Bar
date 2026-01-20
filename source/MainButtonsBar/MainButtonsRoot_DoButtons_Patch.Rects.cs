using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static partial class MainButtonsRoot_DoButtons_Patch
    {
        private static Rect GetEditPlusRect(Rect rect)
        {
            float x = rect.xMax - EditPlusSize - 4f;
            float y = rect.y + 2f;
            return new Rect(x, y, EditPlusSize, EditPlusSize);
        }

        private static Rect GetResizeRect(Rect rect)
        {
            float x = rect.xMax - ResizeHandleSize - 2f;
            float y = rect.yMax - ResizeHandleSize - 2f;
            return new Rect(x, y, ResizeHandleSize, ResizeHandleSize);
        }

        private static List<Rect> BuildRects(List<MainButtonDef> defs, Dictionary<MainButtonDef, float> widths, float startX)
        {
            List<Rect> rects = new List<Rect>(defs.Count);
            float curX = startX;
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                float width = widths[def];
                Rect rect = new Rect(curX, UI.screenHeight - BarBottomOffset, width, BarHeight);
                rects.Add(rect);
                curX += width;
            }

            return rects;
        }

        private static List<Rect> BuildRectsWithPositions(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths,
            Dictionary<MainButtonDef, float> xPositions)
        {
            List<Rect> rects = new List<Rect>(defs.Count);
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                float x = xPositions.ContainsKey(def) ? xPositions[def] : 0f;
                float width = widths[def];
                Rect rect = new Rect(x, UI.screenHeight - BarBottomOffset, width, BarHeight);
                rects.Add(rect);
            }

            return rects;
        }
    }
}
