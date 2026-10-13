using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static partial class MainButtonsRoot_DoButtons_Patch
    {
        public static void SetSmartSpeedMode()
        {
            TimeSpeedWidgetRenderer.SetSmartSpeedMode();
        }

        private static Dictionary<string, float> BuildWidgetWidths(List<string> widgetIds)
        {
            Dictionary<string, float> widths = scratchWidgetWidths;
            widths.Clear();
            for (int i = 0; i < widgetIds.Count; i++)
            {
                string widgetId = widgetIds[i];
                widths[widgetId] = GetWidgetPreferredWidth(widgetId);
            }

            return widths;
        }

        private static Dictionary<string, float> BuildWidgetWidthsFromStored(List<string> widgetIds)
        {
            Dictionary<string, float> widths = scratchWidgetWidths;
            widths.Clear();
            Dictionary<string, float> stored = ModSettings.widgetWidths;
            for (int i = 0; i < widgetIds.Count; i++)
            {
                string widgetId = widgetIds[i];
                float minWidth = GetWidgetMinWidth(widgetId);
                float preferred = GetWidgetPreferredWidth(widgetId);
                float width = preferred;
                if (stored.TryGetValue(widgetId, out float storedWidth))
                {
                    width = storedWidth;
                }

                width = Mathf.Max(minWidth, width);
                widths[widgetId] = width;
                stored[widgetId] = width;
            }

            return widths;
        }

        private static Dictionary<string, float> BuildWidgetXPositionsFromStored(List<string> widgetIds, Dictionary<string, float> widths, float availableWidth)
        {
            Dictionary<string, float> positions = scratchWidgetXPositions;
            positions.Clear();
            Dictionary<string, float> stored = ModSettings.widgetXPositions;

            float totalWidth = 0f;
            for (int i = 0; i < widgetIds.Count; i++)
            {
                totalWidth += widths[widgetIds[i]];
            }

            float defaultX = Mathf.Max(0f, availableWidth - totalWidth);
            for (int i = 0; i < widgetIds.Count; i++)
            {
                string widgetId = widgetIds[i];
                float width = widths[widgetId];
                float maxX = Mathf.Max(0f, availableWidth - width);
                float x;
                if (stored.TryGetValue(widgetId, out float storedX))
                {
                    x = Mathf.Clamp(storedX, 0f, maxX);
                }
                else
                {
                    x = Mathf.Clamp(defaultX, 0f, maxX);
                    defaultX += width;
                }

                positions[widgetId] = x;
                stored[widgetId] = x;
            }

            return positions;
        }

        private static float GetWidgetTotalWidth(List<string> widgetIds, Dictionary<string, float> widths)
        {
            float total = 0f;
            for (int i = 0; i < widgetIds.Count; i++)
            {
                total += widths[widgetIds[i]];
            }

            return total;
        }

        private static void DrawWidgetsRightAligned(List<string> widgetIds, Dictionary<string, float> widths, float rightEdge)
        {
            float total = GetWidgetTotalWidth(widgetIds, widths);
            float curX = rightEdge - total;
            for (int i = 0; i < widgetIds.Count; i++)
            {
                string widgetId = widgetIds[i];
                float width = widths[widgetId];
                Rect rect = new Rect(curX, UI.screenHeight - BarBottomOffset, width, BarHeight);
                DrawWidget(widgetId, rect);
                curX += width;
            }
        }

        private static void DrawFreeSizeWidgets(
            List<MainButtonDef> buttonDefs,
            Dictionary<MainButtonDef, float> buttonWidths,
            Dictionary<MainButtonDef, float> buttonXPositions,
            List<string> widgetIds,
            Dictionary<string, float> widths,
            Dictionary<string, float> xPositions)
        {
            if (widgetIds.Count == 0)
            {
                draggingWidgetId = null;
                resizingWidgetId = null;
                return;
            }

            float draggingWidth = 0f;
            bool drawDraggingGhost = ModSettings.editDropdownsMode
                && !string.IsNullOrEmpty(draggingWidgetId)
                && widgetIds.Contains(draggingWidgetId)
                && widths.TryGetValue(draggingWidgetId, out draggingWidth);
            float draggingGhostX = 0f;
            if (drawDraggingGhost)
            {
                draggingGhostX = GetWidgetDragPreviewX(
                    draggingWidgetId,
                    draggingWidth,
                    buttonDefs,
                    buttonWidths,
                    buttonXPositions,
                    widgetIds,
                    widths,
                    xPositions);
            }

            for (int i = 0; i < widgetIds.Count; i++)
            {
                string widgetId = widgetIds[i];
                if (drawDraggingGhost && widgetId == draggingWidgetId)
                {
                    continue;
                }

                Rect rect = new Rect(xPositions[widgetId], UI.screenHeight - BarBottomOffset, widths[widgetId], BarHeight);
                DrawWidget(widgetId, rect);
                if (ModSettings.editDropdownsMode)
                {
                    DrawResizeHandle(rect);
                }
            }

            if (drawDraggingGhost)
            {
                Rect dragRect = new Rect(draggingGhostX, UI.screenHeight - BarBottomOffset, draggingWidth, BarHeight);
                Color prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.85f);
                DrawWidget(draggingWidgetId, dragRect);
                GUI.color = prev;
                DrawResizeHandle(dragRect);
            }
        }

        private static void HandleWidgetEditInput(
            List<MainButtonDef> buttonDefs,
            Dictionary<MainButtonDef, float> buttonWidths,
            Dictionary<MainButtonDef, float> buttonXPositions,
            List<string> widgetIds,
            Dictionary<string, float> widths,
            Dictionary<string, float> xPositions,
            float availableWidth)
        {
            if (!ModSettings.editDropdownsMode)
            {
                draggingWidgetId = null;
                resizingWidgetId = null;
                return;
            }

            Event evt = Event.current;
            Vector2 mousePos = evt.mousePosition;

            if (!string.IsNullOrEmpty(resizingWidgetId) && !widgetIds.Contains(resizingWidgetId))
            {
                resizingWidgetId = null;
            }
            if (!string.IsNullOrEmpty(draggingWidgetId) && !widgetIds.Contains(draggingWidgetId))
            {
                draggingWidgetId = null;
            }

            if (resizingWidgetId != null)
            {
                if (evt.type == EventType.MouseDrag)
                {
                    float delta = mousePos.x - widgetResizeStartMouseX;
                    float x = xPositions[resizingWidgetId];
                    float minWidth = GetWidgetMinWidth(resizingWidgetId);
                    float maxWidth = Mathf.Max(minWidth, availableWidth - x);
                    float newWidth = Mathf.Clamp(widgetResizeStartWidth + delta * 2f, minWidth, maxWidth);
                    widths[resizingWidgetId] = newWidth;
                    ModSettings.widgetWidths[resizingWidgetId] = newWidth;
                    evt.Use();
                }
                else if (evt.type == EventType.MouseUp)
                {
                    resizingWidgetId = null;
                    evt.Use();
                }

                return;
            }

            if (draggingWidgetId != null)
            {
                if (evt.type == EventType.MouseDrag)
                {
                    evt.Use();
                }
                else if (evt.type == EventType.MouseUp)
                {
                    if (widths.TryGetValue(draggingWidgetId, out float width))
                    {
                        float maxX = Mathf.Max(0f, availableWidth - width);
                        float x = Mathf.Clamp(mousePos.x - widgetDragOffsetX, 0f, maxX);
                        x = SnapWidgetToButtonsAndWidgets(
                            draggingWidgetId,
                            x,
                            width,
                            buttonDefs,
                            buttonWidths,
                            buttonXPositions,
                            widgetIds,
                            widths,
                            xPositions,
                            availableWidth);
                        xPositions[draggingWidgetId] = x;
                        ModSettings.widgetXPositions[draggingWidgetId] = x;
                    }

                    draggingWidgetId = null;
                    evt.Use();
                }

                return;
            }

            if (evt.type != EventType.MouseDown || evt.button != 0)
            {
                return;
            }

            for (int i = widgetIds.Count - 1; i >= 0; i--)
            {
                string widgetId = widgetIds[i];
                Rect rect = new Rect(xPositions[widgetId], UI.screenHeight - BarBottomOffset, widths[widgetId], BarHeight);
                Rect resizeRect = GetResizeRect(rect);
                if (Mouse.IsOver(resizeRect))
                {
                    resizingWidgetId = widgetId;
                    widgetResizeStartWidth = widths[widgetId];
                    widgetResizeStartMouseX = mousePos.x;
                    evt.Use();
                    return;
                }

                if (Mouse.IsOver(rect))
                {
                    draggingWidgetId = widgetId;
                    widgetDragOffsetX = mousePos.x - rect.x;
                    evt.Use();
                    return;
                }
            }
        }

        private static float SnapWidgetToButtonsAndWidgets(
            string widgetId,
            float widgetX,
            float widgetWidth,
            List<MainButtonDef> buttonDefs,
            Dictionary<MainButtonDef, float> buttonWidths,
            Dictionary<MainButtonDef, float> buttonXPositions,
            List<string> widgetIds,
            Dictionary<string, float> widgetWidths,
            Dictionary<string, float> widgetXPositions,
            float availableWidth)
        {
            float threshold = ModSettings.snapThreshold;
            float bestGap = threshold + 0.001f;
            float bestX = widgetX;

            if (buttonDefs != null && buttonWidths != null && buttonXPositions != null)
            {
                for (int i = 0; i < buttonDefs.Count; i++)
                {
                    MainButtonDef def = buttonDefs[i];
                    if (!buttonWidths.TryGetValue(def, out float otherWidth) || !buttonXPositions.TryGetValue(def, out float otherX))
                    {
                        continue;
                    }

                    float otherLeft = otherX;
                    float otherRight = otherLeft + otherWidth;

                    float gapToOtherRight = widgetX - otherRight;
                    if (gapToOtherRight >= 0f && gapToOtherRight <= threshold && gapToOtherRight < bestGap)
                    {
                        bestGap = gapToOtherRight;
                        bestX = otherRight;
                    }

                    float gapToOtherLeft = otherLeft - (widgetX + widgetWidth);
                    if (gapToOtherLeft >= 0f && gapToOtherLeft <= threshold && gapToOtherLeft < bestGap)
                    {
                        bestGap = gapToOtherLeft;
                        bestX = otherLeft - widgetWidth;
                    }
                }
            }

            if (widgetIds != null && widgetWidths != null && widgetXPositions != null)
            {
                for (int i = 0; i < widgetIds.Count; i++)
                {
                    string otherId = widgetIds[i];
                    if (otherId == widgetId)
                    {
                        continue;
                    }

                    if (!widgetWidths.TryGetValue(otherId, out float otherWidth) || !widgetXPositions.TryGetValue(otherId, out float otherX))
                    {
                        continue;
                    }

                    float otherLeft = otherX;
                    float otherRight = otherLeft + otherWidth;

                    float gapToOtherRight = widgetX - otherRight;
                    if (gapToOtherRight >= 0f && gapToOtherRight <= threshold && gapToOtherRight < bestGap)
                    {
                        bestGap = gapToOtherRight;
                        bestX = otherRight;
                    }

                    float gapToOtherLeft = otherLeft - (widgetX + widgetWidth);
                    if (gapToOtherLeft >= 0f && gapToOtherLeft <= threshold && gapToOtherLeft < bestGap)
                    {
                        bestGap = gapToOtherLeft;
                        bestX = otherLeft - widgetWidth;
                    }
                }
            }

            float maxX = Mathf.Max(0f, availableWidth - widgetWidth);
            return Mathf.Clamp(bestX, 0f, maxX);
        }

        private static float GetWidgetDragPreviewX(
            string widgetId,
            float widgetWidth,
            List<MainButtonDef> buttonDefs,
            Dictionary<MainButtonDef, float> buttonWidths,
            Dictionary<MainButtonDef, float> buttonXPositions,
            List<string> widgetIds,
            Dictionary<string, float> widgetWidths,
            Dictionary<string, float> widgetXPositions)
        {
            float maxX = Mathf.Max(0f, UI.screenWidth - widgetWidth);
            float x = Mathf.Clamp(Event.current.mousePosition.x - widgetDragOffsetX, 0f, maxX);
            return SnapWidgetToButtonsAndWidgets(
                widgetId,
                x,
                widgetWidth,
                buttonDefs,
                buttonWidths,
                buttonXPositions,
                widgetIds,
                widgetWidths,
                widgetXPositions,
                UI.screenWidth);
        }

        private static float GetWidgetPreferredWidth(string widgetId)
        {
            if (widgetId == MainBarWidgetIds.Time)
            {
                return TimeWidgetRenderer.GetPreferredWidth();
            }
            if (widgetId == MainBarWidgetIds.TimeIrl)
            {
                return TimeIrlWidgetRenderer.GetPreferredWidth();
            }
            if (widgetId == MainBarWidgetIds.TimeSpeed)
            {
                return TimeSpeedWidgetRenderer.GetPreferredWidth();
            }
            if (widgetId == MainBarWidgetIds.Weather)
            {
                return WeatherWidgetRenderer.GetPreferredWidth();
            }
            if (widgetId == MainBarWidgetIds.FpsTps)
            {
                return FpsTpsWidgetRenderer.GetPreferredWidth();
            }
            if (widgetId == MainBarWidgetIds.Battery)
            {
                return BatteryWidgetRenderer.GetPreferredWidth();
            }

            return 120f;
        }

        private static float GetWidgetMinWidth(string widgetId)
        {
            if (widgetId == MainBarWidgetIds.Time)
            {
                return 150f;
            }
            if (widgetId == MainBarWidgetIds.TimeIrl)
            {
                return 90f;
            }
            if (widgetId == MainBarWidgetIds.TimeSpeed)
            {
                return 120f;
            }
            if (widgetId == MainBarWidgetIds.Weather)
            {
                return 110f;
            }
            if (widgetId == MainBarWidgetIds.FpsTps)
            {
                return 120f;
            }
            if (widgetId == MainBarWidgetIds.Battery)
            {
                return 96f;
            }

            return 80f;
        }

        private static void DrawWidget(string widgetId, Rect rect)
        {
            if (widgetId == MainBarWidgetIds.Time)
            {
                TimeWidgetRenderer.Draw(rect);
                return;
            }

            if (widgetId == MainBarWidgetIds.TimeIrl)
            {
                TimeIrlWidgetRenderer.Draw(rect);
                return;
            }

            if (widgetId == MainBarWidgetIds.TimeSpeed)
            {
                TimeSpeedWidgetRenderer.Draw(rect);
                return;
            }

            if (widgetId == MainBarWidgetIds.Weather)
            {
                WeatherWidgetRenderer.Draw(rect);
                return;
            }

            if (widgetId == MainBarWidgetIds.FpsTps)
            {
                FpsTpsWidgetRenderer.Draw(rect);
                return;
            }

            if (widgetId == MainBarWidgetIds.Battery)
            {
                BatteryWidgetRenderer.Draw(rect);
            }
        }
    }
}
