using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch]
    public static class Compat_CompositeLoadouts_PlaySettings_Patch
    {
        private const string CompositeLoadoutsPackageId = "wiri.compositableloadouts";
        private const string ColoredButtonIconMethod = "Inventory.GUIUtility:ColoredButtonIcon";

        static bool Prepare()
        {
            return ModsConfig.IsActive(CompositeLoadoutsPackageId);
        }

        static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                ColoredButtonIconMethod,
                new[] { typeof(WidgetRow), typeof(Texture2D), typeof(Func<string>), typeof(Color) });
        }

        public static bool Prefix(Texture2D tex, Func<string> tooltip, ref bool __result)
        {
            if (!MapControlsTableContext.Active)
            {
                if (MapControlsTableContext.SuppressExternal)
                {
                    __result = false;
                    return false;
                }

                return true;
            }

            string tooltipText = tooltip == null ? null : tooltip();
            if (!MapControlsTableContext.MatchesFilter(tooltipText))
            {
                __result = false;
                return false;
            }

            if (MapControlsTableContext.Measuring)
            {
                MapControlsTableRenderer.MeasureRowHeight(tooltipText, false);
                MapControlsTableContext.TotalRows++;
                __result = false;
                return false;
            }

            __result = MapControlsTableRenderer.DrawButtonRow(tex, tooltipText);
            return false;
        }
    }
}
