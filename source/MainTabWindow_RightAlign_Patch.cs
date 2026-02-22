using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class MainTabWindow_RightAlign_Eligibility
    {
        private static Type lastWindowOpenedFromMenuType;

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
    }

    [HarmonyPatch(typeof(MainTabWindow), "SetInitialSizeAndPosition")]
    public static class MainTabWindow_SetInitialSizeAndPosition_RightAlign_Patch
    {
        public static void Postfix(MainTabWindow __instance)
        {
            if (!MainTabWindow_RightAlign_Eligibility.ShouldAlign(__instance))
            {
                return;
            }

            Rect rect = __instance.windowRect;
            rect.x = Mathf.Max(0f, UI.screenWidth - rect.width);
            __instance.windowRect = rect;
        }
    }

    [HarmonyPatch(typeof(MainTabWindow), "PostOpen")]
    public static class MainTabWindow_PostOpen_RightAlign_Patch
    {
        public static void Postfix(MainTabWindow __instance)
        {
            if (!MainTabWindow_RightAlign_Eligibility.ShouldAlign(__instance))
            {
                return;
            }

            Rect rect = __instance.windowRect;
            rect.x = Mathf.Max(0f, UI.screenWidth - rect.width);
            __instance.windowRect = rect;
        }
    }
}
