using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using Declutter_Main_Buttons_Bar.Compat;

namespace Declutter_Main_Buttons_Bar
{
    public static class ModSettingsWindow
    {
        private enum SettingsTab
        {
            MainBar,
            Buttons,
            Menus,
            HudAndWidgets,
            Gizmos,
            Advanced
        }

        private const float ForceShowListHeight = 220f;
        private const float ForceShowRowHeight = 28f;
        private const float ForceShowToggleSize = 24f;
        private const float ForceShowRowPadding = 6f;

        private const float AppearanceListHeight = 260f;
        private const float AppearanceRowHeight = 58f;
        private const float AppearanceIconSize = 24f;
        private const float AppearanceRowPadding = 6f;
        private const float AppearanceCustomizeButtonWidth = 100f;
        private const float AppearanceCustomizeButtonHeight = 24f;

        private const float PanelOuterPadding = 8f;
        private const float PanelContentPadding = 17f;

        private const float FooterTopGap = 8f;
        private const float FooterLineHeight = 2f;
        private const float FooterSpacing = 6f;
        private const float FooterButtonHeight = 32f;
        private const float FooterButtonWidth = 240f;

        private static SettingsTab selectedTab = SettingsTab.MainBar;
        private static List<TabRecord> tabRecords;

        private static Vector2 mainBarScrollPosition = Vector2.zero;
        private static Vector2 buttonsScrollPosition = Vector2.zero;
        private static Vector2 menusScrollPosition = Vector2.zero;
        private static Vector2 hudAndWidgetsScrollPosition = Vector2.zero;
        private static Vector2 gizmosScrollPosition = Vector2.zero;
        private static Vector2 advancedScrollPosition = Vector2.zero;
        private static Vector2 forceShowScrollPosition = Vector2.zero;
        private static Vector2 appearanceScrollPosition = Vector2.zero;

        private static float mainBarContentHeight = 600f;
        private static float buttonsContentHeight = 1500f;
        private static float menusContentHeight = 1200f;
        private static float hudAndWidgetsContentHeight = 600f;
        private static float gizmosContentHeight = 400f;
        private static float advancedContentHeight = 400f;

        public static void ResetToInitialTab()
        {
            selectedTab = SettingsTab.MainBar;
        }

        public static void Draw(Rect parent)
        {
            if (tabRecords == null)
            {
                tabRecords = BuildTabRecords();
            }

            Rect outRect = parent.ContractedBy(PanelOuterPadding);
            Rect panelRect = outRect;
            panelRect.yMin += TabDrawer.TabHeight;

            Widgets.DrawMenuSection(panelRect);
            TabDrawer.DrawTabs(panelRect, tabRecords);

            Rect contentRect = panelRect.ContractedBy(PanelContentPadding);

            float footerHeight = CalcFooterHeight(contentRect.width);
            Rect pageRect = contentRect;
            pageRect.yMax -= footerHeight;
            Rect footerRect = contentRect;
            footerRect.yMin = pageRect.yMax;

            DrawSelectedTab(pageRect);
            DrawGlobalResetFooter(footerRect);
        }

        private static List<TabRecord> BuildTabRecords()
        {
            return new List<TabRecord>
            {
                new TabRecord(
                    "DMMB.SettingsTabMainBar".Translate(),
                    () => selectedTab = SettingsTab.MainBar,
                    () => selectedTab == SettingsTab.MainBar),
                new TabRecord(
                    "DMMB.SettingsTabButtons".Translate(),
                    () => selectedTab = SettingsTab.Buttons,
                    () => selectedTab == SettingsTab.Buttons),
                new TabRecord(
                    "DMMB.SettingsTabMenus".Translate(),
                    () => selectedTab = SettingsTab.Menus,
                    () => selectedTab == SettingsTab.Menus),
                new TabRecord(
                    "DMMB.SettingsTabHudAndWidgets".Translate(),
                    () => selectedTab = SettingsTab.HudAndWidgets,
                    () => selectedTab == SettingsTab.HudAndWidgets),
                new TabRecord(
                    "DMMB.SettingsTabGizmos".Translate(),
                    () => selectedTab = SettingsTab.Gizmos,
                    () => selectedTab == SettingsTab.Gizmos),
                new TabRecord(
                    "DMMB.SettingsTabAdvanced".Translate(),
                    () => selectedTab = SettingsTab.Advanced,
                    () => selectedTab == SettingsTab.Advanced),
            };
        }

