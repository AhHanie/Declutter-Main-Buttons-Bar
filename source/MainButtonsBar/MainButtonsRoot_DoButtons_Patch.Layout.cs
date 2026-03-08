using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static partial class MainButtonsRoot_DoButtons_Patch
    {
        private const float CombinedFreeSizeNormalizeIntervalSeconds = 2f;
        private static float nextCombinedFreeSizeNormalizeRealtime = -1f;

        private static void DrawButtons(List<MainButtonDef> allButtons)
        {
            if (ModSettings.useAdvancedEditMode)
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
            DrawExitEditModeButton();
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

            List<string> widgetIds = ModSettings.GetEnabledWidgetIds();
            Dictionary<string, float> widgetWidths = BuildWidgetWidths(widgetIds);
            float widgetTotalWidth = GetWidgetTotalWidth(widgetIds, widgetWidths);

            float availableWidth = Mathf.Max(0f, UI.screenWidth - pinnedWidth - widgetTotalWidth);
            ScaleAndAdjustWidthsToFill(leftDefs, widths, availableWidth);

            MainButtonDef hoveredDef = null;
            Rect hoveredRect = default;

            DrawButtonRow(leftDefs, widths, 0f, availableWidth, true, true, ref hoveredDef, ref hoveredRect, false);
            if (widgetIds.Count > 0)
            {
                DrawWidgetsRightAligned(widgetIds, widgetWidths, UI.screenWidth - pinnedWidth);
            }

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
            List<string> widgetIds = ModSettings.GetEnabledWidgetIds();
            Dictionary<string, float> widgetWidths = BuildWidgetWidths(widgetIds);
            float widgetTotalWidth = GetWidgetTotalWidth(widgetIds, widgetWidths);

            List<MainButtonDef> orderedVisible = GetOrderedVisibleDefs(allButtons, includePinnedMenu: !pinMenuRight);
            float availableWidth = Mathf.Max(0f, UI.screenWidth - pinnedWidth - widgetTotalWidth);
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
            if (widgetIds.Count > 0)
            {
                DrawWidgetsRightAligned(widgetIds, widgetWidths, UI.screenWidth - pinnedWidth);
            }

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
            List<string> widgetIds = ModSettings.GetEnabledWidgetIds();
            Dictionary<string, float> widgetWidths = BuildWidgetWidthsFromStored(widgetIds);
            Dictionary<string, float> widgetXPositions = BuildWidgetXPositionsFromStored(widgetIds, widgetWidths, availableWidth);
            HandleWidgetEditInput(leftDefs, widths, xPositions, widgetIds, widgetWidths, widgetXPositions, availableWidth);
            NormalizeCombinedFreeSizeLayoutThrottled(leftDefs, widths, xPositions, widgetIds, widgetWidths, widgetXPositions, availableWidth);

            MainButtonDef hoveredDef = null;
            Rect hoveredRect = default;

            DrawButtonRowWithPositions(leftDefs, widths, xPositions, widgetWidths, widgetXPositions, availableWidth, true, true, ref hoveredDef, ref hoveredRect);
            DrawFreeSizeWidgets(leftDefs, widths, xPositions, widgetIds, widgetWidths, widgetXPositions);

            UpdateAndDrawDropdown(hoveredDef, hoveredRect);
        }

        public static void ReconcileFreeSizeAfterChange()
        {
            List<MainButtonDef> orderedVisible = GetOrderedVisibleDefs(MainButtonsCache.AllButtonsInOrder, includePinnedMenu: true);
            List<string> widgetIds = ModSettings.GetEnabledWidgetIds();
            if (orderedVisible.Count == 0 && widgetIds.Count == 0)
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
            float availableWidth = UI.screenWidth;
            ScaleWidthsAndPositionsToFit(orderedVisible, widths, xPositions, availableWidth);

            Dictionary<string, float> widgetWidths = BuildWidgetWidthsFromStored(widgetIds);
            Dictionary<string, float> widgetXPositions = BuildWidgetXPositionsFromStored(widgetIds, widgetWidths, availableWidth);
            NormalizeCombinedFreeSizeLayout(
                orderedVisible,
                widths,
                xPositions,
                widgetIds,
                widgetWidths,
                widgetXPositions,
                availableWidth);

            for (int i = 0; i < orderedVisible.Count; i++)
            {
                MainButtonDef def = orderedVisible[i];
                ModSettings.freeSizeWidths[def] = widths[def];
                ModSettings.freeSizeXPositions[def] = xPositions[def];
            }

            for (int i = 0; i < widgetIds.Count; i++)
            {
                string widgetId = widgetIds[i];
                ModSettings.widgetWidths[widgetId] = widgetWidths[widgetId];
                ModSettings.widgetXPositions[widgetId] = widgetXPositions[widgetId];
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
            for (int i = 0; i < allButtons.Count; i++)
            {
                MainButtonDef def = allButtons[i];
                if (!def.Worker.Visible || !ShouldShowOnBar(def))
                {
                    continue;
                }

                if (!includePinnedMenu && def == MainButtonDefOf.Menu)
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
                if (visibleSet.Contains(def) && orderedSet.Add(def))
                {
                    ordered.Add(def);
                }
            }

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
            Dictionary<MainButtonDef, float> widths = scratchFreeSizeWidths;
            widths.Clear();
            Dictionary<MainButtonDef, float> stored = ModSettings.freeSizeWidths;
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                float defaultWidth = def.minimized ? miniWidth : baseWidth;
                if (stored.TryGetValue(def, out float value))
                {
                    widths[def] = value;
                }
                else
                {
                    widths[def] = defaultWidth;
                }
            }

            return widths;
        }

        private static Dictionary<MainButtonDef, float> BuildXPositionsFromStored(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths)
        {
            Dictionary<MainButtonDef, float> positions = scratchFreeSizeXPositions;
            positions.Clear();
            Dictionary<MainButtonDef, float> stored = ModSettings.freeSizeXPositions;

            if (stored.Count == 0)
            {
                float curX = 0f;
                for (int i = 0; i < defs.Count; i++)
                {
                    MainButtonDef def = defs[i];
                    positions[def] = curX;
                    curX += widths[def];
                }

                return positions;
            }

            bool hasStoredPositions = false;
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                if (stored.TryGetValue(def, out float storedX))
                {
                    positions[def] = storedX;
                    hasStoredPositions = true;
                }
                else
                {
                    positions[def] = 0f;
                }
            }

            if (!hasStoredPositions)
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

        private static void ScaleAndAdjustWidthsToFill(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths,
            float availableWidth)
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

            float adjustedTotal = total;
            if (total > availableWidth && total > 0f)
            {
                float scale = availableWidth / total;
                adjustedTotal = 0f;
                for (int i = 0; i < defs.Count; i++)
                {
                    MainButtonDef def = defs[i];
                    float scaledWidth = Mathf.Max(MinFreeSizeWidth, widths[def] * scale);
                    widths[def] = scaledWidth;
                    adjustedTotal += scaledWidth;
                }
            }

            float delta = availableWidth - adjustedTotal;
            if (Mathf.Abs(delta) > 0.1f)
            {
                MainButtonDef last = defs[defs.Count - 1];
                widths[last] = Mathf.Max(MinFreeSizeWidth, widths[last] + delta);
            }
        }

        private static bool ShouldShowOnBar(MainButtonDef def)
        {
            return !ModSettings.IsHiddenFromBar(def);
        }

        private sealed class CombinedFreeSizeElement
        {
            public MainButtonDef buttonDef;
            public string widgetId;
            public float x;
            public float width;

            public bool IsWidget => !string.IsNullOrEmpty(widgetId);
        }

        private static void NormalizeCombinedFreeSizeLayout(
            List<MainButtonDef> buttonDefs,
            Dictionary<MainButtonDef, float> buttonWidths,
            Dictionary<MainButtonDef, float> buttonXPositions,
            List<string> widgetIds,
            Dictionary<string, float> widgetWidths,
            Dictionary<string, float> widgetXPositions,
            float availableWidth)
        {
            List<CombinedFreeSizeElement> elements = new List<CombinedFreeSizeElement>(buttonDefs.Count + widgetIds.Count);
            Dictionary<MainButtonDef, int> buttonOrderIndex = new Dictionary<MainButtonDef, int>(buttonDefs.Count);
            Dictionary<string, int> widgetOrderIndex = new Dictionary<string, int>(widgetIds.Count);

            float totalWidgetWidth = 0f;
            for (int i = 0; i < widgetIds.Count; i++)
            {
                string widgetId = widgetIds[i];
                float minWidth = GetWidgetMinWidth(widgetId);
                float width = widgetWidths.TryGetValue(widgetId, out float storedWidth) ? storedWidth : GetWidgetPreferredWidth(widgetId);
                width = Mathf.Max(minWidth, width);
                widgetWidths[widgetId] = width;
                ModSettings.widgetWidths[widgetId] = width;
                totalWidgetWidth += width;
                widgetOrderIndex[widgetId] = i;
            }

            float totalButtonWidth = 0f;
            for (int i = 0; i < buttonDefs.Count; i++)
            {
                MainButtonDef def = buttonDefs[i];
                float width = buttonWidths.TryGetValue(def, out float storedWidth) ? storedWidth : MinFreeSizeWidth;
                width = Mathf.Max(MinFreeSizeWidth, width);
                buttonWidths[def] = width;
                ModSettings.freeSizeWidths[def] = width;
                totalButtonWidth += width;
                buttonOrderIndex[def] = i;
            }

            float availableButtonWidth = Mathf.Max(0f, availableWidth - totalWidgetWidth);
            if (totalButtonWidth > availableButtonWidth + 0.1f && totalButtonWidth > 0f)
            {
                float scale = availableButtonWidth / totalButtonWidth;
                scale = Mathf.Clamp01(scale);
                if (scale < 1f)
                {
                    totalButtonWidth = 0f;
                    for (int i = 0; i < buttonDefs.Count; i++)
                    {
                        MainButtonDef def = buttonDefs[i];
                        float scaledWidth = Mathf.Max(MinFreeSizeWidth, buttonWidths[def] * scale);
                        buttonWidths[def] = scaledWidth;
                        ModSettings.freeSizeWidths[def] = scaledWidth;
                        totalButtonWidth += scaledWidth;
                    }
                }

                float overflow = totalButtonWidth - availableButtonWidth;
                int safety = 0;
                while (overflow > 0.1f && safety < 8)
                {
                    float shrinkable = 0f;
                    for (int i = 0; i < buttonDefs.Count; i++)
                    {
                        MainButtonDef def = buttonDefs[i];
                        float width = buttonWidths[def];
                        if (width > MinFreeSizeWidth)
                        {
                            shrinkable += width - MinFreeSizeWidth;
                        }
                    }

                    if (shrinkable <= 0.01f)
                    {
                        break;
                    }

                    float ratio = Mathf.Min(1f, overflow / shrinkable);
                    totalButtonWidth = 0f;
                    for (int i = 0; i < buttonDefs.Count; i++)
                    {
                        MainButtonDef def = buttonDefs[i];
                        float width = buttonWidths[def];
                        if (width > MinFreeSizeWidth)
                        {
                            float delta = (width - MinFreeSizeWidth) * ratio;
                            width = Mathf.Max(MinFreeSizeWidth, width - delta);
                            buttonWidths[def] = width;
                            ModSettings.freeSizeWidths[def] = width;
                        }

                        totalButtonWidth += buttonWidths[def];
                    }

                    overflow = totalButtonWidth - availableButtonWidth;
                    safety++;
                }
            }

            for (int i = 0; i < buttonDefs.Count; i++)
            {
                MainButtonDef def = buttonDefs[i];
                float width = buttonWidths[def];
                float maxX = Mathf.Max(0f, availableWidth - width);
                float x = buttonXPositions.TryGetValue(def, out float storedX) ? storedX : 0f;
                x = Mathf.Clamp(x, 0f, maxX);
                buttonXPositions[def] = x;

                elements.Add(new CombinedFreeSizeElement
                {
                    buttonDef = def,
                    x = x,
                    width = width
                });
            }

            for (int i = 0; i < widgetIds.Count; i++)
            {
                string widgetId = widgetIds[i];
                float width = widgetWidths[widgetId];
                float maxX = Mathf.Max(0f, availableWidth - width);
                float x = widgetXPositions.TryGetValue(widgetId, out float storedX) ? storedX : 0f;
                x = Mathf.Clamp(x, 0f, maxX);
                widgetXPositions[widgetId] = x;

                elements.Add(new CombinedFreeSizeElement
                {
                    widgetId = widgetId,
                    x = x,
                    width = width
                });
            }

            if (elements.Count == 0)
            {
                return;
            }

            elements.Sort(delegate (CombinedFreeSizeElement a, CombinedFreeSizeElement b)
            {
                int cmp = a.x.CompareTo(b.x);
                if (cmp != 0)
                {
                    return cmp;
                }

                if (a.IsWidget != b.IsWidget)
                {
                    return a.IsWidget ? 1 : -1;
                }

                if (a.IsWidget)
                {
                    return widgetOrderIndex[a.widgetId].CompareTo(widgetOrderIndex[b.widgetId]);
                }

                return buttonOrderIndex[a.buttonDef].CompareTo(buttonOrderIndex[b.buttonDef]);
            });

            float leftEdge = 0f;
            for (int i = 0; i < elements.Count; i++)
            {
                CombinedFreeSizeElement element = elements[i];
                float maxX = Mathf.Max(0f, availableWidth - element.width);
                float clampedX = Mathf.Clamp(element.x, 0f, maxX);
                if (clampedX < leftEdge)
                {
                    clampedX = leftEdge;
                }
                if (clampedX > maxX)
                {
                    clampedX = maxX;
                }

                element.x = clampedX;
                leftEdge = clampedX + element.width;
            }

            float rightEdge = availableWidth;
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                CombinedFreeSizeElement element = elements[i];
                float maxX = Mathf.Max(0f, availableWidth - element.width);
                float clampedX = Mathf.Clamp(element.x, 0f, maxX);
                float maxAllowed = rightEdge - element.width;
                if (clampedX > maxAllowed)
                {
                    clampedX = maxAllowed;
                }
                if (clampedX < 0f)
                {
                    clampedX = 0f;
                }
                if (clampedX > maxX)
                {
                    clampedX = maxX;
                }

                element.x = clampedX;
                rightEdge = clampedX;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                CombinedFreeSizeElement element = elements[i];
                if (element.IsWidget)
                {
                    widgetXPositions[element.widgetId] = element.x;
                    ModSettings.widgetXPositions[element.widgetId] = element.x;
                }
                else
                {
                    buttonXPositions[element.buttonDef] = element.x;
                    ModSettings.freeSizeXPositions[element.buttonDef] = element.x;
                }
            }
        }

        private static void NormalizeCombinedFreeSizeLayoutThrottled(
            List<MainButtonDef> buttonDefs,
            Dictionary<MainButtonDef, float> buttonWidths,
            Dictionary<MainButtonDef, float> buttonXPositions,
            List<string> widgetIds,
            Dictionary<string, float> widgetWidths,
            Dictionary<string, float> widgetXPositions,
            float availableWidth)
        {
            float now = Time.realtimeSinceStartup;
            if (nextCombinedFreeSizeNormalizeRealtime >= 0f && now < nextCombinedFreeSizeNormalizeRealtime)
            {
                return;
            }

            NormalizeCombinedFreeSizeLayout(
                buttonDefs,
                buttonWidths,
                buttonXPositions,
                widgetIds,
                widgetWidths,
                widgetXPositions,
                availableWidth);

            nextCombinedFreeSizeNormalizeRealtime = now + CombinedFreeSizeNormalizeIntervalSeconds;
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
