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
            if (!ModsConfig.IsActive(CompositeLoadoutsPackageId))
            {
                return false;
            }

            if (TargetMethod() != null)
            {
                return true;
            }

            Log.Warning("[DeclutterMainButtonsBar] Composite Loadouts is active but " + ColoredButtonIconMethod
                + " was not found (its 1.7 build may differ). Its button will not be handled by the searchable play settings.");
            return false;
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
