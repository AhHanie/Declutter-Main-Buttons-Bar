using System;
using HarmonyLib;
using RimWorld;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch(typeof(Dialog_ModSettings), MethodType.Constructor, new Type[] { typeof(Verse.Mod) })]
    public static class Dialog_ModSettings_Constructor_Patch
    {
        public static void Postfix(Verse.Mod mod)
        {
            if (mod is Mod)
            {
                ModSettingsWindow.ResetToInitialTab();
            }
        }
    }
}
