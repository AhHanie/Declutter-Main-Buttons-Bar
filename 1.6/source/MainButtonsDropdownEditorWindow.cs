using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public class MainButtonsDropdownEditorWindow : Window
    {
        private const float RowHeight = 32f;
        private const float SearchHeight = 26f;
        private const float IconSize = 22f;
        private const float ToggleSize = 18f;
        private const float MoveButtonSize = 18f;
        private const float RowPadding = 6f;
        private const float SectionSpacing = 10f;
        private static readonly Color PanelBg = new Color(0.11f, 0.11f, 0.11f, 1f);
        private static readonly Color DisabledButtonColor = new Color(1f, 1f, 1f, 0.35f);
        private static readonly List<MainButtonDef> EmptyDefList = new List<MainButtonDef>();

        private readonly MainButtonDef parentDef;
        private readonly QuickSearchWidget quickSearchWidget = new QuickSearchWidget();
        private Vector2 orderScrollPosition = Vector2.zero;
        private Vector2 availableScrollPosition = Vector2.zero;
        private List<MainButtonDef> cachedDefs = new List<MainButtonDef>();
        private bool triedToFocus;
        private int openFrames;

        public override Vector2 InitialSize => new Vector2(420f, 620f);

        public MainButtonsDropdownEditorWindow(MainButtonDef parentDef)
        {
            this.parentDef = parentDef;
            draggable = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            cachedDefs = MainButtonsCache.AllButtonsInOrderNoDMMBButton
                .Where(def => def != parentDef)
                .ToList();
            quickSearchWidget.Reset();
            CacheSearchState();
            triedToFocus = false;
            openFrames = 0;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            if (!triedToFocus && openFrames == 2)
            {
                quickSearchWidget.Focus();
                triedToFocus = true;
            }

            List<MainButtonDef> orderedEntries = ModSettings.GetDropdownConfig(parentDef)?.entryOrder ?? EmptyDefList;

            float chromeHeight = Text.LineHeight + 4f
                + Text.LineHeight * 2f + 6f
                + Text.LineHeight + 2f
                + SectionSpacing
                + Text.LineHeight + 2f
                + (SearchHeight + 4f);
            float listsHeight = Mathf.Max(RowHeight * 4f, inRect.height - chromeHeight);
            float orderListHeight = Mathf.Max(RowHeight * 2f, Mathf.Floor(listsHeight * 0.42f));
            float availableListHeight = Mathf.Max(RowHeight * 2f, listsHeight - orderListHeight);

            float curY = 0f;

            Rect titleRect = new Rect(0f, curY, inRect.width, Text.LineHeight);
            Widgets.Label(titleRect, "DMMB.DropdownEditorTitle".Translate(parentDef != null ? ModSettings.GetDisplayLabel(parentDef) : string.Empty));
            curY = titleRect.yMax + 4f;

            Text.Font = GameFont.Tiny;
            Rect descRect = new Rect(0f, curY, inRect.width, Text.LineHeight * 2f);
            Widgets.Label(descRect, "DMMB.DropdownEditorDesc".Translate());
            curY = descRect.yMax + 6f;
            Text.Font = GameFont.Small;

            Rect orderTitleRect = new Rect(0f, curY, inRect.width, Text.LineHeight);
            Widgets.Label(orderTitleRect, "DMMB.DropdownOrderSectionTitle".Translate(orderedEntries.Count));
            curY = orderTitleRect.yMax + 2f;

            Rect orderListRect = new Rect(0f, curY, inRect.width, orderListHeight);
            DrawOrderList(orderListRect, orderedEntries);
            curY = orderListRect.yMax + SectionSpacing;

            Rect availableTitleRect = new Rect(0f, curY, inRect.width, Text.LineHeight);
            Widgets.Label(availableTitleRect, "DMMB.DropdownAvailableSectionTitle".Translate());
            curY = availableTitleRect.yMax + 2f;

            Rect topBarRect = new Rect(0f, curY, inRect.width, SearchHeight + 4f);
            Widgets.DrawBoxSolid(topBarRect, PanelBg);
            Rect searchRect = new Rect(6f, topBarRect.y + 4f, inRect.width - 12f, SearchHeight);
            quickSearchWidget.OnGUI(searchRect, CacheSearchState);
            curY = topBarRect.yMax;

            Rect availableListRect = new Rect(0f, curY, inRect.width, availableListHeight);
            DrawAvailableList(availableListRect, new HashSet<MainButtonDef>(orderedEntries));
        }

        private void DrawOrderList(Rect listRect, List<MainButtonDef> orderedEntries)
        {
            Widgets.DrawBoxSolid(listRect, PanelBg);

            if (orderedEntries.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(listRect, "DMMB.DropdownOrderSectionEmpty".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            float viewHeight = orderedEntries.Count * RowHeight;
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, Mathf.Max(viewHeight, listRect.height));

            Widgets.BeginScrollView(listRect, ref orderScrollPosition, viewRect);

            float curY = 0f;
            for (int i = 0; i < orderedEntries.Count; i++)
            {
                MainButtonDef def = orderedEntries[i];
                Rect rowRect = new Rect(0f, curY, viewRect.width, RowHeight);
                Widgets.DrawHighlightIfMouseover(rowRect);

                float x = rowRect.x + RowPadding;
                Rect removeRect = new Rect(x, rowRect.y + (rowRect.height - ToggleSize) / 2f, ToggleSize, ToggleSize);
                x = removeRect.xMax + RowPadding;
                Rect upRect = new Rect(x, rowRect.y + (rowRect.height - MoveButtonSize) / 2f, MoveButtonSize, MoveButtonSize);
                x = upRect.xMax + 2f;
                Rect downRect = new Rect(x, rowRect.y + (rowRect.height - MoveButtonSize) / 2f, MoveButtonSize, MoveButtonSize);
                x = downRect.xMax + RowPadding;
                Rect iconRect = new Rect(x, rowRect.y + (rowRect.height - IconSize) / 2f, IconSize, IconSize);
                Rect textRect = rowRect;
                textRect.xMin = iconRect.xMax + RowPadding;
                textRect.xMax = rowRect.xMax - RowPadding;

                if (Widgets.ButtonImage(removeRect, TexButton.Minus, Color.white, true, "DMMB.DropdownRemoveTooltip".Translate()))
                {
                    ModSettings.SetDropdownEntry(parentDef, def, false);
                    Mod.Settings.Write();
                }

                bool canMoveUp = i > 0;
                if (Widgets.ButtonImage(upRect, TexButton.ReorderUp, canMoveUp ? Color.white : DisabledButtonColor, true, "DMMB.DropdownMoveEarlierTooltip".Translate()) && canMoveUp)
                {
                    ModSettings.MoveDropdownEntry(parentDef, def, -1);
                    Mod.Settings.Write();
                }

                bool canMoveDown = i < orderedEntries.Count - 1;
                if (Widgets.ButtonImage(downRect, TexButton.ReorderDown, canMoveDown ? Color.white : DisabledButtonColor, true, "DMMB.DropdownMoveLaterTooltip".Translate()) && canMoveDown)
                {
                    ModSettings.MoveDropdownEntry(parentDef, def, 1);
                    Mod.Settings.Write();
                }

                DrawRowLabel(iconRect, textRect, def);
                TooltipHandler.TipRegion(rowRect, MainButtonDisplayUtility.BuildTooltip(ModSettings.GetDisplayLabel(def), ModSettings.GetDisplayDescription(def)));

                curY += RowHeight;
            }

            Widgets.EndScrollView();
        }

        private void DrawAvailableList(Rect listRect, HashSet<MainButtonDef> selected)
        {
            List<MainButtonDef> filteredDefs = GetFilteredAvailableDefs(selected);
            float viewHeight = filteredDefs.Count * RowHeight;
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, Mathf.Max(viewHeight, listRect.height));

            Widgets.BeginScrollView(listRect, ref availableScrollPosition, viewRect);

            float curY = 0f;
            for (int i = 0; i < filteredDefs.Count; i++)
            {
                MainButtonDef def = filteredDefs[i];
                Rect rowRect = new Rect(0f, curY, viewRect.width, RowHeight);
                Widgets.DrawHighlightIfMouseover(rowRect);

                Rect addRect = new Rect(rowRect.x + RowPadding, rowRect.y + (rowRect.height - ToggleSize) / 2f, ToggleSize, ToggleSize);
                Rect iconRect = new Rect(addRect.xMax + RowPadding, rowRect.y + (rowRect.height - IconSize) / 2f, IconSize, IconSize);
                Rect textRect = rowRect;
                textRect.xMin = iconRect.xMax + RowPadding;
                textRect.xMax = rowRect.xMax - RowPadding;

                if (Widgets.ButtonImage(addRect, TexButton.Plus, Color.white, true, "DMMB.DropdownAddTooltip".Translate()))
                {
                    ModSettings.SetDropdownEntry(parentDef, def, true);
                    Mod.Settings.Write();
                }

                DrawRowLabel(iconRect, textRect, def);
                TooltipHandler.TipRegion(rowRect, MainButtonDisplayUtility.BuildTooltip(ModSettings.GetDisplayLabel(def), ModSettings.GetDisplayDescription(def)));

                curY += RowHeight;
            }

            Widgets.EndScrollView();
        }

        private static void DrawRowLabel(Rect iconRect, Rect textRect, MainButtonDef def)
        {
            bool enabled = !def.Worker.Disabled;
            string effectiveLabel = ModSettings.GetDisplayLabel(def);
            Texture2D effectiveIcon = ModSettings.GetDisplayIcon(def);
            if (effectiveIcon != null)
            {
                Widgets.DrawTextureFitted(iconRect, effectiveIcon, 1f);
            }

            Color prev = GUI.color;
            TextAnchor prevAnchor = Text.Anchor;
            if (!enabled)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.55f);
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(textRect, effectiveLabel);
            Text.Anchor = prevAnchor;
            GUI.color = prev;
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();
            openFrames++;
        }

        public override void Notify_ClickOutsideWindow()
        {
            base.Notify_ClickOutsideWindow();
            quickSearchWidget.Unfocus();
        }

        private List<MainButtonDef> GetFilteredAvailableDefs(HashSet<MainButtonDef> selected)
        {
            List<MainButtonDef> filtered = new List<MainButtonDef>();
            for (int i = 0; i < cachedDefs.Count; i++)
            {
                MainButtonDef def = cachedDefs[i];
                if (selected.Contains(def))
                {
                    continue;
                }

                if (quickSearchWidget.filter.Active && !MainButtonDisplayUtility.MatchesFilter(quickSearchWidget.filter, def))
                {
                    continue;
                }

                filtered.Add(def);
            }

            return filtered;
        }

        private void CacheSearchState()
        {
            HashSet<MainButtonDef> selected = new HashSet<MainButtonDef>(ModSettings.GetDropdownConfig(parentDef)?.entryOrder ?? EmptyDefList);
            quickSearchWidget.noResultsMatched = GetFilteredAvailableDefs(selected).Count == 0;
        }

    }
}
