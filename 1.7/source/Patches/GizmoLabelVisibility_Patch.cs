using HarmonyLib;
using RimWorld;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class GizmoLabelVisibility
    {
        public static bool HideLabels =>
            ModSettings.hideAllGizmoLabels
            || (ModSettings.hideGizmoLabelsForSelectedColonistsOnly
                && SelectorSelectionState.AllSelectedObjectsAreColonists);

        // Labels are only hidden for commands that have a real icon, so a command without one stays identifiable.
        public static bool ShouldHideLabelsFor(Command command)
        {
            return HideLabels && command.icon != null && command.icon != BaseContent.BadTex;
        }
    }

    // Harmony 3 infix patches: each one wraps a single getter call inside Command.GizmoOnGUIInt, so no IL scanning is needed.
    // The getter results are blanked, and vanilla then skips drawing the (null or empty) label itself.
    [HarmonyPatch(typeof(Command), "GizmoOnGUIInt")]
    public static class Command_GizmoOnGUIInt_HideLabels_Patch
    {
        public static bool Prepare()
        {
            return ModSettings.enableGizmoLabelVisibilityPatches;
        }

        [HarmonyPostfix]
        [HarmonyInfix(typeof(Command), nameof(Command.TopRightLabel), InnerTargetKind.Getter)]
        public static string HideTopRightLabel(string __result, [HarmonyOuter] Command __instance)
        {
            return GizmoLabelVisibility.ShouldHideLabelsFor(__instance) ? null : __result;
        }

        [HarmonyPostfix]
        [HarmonyInfix(typeof(Command), nameof(Command.LabelCap), InnerTargetKind.Getter)]
        public static string HideLabelCap(string __result, [HarmonyOuter] Command __instance)
        {
            return GizmoLabelVisibility.ShouldHideLabelsFor(__instance) ? null : __result;
        }
    }

    // Command_Ability.GizmoOnGUI reads Ability.CooldownTicksRemaining four times. The third read guards the
    // cooldown text overlay (the cooldown bar is drawn by the first two), so only that one is blanked.
    [HarmonyPatch(typeof(Command_Ability), nameof(Command_Ability.GizmoOnGUI))]
    public static class Command_Ability_GizmoOnGUI_HideCooldownText_Patch
    {
        public static bool Prepare()
        {
            return ModSettings.enableGizmoLabelVisibilityPatches;
        }

        [HarmonyPostfix]
        [HarmonyInfix(typeof(Ability), nameof(Ability.CooldownTicksRemaining), InnerTargetKind.Getter, Positions = new[] { 3 })]
        public static int HideCooldownText(int __result)
        {
            return GizmoLabelVisibility.HideLabels ? 0 : __result;
        }
    }
}
