using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class FpsTpsWidgetRenderer
    {
        private const float InnerPadding = 6f;
        private const float MinPreferredWidth = 120f;

        public static float GetPreferredWidth()
        {
            string sample = BuildText(149f, 60f);
            GameFont oldFont = Text.Font;
            Text.Font = GameFont.Small;
            float width = Text.CalcSize(sample).x + InnerPadding * 2f;
            Text.Font = oldFont;
            return Mathf.Max(MinPreferredWidth, width);
        }

        public static void Draw(Rect rect)
        {
            WidgetRenderUtility.DrawBackground(rect);

            float averageFrameTime = Root.AverageFrameTime;
            float fps = 1000f / averageFrameTime;

            float meanTickTime = 0f;
            float tps = 0f;
            if (Find.TickManager != null)
            {
                meanTickTime = Find.TickManager.MeanTickTime;
                float uncappedTps = 1000f / meanTickTime;
                float maxTps = 60f * Find.TickManager.TickRateMultiplier;
                tps = Mathf.Min(uncappedTps, maxTps);
            }

            string text = BuildText(fps, tps);

            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;
            GameFont oldFont = Text.Font;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Widgets.Label(rect.ContractedBy(InnerPadding), text);
            Text.Anchor = oldAnchor;
            Text.WordWrap = oldWordWrap;
            Text.Font = oldFont;

            TooltipHandler.TipRegion(rect, "DMMB.WidgetFpsTpsTooltip".Translate());
        }

        private static string BuildText(float fps, float tps)
        {
            return $"FPS: {fps:F1} | TPS: {tps:F1}";
        }
    }
}
