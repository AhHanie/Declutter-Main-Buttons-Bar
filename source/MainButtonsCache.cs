using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class MainButtonsCache
    {
        public static List<MainButtonDef> AllButtonsInOrder = new List<MainButtonDef>();
        public static List<MainButtonDef> AllButtonsInOrderNoDMMBButton = new List<MainButtonDef>();
        public static List<MainButtonDef> AllButtonsInOrderNoDMMBInspectButton = new List<MainButtonDef>();

        public static void Rebuild()
        {
            AllButtonsInOrder = DefDatabase<MainButtonDef>.AllDefs
                .OrderBy(def => def.order)
                .ToList();

            AllButtonsInOrderNoDMMBButton = new List<MainButtonDef>(AllButtonsInOrder);
            AllButtonsInOrderNoDMMBButton.Remove(MainButtonsMenuDefOf.DMMB_MainButtonsMenu);

            AllButtonsInOrderNoDMMBInspectButton = new List<MainButtonDef>(AllButtonsInOrderNoDMMBButton);
            AllButtonsInOrderNoDMMBInspectButton.Remove(MainButtonDefOf.Inspect);
        }
    }
}
