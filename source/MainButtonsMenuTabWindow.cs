using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public class MainTabWindow_MainButtonsMenu : MainTabWindow
    {
        private const float RowHeight = 64f;
        private const float SearchHeight = 26f;
        private const float IconSize = 28f;
        private const float RowPadding = 6f;
        private const float StarSize = 22f;
        private static readonly Color PanelBg = new Color(0.11f, 0.11f, 0.11f, 1f);
        private static readonly Color RowLine = new Color(1f, 1f, 1f, 0.08f);
        private static readonly CachedTexture StarOutline = new CachedTexture("DMMB/UI/GoldStarOutline");
        private static readonly CachedTexture StarFilled = new CachedTexture("DMMB/UI/GoldStarFilled");

        private readonly QuickSearchWidget quickSearchWidget = new QuickSearchWidget();
        private Vector2 scrollPosition = Vector2.zero;
        private List<MainButtonDef> cachedDefs;
        private List<MainButtonDef> cachedMenuDefs;
        private bool triedToFocus;
        private int openFrames;

        public override Vector2 RequestedTabSize
        {
            get
            {
                float height = Mathf.Min(480f, UI.screenHeight - 35f);
                return new Vector2(380f, height);
            }
        }

        protected override float Margin => 6f;

        public MainTabWindow_MainButtonsMenu()
        {
            closeOnAccept = false;
            cachedDefs = MainButtonsCache.AllButtonsInOrderNoDMMBInspectButton;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            RebuildMenuCache();
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

            Rect topBarRect = new Rect(0f, 0f, inRect.width, SearchHeight + 4f);
            Widgets.DrawBoxSolid(topBarRect, PanelBg);

            Rect searchRect = new Rect(6f, 4f, inRect.width - 12f, SearchHeight);
            Rect listRect = new Rect(0f, topBarRect.height, inRect.width, inRect.height - topBarRect.height);
            Widgets.DrawBoxSolid(listRect, PanelBg);

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
                bool enabled = !def.Worker.Disabled;

                Widgets.DrawHighlightIfMouseover(rowRect);
                Color prev = GUI.color;
                GUI.color = RowLine;
                Widgets.DrawLineHorizontal(rowRect.x + RowPadding, rowRect.yMax - 1f, rowRect.width - RowPadding * 2f);
                GUI.color = prev;

                Rect contentRect = rowRect.ContractedBy(RowPadding);
                Rect starRect = new Rect(contentRect.xMax - StarSize, contentRect.y + (contentRect.height - StarSize) / 2f, StarSize, StarSize);
                Rect iconRect = new Rect(contentRect.x, contentRect.y + (contentRect.height - IconSize) / 2f, IconSize, IconSize);
                Rect textRect = contentRect;
                textRect.xMax = starRect.xMin - RowPadding;

                if (def.Icon != null)
                {
                    Widgets.DrawTextureFitted(iconRect, def.Icon, 1f);
                    textRect.xMin = iconRect.xMax + RowPadding;
                }

                Color prevColor = GUI.color;
                if (!enabled)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                }

                Text.Font = GameFont.Small;
                float titleHeight = Text.LineHeight;
                Text.Font = GameFont.Tiny;
                float descLineHeight = Text.LineHeight;
                float descHeight = descLineHeight * 2f;
                float blockHeight = titleHeight + descHeight;
                float blockY = textRect.y + Mathf.Max(0f, (textRect.height - blockHeight) / 2f);
                Rect titleRect = new Rect(textRect.x, blockY, textRect.width, titleHeight);
                Rect descRect = new Rect(textRect.x, blockY + titleHeight, textRect.width, descHeight);

                Text.Font = GameFont.Small;
                Widgets.Label(titleRect, def.LabelCap);
                Text.Font = GameFont.Tiny;
                GUI.color = enabled ? new Color(1f, 1f, 1f, 0.7f) : new Color(1f, 1f, 1f, 0.35f);
                Widgets.Label(descRect, def.description ?? string.Empty);
                GUI.color = prevColor;
                Text.Font = GameFont.Small;

                bool favorite = ModSettings.IsFavorite(def);
                Texture2D starTex = favorite ? StarFilled.Texture : StarOutline.Texture;
                bool starClicked = Widgets.ButtonImage(starRect, starTex);
                if (starClicked)
                {
                    ModSettings.SetFavorite(def, !favorite);
                    Mod.Settings.Write();
                }

                if (enabled && !starClicked && Widgets.ButtonInvisible(rowRect))
                {
                    def.Worker.InterfaceTryActivate();
                }

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
            List<MainButtonDef> source;
            if (!quickSearchWidget.filter.Active)
            {
                source = cachedMenuDefs;
                return source
                    .OrderByDescending(def => ModSettings.IsFavorite(def))
                    .ThenBy(def => def.order)
                    .ToList();
            }

            List<MainButtonDef> filtered = new List<MainButtonDef>();
            for (int i = 0; i < cachedMenuDefs.Count; i++)
            {
                MainButtonDef def = cachedMenuDefs[i];
                if (quickSearchWidget.filter.Matches(def.LabelCap.ToString()))
                {
                    filtered.Add(def);
                }
            }

            return filtered
                .OrderByDescending(def => ModSettings.IsFavorite(def))
                .ThenBy(def => def.order)
                .ToList();
        }

        private void CacheSearchState()
        {
            bool anyMatch = false;
            if (!quickSearchWidget.filter.Active)
            {
                anyMatch = true;
            }
            else
            {
                for (int i = 0; i < cachedMenuDefs.Count; i++)
                {
                    if (quickSearchWidget.filter.Matches(cachedMenuDefs[i].LabelCap.ToString()))
                    {
                        anyMatch = true;
                        break;
                    }
                }
            }

            quickSearchWidget.noResultsMatched = !anyMatch;
        }

        private void RebuildMenuCache()
        {
            cachedDefs = MainButtonsCache.AllButtonsInOrderNoDMMBInspectButton;
            cachedMenuDefs = cachedDefs
                .Where(def => !ModSettings.IsBlacklistedFromMenu(def))
                .ToList();
        }
    }
}
