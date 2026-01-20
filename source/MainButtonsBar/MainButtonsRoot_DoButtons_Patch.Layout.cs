using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static partial class MainButtonsRoot_DoButtons_Patch
    {
        private static void DrawButtons(List<MainButtonDef> allButtons)
        {
            if (ModSettings.useFreeSizeMode)
            {
                DrawFreeSizeButtons(allButtons);
            }
            else if (ModSettings.useFixedWidthMode)
            {
                DrawFixedWidthButtons(allButtons);
            }
            else
            {
                DrawDynamicWidthButtons(allButtons);
            }

            DrawEditGizmoPreview();
        }

        private static void DrawDynamicWidthButtons(List<MainButtonDef> allButtons)
        {
            List<MainButtonDef> orderedVisible = GetOrderedVisibleDefs(allButtons, includePinnedMenu: true);
            if (orderedVisible.Count == 0)
            {
                return;
            }

            MainButtonDef pinnedMenuDef = MainButtonDefOf.Menu;
            bool menuVisible = pinnedMenuDef != null && pinnedMenuDef.Worker.Visible;
            bool pinMenuRight = ModSettings.pinMenuButtonRight && menuVisible;

            List<MainButtonDef> leftDefs = orderedVisible;
            if (pinMenuRight)
            {
                leftDefs = new List<MainButtonDef>(orderedVisible.Count);
                for (int i = 0; i < orderedVisible.Count; i++)
                {
                    MainButtonDef def = orderedVisible[i];
                    if (def != pinnedMenuDef)
                    {
                        leftDefs.Add(def);
                    }
                }
            }

            float visibleUnits = 0f;
            for (int i = 0; i < orderedVisible.Count; i++)
            {
                MainButtonDef def = orderedVisible[i];
                visibleUnits += def.minimized ? 0.5f : 1f;
            }

            if (visibleUnits <= 0f)
            {
                return;
            }

            float baseWidth = UI.screenWidth / visibleUnits;
            float miniWidth = baseWidth / 2f;
            Dictionary<MainButtonDef, float> widths = BuildWidths(leftDefs, baseWidth, miniWidth);
            float pinnedWidth = 0f;
            if (pinMenuRight)
            {
                pinnedWidth = pinnedMenuDef.minimized ? miniWidth : baseWidth;
            }

            float availableWidth = UI.screenWidth - pinnedWidth;
            AdjustLastWidthToFill(leftDefs, widths, availableWidth);

            MainButtonDef hoveredDef = null;
            Rect hoveredRect = default;

            DrawButtonRow(leftDefs, widths, 0f, availableWidth, true, true, ref hoveredDef, ref hoveredRect, false);

            if (pinMenuRight)
            {
                Rect menuRect = new Rect(UI.screenWidth - pinnedWidth, UI.screenHeight - BarBottomOffset, pinnedWidth, BarHeight);
                DrawSingleButton(pinnedMenuDef, menuRect, UI.screenWidth - availableWidth, ref hoveredDef, ref hoveredRect, allowReorder: false);
            }

            UpdateAndDrawDropdown(hoveredDef, hoveredRect);
        }

        private static void DrawFixedWidthButtons(List<MainButtonDef> allButtons)
        {
            float width = Mathf.Clamp(ModSettings.fixedButtonWidth, 50f, 200f);
            MainButtonDef pinnedMenuDef = MainButtonDefOf.Menu;
            bool menuVisible = pinnedMenuDef != null && pinnedMenuDef.Worker.Visible;
            bool pinMenuRight = ModSettings.pinMenuButtonRight && menuVisible;
            float pinnedWidth = pinMenuRight ? width : 0f;

            List<MainButtonDef> orderedVisible = GetOrderedVisibleDefs(allButtons, includePinnedMenu: !pinMenuRight);
            float availableWidth = UI.screenWidth - pinnedWidth;
            int maxButtons = Mathf.Max(1, Mathf.FloorToInt(availableWidth / width));

            int takeCount = Mathf.Min(maxButtons, orderedVisible.Count);
            List<MainButtonDef> leftDefs = new List<MainButtonDef>(takeCount);
            for (int i = 0; i < takeCount; i++)
            {
                leftDefs.Add(orderedVisible[i]);
            }

            int curX = 0;
            if (ModSettings.centerFixedWidthButtons)
            {
                float totalWidth = leftDefs.Count * width;
                if (totalWidth < availableWidth)
                {
                    curX = Mathf.FloorToInt((availableWidth - totalWidth) / 2f);
                }
            }

            Dictionary<MainButtonDef, float> widths = new Dictionary<MainButtonDef, float>(leftDefs.Count);
            for (int i = 0; i < leftDefs.Count; i++)
            {
                widths[leftDefs[i]] = width;
            }
            MainButtonDef hoveredDef = null;
            Rect hoveredRect = default;

            DrawButtonRow(leftDefs, widths, curX, availableWidth, true, true, ref hoveredDef, ref hoveredRect, false);

            if (pinMenuRight)
            {
                Rect menuRect = new Rect(UI.screenWidth - width, UI.screenHeight - BarBottomOffset, width, BarHeight);
                DrawSingleButton(pinnedMenuDef, menuRect, UI.screenWidth - availableWidth, ref hoveredDef, ref hoveredRect, allowReorder: false);
            }

            UpdateAndDrawDropdown(hoveredDef, hoveredRect);
        }

        private static void DrawFreeSizeButtons(List<MainButtonDef> allButtons)
        {
            List<MainButtonDef> orderedVisible = GetOrderedVisibleDefs(allButtons, includePinnedMenu: true);
            if (orderedVisible.Count == 0)
            {
                return;
            }

            List<MainButtonDef> leftDefs = orderedVisible;

            float visibleUnits = 0f;
            for (int i = 0; i < orderedVisible.Count; i++)
            {
                MainButtonDef def = orderedVisible[i];
                visibleUnits += def.minimized ? 0.5f : 1f;
            }

            float baseWidth = visibleUnits > 0f ? UI.screenWidth / visibleUnits : 100f;
            float miniWidth = baseWidth / 2f;
            Dictionary<MainButtonDef, float> widths = BuildWidthsFromStored(leftDefs, baseWidth, miniWidth);
            Dictionary<MainButtonDef, float> xPositions = BuildXPositionsFromStored(leftDefs, widths);

            float availableWidth = UI.screenWidth;
            ScaleWidthsAndPositionsToFit(leftDefs, widths, xPositions, availableWidth);

            MainButtonDef hoveredDef = null;
            Rect hoveredRect = default;

            DrawButtonRowWithPositions(leftDefs, widths, xPositions, availableWidth, true, true, ref hoveredDef, ref hoveredRect);

            UpdateAndDrawDropdown(hoveredDef, hoveredRect);
        }

        public static void ReconcileFreeSizeAfterVisibilityChange()
        {
            List<MainButtonDef> orderedVisible = GetOrderedVisibleDefs(MainButtonsCache.AllButtonsInOrder, includePinnedMenu: true);
            if (orderedVisible.Count < 2)
            {
                return;
            }

            float visibleUnits = 0f;
            for (int i = 0; i < orderedVisible.Count; i++)
            {
                MainButtonDef def = orderedVisible[i];
                visibleUnits += def.minimized ? 0.5f : 1f;
            }

            float baseWidth = visibleUnits > 0f ? UI.screenWidth / visibleUnits : 100f;
            float miniWidth = baseWidth / 2f;
            Dictionary<MainButtonDef, float> widths = BuildWidthsFromStored(orderedVisible, baseWidth, miniWidth);
            Dictionary<MainButtonDef, float> xPositions = BuildXPositionsFromStored(orderedVisible, widths);

            NormalizeOverlappingGroups(orderedVisible, widths, xPositions);

            for (int i = 0; i < orderedVisible.Count; i++)
            {
                MainButtonDef def = orderedVisible[i];
                ModSettings.freeSizeWidths[def] = widths[def];
                ModSettings.freeSizeXPositions[def] = xPositions[def];
            }
        }

        private static List<MainButtonDef> GetOrderedVisibleDefs(List<MainButtonDef> allButtons, bool includePinnedMenu)
        {
            // Check cache validity (rebuild every 60 frames to catch visibility changes)
            int currentFrame = Time.frameCount;
            bool cacheExpired = lastVisibleCheckFrame < 0 || (currentFrame - lastVisibleCheckFrame) > 60;

            if (!cacheExpired)
            {
                List<MainButtonDef> cached = includePinnedMenu ? cachedOrderedVisible : cachedOrderedVisibleNoPinnedMenu;
                if (cached != null)
                {
                    return cached;
                }
            }

            List<MainButtonDef> visible = new List<MainButtonDef>(allButtons.Count);
            MainButtonDef menuDef = includePinnedMenu ? null : MainButtonDefOf.Menu;
            for (int i = 0; i < allButtons.Count; i++)
            {
                MainButtonDef def = allButtons[i];
                if (def == null || !def.Worker.Visible || !ShouldShowOnBar(def))
                {
                    continue;
                }

                if (menuDef != null && def == menuDef)
                {
                    continue;
                }

                visible.Add(def);
            }

            HashSet<MainButtonDef> visibleSet = new HashSet<MainButtonDef>(visible);
            List<MainButtonDef> ordered = new List<MainButtonDef>(visible.Count);
            HashSet<MainButtonDef> orderedSet = new HashSet<MainButtonDef>();

            for (int i = 0; i < ModSettings.customOrderDefs.Count; i++)
            {
                MainButtonDef def = ModSettings.customOrderDefs[i];
                if (def != null && visibleSet.Contains(def) && orderedSet.Add(def))
                {
                    ordered.Add(def);
                }
            }

            List<MainButtonDef> remainder = new List<MainButtonDef>(visible.Count);
            for (int i = 0; i < visible.Count; i++)
            {
                MainButtonDef def = visible[i];
                if (!orderedSet.Contains(def))
                {
                    remainder.Add(def);
                }
            }

            remainder.Sort((a, b) => a.order.CompareTo(b.order));
            ordered.AddRange(remainder);

            // Cache the result
            if (includePinnedMenu)
            {
                cachedOrderedVisible = ordered;
            }
            else
            {
                cachedOrderedVisibleNoPinnedMenu = ordered;
            }

            lastVisibleCheckFrame = currentFrame;

            return ordered;
        }

        private static Dictionary<MainButtonDef, float> BuildWidths(List<MainButtonDef> defs, float baseWidth, float miniWidth)
        {
            Dictionary<MainButtonDef, float> widths = new Dictionary<MainButtonDef, float>();
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                widths[def] = def.minimized ? miniWidth : baseWidth;
            }

            return widths;
        }

        private static Dictionary<MainButtonDef, float> BuildWidthsFromStored(List<MainButtonDef> defs, float baseWidth, float miniWidth)
        {
            Dictionary<MainButtonDef, float> widths = new Dictionary<MainButtonDef, float>();
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                float defaultWidth = def.minimized ? miniWidth : baseWidth;
                widths[def] = GetStoredWidth(def, defaultWidth);
            }

            return widths;
        }

        private static float GetStoredWidth(MainButtonDef def, float defaultWidth)
        {
            if (def == null)
            {
                return defaultWidth;
            }

            if (ModSettings.freeSizeWidths.TryGetValue(def, out float value))
            {
                return value;
            }

            return defaultWidth;
        }

        private static Dictionary<MainButtonDef, float> BuildXPositionsFromStored(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths)
        {
            Dictionary<MainButtonDef, float> positions = new Dictionary<MainButtonDef, float>();

            bool hasStoredPositions = false;
            for (int i = 0; i < defs.Count; i++)
            {
                if (ModSettings.freeSizeXPositions.ContainsKey(defs[i]))
                {
                    hasStoredPositions = true;
                    break;
                }
            }

            if (hasStoredPositions)
            {
                for (int i = 0; i < defs.Count; i++)
                {
                    MainButtonDef def = defs[i];
                    if (ModSettings.freeSizeXPositions.TryGetValue(def, out float storedX))
                    {
                        positions[def] = storedX;
                    }
                    else
                    {
                        positions[def] = 0f;
                    }
                }
            }
            else
            {
                float curX = 0f;
                for (int i = 0; i < defs.Count; i++)
                {
                    MainButtonDef def = defs[i];
                    positions[def] = curX;
                    curX += widths[def];
                }
            }

            return positions;
        }

        private static void ScaleWidthsAndPositionsToFit(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths,
            Dictionary<MainButtonDef, float> xPositions,
            float availableWidth)
        {
            if (defs.Count == 0)
            {
                return;
            }

            float maxX = 0f;
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                float rightEdge = xPositions[def] + widths[def];
                if (rightEdge > maxX)
                {
                    maxX = rightEdge;
                }
            }

            if (maxX <= availableWidth)
            {
                return;
            }

            float scale = availableWidth / maxX;
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                xPositions[def] = xPositions[def] * scale;
                widths[def] = Mathf.Max(MinFreeSizeWidth, widths[def] * scale);
            }
        }

        private static void ScaleWidthsToFit(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths,
            float availableWidth)
        {
            float total = 0f;
            for (int i = 0; i < defs.Count; i++)
            {
                total += widths[defs[i]];
            }

            if (total <= availableWidth || total <= 0f)
            {
                return;
            }

            float scale = availableWidth / total;
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                widths[def] = Mathf.Max(MinFreeSizeWidth, widths[def] * scale);
            }
        }

        private static void AdjustLastWidthToFill(List<MainButtonDef> defs, Dictionary<MainButtonDef, float> widths, float availableWidth)
        {
            if (defs.Count == 0)
            {
                return;
            }

            float total = 0f;
            for (int i = 0; i < defs.Count; i++)
            {
                total += widths[defs[i]];
            }

            float delta = availableWidth - total;
            if (Mathf.Abs(delta) > 0.1f)
            {
                MainButtonDef last = defs[defs.Count - 1];
                widths[last] = Mathf.Max(MinFreeSizeWidth, widths[last] + delta);
            }
        }

        private static bool ShouldShowOnBar(MainButtonDef def)
        {
            if (def == MainButtonsMenuDefOf.DMMB_MainButtonsMenu)
            {
                return true;
            }

            return !ModSettings.IsHiddenFromBar(def);
        }

        private static void NormalizeOverlappingGroups(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths,
            Dictionary<MainButtonDef, float> xPositions)
        {
            int index = 0;
            while (index < defs.Count)
            {
                MainButtonDef startDef = defs[index];
                float groupLeft = xPositions[startDef];
                float groupRight = groupLeft + widths[startDef];
                bool hasOverlap = false;
                int end = index;

                while (end + 1 < defs.Count)
                {
                    MainButtonDef nextDef = defs[end + 1];
                    float nextX = xPositions[nextDef];
                    if (nextX <= groupRight + 0.1f)
                    {
                        if (nextX < groupRight - 0.1f)
                        {
                            hasOverlap = true;
                        }

                        end++;
                        float nextRight = nextX + widths[nextDef];
                        if (nextRight > groupRight)
                        {
                            groupRight = nextRight;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (end > index && hasOverlap)
                {
                    int count = end - index + 1;
                    float groupWidth = groupRight - groupLeft;
                    float targetWidth = Mathf.Max(MinFreeSizeWidth, groupWidth / count);
                    float curX = groupLeft;
                    for (int i = index; i <= end; i++)
                    {
                        MainButtonDef def = defs[i];
                        widths[def] = targetWidth;
                        xPositions[def] = curX;
                        curX += targetWidth;
                    }
                }

                index = end + 1;
            }
        }
    }
}
