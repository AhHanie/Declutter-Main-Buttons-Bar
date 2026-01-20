using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static partial class MainButtonsRoot_DoButtons_Patch
    {
        private static void HandleEditInput(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths,
            List<Rect> rects,
            float availableWidth,
            bool allowReorder,
            float startX,
            bool useFreeSize)
        {
            if (!ModSettings.editDropdownsMode)
            {
                return;
            }

            Event evt = Event.current;
            Vector2 mousePos = evt.mousePosition;

            if (resizingDef != null)
            {
                if (evt.type == EventType.MouseDrag)
                {
                    float delta = mousePos.x - resizeStartMouseX;
                    float newWidth = resizeStartWidth + delta * 2f;
                    float maxWidth = GetMaxWidthForDef(resizingDef, widths, availableWidth);
                    newWidth = Mathf.Clamp(newWidth, MinFreeSizeWidth, maxWidth);
                    widths[resizingDef] = newWidth;
                    ModSettings.freeSizeWidths[resizingDef] = newWidth;
                    evt.Use();
                }
                else if (evt.type == EventType.MouseUp)
                {
                    resizingDef = null;
                    evt.Use();
                }

                return;
            }

            if (draggingDef != null)
            {
                if (evt.type == EventType.MouseUp)
                {
                    List<MainButtonDef> finalOrder = currentDragOrder.Count > 0 ? currentDragOrder : defs;
                    CommitCustomOrder(finalOrder, MainButtonsCache.AllButtonsInOrder);
                    draggingDef = null;
                    evt.Use();
                }

                return;
            }

            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                Rect rect = rects[i];
                Rect plusRect = GetEditPlusRect(rect);
                Rect resizeRect = GetResizeRect(rect);

                if (evt.type != EventType.MouseDown || evt.button != 0)
                {
                    continue;
                }

                if (Mouse.IsOver(plusRect))
                {
                    evt.Use();
                    Find.WindowStack.Add(new MainButtonsDropdownEditorWindow(def));
                    return;
                }

                if (Mouse.IsOver(resizeRect))
                {
                    EnsureFreeSizeModeForEdit(defs, widths);
                    resizingDef = def;
                    resizeStartWidth = widths[def];
                    resizeStartMouseX = mousePos.x;
                    evt.Use();
                    return;
                }

                if (allowReorder && Mouse.IsOver(rect))
                {
                    EnsureFreeSizeModeForEdit(defs, widths);
                    draggingDef = def;
                    dragOffsetX = mousePos.x - rect.x;
                    evt.Use();
                    return;
                }
            }

        }

        private static void HandleEditInputWithPositions(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths,
            Dictionary<MainButtonDef, float> xPositions,
            List<Rect> rects,
            float availableWidth,
            bool allowReorder)
        {
            if (!ModSettings.editDropdownsMode)
            {
                return;
            }

            Event evt = Event.current;
            Vector2 mousePos = evt.mousePosition;

            if (resizingDef != null)
            {
                if (evt.type == EventType.MouseDrag)
                {
                    float delta = mousePos.x - resizeStartMouseX;
                    float newWidth = resizeStartWidth + delta * 2f;
                    float maxWidth = availableWidth - xPositions[resizingDef];
                    newWidth = Mathf.Clamp(newWidth, MinFreeSizeWidth, maxWidth);
                    widths[resizingDef] = newWidth;
                    ModSettings.freeSizeWidths[resizingDef] = newWidth;
                    evt.Use();
                }
                else if (evt.type == EventType.MouseUp)
                {
                    resizingDef = null;
                    evt.Use();
                }

                return;
            }

            if (draggingDef != null)
            {
                if (evt.type == EventType.MouseUp)
                {
                    List<MainButtonDef> newOrder = currentDragOrder.Count > 0 ? currentDragOrder : defs;

                    float dragWidth = widths[draggingDef];
                    float maxX = availableWidth - dragWidth;
                    float finalDragX = Mathf.Clamp(mousePos.x - dragOffsetX, 0f, maxX);

                    int dragIndex = newOrder.IndexOf(draggingDef);

                    if (dragIndex > 0)
                    {
                        MainButtonDef prevDef = newOrder[dragIndex - 1];
                        float prevRight = xPositions[prevDef] + widths[prevDef];
                        float gapToPrev = finalDragX - prevRight;
                        if (gapToPrev >= 0f && gapToPrev <= ModSettings.snapThreshold)
                        {
                            finalDragX = prevRight;
                        }
                    }

                    if (dragIndex < newOrder.Count - 1)
                    {
                        MainButtonDef nextDef = newOrder[dragIndex + 1];
                        float nextX = xPositions[nextDef];
                        float gapToNext = nextX - (finalDragX + dragWidth);
                        if (gapToNext >= 0f && gapToNext <= ModSettings.snapThreshold)
                        {
                            finalDragX = nextX - dragWidth;
                        }
                    }

                    Dictionary<MainButtonDef, float> testPositions = new Dictionary<MainButtonDef, float>(xPositions);
                    testPositions[draggingDef] = finalDragX;

                    RecalculatePositionsForOrder(newOrder, widths, testPositions, finalDragX, draggingDef, availableWidth);

                    bool wouldOverflow = false;
                    for (int i = 0; i < newOrder.Count; i++)
                    {
                        MainButtonDef def = newOrder[i];
                        float x = testPositions[def];
                        float width = widths[def];

                        if (x < -0.5f || x + width > availableWidth + 0.5f)
                        {
                            wouldOverflow = true;
                            break;
                        }

                        if (i < newOrder.Count - 1)
                        {
                            MainButtonDef nextDef = newOrder[i + 1];
                            float nextX = testPositions[nextDef];
                            if (x + width > nextX + 0.5f)
                            {
                                wouldOverflow = true;
                                break;
                            }
                        }
                    }

                    if (!wouldOverflow)
                    {
                        foreach (var kvp in testPositions)
                        {
                            xPositions[kvp.Key] = kvp.Value;
                        }

                        CommitCustomOrder(newOrder, MainButtonsCache.AllButtonsInOrder);

                        foreach (var kvp in xPositions)
                        {
                            ModSettings.freeSizeXPositions[kvp.Key] = kvp.Value;
                        }
                    }

                    draggingDef = null;
                    evt.Use();
                }

                return;
            }

            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                Rect rect = rects[i];
                Rect plusRect = GetEditPlusRect(rect);
                Rect resizeRect = GetResizeRect(rect);

                if (evt.type != EventType.MouseDown || evt.button != 0)
                {
                    continue;
                }

                if (Mouse.IsOver(plusRect))
                {
                    evt.Use();
                    Find.WindowStack.Add(new MainButtonsDropdownEditorWindow(def));
                    return;
                }

                if (Mouse.IsOver(resizeRect))
                {
                    EnsureFreeSizeModeForEdit(defs, widths);
                    resizingDef = def;
                    resizeStartWidth = widths[def];
                    resizeStartMouseX = mousePos.x;
                    evt.Use();
                    return;
                }

                if (allowReorder && Mouse.IsOver(rect))
                {
                    EnsureFreeSizeModeForEdit(defs, widths);
                    draggingDef = def;
                    dragOffsetX = mousePos.x - rect.x;
                    evt.Use();
                    return;
                }
            }

        }

        private static List<MainButtonDef> GetDragSwapOrder(List<MainButtonDef> defs, List<Rect> rects)
        {
            if (draggingDef == null || !defs.Contains(draggingDef))
            {
                return defs;
            }

            float dragX = Event.current.mousePosition.x - dragOffsetX;

            List<MainButtonDef> reordered = new List<MainButtonDef>(defs);
            reordered.Remove(draggingDef);

            int insertIndex = 0;
            for (int i = 0; i < reordered.Count; i++)
            {
                int origIndex = defs.IndexOf(reordered[i]);
                if (origIndex >= 0 && origIndex < rects.Count)
                {
                    Rect rect = rects[origIndex];
                    if (dragX > rect.x + rect.width / 2f)
                    {
                        insertIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            reordered.Insert(insertIndex, draggingDef);
            return reordered;
        }

        private static void CalculateDragPositions(
            List<MainButtonDef> orderedDefs,
            Dictionary<MainButtonDef, float> widths,
            Dictionary<MainButtonDef, float> positions,
            float availableWidth)
        {
            if (draggingDef == null)
            {
                return;
            }

            float dragWidth = widths[draggingDef];
            float maxX = availableWidth - dragWidth;
            float dragX = Mathf.Clamp(Event.current.mousePosition.x - dragOffsetX, 0f, maxX);

            int dragIndex = orderedDefs.IndexOf(draggingDef);
            if (dragIndex < 0)
            {
                return;
            }

            if (dragIndex > 0)
            {
                MainButtonDef prevDef = orderedDefs[dragIndex - 1];
                float prevRight = positions[prevDef] + widths[prevDef];
                float gapToPrev = dragX - prevRight;
                if (gapToPrev >= 0f && gapToPrev <= ModSettings.snapThreshold)
                {
                    dragX = prevRight;
                }
            }

            if (dragIndex < orderedDefs.Count - 1)
            {
                MainButtonDef nextDef = orderedDefs[dragIndex + 1];
                float nextX = positions[nextDef];
                float gapToNext = nextX - (dragX + dragWidth);
                if (gapToNext >= 0f && gapToNext <= ModSettings.snapThreshold)
                {
                    dragX = nextX - dragWidth;
                }
            }

            positions[draggingDef] = dragX;
        }

        private static List<MainButtonDef> GetDragOrderWithPositions(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths,
            Dictionary<MainButtonDef, float> xPositions,
            List<Rect> rects,
            float availableWidth)
        {
            if (draggingDef == null || !defs.Contains(draggingDef))
            {
                return defs;
            }

            float dragX = Event.current.mousePosition.x - dragOffsetX;
            float dragCenterX = dragX + widths[draggingDef] / 2f;

            List<MainButtonDef> reordered = new List<MainButtonDef>(defs);
            reordered.Remove(draggingDef);

            int insertIndex = 0;
            for (int i = 0; i < reordered.Count; i++)
            {
                MainButtonDef def = reordered[i];
                if (xPositions.ContainsKey(def))
                {
                    float defCenterX = xPositions[def] + widths[def] / 2f;
                    if (dragCenterX > defCenterX)
                    {
                        insertIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            reordered.Insert(insertIndex, draggingDef);
            return reordered;
        }

        private static void RecalculatePositionsForOrder(
            List<MainButtonDef> orderedDefs,
            Dictionary<MainButtonDef, float> widths,
            Dictionary<MainButtonDef, float> xPositions,
            float draggedButtonFinalX,
            MainButtonDef draggedButton,
            float availableWidth)
        {
            for (int i = 0; i < orderedDefs.Count; i++)
            {
                MainButtonDef def = orderedDefs[i];
                float width = widths[def];
                float x = xPositions.ContainsKey(def) ? xPositions[def] : 0f;

                for (int j = 0; j < i; j++)
                {
                    MainButtonDef prevDef = orderedDefs[j];
                    float prevX = xPositions[prevDef];
                    float prevWidth = widths[prevDef];
                    float prevRight = prevX + prevWidth;

                    if (x < prevRight)
                    {
                        x = prevRight;
                    }
                }

                float maxX = availableWidth - width;
                x = Mathf.Clamp(x, 0f, maxX);
                xPositions[def] = x;
            }
        }

        private static float GetMaxWidthForDef(MainButtonDef def, Dictionary<MainButtonDef, float> widths, float availableWidth)
        {
            float total = 0f;
            foreach (KeyValuePair<MainButtonDef, float> entry in widths)
            {
                total += entry.Value;
            }

            float currentWidth = widths[def];
            float otherWidth = total - currentWidth;
            return Mathf.Max(MinFreeSizeWidth, availableWidth - otherWidth);
        }

        private static void EnsureFreeSizeModeForEdit(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths)
        {
            if (!ModSettings.useFreeSizeMode)
            {
                ModSettings.useFreeSizeMode = true;
                ModSettings.useFixedWidthMode = false;
            }

            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                if (!ModSettings.freeSizeWidths.ContainsKey(def))
                {
                    ModSettings.freeSizeWidths[def] = widths[def];
                }
            }
        }

        private static void CommitCustomOrder(List<MainButtonDef> visibleOrder, List<MainButtonDef> allButtons)
        {
            List<MainButtonDef> newOrder = new List<MainButtonDef>();
            for (int i = 0; i < visibleOrder.Count; i++)
            {
                MainButtonDef def = visibleOrder[i];
                if (def != null && !newOrder.Contains(def))
                {
                    newOrder.Add(def);
                }
            }

            for (int i = 0; i < ModSettings.customOrderDefs.Count; i++)
            {
                MainButtonDef def = ModSettings.customOrderDefs[i];
                if (def != null && !newOrder.Contains(def))
                {
                    newOrder.Add(def);
                }
            }

            for (int i = 0; i < allButtons.Count; i++)
            {
                MainButtonDef def = allButtons[i];
                if (def != null && !newOrder.Contains(def))
                {
                    newOrder.Add(def);
                }
            }

            ModSettings.customOrderDefs = newOrder;
            InvalidateOrderedVisibleCache();
        }
    }
}
