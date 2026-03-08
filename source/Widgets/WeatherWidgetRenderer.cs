using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class WeatherWidgetRenderer
    {
        public static float GetPreferredWidth()
        {
            string sample = GetWeatherText();
            return Mathf.Max(120f, WidgetRenderUtility.MeasureSmallTextWidth(sample) + 24f);
        }

        public static void Draw(Rect rect)
        {
            WidgetRenderUtility.DrawCenteredText(rect, GetWeatherText(), "DMMB.WidgetWeatherTooltip".Translate());
        }

        private static string GetWeatherText()
        {
            if (Find.CurrentMap == null)
            {
                return string.Empty;
            }

            string temperature = Mathf.Round(Find.CurrentMap.mapTemperature.OutdoorTemp).ToStringTemperature("F0");
            string weather = Find.CurrentMap.weatherManager.CurWeatherPerceived.LabelCap;
            return temperature + " | " + weather;
        }
    }
}
