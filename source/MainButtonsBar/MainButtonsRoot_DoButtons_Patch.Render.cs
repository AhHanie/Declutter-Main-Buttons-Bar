using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static partial class MainButtonsRoot_DoButtons_Patch
    {
        private static void DrawButtonRow(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths,
            float startX,
            float availableWidth,
            bool allowDropdown,
            bool allowReorder,
            ref MainButtonDef hoveredDef,
            ref Rect hoveredRect,
            bool useFreeSize)
        {
            if (defs.Count == 0)
            {
                return;
            }

            List<MainButtonDef> baseDefs = defs;
            if (draggingDef != null && IsCurrentDragOrderCompatible(defs))
            {
                baseDefs = currentDragOrder;
            }

            List<Rect> baseRects = BuildRects(baseDefs, widths, startX);
            HandleEditInput(baseDefs, widths, baseRects, availableWidth, allowReorder, startX, useFreeSize);

            List<MainButtonDef> drawOrder = GetDragSwapOrder(baseDefs, baseRects);
            currentDragOrder = drawOrder;
            List<Rect> drawRects = BuildRects(drawOrder, widths, startX);

            for (int i = 0; i < drawOrder.Count; i++)
            {
                MainButtonDef def = drawOrder[i];
                if (def == draggingDef)
                {
                    continue;
                }

                Rect rect = drawRects[i];
                DrawMainButton(def, rect, ref hoveredDef, ref hoveredRect, allowDropdown);
            }

            if (draggingDef != null)
            {
                float width = widths[draggingDef];
                float maxX = startX + availableWidth - width;
                float x = Mathf.Clamp(Event.current.mousePosition.x - dragOffsetX, startX, maxX);
                Rect dragRect = new Rect(x, UI.screenHeight - BarBottomOffset, width, BarHeight);
                Color prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.85f);
                DrawMainButton(draggingDef, dragRect, ref hoveredDef, ref hoveredRect, allowDropdown: false);
                GUI.color = prev;
            }
        }

        private static void DrawButtonRowWithPositions(
            List<MainButtonDef> defs,
            Dictionary<MainButtonDef, float> widths,
            Dictionary<MainButtonDef, float> xPositions,
            float availableWidth,
            bool allowDropdown,
            bool allowReorder,
            ref MainButtonDef hoveredDef,
            ref Rect hoveredRect)
        {
            if (defs.Count == 0)
            {
                return;
            }

            List<MainButtonDef> baseDefs = defs;
            if (draggingDef != null && IsCurrentDragOrderCompatible(defs))
            {
                baseDefs = currentDragOrder;
            }

            List<Rect> baseRects = BuildRectsWithPositions(baseDefs, widths, xPositions);
            HandleEditInputWithPositions(baseDefs, widths, xPositions, baseRects, availableWidth, allowReorder);

            List<MainButtonDef> drawOrder = GetDragOrderWithPositions(baseDefs, widths, xPositions, baseRects, availableWidth);
            currentDragOrder = drawOrder;

            Dictionary<MainButtonDef, float> drawPositions = new Dictionary<MainButtonDef, float>(xPositions);

            if (draggingDef != null && ModSettings.editDropdownsMode)
            {
                CalculateDragPositions(drawOrder, widths, drawPositions, availableWidth);
            }

            List<Rect> drawRects = BuildRectsWithPositions(drawOrder, widths, drawPositions);

            for (int i = 0; i < drawOrder.Count; i++)
            {
                MainButtonDef def = drawOrder[i];
                if (def == draggingDef)
                {
                    continue;
                }

                Rect rect = drawRects[i];
                DrawMainButton(def, rect, ref hoveredDef, ref hoveredRect, allowDropdown);
            }

            if (draggingDef != null)
            {
                float x = drawPositions[draggingDef];
                float width = widths[draggingDef];

                Rect dragRect = new Rect(x, UI.screenHeight - BarBottomOffset, width, BarHeight);
                Color prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.85f);
                DrawMainButton(draggingDef, dragRect, ref hoveredDef, ref hoveredRect, allowDropdown: false);
                GUI.color = prev;
            }
        }

        private static void DrawSingleButton(
            MainButtonDef def,
            Rect rect,
            float availableWidth,
            ref MainButtonDef hoveredDef,
            ref Rect hoveredRect,
            bool allowReorder)
        {
            List<MainButtonDef> defs = new List<MainButtonDef> { def };
            Dictionary<MainButtonDef, float> widths = new Dictionary<MainButtonDef, float> { { def, rect.width } };
            List<Rect> rects = new List<Rect> { rect };

            HandleEditInput(defs, widths, rects, availableWidth, allowReorder, rect.x, useFreeSize: false);
            DrawMainButton(def, rect, ref hoveredDef, ref hoveredRect, allowDropdown: true);
        }

        private static void DrawMainButton(
            MainButtonDef def,
            Rect rect,
            ref MainButtonDef hoveredDef,
            ref Rect hoveredRect,
            bool allowDropdown)
        {
            def.Worker.DoButton(rect);

            if (ModSettings.editDropdownsMode)
            {
                DrawEditPlusOverlay(rect);
                DrawResizeHandle(rect);
                return;
            }

            if (allowDropdown && ModSettings.HasDropdown(def) && Mouse.IsOver(rect))
            {
                hoveredDef = def;
                hoveredRect = rect;
            }
        }

        private static void DrawEditPlusOverlay(Rect rect)
        {
            Rect plusRect = GetEditPlusRect(rect);
            bool hovered = Mouse.IsOver(plusRect);
            Texture2D tex = hovered ? DMMBTextures.PlusGold.Texture : DMMBTextures.PlusGray.Texture;
            Color prev = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(plusRect, tex);
            GUI.color = prev;
            TooltipHandler.TipRegion(plusRect, "DMMB.DropdownEditButton".Translate());
        }

        private static void DrawResizeHandle(Rect rect)
        {
            Rect handleRect = GetResizeRect(rect);
            Color prev = GUI.color;
            GUI.color = Mouse.IsOver(handleRect) ? new Color(1f, 1f, 1f, 0.9f) : new Color(1f, 1f, 1f, 0.6f);

            float x0 = handleRect.x;
            float x1 = handleRect.xMax;
            float y0 = handleRect.y;
            float y1 = handleRect.yMax;
            Widgets.DrawLineHorizontal(x0, y1, handleRect.width);
            Widgets.DrawLineVertical(x1, y0, handleRect.height);

            GUI.color = prev;
        }

        private static bool IsCurrentDragOrderCompatible(List<MainButtonDef> defs)
        {
            if (currentDragOrder.Count != defs.Count)
            {
                return false;
            }

            for (int i = 0; i < currentDragOrder.Count; i++)
            {
                if (!defs.Contains(currentDragOrder[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static void DrawEditGizmoPreview()
        {
            if (!ModSettings.editDropdownsMode)
            {
                gizmoDragActive = false;
                return;
            }

            EnsurePreviewGizmos();

            float startX = GetDefaultGizmoStartX();
            float maxWidth = GetGizmoMaxWidth(startX);
            float gridWidth = GetPreviewGridWidth(previewGizmos, maxWidth);

            GizmoGridDrawer_DrawGizmoGrid_Patch.BeginPreviewDraw();
            DrawGizmoGridPreview(previewGizmos, startX, out _);
            GizmoGridDrawer_DrawGizmoGrid_Patch.EndPreviewDraw();

            if (!GizmoGridDrawer_DrawGizmoGrid_Patch.TryGetLastPreviewPosition(out Vector2 previewPos))
            {
                return;
            }

            lastPreviewGizmoRect = new Rect(previewPos.x, previewPos.y, gridWidth, 75f);
            Rect handleRect = GetGizmoPreviewHandleRect(lastPreviewGizmoRect);
            HandleGizmoPreviewDrag(handleRect);
            DrawGizmoPreviewOutline(lastPreviewGizmoRect);
            DrawGizmoPreviewHandle(handleRect);
        }

        private static void EnsurePreviewGizmos()
        {
            if (previewGizmos.Count > 0)
            {
                return;
            }

            Command_Action gizmoA = new Command_Action
            {
                defaultLabel = "Gizmo A",
                defaultDesc = "Edit mode preview gizmo.",
                icon = BaseContent.BadTex,
                action = delegate { }
            };

            Command_Action gizmoB = new Command_Action
            {
                defaultLabel = "Gizmo B",
                defaultDesc = "Edit mode preview gizmo.",
                icon = BaseContent.BadTex,
                action = delegate { }
            };

            previewGizmos.Add(gizmoA);
            previewGizmos.Add(gizmoB);
        }

        private static Rect GetGizmoPreviewHandleRect(Rect gizmoRect)
        {
            const float handleHeight = 36f;
            const float handleGap = 4f;
            return new Rect(gizmoRect.x, gizmoRect.y - handleHeight - handleGap, gizmoRect.width, handleHeight);
        }

        private static void HandleGizmoPreviewDrag(Rect handleRect)
        {
            Event evt = Event.current;
            Vector2 mousePos = evt.mousePosition;

            if (gizmoDragActive)
            {
                if (evt.type == EventType.MouseDrag)
                {
                    Vector2 delta = mousePos - gizmoDragStartMouse;
                    ModSettings.gizmoDrawerOffsetX = gizmoDragStartOffset.x + delta.x;
                    ModSettings.gizmoDrawerOffsetY = gizmoDragStartOffset.y + delta.y;
                    evt.Use();
                }
                else if (evt.type == EventType.MouseUp)
                {
                    gizmoDragActive = false;
                    Mod.Settings.Write();
                    evt.Use();
                }

                return;
            }

            if (evt.type == EventType.MouseDown && evt.button == 0 && Mouse.IsOver(handleRect))
            {
                gizmoDragActive = true;
                gizmoDragStartMouse = mousePos;
                gizmoDragStartOffset = new Vector2(ModSettings.gizmoDrawerOffsetX, ModSettings.gizmoDrawerOffsetY);
                evt.Use();
            }
        }

        private static void DrawGizmoPreviewOutline(Rect rect)
        {
            Color prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.35f);
            Widgets.DrawBox(rect, 1);
            GUI.color = prev;
        }

        private static void DrawGizmoPreviewHandle(Rect rect)
        {
            Color prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            Widgets.DrawBoxSolid(rect, GUI.color);
            GUI.color = new Color(1f, 1f, 1f, 0.6f);
            Widgets.DrawBox(rect, 1);

            float gripX = rect.x + 6f;
            float gripY = rect.y + 5f;
            float gripHeight = rect.height - 10f;
            Widgets.DrawLineVertical(gripX, gripY, gripHeight);
            Widgets.DrawLineVertical(gripX + 3f, gripY, gripHeight);
            Widgets.DrawLineVertical(gripX + 6f, gripY, gripHeight);

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;
            Widgets.Label(rect, "DMMB.GizmoPreviewTitle".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = prev;
        }

        private static float GetPreviewGridWidth(List<Gizmo> gizmos, float maxWidth)
        {
            if (gizmos.Count == 0)
            {
                return 75f;
            }

            float spacing = GizmoGridDrawer.GizmoSpacing.x;
            float total = 0f;
            for (int i = 0; i < gizmos.Count; i++)
            {
                if (i > 0)
                {
                    total += spacing;
                }

                total += gizmos[i].GetWidth(maxWidth);
            }

            return total;
        }

        private static float GetGizmoMaxWidth(float startX)
        {
            return UI.screenWidth - 147f - startX;
        }

        private static float GetDefaultGizmoStartX()
        {
            float startX = GizmoGridDrawer.GizmoStartX;
            IInspectPane inspectPane = Find.WindowStack.WindowOfType<IInspectPane>();
            if (inspectPane != null)
            {
                startX += InspectPaneUtility.PaneWidthFor(inspectPane);
            }

            return startX;
        }

        private static void DrawGizmoGridPreview(List<Gizmo> gizmos, float startX, out Gizmo mouseover)
        {
            GizmoGridDrawer.DrawGizmoGrid(gizmos, startX, out mouseover);
        }
    }
}
