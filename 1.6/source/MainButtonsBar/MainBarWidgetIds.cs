using System.Collections.Generic;

namespace Declutter_Main_Buttons_Bar
{
    public static class MainBarWidgetIds
    {
        public const string Time = "Time";
        public const string TimeIrl = "TimeIrl";
        public const string TimeSpeed = "TimeSpeed";
        public const string Weather = "Weather";
        public const string FpsTps = "FpsTps";
        public const string Battery = "Battery";

        public static readonly List<string> All = new List<string>
        {
            Time,
            TimeIrl,
            TimeSpeed,
            Weather,
            FpsTps,
            Battery
        };

        public static bool IsKnown(string widgetId)
        {
            return widgetId == Time
                || widgetId == TimeIrl
                || widgetId == TimeSpeed
                || widgetId == Weather
                || widgetId == FpsTps
                || widgetId == Battery;
        }
    }
}
