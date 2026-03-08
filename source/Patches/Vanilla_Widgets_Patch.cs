using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoDate))]
    public static class GlobalControlsUtility_DoDate_DisableVanillaPatch
    {
        public static bool Prefix()
        {
            return !ModSettings.disableVanillaDateReadout;
        }
    }

    [HarmonyPatch(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoTimespeedControls))]
    public static class GlobalControlsUtility_DoTimespeedControls_DisableVanillaPatch
    {
        public static bool Prefix()
        {
            return !ModSettings.disableVanillaTimeControls;
        }
    }

    [HarmonyPatch(typeof(WeatherManager), nameof(WeatherManager.DoWeatherGUI))]
    public static class WeatherManager_DoWeatherGUI_DisableVanillaPatch
    {
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
        public static bool Prefix()
        {
            return !ModSettings.disableVanillaConditionsWidget;
        }
    }

    [HarmonyPatch(typeof(GlobalControls), "TemperatureString")]
    public static class GlobalControls_TemperatureString_DisableVanillaPatch
    {
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
