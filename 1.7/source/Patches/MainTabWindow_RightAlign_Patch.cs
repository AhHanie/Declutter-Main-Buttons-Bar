using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class MainTabWindow_RightAlign_Eligibility
    {
        public static Type lastWindowOpenedFromMenuType;

        public static void NotifyMainButtonOpenedFromMenu(MainButtonDef def)
        {
            Type openedType = def.tabWindowClass;
            if (openedType == null || openedType == typeof(MainTabWindow_MainButtonsMenu))
            {
                return;
            }

            lastWindowOpenedFromMenuType = openedType;
        }

        public static bool ShouldAlign(MainTabWindow window)
        {
            if (!ModSettings.pinMainButtonsMenuWindowRight || window == null)
            {
                return false;
            }

            if (window is MainTabWindow_MainButtonsMenu)
            {
                return true;
            }

            if (window is MainTabWindow_Research)
            {
                return false;
            }

            if (window is MainTabWindow_Inspect)
            {
                return false;
            }

            if (lastWindowOpenedFromMenuType == null)
            {
                return false;
            }

            if (window.GetType() != lastWindowOpenedFromMenuType)
            {
                return false;
            }

            return true;
        }

        public static void AlignToRight(MainTabWindow window)
        {
            if (!ShouldAlign(window))
            {
                return;
            }

            Rect rect = window.windowRect;
            rect.x = Mathf.Max(0f, UI.screenWidth - rect.width);
            window.windowRect = rect;
        }
    }

    [HarmonyPatch(typeof(MainTabWindow), "SetInitialSizeAndPosition")]
    public static class MainTabWindow_SetInitialSizeAndPosition_RightAlign_Patch
    {
        public static void Postfix(MainTabWindow __instance)
        {
            MainTabWindow_RightAlign_Eligibility.AlignToRight(__instance);
        }
    }

    [HarmonyPatch(typeof(MainTabWindow), "PostOpen")]
    public static class MainTabWindow_PostOpen_RightAlign_Patch
    {
        public static void Postfix(MainTabWindow __instance)
        {
            MainTabWindow_RightAlign_Eligibility.AlignToRight(__instance);
        }
    }
}
