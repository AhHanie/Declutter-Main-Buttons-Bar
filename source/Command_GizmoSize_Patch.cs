using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch(typeof(Command), nameof(Command.GetWidth))]
    public static class Command_GetWidth_Patch
    {
        public static void Postfix(ref float __result)
        {
            if (!ModSettings.gizmoScaleMapOnly || GizmoGridDrawer_DrawGizmoGrid_Patch.ApplyOffset)
            {
                __result *= ModSettings.gizmoDrawerScale;
            }
        }
    }

    [HarmonyPatch(typeof(Command), "GizmoOnGUIInt")]
    public static class Command_GizmoOnGUI_Patch
    {
        public static void Prefix(ref Rect butRect)
        {
            if (!ModSettings.gizmoScaleMapOnly || GizmoGridDrawer_DrawGizmoGrid_Patch.ApplyOffset)
            {
                butRect.height *= ModSettings.gizmoDrawerScale;
            }
        }
    }
}
