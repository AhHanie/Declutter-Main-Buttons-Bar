using RimWorld;

namespace Declutter_Main_Buttons_Bar
{
    [DefOf]
    public static class MainButtonsMenuDefOf
    {
        public static MainButtonDef DMMB_MainButtonsMenu;
        public static MainButtonDef DMMB_PlaySettingsMenu;

        static MainButtonsMenuDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MainButtonsMenuDefOf));
        }
    }
}
