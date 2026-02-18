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
            LongEventHandler.QueueLongEvent(Init, "Declutter Main Buttons Bar Init", doAsynchronously: true, null);
        }

        public void Init()
        {
            Settings = GetSettings<ModSettings>();
            new Harmony("sk.dmmb").PatchAll();
            MainButtonsCache.Rebuild();
            if (ModSettings.DetectAndHideNewButtonsFromBarIfNeeded())
            {
                Settings.Write();
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
