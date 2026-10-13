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
            MainButtonsDrawAtlasOptimizationScope.Begin();
            try
            {
                List<MainButtonDef> allButtons = MainButtonsCache.AllButtonsInOrder;
                DrawButtons(allButtons);
            }
            finally
            {
                // A throwing draw must not leave the atlas scope active for unrelated widgets drawn later this frame.
                MainButtonsDrawAtlasOptimizationScope.End();
            }

            return false;
        }
    }
}
