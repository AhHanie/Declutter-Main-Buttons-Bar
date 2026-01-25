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
            float viewHeight = 320f
                + (MainButtonsCache.AllButtonsInOrderNoDMMBButton.Count * 28f)
                + (MainButtonsCache.AllButtonsInOrderNoDMMBInspectButton.Count * 28f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            listing.CheckboxLabeled("DMMB.SettingsFreeSizeToggle".Translate(), ref ModSettings.useFreeSizeMode);
            if (ModSettings.useFreeSizeMode)
            {
                ModSettings.useFixedWidthMode = false;
            }

            if (ModSettings.useFreeSizeMode)
            {
                listing.Label("DMMB.SettingsSnapThresholdLabel".Translate(Mathf.RoundToInt(ModSettings.snapThreshold)));
                ModSettings.snapThreshold = listing.Slider(ModSettings.snapThreshold, 0f, 30f);
            }

            listing.CheckboxLabeled("DMMB.SettingsFixedWidthToggle".Translate(), ref ModSettings.useFixedWidthMode);
            if (ModSettings.useFixedWidthMode)
            {
                ModSettings.useFreeSizeMode = false;
            }

            if (ModSettings.useFixedWidthMode)
            {
                listing.Label("DMMB.SettingsFixedWidthLabel".Translate(Mathf.RoundToInt(ModSettings.fixedButtonWidth)));
                ModSettings.fixedButtonWidth = listing.Slider(ModSettings.fixedButtonWidth, 50f, 200f);
                listing.CheckboxLabeled("DMMB.SettingsFixedWidthCenter".Translate(), ref ModSettings.centerFixedWidthButtons);
            }

            listing.CheckboxLabeled("DMMB.SettingsPinMenuRight".Translate(), ref ModSettings.pinMenuButtonRight);
            bool useMenu = ModSettings.useSearchablePlaySettingsMenu;
            listing.CheckboxLabeled("DMMB.SettingsPlaySettingsMenuToggle".Translate(), ref useMenu);
            if (useMenu != ModSettings.useSearchablePlaySettingsMenu)
            {
                ModSettings.useSearchablePlaySettingsMenu = useMenu;
                if (useMenu)
                {
                    ModSettings.revealPlaySettingsOnHover = false;
                }
            }

            bool hoverReveal = ModSettings.revealPlaySettingsOnHover;
            listing.CheckboxLabeled("DMMB.SettingsPlaySettingsHoverToggle".Translate(), ref hoverReveal, "DMMB.SettingsPlaySettingsHoverToggleDesc".Translate());
            if (hoverReveal != ModSettings.revealPlaySettingsOnHover)
            {
                ModSettings.revealPlaySettingsOnHover = hoverReveal;
                if (hoverReveal)
                {
                    ModSettings.useSearchablePlaySettingsMenu = false;
                }
            }
            listing.CheckboxLabeled(
                "DMMB.SettingsGizmoScaleMapOnly".Translate(),
                ref ModSettings.gizmoScaleMapOnly,
                "DMMB.SettingsGizmoScaleMapOnlyDesc".Translate());
            listing.Label("DMMB.SettingsGizmoScaleLabel".Translate(Mathf.RoundToInt(ModSettings.gizmoDrawerScale * 100f)));
            ModSettings.gizmoDrawerScale = listing.Slider(ModSettings.gizmoDrawerScale, 0.5f, 1.5f);
            listing.Label("DMMB.SettingsGizmoSpacingXLabel".Translate(Mathf.RoundToInt(ModSettings.gizmoSpacingX)));
            ModSettings.gizmoSpacingX = listing.Slider(ModSettings.gizmoSpacingX, 0f, 20f);
            listing.Label("DMMB.SettingsGizmoSpacingYLabel".Translate(Mathf.RoundToInt(ModSettings.gizmoSpacingY)));
            ModSettings.gizmoSpacingY = listing.Slider(ModSettings.gizmoSpacingY, -10f, 30f);

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
                ModSettings.ResetToDefaults();
            }

            listing.GapLine();

            listing.Gap(20f);
            listing.Label("DMMB.SettingsDesc".Translate());
            listing.GapLine();

            for (int i = 0; i < MainButtonsCache.AllButtonsInOrder.Count; i++)
            {
                MainButtonDef def = MainButtonsCache.AllButtonsInOrder[i];
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

            Rect syncRow = listing.GetRect(Text.LineHeight + 2f);
            if (Widgets.ButtonText(syncRow, "DMMB.SettingsMenuBlacklistUseHidden".Translate()))
            {
                PopulateMenuBlacklistFromHidden();
                Mod.Settings.Write();
            }

            listing.Gap(6f);

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

        private static void PopulateMenuBlacklistFromHidden()
        {
            List<MainButtonDef> defs = MainButtonsCache.AllButtonsInOrderNoDMMBInspectButton;
            List<MainButtonDef> newBlacklist = new List<MainButtonDef>(defs.Count);
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                if (def == null)
                {
                    continue;
                }

                if (!ModSettings.IsHiddenFromBar(def))
                {
                    newBlacklist.Add(def);
                }
            }

            ModSettings.blacklistedFromMenuDefs = newBlacklist;
        }
    }
}
