using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Declutter_Main_Buttons_Bar
{
    [StaticConstructorOnStartup]
    public static class MainButtonAppearanceRenderer
    {
        private const float IconOnlySize = 32f;
        private const float IconWithLabelSize = 24f;
        private const float IconPadding = 6f;
        private const float IconTextGap = 4f;
        private static readonly Texture2D BarFillTex = SolidColorMaterials.NewSolidColorTexture(TexUI.FinishedResearchColorTransparent);

        public static bool Supports(MainButtonDef def)
        {
            if (def?.Worker == null)
            {
                return false;
            }

            Type workerType = def.Worker.GetType();
            return workerType == typeof(MainButtonWorker)
                || workerType == typeof(MainButtonWorker_ToggleTab)
                || workerType == typeof(MainButtonWorker_ToggleWorld)
                || workerType == typeof(MainButtonWorker_ToggleResearchTab)
                || workerType == typeof(MainButtonWorker_ToggleMechTab);
        }

        public static void DrawOrFallback(MainButtonDef def, Rect rect)
        {
            if (Supports(def) && ModSettings.GetAppearance(def) != null)
            {
                Draw(def, rect);
            }
            else
            {
                def.Worker.DoButton(rect);
            }
        }

        public static void Draw(MainButtonDef def, Rect rect)
        {
            string label = ModSettings.GetDisplayLabel(def) ?? string.Empty;
            Texture2D icon = ModSettings.GetDisplayIcon(def);
            bool forceIconOnly = icon != null && ModSettings.GetPreferIconOnly(def);
            DrawInternal(def, rect, label, icon, interactive: true, forceIconOnly);
        }

        // label is still used for the tooltip title even when forceIconOnly hides it on-button.
        public static void DrawPreview(MainButtonDef def, Rect rect, string label, Texture2D icon, bool forceIconOnly = false)
        {
            DrawInternal(def, rect, label ?? string.Empty, icon, interactive: false, forceIconOnly);
        }

        private static void DrawInternal(MainButtonDef def, Rect rect, string label, Texture2D icon, bool interactive, bool forceIconOnly = false)
        {
            GameFont prevFont = Text.Font;
            TextAnchor prevAnchor = Text.Anchor;
            bool prevWrap = Text.WordWrap;
            Color prevColor = GUI.color;

            try
            {
                Text.Font = GameFont.Small;

                MainButtonWorker worker = def.Worker;

                // The preview is about the customized label/icon, not the button's real
                // activation state -- a disabled-without-a-map or no-active-game button would
                // otherwise render as an empty box, defeating the point of previewing it. Only
                // the live bar (interactive) reads the worker's real Disabled/ButtonBarPercent;
                // some stock workers (e.g. Ideology's Ideos tab) dereference game-only state
                // there, which is only ever safe with a game actually running.
                bool disabled = interactive && worker.Disabled;

                if (disabled)
                {
                    Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
                    if (interactive && Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
                    {
                        Event.current.Use();
                    }

                    return;
                }

                bool hasIcon = icon != null;
                bool hovered = Mouse.IsOver(rect);

                float availableForLabel = hasIcon
                    ? rect.width - IconPadding - IconWithLabelSize - IconTextGap - 2f
                    : rect.width - 2f;

                string drawLabel = forceIconOnly ? string.Empty : label;
                float labelWidth = drawLabel.Length > 0 ? Text.CalcSize(drawLabel).x : 0f;
                if (labelWidth > availableForLabel)
                {
                    drawLabel = drawLabel.Shorten();
                    labelWidth = Text.CalcSize(drawLabel).x;
                    if (labelWidth > availableForLabel)
                    {
                        drawLabel = string.Empty;
                        labelWidth = 0f;
                    }
                }

                bool hasLabel = drawLabel.Length > 0;
                float textLeftMargin;
                if (hasIcon)
                {
                    textLeftMargin = hasLabel ? IconPadding + IconWithLabelSize + IconTextGap : -1f;
                }
                else
                {
                    textLeftMargin = labelWidth > 0.85f * rect.width - 1f ? 2f : -1f;
                }

                float barPercent = interactive ? worker.ButtonBarPercent : 0f;
                bool activated;
                if (interactive)
                {
                    MouseoverSounds.DoRegion(rect, SoundDefOf.Mouseover_Category);
                    DrawButtonVisual(rect, drawLabel, barPercent, textLeftMargin, hovered);
                    activated = Widgets.ButtonInvisible(rect, doMouseoverSound: false);
                }
                else
                {
                    DrawButtonVisual(rect, drawLabel, barPercent, textLeftMargin, hovered);
                    activated = false;
                }

                if (hasIcon)
                {
                    DrawIcon(rect, icon, hasLabel, hovered);
                }

                if (!interactive)
                {
                    return;
                }

                if (activated)
                {
                    worker.InterfaceTryActivate();
                }

                if (Find.MainTabsRoot.OpenTab != def && !Find.WindowStack.NonImmediateDialogWindowOpen)
                {
                    UIHighlighter.HighlightOpportunity(rect, def.cachedHighlightTagClosed);
                }

                if (Mouse.IsOver(rect) && !def.description.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(rect, label.Colorize(ColorLibrary.Yellow) + "\n\n" + def.description);
                }
            }
            finally
            {
                Text.Font = prevFont;
                Text.Anchor = prevAnchor;
                Text.WordWrap = prevWrap;
                GUI.color = prevColor;
            }
        }

        private static void DrawButtonVisual(Rect rect, string label, float barPercent, float textLeftMargin, bool hovered)
        {
            if (hovered)
            {
                GUI.color = GenUI.MouseoverColor;
            }

            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
            GUI.color = Color.white;

            if (barPercent > 0.001f)
            {
                Widgets.FillableBar(rect.ContractedBy(1f), barPercent, BarFillTex, null, doBorder: false);
            }

            Rect textRect = new Rect(rect);
            textRect.x += textLeftMargin < 0f ? rect.width * 0.15f : textLeftMargin;
            if (hovered)
            {
                textRect.x += 2f;
                textRect.y -= 2f;
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Widgets.Label(textRect, label);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
        }

        private static void DrawIcon(Rect rect, Texture2D icon, bool hasLabel, bool hovered)
        {
            float size = hasLabel ? IconWithLabelSize : IconOnlySize;
            size = Mathf.Min(size, rect.height - 4f);

            Vector2 position = hasLabel
                ? new Vector2(rect.x + IconPadding, rect.center.y - size / 2f)
                : new Vector2(rect.center.x - size / 2f, rect.center.y - size / 2f);

            if (hovered)
            {
                position += new Vector2(2f, -2f);
            }

            GUI.DrawTexture(new Rect(position.x, position.y, size, size), icon);
        }
    }
}
