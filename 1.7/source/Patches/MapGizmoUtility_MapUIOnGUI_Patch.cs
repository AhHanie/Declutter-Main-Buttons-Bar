using HarmonyLib;
using RimWorld;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    // Harmony 3 infix patch: the gizmo offset/scale only applies while the map's gizmo grid is being drawn,
    // so the flag brackets that one call instead of the whole of MapGizmoUtility.MapUIOnGUI.
    [HarmonyPatch(typeof(MapGizmoUtility), nameof(MapGizmoUtility.MapUIOnGUI))]
    public static class MapGizmoUtility_MapUIOnGUI_Patch
    {
        [HarmonyPrefix]
        [HarmonyInfix(typeof(GizmoGridDrawer), nameof(GizmoGridDrawer.DrawGizmoGridFor))]
        public static void BeginMapGizmoGrid()
        {
            GizmoGridDrawer_DrawGizmoGrid_Patch.ApplyOffset = true;
        }

        [HarmonyPostfix]
        [HarmonyInfix(typeof(GizmoGridDrawer), nameof(GizmoGridDrawer.DrawGizmoGridFor))]
        public static void EndMapGizmoGrid()
        {
            GizmoGridDrawer_DrawGizmoGrid_Patch.ApplyOffset = false;
        }
    }
}
