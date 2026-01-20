using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static partial class MainButtonsRoot_DoButtons_Patch
    {
        private static void UpdateAndDrawDropdown(MainButtonDef hoveredDef, Rect hoveredRect)
        {
            if (ModSettings.editDropdownsMode)
            {
                ClearDropdownState();
                return;
            }

            if (hoveredDef != null)
            {
                openDropdownDef = hoveredDef;
                openDropdownButtonRect = hoveredRect;
            }

            if (openDropdownDef == null)
            {
                return;
            }

            if (!openDropdownDef.Worker.Visible || !ShouldShowOnBar(openDropdownDef))
            {
                ClearDropdownState();
                return;
            }

            List<MainButtonDef> entries = ModSettings.GetDropdownEntries(openDropdownDef);
            if (entries.Count == 0)
            {
                ClearDropdownState();
                return;
            }

            Rect dropdownRect = CalculateDropdownRect(openDropdownButtonRect, entries.Count);
            openDropdownRect = dropdownRect;
            DrawDropdownList(dropdownRect, entries);

            if (!IsMouseOverDropdownArea(openDropdownButtonRect, dropdownRect))
            {
                ClearDropdownState();
            }
        }

        private static Rect CalculateDropdownRect(Rect buttonRect, int entryCount)
        {
            float height = DropdownRowHeight * entryCount;
            float width = buttonRect.width;
            float x = buttonRect.x;
            float y = buttonRect.y - height - 4f;
            if (y < 0f)
            {
                y = buttonRect.yMax + 4f;
            }

            return new Rect(x, y, width, height);
        }

        private static bool IsMouseOverDropdownArea(Rect buttonRect, Rect dropdownRect)
        {
            if (Mouse.IsOver(buttonRect) || Mouse.IsOver(dropdownRect))
            {
                return true;
            }

            float gapYMin = Mathf.Min(buttonRect.yMin, dropdownRect.yMin);
            float gapYMax = Mathf.Max(buttonRect.yMax, dropdownRect.yMax);
            Rect bridgeRect = new Rect(dropdownRect.x, gapYMin, dropdownRect.width, gapYMax - gapYMin);
            return Mouse.IsOver(bridgeRect);
        }

        private static void DrawDropdownList(Rect rect, List<MainButtonDef> entries)
        {
            float curY = rect.y;
            for (int i = 0; i < entries.Count; i++)
            {
                MainButtonDef def = entries[i];
                Rect rowRect = new Rect(rect.x, curY, rect.width, DropdownRowHeight);
                def.Worker.DoButton(rowRect);
                curY += DropdownRowHeight;
            }
        }
    }
}
