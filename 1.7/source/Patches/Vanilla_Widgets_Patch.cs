using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoDate))]
    public static class GlobalControlsUtility_DoDate_DisableVanillaPatch
    {
        public static bool Prepare()
        {
            return ModSettings.enableVanillaWidgetPatches;
        }

        public static bool Prefix()
        {
            return !ModSettings.disableVanillaDateReadout;
        }
    }

    [HarmonyPatch(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoTimespeedControls))]
    public static class GlobalControlsUtility_DoTimespeedControls_DisableVanillaPatch
    {
        public static bool Prepare()
        {
            return ModSettings.enableVanillaWidgetPatches;
        }

        public static bool Prefix()
        {
            return !ModSettings.disableVanillaTimeControls;
        }
    }

    [HarmonyPatch(typeof(WeatherManager), nameof(WeatherManager.DoWeatherGUI))]
    public static class WeatherManager_DoWeatherGUI_DisableVanillaPatch
    {
        public static bool Prepare()
        {
            return ModSettings.enableVanillaWidgetPatches;
        }

        public static bool Prefix(ref Rect rect)
        {
            if (!ModSettings.disableVanillaWeatherWidget)
            {
                return true;
            }

            rect.height = 0f;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameConditionManager), nameof(GameConditionManager.DoConditionsUI))]
    public static class GameConditionManager_DoConditionsUI_DisableVanillaPatch
    {
        public static bool Prepare()
        {
            return ModSettings.enableVanillaWidgetPatches;
        }

        public static bool Prefix()
        {
            return !ModSettings.disableVanillaConditionsWidget;
        }
    }

    [HarmonyPatch(typeof(ResourceReadout), nameof(ResourceReadout.ResourceReadoutOnGUI))]
    public static class ResourceReadout_ResourceReadoutOnGUI_VisibilityPatch
    {
        public static bool Prepare()
        {
            return ModSettings.enableVanillaWidgetPatches;
        }

        public static bool Prefix()
        {
            if (ModSettings.disableVanillaResourceReadout)
            {
                return false;
            }

            if (!ModSettings.revealVanillaResourceReadoutOnHover)
            {
                return true;
            }

            return Mouse.IsOver(GetHoverRect());
        }

        private static Rect GetHoverRect()
        {
            bool categorized = Prefs.ResourceReadoutCategorized;
            return new Rect(
                categorized ? 2f : 7f,
                0f,
                categorized ? 124f : 110f,
                UI.screenHeight);
        }
    }

    [HarmonyPatch(typeof(MouseoverReadout), nameof(MouseoverReadout.MouseoverReadoutOnGUI))]
    public static class MouseoverReadout_MouseoverReadoutOnGUI_DisableVanillaPatch
    {
        public static bool Prepare()
        {
            return ModSettings.enableVanillaWidgetPatches;
        }

        public static bool Prefix()
        {
            return !ModSettings.disableVanillaMouseoverReadout;
        }
    }

    [HarmonyPatch(typeof(GlobalControls), "TemperatureString")]
    public static class GlobalControls_TemperatureString_DisableVanillaPatch
    {
        public static bool Prepare()
        {
            return ModSettings.enableVanillaWidgetPatches;
        }

        public static bool Prefix(ref string __result)
        {
            if (!ModSettings.disableVanillaTemperatureWidget)
            {
                return true;
            }

            __result = string.Empty;
            return false;
        }
    }
}
