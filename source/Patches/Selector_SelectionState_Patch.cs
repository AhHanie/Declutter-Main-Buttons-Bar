using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class SelectorSelectionState
    {
        public static bool AllSelectedObjectsAreColonists { get; private set; }

        public static void Refresh(Selector selector)
        {
            List<object> selected = selector.SelectedObjectsListForReading;
            if (selected.Count == 0)
            {
                AllSelectedObjectsAreColonists = false;
                return;
            }

            for (int i = 0; i < selected.Count; i++)
            {
                if (!(selected[i] is Pawn pawn) || !pawn.IsColonist)
                {
                    AllSelectedObjectsAreColonists = false;
                    return;
                }
            }

            AllSelectedObjectsAreColonists = true;
        }
    }

    [HarmonyPatch(typeof(Selector), "SelectInternal")]
    public static class Selector_SelectInternal_SelectionState_Patch
    {
        public static bool Prepare()
        {
            return ModSettings.enableSelectorSelectionStatePatches;
        }

        public static void Postfix(Selector __instance)
        {
            SelectorSelectionState.Refresh(__instance);
        }
    }

    [HarmonyPatch(typeof(Selector), "DeselectInternal")]
    public static class Selector_DeselectInternal_SelectionState_Patch
    {
        public static bool Prepare()
        {
            return ModSettings.enableSelectorSelectionStatePatches;
        }

        public static void Postfix(Selector __instance)
        {
            SelectorSelectionState.Refresh(__instance);
        }
    }

    [HarmonyPatch(typeof(Selector), nameof(Selector.ClearSelection))]
    public static class Selector_ClearSelection_SelectionState_Patch
    {
        public static bool Prepare()
        {
            return ModSettings.enableSelectorSelectionStatePatches;
        }

        public static void Postfix(Selector __instance)
        {
            SelectorSelectionState.Refresh(__instance);
        }
    }
}
