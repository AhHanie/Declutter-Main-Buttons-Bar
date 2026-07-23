using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public class MainButtonAppearanceConfig : IExposable
    {
        public const int MaxLabelLength = 40;
        public const int MaxDescriptionLength = 240;
        public string customLabel;
        public string customDescription;
        public string iconPath;
        public bool showIcon = true;
        // When true and an icon is shown, the bar/dropdown renderer hides the label entirely
        // instead of trying to fit both side by side. Only affects bar-style rendering -- the
        // menu, dropdown editor, and settings list always show the full label for identification.
        public bool preferIconOnly;

        public bool IsDefault => customLabel == null && customDescription == null && iconPath == null && showIcon && !preferIconOnly;

        public void ExposeData()
        {
            Scribe_Values.Look(ref customLabel, "customLabel");
            Scribe_Values.Look(ref customDescription, "customDescription");
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

        public static string NormalizeDescription(string description)
        {
            if (description == null)
            {
                return null;
            }

            string trimmed = description.Trim();
            if (trimmed.Length > MaxDescriptionLength)
            {
                trimmed = trimmed.Substring(0, MaxDescriptionLength);
            }

            return trimmed;
        }
    }
}
