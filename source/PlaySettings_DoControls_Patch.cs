using HarmonyLib;
using RimWorld;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch(typeof(PlaySettings), "DoMapControls")]
    [HarmonyPriority(Priority.Last)]
    public static class PlaySettings_DoMapControls_Patch
    {
        public static void Postfix(WidgetRow row)
        {
            PlaySettingsToggleDrawer.DrawMapControlsMenuButton(row);
            PlaySettingsToggleDrawer.DrawDropdownEditToggle(row);
        }
    }

    [HarmonyPatch(typeof(PlaySettings), "DoWorldViewControls")]
    public static class PlaySettings_DoWorldViewControls_Patch
    {
        public static void Postfix(WidgetRow row)
        {
            PlaySettingsToggleDrawer.DrawDropdownEditToggle(row);
        }
    }

    public static class PlaySettingsToggleDrawer
    {
        public static void DrawMapControlsMenuButton(WidgetRow row)
        {
            if (!ModSettings.useSearchablePlaySettingsMenu)
            {
                return;
            }

            if (MapControlsTableContext.Active)
            {
                return;
            }

            string tooltip = "DMMB.PlaySettingsMenuButton".Translate();
            bool previousSuppress = MapControlsTableContext.SuppressExternal;
            MapControlsTableContext.SuppressExternal = false;
            bool clicked = row.ButtonIcon(DMMBTextures.PlaySettingsTable.Texture, tooltip);
            MapControlsTableContext.SuppressExternal = previousSuppress;
            if (clicked)
            {
                Find.WindowStack.Add(new MapControlsTableWindow());
            }
        }

        public static void DrawDropdownEditToggle(WidgetRow row)
        {
            if (ModSettings.useSearchablePlaySettingsMenu && !MapControlsTableContext.Active)
            {
                return;
            }

            bool editMode = ModSettings.editDropdownsMode;
            string tooltip = "DMMB.PlaySettingsEditDropdowns".Translate();
            row.ToggleableIcon(ref editMode, DMMBTextures.UiToggle.Texture, tooltip, SoundDefOf.Mouseover_ButtonToggle);
            if (editMode != ModSettings.editDropdownsMode)
            {
                ModSettings.editDropdownsMode = editMode;
                if (!editMode)
                {
                    MainButtonsRoot_DoButtons_Patch.ClearDropdownState();
                    Mod.Settings.Write();
                }
            }
        }
    }
}