        private static void DrawSelectedTab(Rect rect)
        {
            switch (selectedTab)
            {
                case SettingsTab.MainBar:
                    DrawMainBarTab(rect);
                    break;
                case SettingsTab.Buttons:
                    DrawButtonsTab(rect);
                    break;
                case SettingsTab.Menus:
                    DrawMenusTab(rect);
                    break;
                case SettingsTab.HudAndWidgets:
                    DrawHudAndWidgetsTab(rect);
                    break;
                case SettingsTab.Gizmos:
                    DrawGizmosTab(rect);
                    break;
                case SettingsTab.Advanced:
                    DrawAdvancedTab(rect);
                    break;
            }
        }

        private static void DrawScrollableTab(Rect rect, ref Vector2 scrollPosition, ref float contentHeight, Action<Listing_Standard> drawContents)
        {
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, Mathf.Max(contentHeight, rect.height));
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.maxOneColumn = true;
            listing.Begin(viewRect);
            drawContents(listing);
            listing.End();

            Widgets.EndScrollView();
            contentHeight = listing.CurHeight;
        }

        private static void DrawMainBarTab(Rect rect)
        {
            DrawScrollableTab(rect, ref mainBarScrollPosition, ref mainBarContentHeight, listing =>
            {
                if (Compat_FernyModConfigs.IsEnabled())
                {
                    listing.CheckboxLabeled(
                        "DMMB.SettingsAutoLoadFernyModConfigs".Translate(),
                        ref ModSettings.autoLoadFernyModConfigs,
                        "DMMB.SettingsAutoLoadFernyModConfigsDesc".Translate());

                    if (!string.IsNullOrEmpty(ModSettings.lastFernyPresetName) && !string.IsNullOrEmpty(ModSettings.lastFernyPresetVersion))
                    {
                        listing.Label("DMMB.SettingsFernyPresetStatus".Translate(ModSettings.lastFernyPresetName, ModSettings.lastFernyPresetVersion));
                    }

                    listing.GapLine();
                }

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
            });
        }

        private static void DrawButtonsTab(Rect rect)
        {
            DrawScrollableTab(rect, ref buttonsScrollPosition, ref buttonsContentHeight, listing =>
            {
                listing.Label("DMMB.SettingsMainButtonVisibilityTitle".Translate());
                listing.Gap(4f);
                listing.Label("DMMB.SettingsDesc".Translate());
                listing.GapLine();

                for (int i = 0; i < MainButtonsCache.AllButtonsAlphabetical.Count; i++)
                {
                    MainButtonDef def = MainButtonsCache.AllButtonsAlphabetical[i];
                    bool showOnBar = !ModSettings.IsHiddenFromBar(def);
                    bool newValue = showOnBar;
                    listing.CheckboxLabeled(def.LabelCap, ref newValue, ModSettings.GetDisplayDescription(def));
                    if (newValue != showOnBar)
                    {
                        ModSettings.SetHiddenFromBar(def, !newValue);
                    }
                }

                listing.GapLine();
                listing.Gap(20f);
                listing.Label("DMMB.SettingsForceShowTitle".Translate());
                listing.Gap(4f);
                listing.Label("DMMB.SettingsForceShowDesc".Translate());
                listing.Gap(6f);
                DrawForceShowList(listing.GetRect(ForceShowListHeight));

                listing.GapLine();
                listing.Gap(20f);

                listing.Label("DMMB.SettingsAppearanceTitle".Translate());
                listing.Gap(4f);
                listing.Label("DMMB.SettingsAppearanceDesc".Translate());
                listing.Gap(6f);
                DrawAppearanceList(listing.GetRect(AppearanceListHeight));
            });
        }

