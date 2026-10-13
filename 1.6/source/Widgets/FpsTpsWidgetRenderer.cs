using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class FpsTpsWidgetRenderer
    {
        private const float InnerPadding = 6f;
        private const float MinPreferredWidth = 120f;
        private const float TpsSampleInterval = 1f;

        private static int lastTpsFrame = -1;
        private static int sampleStartTickCount;
        private static float sampleStartTime;
        private static float displayedTps;
        private static bool tpsInitialized;

        public static float GetPreferredWidth()
        {
            string sample = BuildText(149f, 9000f);
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
            float tps = GetCurrentTps();

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

        private static float GetCurrentTps()
        {
            if (Find.TickManager == null)
            {
                ResetTpsTracking();
                return 0f;
            }

            int frame = Time.frameCount;
            if (lastTpsFrame == frame)
            {
                return displayedTps;
            }

            lastTpsFrame = frame;

            int currentTicks = Find.TickManager.TicksGame;
            float now = Time.realtimeSinceStartup;
            if (!tpsInitialized || currentTicks < sampleStartTickCount || now <= sampleStartTime)
            {
                sampleStartTickCount = currentTicks;
                sampleStartTime = now;
                displayedTps = 0f;
                tpsInitialized = true;
                return displayedTps;
            }

            float elapsed = now - sampleStartTime;
            if (elapsed >= TpsSampleInterval)
            {
                int ticksElapsed = currentTicks - sampleStartTickCount;
                displayedTps = ticksElapsed / elapsed;
                sampleStartTickCount = currentTicks;
                sampleStartTime = now;
            }

            return displayedTps;
        }

        private static void ResetTpsTracking()
        {
            lastTpsFrame = -1;
            sampleStartTickCount = 0;
            sampleStartTime = 0f;
            displayedTps = 0f;
            tpsInitialized = false;
        }
    }
}
