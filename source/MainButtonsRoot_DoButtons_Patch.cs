using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch(typeof(MainButtonsRoot), "DoButtons")]
    public static class MainButtonsRoot_DoButtons_Patch
    {
        public static bool Prefix(MainButtonsRoot __instance)
        {
            List<MainButtonDef> allButtonsInOrder = MainButtonsCache.AllButtonsInOrder;
            if (ModSettings.useFixedWidthMode)
            {
                DrawFixedWidthButtons(allButtonsInOrder);
                return false;
            }

            float visibleUnits = 0f;
            for (int i = 0; i < allButtonsInOrder.Count; i++)
            {
                MainButtonDef def = allButtonsInOrder[i];
                if (def.Worker.Visible && ShouldShowOnBar(def))
                {
                    visibleUnits += def.minimized ? 0.5f : 1f;
                }
            }

            if (visibleUnits <= 0f)
            {
                return false;
            }

            GUI.color = Color.white;

            int baseWidth = (int)((float)UI.screenWidth / visibleUnits);
            int miniWidth = baseWidth / 2;
            int lastVisibleIndex = allButtonsInOrder.FindLastIndex(def => def.Worker.Visible && ShouldShowOnBar(def));
            int curX = 0;

            for (int i = 0; i < allButtonsInOrder.Count; i++)
            {
                MainButtonDef def = allButtonsInOrder[i];
                if (!def.Worker.Visible || !ShouldShowOnBar(def))
                {
                    continue;
                }

                int width = def.minimized ? miniWidth : baseWidth;
                if (i == lastVisibleIndex)
                {
                    width = UI.screenWidth - curX;
                }

                Rect rect = new Rect(curX, UI.screenHeight - 35, width, 36f);
                def.Worker.DoButton(rect);
                curX += width;
            }

            return false;
        }

        private static void DrawFixedWidthButtons(List<MainButtonDef> allButtonsInOrder)
        {
            float width = Mathf.Clamp(ModSettings.fixedButtonWidth, 50f, 200f);
            int maxButtons = Mathf.Max(1, Mathf.FloorToInt(UI.screenWidth / width));
            int drawn = 0;
            int curX = 0;

            MainButtonDef menuDef = MainButtonsMenuDefOf.DMMB_MainButtonsMenu;
            Rect menuRect = new Rect(curX, UI.screenHeight - 35, width, 36f);
            menuDef.Worker.DoButton(menuRect);
            curX += (int)width;
            drawn++;

            for (int i = 0; i < allButtonsInOrder.Count && drawn < maxButtons; i++)
            {
                MainButtonDef def = allButtonsInOrder[i];
                if (def == menuDef)
                {
                    continue;
                }

                if (!def.Worker.Visible || !ShouldShowOnBar(def))
                {
                    continue;
                }

                Rect rect = new Rect(curX, UI.screenHeight - 35, width, 36f);
                def.Worker.DoButton(rect);
                curX += (int)width;
                drawn++;
            }
        }

        private static bool ShouldShowOnBar(MainButtonDef def)
        {
            if (def == MainButtonsMenuDefOf.DMMB_MainButtonsMenu)
            {
                return true;
            }

            return !ModSettings.IsHiddenFromBar(def);
        }
    }
}