        private static void DrawMenusTab(Rect rect)
        {
            DrawScrollableTab(rect, ref menusScrollPosition, ref menusContentHeight, listing =>
            {
                listing.Label("DMMB.SettingsMenuBehaviorTitle".Translate());
                listing.GapLine();

                listing.CheckboxLabeled(
                    "DMMB.SettingsPinMainButtonsMenuWindowRight".Translate(),
                    ref ModSettings.pinMainButtonsMenuWindowRight,
                    "DMMB.SettingsPinMainButtonsMenuWindowRightDesc".Translate());

                CheckboxLabeledWithNewBadge(
                    listing,
                    "DMMB.SettingsFocusMainButtonsMenuSearch".Translate(),
                    ref ModSettings.focusMainButtonsMenuSearch,
                    "DMMB.SettingsFocusMainButtonsMenuSearchDesc".Translate());

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
                    "DMMB.SettingsPlaySettingsHoverReserveSpaceToggle".Translate(),
                    ref ModSettings.reservePlaySettingsHoverSpace,
                    "DMMB.SettingsPlaySettingsHoverReserveSpaceToggleDesc".Translate());

                listing.CheckboxLabeled(
                    "DMMB.SettingsHidePlaySettingsEditModeButton".Translate(),
                    ref ModSettings.hideEditModePlaySettingsButton,
                    "DMMB.SettingsHidePlaySettingsEditModeButtonDesc".Translate());

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

                for (int i = 0; i < MainButtonsCache.AllButtonsAlphabeticalNoDMMBInspectButton.Count; i++)
                {
                    MainButtonDef def = MainButtonsCache.AllButtonsAlphabeticalNoDMMBInspectButton[i];
                    bool showInMenu = !ModSettings.IsBlacklistedFromMenu(def);
                    bool newValue = showInMenu;
                    listing.CheckboxLabeled(def.LabelCap, ref newValue, ModSettings.GetDisplayDescription(def));
                    if (newValue != showInMenu)
                    {
                        ModSettings.SetBlacklistedFromMenu(def, !newValue);
                    }
                }
            });
        }

        private static void DrawHudAndWidgetsTab(Rect rect)
        {
            DrawScrollableTab(rect, ref hudAndWidgetsScrollPosition, ref hudAndWidgetsContentHeight, listing =>
            {
                bool revealResourceReadoutOnHover = ModSettings.revealVanillaResourceReadoutOnHover;
                listing.CheckboxLabeled(
                    "DMMB.SettingsWidgetRevealVanillaResourceReadoutOnHover".Translate(),
                    ref revealResourceReadoutOnHover,
                    "DMMB.SettingsWidgetRevealVanillaResourceReadoutOnHoverDesc".Translate());
                if (revealResourceReadoutOnHover != ModSettings.revealVanillaResourceReadoutOnHover)
                {
                    ModSettings.SetVanillaResourceReadoutHover(revealResourceReadoutOnHover);
                }

                bool disableResourceReadout = ModSettings.disableVanillaResourceReadout;
                listing.CheckboxLabeled(
                    "DMMB.SettingsWidgetDisableVanillaResourceReadout".Translate(),
                    ref disableResourceReadout,
                    "DMMB.SettingsWidgetDisableVanillaResourceReadoutDesc".Translate());
                if (disableResourceReadout != ModSettings.disableVanillaResourceReadout)
                {
                    ModSettings.SetVanillaResourceReadoutDisabled(disableResourceReadout);
                }

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
            });
        }

        private static void DrawGizmosTab(Rect rect)
        {
            DrawScrollableTab(rect, ref gizmosScrollPosition, ref gizmosContentHeight, listing =>
            {
                listing.CheckboxLabeled(
                    "DMMB.SettingsHideAllGizmoLabels".Translate(),
                    ref ModSettings.hideAllGizmoLabels,
                    "DMMB.SettingsHideAllGizmoLabelsDesc".Translate());
                listing.CheckboxLabeled(
                    "DMMB.SettingsHideGizmoLabelsForSelectedColonistsOnly".Translate(),
                    ref ModSettings.hideGizmoLabelsForSelectedColonistsOnly,
                    "DMMB.SettingsHideGizmoLabelsForSelectedColonistsOnlyDesc".Translate());
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
            });
        }

        private static void DrawAdvancedTab(Rect rect)
        {
            DrawScrollableTab(rect, ref advancedScrollPosition, ref advancedContentHeight, listing =>
            {
                listing.Label("DMMB.SettingsAdvancedTitle".Translate());
                listing.Gap(4f);
                listing.Label("DMMB.SettingsAdvancedDesc".Translate());
                listing.Gap(6f);
                listing.CheckboxLabeled(
                    "DMMB.SettingsEnableCommandGizmoSizePatches".Translate(),
                    ref ModSettings.enableCommandGizmoSizePatches,
                    "DMMB.SettingsEnableCommandGizmoSizePatchesDesc".Translate());
                listing.CheckboxLabeled(
                    "DMMB.SettingsEnableGizmoLabelVisibilityPatches".Translate(),
                    ref ModSettings.enableGizmoLabelVisibilityPatches,
                    "DMMB.SettingsEnableGizmoLabelVisibilityPatchesDesc".Translate());
                listing.CheckboxLabeled(
                    "DMMB.SettingsEnableSelectorSelectionStatePatches".Translate(),
                    ref ModSettings.enableSelectorSelectionStatePatches,
                    "DMMB.SettingsEnableSelectorSelectionStatePatchesDesc".Translate());
                listing.CheckboxLabeled(
                    "DMMB.SettingsEnableVanillaWidgetPatches".Translate(),
                    ref ModSettings.enableVanillaWidgetPatches,
                    "DMMB.SettingsEnableVanillaWidgetPatchesDesc".Translate());
                listing.CheckboxLabeled(
                    "DMMB.SettingsEnableAtlasOptimizationPatch".Translate(),
                    ref ModSettings.enableAtlasOptimizationPatch,
                    "DMMB.SettingsEnableAtlasOptimizationPatchDesc".Translate());
            });
        }

        private static float CalcFooterHeight(float width)
        {
            float warningHeight = Text.CalcHeight("DMMB.SettingsResetFooterWarning".Translate(), width);
            return FooterTopGap + FooterLineHeight + FooterSpacing + warningHeight + FooterSpacing + FooterButtonHeight;
        }

        private static void DrawGlobalResetFooter(Rect rect)
        {
            float lineY = rect.y + FooterTopGap;
            Widgets.DrawLineHorizontal(rect.x, lineY, rect.width);

            TaggedString warning = "DMMB.SettingsResetFooterWarning".Translate();
            float warningHeight = Text.CalcHeight(warning, rect.width);
            Rect warningRect = new Rect(rect.x, lineY + FooterLineHeight + FooterSpacing, rect.width, warningHeight);
            Widgets.Label(warningRect, warning);

            Rect buttonRect = new Rect(
                rect.x,
                warningRect.yMax + FooterSpacing,
                Mathf.Min(FooterButtonWidth, rect.width),
                FooterButtonHeight);
            if (Widgets.ButtonText(buttonRect, "DMMB.SettingsResetAllButton".Translate()))
            {
                ConfirmResetAllSettings();
            }
        }

        private static void ConfirmResetAllSettings()
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "DMMB.SettingsResetConfirmation".Translate(),
                ModSettings.ResetToDefaults,
                destructive: true));
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

        private static void DrawAppearanceList(Rect rect)
        {
            List<MainButtonDef> defs = MainButtonsCache.AllButtonsAlphabetical;
            float contentHeight = defs.Count * AppearanceRowHeight;
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, Mathf.Max(contentHeight, rect.height));
            Widgets.BeginScrollView(rect, ref appearanceScrollPosition, viewRect);

            Text.Font = GameFont.Small;
            float smallLineHeight = Text.LineHeight;
            Text.Font = GameFont.Tiny;
            float tinyLineHeight = Text.LineHeight;
            Text.Font = GameFont.Small;

            TextAnchor prevAnchor = Text.Anchor;
            float curY = 0f;
            for (int i = 0; i < defs.Count; i++)
            {
                MainButtonDef def = defs[i];
                Rect rowRect = new Rect(0f, curY, viewRect.width, AppearanceRowHeight);
                Widgets.DrawHighlightIfMouseover(rowRect);

                Rect contentRect = rowRect.ContractedBy(AppearanceRowPadding);
                Rect buttonRect = new Rect(
                    contentRect.xMax - AppearanceCustomizeButtonWidth,
                    contentRect.y + (contentRect.height - AppearanceCustomizeButtonHeight) / 2f,
                    AppearanceCustomizeButtonWidth,
                    AppearanceCustomizeButtonHeight);
                Rect iconRect = new Rect(
                    contentRect.x,
                    contentRect.y + (contentRect.height - AppearanceIconSize) / 2f,
                    AppearanceIconSize,
                    AppearanceIconSize);

                Texture2D icon = ModSettings.GetDisplayIcon(def);
                if (icon != null)
                {
                    Widgets.DrawTextureFitted(iconRect, icon, 1f);
                }

                Rect labelRect = contentRect;
                labelRect.xMin = iconRect.xMax + AppearanceRowPadding;
                labelRect.xMax = buttonRect.xMin - AppearanceRowPadding;

                string effectiveLabel = ModSettings.GetDisplayLabel(def);
                MainButtonAppearanceConfig config = ModSettings.GetAppearance(def);
                bool renamed = config != null && config.customLabel != null;

                Text.Anchor = TextAnchor.MiddleLeft;
                if (renamed)
                {
                    float stackHeight = smallLineHeight + tinyLineHeight;
                    float stackY = labelRect.y + Mathf.Max(0f, (labelRect.height - stackHeight) / 2f);
                    Rect nameRect = new Rect(labelRect.x, stackY, labelRect.width, smallLineHeight);
                    Rect hintRect = new Rect(labelRect.x, nameRect.yMax, labelRect.width, tinyLineHeight);

                    Widgets.Label(nameRect, effectiveLabel);

                    Text.Font = GameFont.Tiny;
                    Color prevColor = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, 0.6f);
                    Widgets.Label(hintRect, "DMMB.AppearanceOriginalNameHint".Translate(def.LabelCap));
                    GUI.color = prevColor;
                    Text.Font = GameFont.Small;
                }
                else
                {
                    Widgets.Label(labelRect, effectiveLabel);
                }

                Text.Anchor = prevAnchor;

                if (Widgets.ButtonText(buttonRect, "DMMB.AppearanceCustomizeButton".Translate()))
                {
                    Find.WindowStack.Add(new MainButtonAppearanceEditorWindow(def));
                }

                TooltipHandler.TipRegion(rowRect, MainButtonDisplayUtility.BuildTooltip(effectiveLabel, ModSettings.GetDisplayDescription(def)));
                curY += AppearanceRowHeight;
            }

            Text.Anchor = prevAnchor;
            Widgets.EndScrollView();
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

                TooltipHandler.TipRegion(rowRect, ModSettings.GetDisplayDescription(def));
                curY += ForceShowRowHeight;
            }

            Text.Anchor = prevAnchor;
            Widgets.EndScrollView();
        }

        private static void CheckboxLabeledWithNewBadge(Listing_Standard listing, string label, ref bool checkOn, string tooltip = null)
        {
            const float badgeHeight = 32f;
            Rect row = listing.GetRect(Mathf.Max(Text.LineHeight, badgeHeight + 2f));
            Rect checkboxRect = row;
            checkboxRect.xMax -= 44f;
            Widgets.DrawHighlightIfMouseover(checkboxRect);
            Widgets.CheckboxLabeled(checkboxRect, label, ref checkOn);
            if (!string.IsNullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(checkboxRect, tooltip);
            }
            DrawNewBadge(row, badgeHeight);
        }

        private static void DrawNewBadge(Rect row, float badgeHeight)
        {
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
