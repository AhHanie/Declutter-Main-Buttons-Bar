using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Declutter_Main_Buttons_Bar
{
    public static class ModSettingsWindow
    {
        private static Vector2 scrollPosition = Vector2.zero;

        public static void Draw(Rect parent)
        {
            Rect outRect = parent.ContractedBy(8f);
            float viewHeight = 140f + MainButtonsCache.AllButtonsInOrderNoDMMBButton.Count * 28f;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            listing.CheckboxLabeled("DMMB.SettingsFixedWidthToggle".Translate(), ref ModSettings.useFixedWidthMode);

            listing.Label("DMMB.SettingsFixedWidthLabel".Translate(Mathf.RoundToInt(ModSettings.fixedButtonWidth)));
            ModSettings.fixedButtonWidth = listing.Slider(ModSettings.fixedButtonWidth, 50f, 200f);
            listing.GapLine();

            if (listing.ButtonText("DMMB.SettingsReset".Translate()))
            {
                ModSettings.hiddenFromBarDefs.Clear();
                ModSettings.favoriteDefs.Clear();
                ModSettings.useFixedWidthMode = false;
                ModSettings.fixedButtonWidth = 120f;
            }

            listing.GapLine();

            listing.Label("DMMB.SettingsDesc".Translate());
            listing.GapLine();

            for (int i = 0; i < MainButtonsCache.AllButtonsInOrderNoDMMBButton.Count; i++)
            {
                MainButtonDef def = MainButtonsCache.AllButtonsInOrderNoDMMBButton[i];
                bool showOnBar = !ModSettings.IsHiddenFromBar(def);
                bool newValue = showOnBar;
                listing.CheckboxLabeled(def.LabelCap, ref newValue, def.description);
                if (newValue != showOnBar)
                {
                    ModSettings.SetHiddenFromBar(def, !newValue);
                }
            }

            listing.End();
            Widgets.EndScrollView();
        }
    }
}
