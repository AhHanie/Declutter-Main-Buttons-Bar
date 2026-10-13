using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    internal static class WidgetRenderUtility
    {
        public static void DrawBackground(Rect rect)
        {
            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
        }

        public static void DrawCenteredText(Rect rect, string text, string tooltip)
        {
            DrawBackground(rect);
            Rect inner = rect.ContractedBy(6f);
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;
            GameFont oldFont = Text.Font;

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Widgets.Label(inner, text);

            Text.Anchor = oldAnchor;
            Text.WordWrap = oldWordWrap;
            Text.Font = oldFont;

            if (!string.IsNullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
        }

        public static float MeasureSmallTextWidth(string text)
        {
            GameFont oldFont = Text.Font;
            Text.Font = GameFont.Small;
            float width = Text.CalcSize(text).x;
            Text.Font = oldFont;
            return width;
        }

        public static string TruncateSmallText(string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (MeasureSmallTextWidth(text) <= maxWidth)
            {
                return text;
            }

            const string ellipsis = "...";
            if (MeasureSmallTextWidth(ellipsis) >= maxWidth)
            {
                return ellipsis;
            }

            int low = 0;
            int high = text.Length;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                string candidate = text.Substring(0, mid).TrimEnd() + ellipsis;
                if (MeasureSmallTextWidth(candidate) <= maxWidth)
                {
                    low = mid;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return text.Substring(0, low).TrimEnd() + ellipsis;
        }
    }
}
