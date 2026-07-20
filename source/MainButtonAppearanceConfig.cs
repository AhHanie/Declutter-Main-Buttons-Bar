using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public class MainButtonAppearanceConfig : IExposable
    {
        public const int MaxLabelLength = 40;
        public string customLabel;
        public string iconPath;
        public bool showIcon = true;
        // When true and an icon is shown, the bar/dropdown renderer hides the label entirely
        // instead of trying to fit both side by side. Only affects bar-style rendering -- the
        // menu, dropdown editor, and settings list always show the full label for identification.
        public bool preferIconOnly;

        public bool IsDefault => customLabel == null && iconPath == null && showIcon && !preferIconOnly;

        public void ExposeData()
        {
            Scribe_Values.Look(ref customLabel, "customLabel");
            Scribe_Values.Look(ref iconPath, "iconPath");
            Scribe_Values.Look(ref showIcon, "showIcon", true);
            Scribe_Values.Look(ref preferIconOnly, "preferIconOnly", false);
        }

        public static string NormalizeLabel(string label)
        {
            if (label == null)
            {
                return null;
            }

            string trimmed = label.Trim();
            if (trimmed.Length > MaxLabelLength)
            {
                trimmed = trimmed.Substring(0, MaxLabelLength);
            }

            return trimmed;
        }
    }
}
