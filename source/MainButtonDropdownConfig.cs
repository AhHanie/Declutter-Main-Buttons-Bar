using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public class MainButtonDropdownConfig : IExposable
    {
        public MainButtonDef parent;
        public List<MainButtonDef> entries = new List<MainButtonDef>();

        public void ExposeData()
        {
            Scribe_Defs.Look(ref parent, "parent");
            Scribe_Collections.Look(ref entries, "entries", LookMode.Def);
            if (entries == null)
            {
                entries = new List<MainButtonDef>();
            }
        }
    }
}
