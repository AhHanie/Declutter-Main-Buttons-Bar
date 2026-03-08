using System;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using RimWorld.Planet;

namespace Declutter_Main_Buttons_Bar
{
    public static class TimeWidgetRenderer
    {
        private const float InnerPadding = 6f;
        private const float MinPreferredWidth = 160f;
        private const float PreferredWidthCacheIntervalSeconds = 2f;

        private static float cachedPreferredWidth = 220f;
        private static float nextPreferredWidthRefreshRealtime = -1f;

        public static float GetPreferredWidth()
        {
            float now = Time.realtimeSinceStartup;
            if (nextPreferredWidthRefreshRealtime >= 0f && now < nextPreferredWidthRefreshRealtime)
            {
                return cachedPreferredWidth;
            }

            cachedPreferredWidth = ComputePreferredWidth();
            nextPreferredWidthRefreshRealtime = now + PreferredWidthCacheIntervalSeconds;
            return cachedPreferredWidth;
        }

        private static float ComputePreferredWidth()
        {
            if (!TryGetLongLat(out Vector2 longLat))
            {
                return 220f;
            }

            int ticksAbs = Find.TickManager.TicksAbs;
            string dateText = GenDate.DateReadoutStringAt(ticksAbs, longLat);
            string timeText = GetTimeText(ticksAbs, longLat.x);

            GameFont oldFont = Text.Font;
            Text.Font = GameFont.Small;
            float width = InnerPadding * 2f + Text.CalcSize(dateText).x + 10f + Text.CalcSize(timeText).x;
            Text.Font = oldFont;

            return Mathf.Max(MinPreferredWidth, width);
        }

        public static void Draw(Rect rect)
        {
            WidgetRenderUtility.DrawBackground(rect);

            if (!TryGetLongLat(out Vector2 longLat))
            {
                return;
            }

            int ticksAbs = Find.TickManager.TicksAbs;
            string dateText = GenDate.DateReadoutStringAt(ticksAbs, longLat);
            string timeText = GetTimeText(ticksAbs, longLat.x);
            Season season = GenDate.Season(ticksAbs, longLat);

            Rect inner = rect.ContractedBy(InnerPadding);
            Rect textRect = inner;
            if (textRect.width <= 8f)
            {
                return;
            }

            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;

            Text.Font = GameFont.Small;
            Text.WordWrap = false;

            float timeWidth = Text.CalcSize(timeText).x + 6f;
            timeWidth = Mathf.Min(timeWidth, Mathf.Max(20f, textRect.width * 0.45f));

            Rect timeRect = new Rect(textRect.xMax - timeWidth, textRect.y, timeWidth, textRect.height);
            Rect dateRect = new Rect(textRect.x, textRect.y, textRect.width - timeWidth, textRect.height);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(dateRect, dateText);
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(timeRect, timeText);

            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
            Text.WordWrap = oldWordWrap;

            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, BuildDateTooltip(longLat, season));
            }
        }

        private static bool TryGetLongLat(out Vector2 longLat)
        {
            if (WorldRendererUtility.WorldSelected && Find.WorldSelector.SelectedTile.Valid)
            {
                longLat = Find.WorldGrid.LongLatOf(Find.WorldSelector.SelectedTile);
                return true;
            }

            if (WorldRendererUtility.WorldSelected && Find.WorldSelector.NumSelectedObjects > 0)
            {
                longLat = Find.WorldGrid.LongLatOf(Find.WorldSelector.FirstSelectedObject.Tile);
                return true;
            }

            if (Find.CurrentMap != null)
            {
                longLat = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
                return true;
            }

            longLat = default;
            return false;
        }

        private static string GetTimeText(int ticksAbs, float longitude)
        {
            float hourFloat = GenDate.HourFloat(ticksAbs, longitude);
            int hour = Mathf.FloorToInt(hourFloat);
            int minute = Mathf.FloorToInt((hourFloat - hour) * 60f);

            if (Prefs.TwelveHourClockMode)
            {
                string suffix = hour >= 12 ? "PM" : "AM";
                int hour12 = hour % 12;
                if (hour12 == 0)
                {
                    hour12 = 12;
                }

                return hour12.ToString("00") + ":" + minute.ToString("00") + " " + suffix;
            }

            return hour.ToString("00") + ":" + minute.ToString("00");
        }

        private static TipSignal BuildDateTooltip(Vector2 longLat, Season season)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                Quadrum quadrum = (Quadrum)i;
                stringBuilder.AppendLine(quadrum.Label() + " - " + quadrum.GetSeason(longLat.y).LabelCap());
            }

            TaggedString text = "DateReadoutTip".Translate(
                GenDate.DaysPassed,
                15,
                season.LabelCap(),
                15,
                GenDate.Quadrum(GenTicks.TicksAbs, longLat.x).Label(),
                stringBuilder.ToString());

            return new TipSignal(text, 86423);
        }
    }
}
