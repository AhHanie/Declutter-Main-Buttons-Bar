using RimWorld;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class MainButtonDisplayUtility
    {
        public static bool MatchesFilter(QuickSearchFilter filter, MainButtonDef def)
        {
            return filter.Matches(ModSettings.GetDisplayLabel(def)) || filter.Matches(def.defName);
        }

        public static string BuildTooltip(string effectiveLabel, string description)
        {
            if (description.NullOrEmpty())
            {
                return effectiveLabel;
            }

            return effectiveLabel + "\n\n" + description;
        }
    }
}
