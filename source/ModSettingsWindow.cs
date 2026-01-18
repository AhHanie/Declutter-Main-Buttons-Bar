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
            float viewHeight = 256f
                + (MainButtonsCache.AllButtonsInOrderNoDMMBButton.Count * 28f)
                + (MainButtonsCache.AllButtonsInOrderNoDMMBInspectButton.Count * 28f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            listing.CheckboxLabeled("DMMB.SettingsFixedWidthToggle".Translate(), ref ModSettings.useFixedWidthMode);

            listing.Label("DMMB.SettingsFixedWidthLabel".Translate(Mathf.RoundToInt(ModSettings.fixedButtonWidth)));
            ModSettings.fixedButtonWidth = listing.Slider(ModSettings.fixedButtonWidth, 50f, 200f);

            listing.CheckboxLabeled("DMMB.SettingsPinMenuRight".Translate(), ref ModSettings.pinMenuButtonRight);
            listing.CheckboxLabeled("DMMB.SettingsGizmoBottom".Translate(), ref ModSettings.drawGizmosAtBottom);
            listing.Label("DMMB.SettingsGizmoOffsetLabel".Translate(Mathf.RoundToInt(ModSettings.gizmoBottomOffset)));
            ModSettings.gizmoBottomOffset = listing.Slider(ModSettings.gizmoBottomOffset, 0f, 120f);

            Rect resetRow = listing.GetRect(Text.LineHeight);
            Rect resetLabelRect = resetRow;
            resetLabelRect.width *= 0.6f;
            Rect resetButtonRect = resetRow;
            resetButtonRect.xMin = resetLabelRect.xMax;
            resetButtonRect.width = Mathf.Min(140f, resetButtonRect.width);
            resetButtonRect.x = resetRow.xMax - resetButtonRect.width;

            Widgets.Label(resetLabelRect, "DMMB.SettingsResetLabel".Translate());
            if (Widgets.ButtonText(resetButtonRect, "DMMB.SettingsReset".Translate()))
            {
                ModSettings.hiddenFromBarDefs.Clear();
                ModSettings.favoriteDefs.Clear();
                ModSettings.blacklistedFromMenuDefs.Clear();
                ModSettings.useFixedWidthMode = false;
                ModSettings.fixedButtonWidth = 120f;
                ModSettings.pinMenuButtonRight = false;
                ModSettings.drawGizmosAtBottom = false;
                ModSettings.gizmoBottomOffset = 35f;
            }

            listing.GapLine();

            listing.Gap(20f);
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

            listing.GapLine();
            listing.Gap(20f);
            listing.Label("DMMB.SettingsMenuBlacklistDesc".Translate());
            listing.GapLine();

            for (int i = 0; i < MainButtonsCache.AllButtonsInOrderNoDMMBInspectButton.Count; i++)
            {
                MainButtonDef def = MainButtonsCache.AllButtonsInOrderNoDMMBInspectButton[i];
                bool showInMenu = !ModSettings.IsBlacklistedFromMenu(def);
                bool newValue = showInMenu;
                listing.CheckboxLabeled(def.LabelCap, ref newValue, def.description);
                if (newValue != showInMenu)
                {
                    ModSettings.SetBlacklistedFromMenu(def, !newValue);
                }
            }

            listing.End();
            Widgets.EndScrollView();
        }
    }
}
