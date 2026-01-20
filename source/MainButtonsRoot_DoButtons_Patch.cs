using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch(typeof(MainButtonsRoot), "DoButtons")]
    public static partial class MainButtonsRoot_DoButtons_Patch
    {
        public static bool Prefix(MainButtonsRoot __instance)
        {
            List<MainButtonDef> allButtons = MainButtonsCache.AllButtonsInOrder;
            DrawButtons(allButtons);
            return false;
        }
    }
}
