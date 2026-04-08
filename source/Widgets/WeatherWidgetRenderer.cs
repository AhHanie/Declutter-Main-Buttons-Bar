using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class WeatherWidgetRenderer
    {
        private const float InnerPadding = 6f;
        private const float IconGap = 4f;
        private const float IconSize = 20f;
        private const float MinPreferredWidth = 132f;

        private static readonly Dictionary<string, string> CachedDisplayTextByWeatherText = new Dictionary<string, string>();
        private static int cachedDisplayWidth = -1;

        public static float GetPreferredWidth()
        {
            string sample = GetWeatherText();
            float width = InnerPadding * 2f + IconSize + IconGap + WidgetRenderUtility.MeasureSmallTextWidth(sample);
            return Mathf.Max(MinPreferredWidth, width);
        }

        public static void Draw(Rect rect)
        {
            WidgetRenderUtility.DrawBackground(rect);
            if (Find.CurrentMap == null)
            {
                return;
            }

            WeatherDef weatherDef = Find.CurrentMap.weatherManager.CurWeatherPerceived;
            Rect inner = rect.ContractedBy(InnerPadding);
            float iconSize = Mathf.Min(IconSize, inner.height);
            Rect iconRect = new Rect(inner.x, inner.center.y - iconSize * 0.5f, iconSize, iconSize);
            Widgets.DrawTextureFitted(iconRect, DMMBTextures.GetWeatherIcon(weatherDef), 1f);

            Rect textRect = inner;
            textRect.xMin = iconRect.xMax + IconGap;
            if (textRect.width > 2f)
            {
                string weatherText = GetWeatherText();
                string displayText = GetDisplayText(weatherText, textRect.width);
                bool textFits = displayText == weatherText;
                TextAnchor oldAnchor = Text.Anchor;
                bool oldWordWrap = Text.WordWrap;
                GameFont oldFont = Text.Font;
                Text.Anchor = textFits ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
                Text.WordWrap = false;
                Text.Font = GameFont.Small;
                Widgets.Label(textRect, displayText);
                Text.Anchor = oldAnchor;
                Text.WordWrap = oldWordWrap;
                Text.Font = oldFont;
            }

            TooltipHandler.TipRegion(rect, BuildTooltip(weatherDef));
        }

        private static string GetWeatherText()
        {
            string temperature = Mathf.Round(Find.CurrentMap.mapTemperature.OutdoorTemp).ToStringTemperature("F0");
            string weather = Find.CurrentMap.weatherManager.CurWeatherPerceived.LabelCap;
            return temperature + " | " + weather;
        }

        private static string GetDisplayText(string weatherText, float maxWidth)
        {
            int widthKey = Mathf.FloorToInt(maxWidth);
            if (cachedDisplayWidth != widthKey)
            {
                cachedDisplayWidth = widthKey;
                CachedDisplayTextByWeatherText.Clear();
            }

            if (CachedDisplayTextByWeatherText.TryGetValue(weatherText, out string cachedDisplayText))
            {
                return cachedDisplayText;
            }

            string displayText = WidgetRenderUtility.MeasureSmallTextWidth(weatherText) <= widthKey
                ? weatherText
                : WidgetRenderUtility.TruncateSmallText(weatherText, widthKey);

            CachedDisplayTextByWeatherText[weatherText] = displayText;
            return displayText;
        }

        private static string BuildTooltip(WeatherDef weatherDef)
        {
            string tooltip = "DMMB.WidgetWeatherTooltip".Translate();

            if (weatherDef.description.NullOrEmpty())
            {
                return tooltip + "\n" + weatherDef.LabelCap;
            }

            return tooltip + "\n" + weatherDef.LabelCap + "\n" + weatherDef.description;
        }
    }
}
