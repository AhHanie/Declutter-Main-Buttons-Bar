using HarmonyLib;
using RimWorld;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch(typeof(MapGizmoUtility), "MapUIOnGUI")]
    public static class MapGizmoUtility_MapUIOnGUI_Patch
    {
        public static void Prefix()
        {
            GizmoGridDrawer_DrawGizmoGrid_Patch.ApplyOffset = true;
        }

        public static void Postfix()
        {
            GizmoGridDrawer_DrawGizmoGrid_Patch.ApplyOffset = false;
        }
    }
}
