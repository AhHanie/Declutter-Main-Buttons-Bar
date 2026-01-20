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
        private const float IconSize = 22f;
        private const float ToggleSize = 18f;
        private const float RowPadding = 6f;
        private readonly MainButtonDef parentDef;
        private Vector2 scrollPosition = Vector2.zero;
        private List<MainButtonDef> cachedDefs = new List<MainButtonDef>();

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
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect titleRect = new Rect(0f, 0f, inRect.width, Text.LineHeight);
            Widgets.Label(titleRect, "DMMB.DropdownEditorTitle".Translate(parentDef?.LabelCap ?? string.Empty));

            Text.Font = GameFont.Tiny;
            Rect descRect = new Rect(0f, titleRect.yMax + 4f, inRect.width, Text.LineHeight * 2f);
            Widgets.Label(descRect, "DMMB.DropdownEditorDesc".Translate());
            Text.Font = GameFont.Small;

            Rect listRect = new Rect(0f, descRect.yMax + 6f, inRect.width, inRect.height - descRect.yMax - 6f);
            float viewHeight = cachedDefs.Count * RowHeight;
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, Mathf.Max(viewHeight, listRect.height));

            Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);

            float curY = 0f;
            for (int i = 0; i < cachedDefs.Count; i++)
            {
                MainButtonDef def = cachedDefs[i];
                Rect rowRect = new Rect(0f, curY, viewRect.width, RowHeight);
                Widgets.DrawHighlightIfMouseover(rowRect);

                bool enabled = !def.Worker.Disabled;
                Rect toggleRect = new Rect(rowRect.x + RowPadding, rowRect.y + (rowRect.height - ToggleSize) / 2f, ToggleSize, ToggleSize);
                Rect iconRect = new Rect(toggleRect.xMax + RowPadding, rowRect.y + (rowRect.height - IconSize) / 2f, IconSize, IconSize);
                Rect textRect = rowRect;
                textRect.xMin = iconRect.xMax + RowPadding;
                textRect.xMax = rowRect.xMax - RowPadding;

                bool inDropdown = ModSettings.IsInDropdown(parentDef, def);
                bool canToggle = !ModSettings.IsHiddenFromBar(def) || inDropdown;
                
                Texture2D toggleTex = inDropdown
                    ? TexButton.Minus
                    : TexButton.Plus;

                if (canToggle && Widgets.ButtonImage(toggleRect, toggleTex))
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
    }
}
