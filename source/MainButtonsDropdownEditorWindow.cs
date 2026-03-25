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
        private const float RowPadding = 6f;
        private static readonly Color PanelBg = new Color(0.11f, 0.11f, 0.11f, 1f);
        private readonly MainButtonDef parentDef;
        private readonly QuickSearchWidget quickSearchWidget = new QuickSearchWidget();
        private Vector2 scrollPosition = Vector2.zero;
        private List<MainButtonDef> cachedDefs = new List<MainButtonDef>();
        private bool triedToFocus;
        private int openFrames;

        public override Vector2 InitialSize => new Vector2(420f, 520f);

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

            Rect titleRect = new Rect(0f, 0f, inRect.width, Text.LineHeight);
            Widgets.Label(titleRect, "DMMB.DropdownEditorTitle".Translate(parentDef?.LabelCap ?? string.Empty));

            Text.Font = GameFont.Tiny;
            Rect descRect = new Rect(0f, titleRect.yMax + 4f, inRect.width, Text.LineHeight * 2f);
            Widgets.Label(descRect, "DMMB.DropdownEditorDesc".Translate());
            Text.Font = GameFont.Small;

            Rect topBarRect = new Rect(0f, descRect.yMax + 6f, inRect.width, SearchHeight + 4f);
            Widgets.DrawBoxSolid(topBarRect, PanelBg);

            Rect searchRect = new Rect(6f, topBarRect.y + 4f, inRect.width - 12f, SearchHeight);
            Rect listRect = new Rect(0f, topBarRect.yMax, inRect.width, inRect.height - topBarRect.yMax);
            quickSearchWidget.OnGUI(searchRect, CacheSearchState);

            List<MainButtonDef> filteredDefs = GetFilteredDefs();
            float viewHeight = filteredDefs.Count * RowHeight;
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, Mathf.Max(viewHeight, listRect.height));

            Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);

            float curY = 0f;
            for (int i = 0; i < filteredDefs.Count; i++)
            {
                MainButtonDef def = filteredDefs[i];
                Rect rowRect = new Rect(0f, curY, viewRect.width, RowHeight);
                Widgets.DrawHighlightIfMouseover(rowRect);

                bool enabled = !def.Worker.Disabled;
                Rect toggleRect = new Rect(rowRect.x + RowPadding, rowRect.y + (rowRect.height - ToggleSize) / 2f, ToggleSize, ToggleSize);
                Rect iconRect = new Rect(toggleRect.xMax + RowPadding, rowRect.y + (rowRect.height - IconSize) / 2f, IconSize, IconSize);
                Rect textRect = rowRect;
                textRect.xMin = iconRect.xMax + RowPadding;
                textRect.xMax = rowRect.xMax - RowPadding;

                bool inDropdown = ModSettings.IsInDropdown(parentDef, def);
                
                Texture2D toggleTex = inDropdown
                    ? TexButton.Minus
                    : TexButton.Plus;

                if (Widgets.ButtonImage(toggleRect, toggleTex))
                {
                    ModSettings.SetDropdownEntry(parentDef, def, !inDropdown);
                }

                if (def.Icon != null)
                {
                    Widgets.DrawTextureFitted(iconRect, def.Icon, 1f);
                }

                Color prev = GUI.color;
                TextAnchor prevAnchor = Text.Anchor;
                if (!enabled)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.55f);
                }

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(textRect, def.LabelCap);
                Text.Anchor = prevAnchor;
                GUI.color = prev;

                TooltipHandler.TipRegion(rowRect, def.description ?? string.Empty);
                curY += RowHeight;
            }

            Widgets.EndScrollView();
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

        private List<MainButtonDef> GetFilteredDefs()
        {
            if (!quickSearchWidget.filter.Active)
            {
                return cachedDefs;
            }

            List<MainButtonDef> filtered = new List<MainButtonDef>();
            for (int i = 0; i < cachedDefs.Count; i++)
            {
                MainButtonDef def = cachedDefs[i];
                if (MatchesFilter(def))
                {
                    filtered.Add(def);
                }
            }

            return filtered;
        }

        private void CacheSearchState()
        {
            bool anyMatch = false;
            if (!quickSearchWidget.filter.Active)
            {
                anyMatch = cachedDefs.Count > 0;
            }
            else
            {
                for (int i = 0; i < cachedDefs.Count; i++)
                {
                    if (MatchesFilter(cachedDefs[i]))
                    {
                        anyMatch = true;
                        break;
                    }
                }
            }

            quickSearchWidget.noResultsMatched = !anyMatch;
        }

        private bool MatchesFilter(MainButtonDef def)
        {
            return quickSearchWidget.filter.Matches(def.LabelCap.ToString())
                || quickSearchWidget.filter.Matches(def.defName);
        }
    }
}
