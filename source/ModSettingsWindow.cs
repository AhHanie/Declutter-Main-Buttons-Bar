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
        private const float ForceShowListHeight = 220f;
        private const float ForceShowRowHeight = 28f;
        private const float ForceShowToggleSize = 24f;
        private const float ForceShowRowPadding = 6f;

        private static Vector2 scrollPosition = Vector2.zero;
        private static Vector2 forceShowScrollPosition = Vector2.zero;

        public static void Draw(Rect parent)
        {
            Rect outRect = parent.ContractedBy(8f);
            float viewHeight = 1240f
                + (MainButtonsCache.AllButtonsInOrder.Count * 28f)
                + (MainButtonsCache.AllButtonsInOrderNoDMMBInspectButton.Count * 28f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            bool oldAdvancedEditMode = ModSettings.useAdvancedEditMode;

            listing.CheckboxLabeled(
                "DMMB.AdvancedEditMode".Translate(),
                ref ModSettings.useAdvancedEditMode,
                "DMMB.AdvancedEditModeDesc".Translate());
            if (ModSettings.useAdvancedEditMode)
            {
                ModSettings.useFixedWidthMode = false;
            }

            if (!oldAdvancedEditMode && ModSettings.useAdvancedEditMode)
            {
                MainButtonsRoot_DoButtons_Patch.ReconcileFreeSizeAfterChange();
            }

            if (ModSettings.useAdvancedEditMode)
            {
                listing.Label("DMMB.SettingsSnapThresholdLabel".Translate(Mathf.RoundToInt(ModSettings.snapThreshold)));
                ModSettings.snapThreshold = listing.Slider(ModSettings.snapThreshold, 0f, 30f);
            }

            listing.CheckboxLabeled("DMMB.SettingsFixedWidthToggle".Translate(), ref ModSettings.useFixedWidthMode);
            if (ModSettings.useFixedWidthMode)
            {
                ModSettings.useAdvancedEditMode = false;
            }

            if (ModSettings.useFixedWidthMode)
            {
                listing.Label("DMMB.SettingsFixedWidthLabel".Translate(Mathf.RoundToInt(ModSettings.fixedButtonWidth)));
                ModSettings.fixedButtonWidth = listing.Slider(ModSettings.fixedButtonWidth, 50f, 200f);
                listing.CheckboxLabeled("DMMB.SettingsFixedWidthCenter".Translate(), ref ModSettings.centerFixedWidthButtons);
            }

            listing.CheckboxLabeled("DMMB.SettingsPinMenuRight".Translate(), ref ModSettings.pinMenuButtonRight);
            listing.CheckboxLabeled(
                "DMMB.SettingsPinMainButtonsMenuWindowRight".Translate(),
                ref ModSettings.pinMainButtonsMenuWindowRight,
                "DMMB.SettingsPinMainButtonsMenuWindowRightDesc".Translate());
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
                "DMMB.SettingsHidePlaySettingsEditModeButton".Translate(),
                ref ModSettings.hideEditModePlaySettingsButton,
                "DMMB.SettingsHidePlaySettingsEditModeButtonDesc".Translate());

            listing.CheckboxLabeled(
                "DMMB.SettingsDefaultNewButtonsHidden".Translate(),
                ref ModSettings.defaultNewButtonsToHidden,
                "DMMB.SettingsDefaultNewButtonsHiddenDesc".Translate());
            bool oldExperimentalAtlasOptimization = ModSettings.experimentalMainButtonsAtlasOptimization;
            listing.CheckboxLabeled(
                "DMMB.SettingsExperimentalAtlasOptimization".Translate(),
                ref ModSettings.experimentalMainButtonsAtlasOptimization,
                "DMMB.SettingsExperimentalAtlasOptimizationDesc".Translate());
            if (oldExperimentalAtlasOptimization != ModSettings.experimentalMainButtonsAtlasOptimization)
            {
                MainButtonsAtlasTextureCache.ClearCache();
            }
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetDisableVanillaResourceReadout".Translate(),
                ref ModSettings.disableVanillaResourceReadout,
                "DMMB.SettingsWidgetDisableVanillaResourceReadoutDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetDisableVanillaMouseoverReadout".Translate(),
                ref ModSettings.disableVanillaMouseoverReadout,
                "DMMB.SettingsWidgetDisableVanillaMouseoverReadoutDesc".Translate());
            listing.Gap(6f);
            listing.Label("DMMB.SettingsWidgetsTitle".Translate());
            listing.GapLine();
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetTime".Translate(),
                ref ModSettings.showTimeWidget,
                "DMMB.SettingsWidgetTimeDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetTimeIrl".Translate(),
                ref ModSettings.showTimeIrlWidget,
                "DMMB.SettingsWidgetTimeIrlDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetTimeSpeed".Translate(),
                ref ModSettings.showTimeSpeedWidget,
                "DMMB.SettingsWidgetTimeSpeedDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetWeather".Translate(),
                ref ModSettings.showWeatherWidget,
                "DMMB.SettingsWidgetWeatherDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetFpsTps".Translate(),
                ref ModSettings.showFpsTpsWidget,
                "DMMB.SettingsWidgetFpsTpsDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetBattery".Translate(),
                ref ModSettings.showBatteryWidget,
                "DMMB.SettingsWidgetBatteryDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetDisableVanillaDateReadout".Translate(),
                ref ModSettings.disableVanillaDateReadout,
                "DMMB.SettingsWidgetDisableVanillaDateReadoutDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetDisableVanillaTimeControls".Translate(),
                ref ModSettings.disableVanillaTimeControls,
                "DMMB.SettingsWidgetDisableVanillaTimeControlsDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetDisableVanillaWeatherWidget".Translate(),
                ref ModSettings.disableVanillaWeatherWidget,
                "DMMB.SettingsWidgetDisableVanillaWeatherWidgetDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetDisableVanillaConditionsWidget".Translate(),
                ref ModSettings.disableVanillaConditionsWidget,
                "DMMB.SettingsWidgetDisableVanillaConditionsWidgetDesc".Translate());
            listing.CheckboxLabeled(
                "DMMB.SettingsWidgetDisableVanillaTemperatureWidget".Translate(),
                ref ModSettings.disableVanillaTemperatureWidget,
                "DMMB.SettingsWidgetDisableVanillaTemperatureWidgetDesc".Translate());
            listing.GapLine();

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

            listing.GapLine();
            listing.Gap(20f);
            DrawLabelWithNewBadge(listing, "DMMB.SettingsForceShowTitle".Translate());
            listing.Gap(4f);
            listing.Label("DMMB.SettingsForceShowDesc".Translate());
            listing.Gap(6f);
            DrawForceShowList(listing.GetRect(ForceShowListHeight));

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

        private static void DrawForceShowList(Rect rect)
        {
            Rect outRect = rect;
            float contentHeight = MainButtonsCache.AllButtonsInOrder.Count * ForceShowRowHeight;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(contentHeight, outRect.height));
            Widgets.BeginScrollView(outRect, ref forceShowScrollPosition, viewRect);

            TextAnchor prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;

            float curY = 0f;
            for (int i = 0; i < MainButtonsCache.AllButtonsInOrder.Count; i++)
            {
                MainButtonDef def = MainButtonsCache.AllButtonsInOrder[i];
                Rect rowRect = new Rect(0f, curY, viewRect.width, ForceShowRowHeight);
                Widgets.DrawHighlightIfMouseover(rowRect);

                Rect contentRect = rowRect.ContractedBy(ForceShowRowPadding);
                Rect toggleRect = new Rect(
                    contentRect.xMax - ForceShowToggleSize,
                    contentRect.y + (contentRect.height - ForceShowToggleSize) * 0.5f,
                    ForceShowToggleSize,
                    ForceShowToggleSize);
                Rect labelRect = contentRect;
                labelRect.xMax = toggleRect.xMin - ForceShowRowPadding;

                Widgets.Label(labelRect, def.LabelCap);

                bool forceShown = ModSettings.IsForceShown(def);
                bool newValue = forceShown;
                Widgets.Checkbox(toggleRect.x, toggleRect.y, ref newValue, ForceShowToggleSize, paintable: false);

                bool rowClicked = Widgets.ButtonInvisible(new Rect(rowRect.x, rowRect.y, toggleRect.xMin - rowRect.x, rowRect.height));
                if (!rowClicked && rowRect.xMax > toggleRect.xMax)
                {
                    rowClicked = Widgets.ButtonInvisible(new Rect(toggleRect.xMax, rowRect.y, rowRect.xMax - toggleRect.xMax, rowRect.height));
                }

                if (rowClicked)
                {
                    newValue = !newValue;
                }

                if (newValue != forceShown)
                {
                    ModSettings.SetForceShown(def, newValue);
                }

                TooltipHandler.TipRegion(rowRect, def.description ?? string.Empty);
                curY += ForceShowRowHeight;
            }

            Text.Anchor = prevAnchor;
            Widgets.EndScrollView();
        }

        private static void DrawLabelWithNewBadge(Listing_Standard listing, string label)
        {
            const float badgeHeight = 32f;
            Rect row = listing.GetRect(Mathf.Max(Text.LineHeight, badgeHeight + 2f));
            Rect labelRect = row;
            labelRect.xMax -= 44f;
            TextAnchor prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = prevAnchor;

            Texture2D newIcon = DMMBTextures.New.Texture;

            float iconHeight = badgeHeight;
            float iconWidth = iconHeight * ((float)newIcon.width / Mathf.Max(1f, newIcon.height));
            Rect iconRect = new Rect(
                row.xMax - iconWidth - 2f,
                row.y + (row.height - iconHeight) * 0.5f,
                iconWidth,
                iconHeight);
            Widgets.DrawTextureFitted(iconRect, newIcon, 1f);
        }
    }
}
