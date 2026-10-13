using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class BatteryWidgetRenderer
    {
        private const float CacheIntervalSeconds = 60f;
        private const float InnerPadding = 4f;
        private const float IconGap = 5f;
        private const float IconSize = 26f;
        private const float IconOffsetX = 2f;
        private const float MinPreferredWidth = 96f;

        private static float cachedBatteryLevel = -1f;
        private static BatteryStatus cachedBatteryStatus = BatteryStatus.Unknown;
        private static float nextRefreshRealtime = -1f;

        public static float GetPreferredWidth()
        {
            GameFont oldFont = Text.Font;
            Text.Font = GameFont.Small;
            float textWidth = Text.CalcSize("100%").x;
            Text.Font = oldFont;
            float width = InnerPadding * 2f + IconSize + IconGap + textWidth;
            return Mathf.Max(MinPreferredWidth, width);
        }

        public static void Draw(Rect rect)
        {
            WidgetRenderUtility.DrawBackground(rect);
            RefreshCacheIfNeeded();

            bool hasBattery = HasBattery();
            bool charging = hasBattery && cachedBatteryStatus == BatteryStatus.Charging;
            DrawCore(rect, hasBattery, charging, Mathf.Clamp01(cachedBatteryLevel), GetPercentText(hasBattery));
        }

        public static void DrawDebug(Rect rect, float debugLevel01 = 0.59f, bool debugCharging = true, bool debugHasBattery = true)
        {
            WidgetRenderUtility.DrawBackground(rect);

            float level = Mathf.Clamp01(debugLevel01);
            string percent = debugHasBattery ? Mathf.RoundToInt(level * 100f) + "%" : "--";
            DrawCore(rect, debugHasBattery, debugCharging, level, percent);
        }

        private static void DrawCore(Rect rect, bool hasBattery, bool charging, float level01, string percentText)
        {
            Texture2D shell = GetShellTexture(hasBattery, charging);
            Rect inner = rect.ContractedBy(InnerPadding);
            Rect iconRect = new Rect(inner.x + IconOffsetX, inner.center.y - IconSize * 0.5f, IconSize, IconSize);

            if (hasBattery)
            {
                DrawChargeFill(iconRect, level01);
            }

            GUI.DrawTexture(iconRect, shell ?? BaseContent.BadTex);

            Rect textRect = new Rect(iconRect.xMax + IconGap, inner.y, inner.xMax - (iconRect.xMax + IconGap), inner.height);
            if (textRect.width > 1f)
            {
                TextAnchor oldAnchor = Text.Anchor;
                bool oldWordWrap = Text.WordWrap;
                GameFont oldFont = Text.Font;
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.WordWrap = false;
                Text.Font = GameFont.Small;
                Widgets.Label(textRect, percentText);
                Text.Anchor = oldAnchor;
                Text.WordWrap = oldWordWrap;
                Text.Font = oldFont;
            }

            TooltipHandler.TipRegion(rect, BuildTooltip(hasBattery, charging));
        }

        private static void RefreshCacheIfNeeded()
        {
            float now = Time.realtimeSinceStartup;
            if (nextRefreshRealtime >= 0f && now < nextRefreshRealtime)
            {
                return;
            }

            cachedBatteryLevel = SystemInfo.batteryLevel;
            cachedBatteryStatus = SystemInfo.batteryStatus;
            nextRefreshRealtime = now + CacheIntervalSeconds;
        }

        private static bool HasBattery()
        {
            return cachedBatteryLevel >= 0f && cachedBatteryStatus != BatteryStatus.Unknown;
        }

        private static Texture2D GetShellTexture(bool hasBattery, bool charging)
        {
            if (!hasBattery)
            {
                return DMMBTextures.BatteryShellNotFound.Texture;
            }

            if (charging)
            {
                return DMMBTextures.BatteryShellCharging.Texture;
            }

            return DMMBTextures.BatteryShell.Texture;
        }

        private static void DrawChargeFill(Rect iconRect, float level)
        {
            // Battery shell texture is horizontal; keep fill tightly inside the body.
            const float leftInset = 5f;
            const float rightInset = 11f;
            const float topInset = 8f;
            const float bottomInset = 8f;
            Rect chargeArea = new Rect(
                iconRect.x + leftInset,
                iconRect.y + topInset,
                iconRect.width - leftInset - rightInset,
                iconRect.height - topInset - bottomInset);
            if (chargeArea.width <= 1f || chargeArea.height <= 1f)
            {
                return;
            }

            float fillWidth = chargeArea.width * Mathf.Clamp01(level);
            if (level > 0f && fillWidth < 1f)
            {
                fillWidth = 1f;
            }

            Rect fillRect = new Rect(chargeArea.x, chargeArea.y, fillWidth, chargeArea.height);
            Widgets.DrawBoxSolid(fillRect, new Color(0.2f, 0.85f, 0.2f, 0.95f));
        }

        private static string GetPercentText(bool hasBattery)
        {
            if (!hasBattery)
            {
                return "--";
            }

            int pct = Mathf.RoundToInt(Mathf.Clamp01(cachedBatteryLevel) * 100f);
            return pct + "%";
        }

        private static string BuildTooltip(bool hasBattery, bool charging)
        {
            if (!hasBattery)
            {
                return "DMMB.WidgetBatteryTooltip".Translate() + "\n" + "DMMB.WidgetBatteryStateNoBattery".Translate();
            }

            string state;
            if (charging)
            {
                state = "DMMB.WidgetBatteryStateCharging".Translate();
            }
            else if (cachedBatteryStatus == BatteryStatus.Full)
            {
                state = "DMMB.WidgetBatteryStateFull".Translate();
            }
            else if (cachedBatteryStatus == BatteryStatus.Discharging)
            {
                state = "DMMB.WidgetBatteryStateDischarging".Translate();
            }
            else
            {
                state = "DMMB.WidgetBatteryStateNotCharging".Translate();
            }

            return "DMMB.WidgetBatteryTooltip".Translate() + "\n" + GetPercentText(true) + " - " + state;
        }
    }
}
