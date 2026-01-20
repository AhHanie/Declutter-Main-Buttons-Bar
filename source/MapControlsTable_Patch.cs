using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class MapControlsTableContext
    {
        public static bool Active;
        public static bool Measuring;
        public static string SearchText = string.Empty;
        public static float RowHeight;
        public static float MinRowHeight;
        public static float MaxRowHeight;
        public static float ViewWidth;
        public static float CurY;
        public static int TotalRows;
        public static Rect LastRowRect;
        public static bool SuppressExternal;

        public static void BeginMeasure(string searchText, float minRowHeight, float viewWidth)
        {
            Active = true;
            Measuring = true;
            SearchText = searchText ?? string.Empty;
            MinRowHeight = minRowHeight;
            MaxRowHeight = minRowHeight;
            ViewWidth = viewWidth;
            CurY = 0f;
            TotalRows = 0;
            LastRowRect = default;
        }

        public static void BeginRender(string searchText, float rowHeight, float viewWidth)
        {
            Active = true;
            Measuring = false;
            SearchText = searchText ?? string.Empty;
            RowHeight = rowHeight;
            ViewWidth = viewWidth;
            CurY = 0f;
            LastRowRect = default;
        }

        public static void End()
        {
            Active = false;
            Measuring = false;
        }

        public static bool MatchesFilter(string tooltip)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return true;
            }

            if (string.IsNullOrEmpty(tooltip))
            {
                return false;
            }

            return tooltip.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static Rect NextRowRect()
        {
            return new Rect(0f, CurY, ViewWidth, RowHeight);
        }

        public static void AdvanceRow()
        {
            CurY += RowHeight;
        }
    }

    [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
    public static class MapControlsTable_DoPlaySettingsGlobalControls_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix()
        {
            if (ModSettings.useSearchablePlaySettingsMenu && !MapControlsTableContext.Active)
            {
                MapControlsTableContext.SuppressExternal = true;
            }

            return true;
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix()
        {
            MapControlsTableContext.SuppressExternal = false;
        }
    }

    [HarmonyPatch]
    public static class MapControlsTable_ToggleableIcon_Patch
    {
        static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(WidgetRow))
                .Where(method => method.Name == "ToggleableIcon")
                .Where(method =>
                {
                    var parameters = method.GetParameters();
                    return parameters.Length >= 3
                        && parameters[0].ParameterType == typeof(bool).MakeByRefType();
                });
        }

        public static bool Prefix(ref bool toggleable, Texture2D tex, string tooltip)
        {
            if (!MapControlsTableContext.Active)
            {
                if (MapControlsTableContext.SuppressExternal)
                {
                    return false;
                }

                return true;
            }

            if (!MapControlsTableContext.MatchesFilter(tooltip))
            {
                return false;
            }

            if (MapControlsTableContext.Measuring)
            {
                MapControlsTableRenderer.MeasureRowHeight(tooltip, true);
                MapControlsTableContext.TotalRows++;
                return false;
            }

            MapControlsTableRenderer.DrawToggleRow(ref toggleable, tex, tooltip);
            return false;
        }
    }

    [HarmonyPatch]
    public static class MapControlsTable_ButtonIcon_Patch
    {
        static IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(WidgetRow))
                .Where(method => method.Name == "ButtonIcon")
                .Where(method =>
                {
                    var parameters = method.GetParameters();
                    return parameters.Length >= 2
                        && parameters[0].ParameterType == typeof(Texture2D)
                        && parameters[1].ParameterType == typeof(string);
                });
        }

        public static bool Prefix(Texture2D tex, string tooltip, ref bool __result)
        {
            if (!MapControlsTableContext.Active)
            {
                if (MapControlsTableContext.SuppressExternal)
                {
                    return MapControlsTableHelpers.IsMenuButton(tex, tooltip);
                }

                return true;
            }

            if (!MapControlsTableContext.MatchesFilter(tooltip))
            {
                __result = false;
                return false;
            }

            if (MapControlsTableContext.Measuring)
            {
                MapControlsTableRenderer.MeasureRowHeight(tooltip, false);
                MapControlsTableContext.TotalRows++;
                __result = false;
                return false;
            }

            __result = MapControlsTableRenderer.DrawButtonRow(tex, tooltip);
            return false;
        }
    }

    [HarmonyPatch]
    public static class MapControlsTable_ButtonIconRect_Patch
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(WidgetRow), "ButtonIconRect");
        }

        public static bool Prefix(ref Rect __result)
        {
            if (!MapControlsTableContext.Active || MapControlsTableContext.Measuring)
            {
                return true;
            }

            __result = MapControlsTableContext.NextRowRect();
            return false;
        }
    }

    public static class MapControlsTableHelpers
    {
        public static bool IsMenuButton(Texture2D tex, string tooltip)
        {
            return tex == DMMBTextures.PlaySettingsTable.Texture;
        }
    }

    public static class MapControlsTableRenderer
    {
        private const float IconSize = 24f;
        private const float ToggleSize = 20f;
        private const float RowPadding = 6f;
        private static readonly Color RowLine = new Color(1f, 1f, 1f, 0.08f);

        public static void MeasureRowHeight(string tooltip, bool hasToggle)
        {
            string label = MapControlsTableWindow.GetDisplayLabel(tooltip);
            float textWidth = GetTextWidth(hasToggle);
            float textHeight;
            GameFont prevFont = Text.Font;
            Text.Font = GameFont.Small;
            textHeight = Text.CalcHeight(label ?? string.Empty, textWidth);
            Text.Font = prevFont;

            float baseHeight = Mathf.Max(IconSize, hasToggle ? ToggleSize : IconSize) + RowPadding * 2f;
            float desiredHeight = Mathf.Max(baseHeight, textHeight + RowPadding * 2f);
            if (desiredHeight > MapControlsTableContext.MaxRowHeight)
            {
                MapControlsTableContext.MaxRowHeight = desiredHeight;
            }
        }

        public static void DrawToggleRow(ref bool toggleable, Texture2D icon, string tooltip)
        {
            Rect rowRect = MapControlsTableContext.NextRowRect();
            MapControlsTableContext.LastRowRect = rowRect;

            Widgets.DrawHighlightIfMouseover(rowRect);
            Color prev = GUI.color;
            GUI.color = RowLine;
            Widgets.DrawLineHorizontal(rowRect.x + RowPadding, rowRect.yMax - 1f, rowRect.width - RowPadding * 2f);
            GUI.color = prev;

            Rect contentRect = rowRect.ContractedBy(RowPadding);
            Rect toggleRect = new Rect(contentRect.xMax - ToggleSize, contentRect.y + (contentRect.height - ToggleSize) / 2f, ToggleSize, ToggleSize);
            Rect iconRect = new Rect(contentRect.x, contentRect.y + (contentRect.height - IconSize) / 2f, IconSize, IconSize);
            Rect textRect = contentRect;
            textRect.xMax = toggleRect.xMin - RowPadding;

            if (icon != null)
            {
                Widgets.DrawTextureFitted(iconRect, icon, 1f);
                textRect.xMin = iconRect.xMax + RowPadding;
            }

            TextAnchor prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(textRect, MapControlsTableWindow.GetDisplayLabel(tooltip));
            Text.Anchor = prevAnchor;

            bool value = toggleable;
            bool newValue = value;
            Widgets.Checkbox(toggleRect.x, toggleRect.y, ref newValue, ToggleSize, paintable: false);
            if (newValue != value)
            {
                toggleable = newValue;
            }

            if (!string.IsNullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(rowRect, tooltip);
            }

            MapControlsTableContext.AdvanceRow();
        }

        public static bool DrawButtonRow(Texture2D icon, string tooltip)
        {
            Rect rowRect = MapControlsTableContext.NextRowRect();
            MapControlsTableContext.LastRowRect = rowRect;

            Widgets.DrawHighlightIfMouseover(rowRect);
            Color prev = GUI.color;
            GUI.color = RowLine;
            Widgets.DrawLineHorizontal(rowRect.x + RowPadding, rowRect.yMax - 1f, rowRect.width - RowPadding * 2f);
            GUI.color = prev;

            Rect contentRect = rowRect.ContractedBy(RowPadding);
            Rect iconRect = new Rect(contentRect.x, contentRect.y + (contentRect.height - IconSize) / 2f, IconSize, IconSize);
            Rect textRect = contentRect;

            if (icon != null)
            {
                Widgets.DrawTextureFitted(iconRect, icon, 1f);
                textRect.xMin = iconRect.xMax + RowPadding;
            }

            TextAnchor prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(textRect, MapControlsTableWindow.GetDisplayLabel(tooltip));
            Text.Anchor = prevAnchor;

            bool clicked = Widgets.ButtonInvisible(rowRect);
            if (!string.IsNullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(rowRect, tooltip);
            }

            MapControlsTableContext.AdvanceRow();
            return clicked;
        }

        private static float GetTextWidth(bool hasToggle)
        {
            float width = MapControlsTableContext.ViewWidth - RowPadding * 2f;
            if (width <= 0f)
            {
                return 0f;
            }

            width -= IconSize + RowPadding;
            if (hasToggle)
            {
                width -= ToggleSize + RowPadding;
            }

            return Mathf.Max(0f, width);
        }
    }
}
