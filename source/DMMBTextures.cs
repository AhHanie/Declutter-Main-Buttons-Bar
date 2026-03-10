using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class DMMBTextures
    {
        private const string WeatherTextureRoot = "DMMB/UI/Weather/";

        private static readonly Dictionary<string, Texture2D> WeatherIcons = new Dictionary<string, Texture2D>();

        public static readonly CachedTexture UiToggle = new CachedTexture("DMMB/UI/UI");
        public static readonly CachedTexture PlusGold = new CachedTexture("DMMB/UI/PlusGold");
        public static readonly CachedTexture PlusGray = new CachedTexture("DMMB/UI/PlusGray");
        public static readonly CachedTexture StarOutline = new CachedTexture("DMMB/UI/GoldStarOutline");
        public static readonly CachedTexture StarFilled = new CachedTexture("DMMB/UI/GoldStarFilled");
        public static readonly CachedTexture BatteryShell = new CachedTexture("DMMB/UI/BatteryShell");
        public static readonly CachedTexture BatteryShellCharging = new CachedTexture("DMMB/UI/BatteryShellCharging");
        public static readonly CachedTexture BatteryShellNotFound = new CachedTexture("DMMB/UI/BatteryShellNotFound");
        public static readonly CachedTexture New = new CachedTexture("DMMB/UI/New");
        public static readonly CachedTexture Clock = new CachedTexture("DMMB/UI/Clock");
        public static readonly CachedTexture SeasonSpring = new CachedTexture("DMMB/UI/Seasons/Spring");
        public static readonly CachedTexture SeasonSummer = new CachedTexture("DMMB/UI/Seasons/Summer");
        public static readonly CachedTexture SeasonFall = new CachedTexture("DMMB/UI/Seasons/Fall");
        public static readonly CachedTexture SeasonWinter = new CachedTexture("DMMB/UI/Seasons/Winter");
        public static readonly CachedTexture SeasonPermanentSummer = new CachedTexture("DMMB/UI/Seasons/PermnantSummer");
        public static readonly CachedTexture SeasonPermanentWinter = new CachedTexture("DMMB/UI/Seasons/PermnantWinter");
        public static readonly CachedTexture WeatherUnknown = new CachedTexture("DMMB/UI/Weather/UnknownWeather");

        public static Texture2D GetSeasonIcon(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    return SeasonSpring.Texture;
                case Season.Summer:
                    return SeasonSummer.Texture;
                case Season.Fall:
                    return SeasonFall.Texture;
                case Season.Winter:
                    return SeasonWinter.Texture;
                case Season.PermanentSummer:
                    return SeasonPermanentSummer.Texture;
                case Season.PermanentWinter:
                    return SeasonPermanentWinter.Texture;
                default:
                    return WeatherUnknown.Texture;
            }
        }

        public static Texture2D GetWeatherIcon(WeatherDef weatherDef)
        {
            if (WeatherIcons.TryGetValue(weatherDef.defName, out Texture2D cached))
            {
                return cached;
            }

            Texture2D texture = ContentFinder<Texture2D>.Get(WeatherTextureRoot + weatherDef.defName, false)
                ?? WeatherUnknown.Texture;
            WeatherIcons[weatherDef.defName] = texture;
            return texture;
        }
    }
}
