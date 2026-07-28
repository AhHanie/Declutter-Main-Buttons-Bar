using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace Declutter_Main_Buttons_Bar
{
    // MainButtonWorker.Disabled is virtual, and mods can override it. Patch the getter used by
    // every loaded main-button worker so force-show consistently makes the selected button usable.
    [HarmonyPatch]
    public static class MainButtonWorker_Disabled_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            HashSet<MethodBase> methods = new HashSet<MethodBase>();
            MethodBase baseGetter = AccessTools.PropertyGetter(typeof(MainButtonWorker), nameof(MainButtonWorker.Disabled));
            if (baseGetter != null)
            {
                methods.Add(baseGetter);
            }

            List<MainButtonDef> defs = MainButtonsCache.AllButtonsInOrder;
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                if (def == null)
                {
                    continue;
                }

                MethodBase getter = AccessTools.PropertyGetter(def.Worker.GetType(), nameof(MainButtonWorker.Disabled));
                if (getter != null)
                {
                    methods.Add(getter);
                }
            }

            return methods;
        }

        public static bool Prefix(MainButtonWorker __instance, ref bool __result)
        {
            if (__instance != null && ModSettings.IsForceShown(__instance.def))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
