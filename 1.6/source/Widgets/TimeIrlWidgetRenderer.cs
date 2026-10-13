using System;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class TimeIrlWidgetRenderer
    {
        private const float PreferredWidth = 96f;
        private const float IconSize = 32f;
        private const float IconGap = 4f;

        public static float GetPreferredWidth()
        {
            return PreferredWidth;
        }

        public static void Draw(Rect rect)
        {
            WidgetRenderUtility.DrawBackground(rect);
            Rect inner = rect.ContractedBy(6f);

            float iconSize = Mathf.Min(IconSize, inner.height);
            Rect iconRect = new Rect(inner.x, inner.center.y - iconSize * 0.5f, iconSize, iconSize);
            Texture2D icon = DMMBTextures.Clock.Texture;
            Widgets.DrawTextureFitted(iconRect, icon ?? BaseContent.BadTex, 1f);

            Rect textRect = inner;
            textRect.xMin = iconRect.xMax + IconGap;
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;
            GameFont oldFont = Text.Font;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Widgets.Label(textRect, GetTimeIrlText());
            Text.Anchor = oldAnchor;
            Text.WordWrap = oldWordWrap;
            Text.Font = oldFont;
            TooltipHandler.TipRegion(rect, "DMMB.WidgetTimeIrlTooltip".Translate());
        }

        private static string GetTimeIrlText()
        {
            if (Prefs.TwelveHourClockMode)
            {
                return DateTime.Now.ToString("hh:mm tt");
            }

            return DateTime.Now.ToString("HH:mm");
        }
    }
}
