using Declutter_Main_Buttons_Bar.Compat;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Declutter_Main_Buttons_Bar
{
    public class Mod: Verse.Mod
    {
        public static ModSettings Settings;

        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "DMMB.LoadingLabel", doAsynchronously: true, null);
        }

        public void Init()
        {
            MainButtonsCache.Rebuild();
            Settings = GetSettings<ModSettings>();
            new Harmony("sk.dmmb").PatchAll();
            bool settingsChanged = ModSettings.DetectAndHideNewButtonsFromBarIfNeeded();
            if (ModSettings.EnsureCustomOrderCoverage())
            {
                settingsChanged = true;
            }
            if (settingsChanged)
            {
                Settings.Write();
            }
            if (Compat_SmartSpeed.IsEnabled())
            {
                MainButtonsRoot_DoButtons_Patch.SetSmartSpeedMode();
            }
        }

        public override string SettingsCategory()
        {
            return "DMMB.SettingsTitle".Translate();
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            ModSettingsWindow.Draw(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }
}
