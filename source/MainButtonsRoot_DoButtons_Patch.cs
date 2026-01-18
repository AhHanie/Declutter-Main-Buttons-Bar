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
            MainButtonDef menuButtonDef = MainButtonDefOf.Menu;
            bool menuVisible = menuButtonDef != null && menuButtonDef.Worker.Visible;
            bool pinMenuRight = ModSettings.pinMenuButtonRight && menuVisible;
            int reservedMenuWidth = 0;
            if (pinMenuRight)
            {
                reservedMenuWidth = menuButtonDef.minimized ? miniWidth : baseWidth;
            }

            for (int i = 0; i < allButtonsInOrder.Count; i++)
            {
                MainButtonDef def = allButtonsInOrder[i];
                if (!def.Worker.Visible || !ShouldShowOnBar(def))
                {
                    continue;
                }

                if (pinMenuRight && def == menuButtonDef)
                {
                    continue;
                }

                int width = def.minimized ? miniWidth : baseWidth;
                if (i == lastVisibleIndex)
                {
                    width = UI.screenWidth - reservedMenuWidth - curX;
                }

                Rect rect = new Rect(curX, UI.screenHeight - 35, width, 36f);
                def.Worker.DoButton(rect);
                curX += width;
            }

            if (pinMenuRight)
            {
                int width = reservedMenuWidth;
                if (width <= 0)
                {
                    width = baseWidth;
                }

                Rect rect = new Rect(UI.screenWidth - width, UI.screenHeight - 35, width, 36f);
                menuButtonDef.Worker.DoButton(rect);
            }

            return false;
        }

        private static void DrawFixedWidthButtons(List<MainButtonDef> allButtonsInOrder)
        {
            float width = Mathf.Clamp(ModSettings.fixedButtonWidth, 50f, 200f);
            int maxButtons = Mathf.Max(1, Mathf.FloorToInt(UI.screenWidth / width));
            int drawn = 0;
            int curX = 0;
            MainButtonDef vanillaMenuDef = MainButtonDefOf.Menu;
            bool menuVisible = vanillaMenuDef != null && vanillaMenuDef.Worker.Visible;
            bool pinMenuRight = ModSettings.pinMenuButtonRight && menuVisible;

            MainButtonDef menuDef = MainButtonsMenuDefOf.DMMB_MainButtonsMenu;
            if (pinMenuRight)
            {
                int remainingSlots = Mathf.Max(0, maxButtons - 1);
                int maxWidth = Mathf.FloorToInt(UI.screenWidth - width);
                maxButtons = Mathf.Max(1, remainingSlots + 1);

                int maxDrawWidth = Mathf.Max(0, maxWidth);
                int maxDrawn = Mathf.Max(0, Mathf.FloorToInt(maxDrawWidth / width));
                maxDrawn = Mathf.Min(maxDrawn, remainingSlots);
                maxButtons = maxDrawn + 1;
            }

            Rect menuRect = new Rect(curX, UI.screenHeight - 35, width, 36f);
            menuDef.Worker.DoButton(menuRect);
            curX += (int)width;
            drawn++;

            for (int i = 0; i < allButtonsInOrder.Count && drawn < maxButtons; i++)
            {
                MainButtonDef def = allButtonsInOrder[i];
                if (def == menuDef || (pinMenuRight && def == vanillaMenuDef))
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

            if (pinMenuRight)
            {
                Rect menuRectRight = new Rect(UI.screenWidth - width, UI.screenHeight - 35, width, 36f);
                vanillaMenuDef.Worker.DoButton(menuRectRight);
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
