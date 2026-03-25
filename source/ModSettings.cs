using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public class ModSettings : Verse.ModSettings
    {
        public static List<MainButtonDef> hiddenFromBarDefs = new List<MainButtonDef>();
        public static HashSet<MainButtonDef> forceShowDefs = new HashSet<MainButtonDef>();
        public static List<MainButtonDef> favoriteDefs = new List<MainButtonDef>();
        public static List<MainButtonDef> blacklistedFromMenuDefs = new List<MainButtonDef>();
        public static List<MainButtonDropdownConfig> dropdownConfigs = new List<MainButtonDropdownConfig>();
        public static List<MainButtonDef> customOrderDefs = new List<MainButtonDef>();
        public static Dictionary<MainButtonDef, float> freeSizeWidths = new Dictionary<MainButtonDef, float>();
        public static Dictionary<MainButtonDef, float> freeSizeXPositions = new Dictionary<MainButtonDef, float>();
        public static float snapThreshold = 8f;
        public static bool editDropdownsMode = false;
        public static bool useAdvancedEditMode = false;
        public static bool useFixedWidthMode = false;
        public static float fixedButtonWidth = 120f;
        public static bool centerFixedWidthButtons = false;
        public static bool pinMenuButtonRight = false;
        public static bool pinMainButtonsMenuWindowRight = false;
        public static bool useSearchablePlaySettingsMenu = true;
        public static bool revealPlaySettingsOnHover = false;
        public static bool hideEditModePlaySettingsButton = false;
        public static bool defaultNewButtonsToHidden = false;
        public static bool showTimeWidget = false;
        public static bool showTimeIrlWidget = false;
        public static bool showTimeSpeedWidget = false;
        public static bool showWeatherWidget = false;
        public static bool showFpsTpsWidget = false;
        public static bool showBatteryWidget = false;
        public static bool disableVanillaDateReadout = false;
        public static bool disableVanillaTimeControls = false;
        public static bool disableVanillaWeatherWidget = false;
        public static bool disableVanillaConditionsWidget = false;
        public static bool revealVanillaResourceReadoutOnHover = false;
        public static bool disableVanillaResourceReadout = false;
        public static bool disableVanillaMouseoverReadout = false;
        public static bool disableVanillaTemperatureWidget = false;
        public static bool experimentalMainButtonsAtlasOptimization = false;
        public static Dictionary<string, float> widgetWidths = new Dictionary<string, float>();
        public static Dictionary<string, float> widgetXPositions = new Dictionary<string, float>();
        public static List<string> knownMainButtonDefNames = new List<string>();
        public static float gizmoDrawerOffsetX = 0f;
        public static float gizmoDrawerOffsetY = 0f;
        public static float gizmoDrawerScale = 1f;
        public static float gizmoSpacingX = 5f;
        public static float gizmoSpacingY = 14f;
        public static bool gizmoScaleMapOnly = false;

        private static HashSet<MainButtonDef> hiddenFromBarSet;
        private static Dictionary<MainButtonDef, List<MainButtonDef>> dropdownEntriesCache;
        private static bool dropdownCacheDirty = true;
        private static readonly List<string> enabledWidgetIdsCache = new List<string>();
        private static bool enabledWidgetCacheInitialized;
        private static int enabledWidgetCacheFlags = -1;

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref hiddenFromBarDefs, "hiddenFromBarDefs", LookMode.Def);
            if (hiddenFromBarDefs == null)
            {
                hiddenFromBarDefs = new List<MainButtonDef>();
            }

            Scribe_Collections.Look(ref forceShowDefs, "forceShowDefs", LookMode.Def);
            if (forceShowDefs == null)
            {
                forceShowDefs = new HashSet<MainButtonDef>();
            }

            Scribe_Collections.Look(ref favoriteDefs, "favoriteDefs", LookMode.Def);
            if (favoriteDefs == null)
            {
                favoriteDefs = new List<MainButtonDef>();
            }

            Scribe_Collections.Look(ref blacklistedFromMenuDefs, "blacklistedFromMenuDefs", LookMode.Def);
            if (blacklistedFromMenuDefs == null)
            {
                blacklistedFromMenuDefs = new List<MainButtonDef>();
            }

            Scribe_Collections.Look(ref dropdownConfigs, "dropdownConfigs", LookMode.Deep);
            if (dropdownConfigs == null)
            {
                dropdownConfigs = new List<MainButtonDropdownConfig>();
            }

            Scribe_Collections.Look(ref customOrderDefs, "customOrderDefs", LookMode.Def);
            if (customOrderDefs == null)
            {
                customOrderDefs = new List<MainButtonDef>();
            }

            Scribe_Collections.Look(ref freeSizeWidths, "freeSizeWidths", LookMode.Def, LookMode.Value);
            if (freeSizeWidths == null)
            {
                freeSizeWidths = new Dictionary<MainButtonDef, float>();
            }

            Scribe_Collections.Look(ref freeSizeXPositions, "freeSizeXPositions", LookMode.Def, LookMode.Value);
            if (freeSizeXPositions == null)
            {
                freeSizeXPositions = new Dictionary<MainButtonDef, float>();
            }

            Scribe_Values.Look(ref useAdvancedEditMode, "useFreeSizeMode", false);
            Scribe_Values.Look(ref useFixedWidthMode, "useFixedWidthMode", false);
            Scribe_Values.Look(ref fixedButtonWidth, "fixedButtonWidth", 120f);
            Scribe_Values.Look(ref snapThreshold, "snapThreshold", 8f);
            Scribe_Values.Look(ref centerFixedWidthButtons, "centerFixedWidthButtons", false);
            Scribe_Values.Look(ref pinMenuButtonRight, "pinMenuButtonRight", false);
            Scribe_Values.Look(ref pinMainButtonsMenuWindowRight, "pinMainButtonsMenuWindowRight", false);
            Scribe_Values.Look(ref useSearchablePlaySettingsMenu, "useSearchablePlaySettingsMenu", true);
            Scribe_Values.Look(ref revealPlaySettingsOnHover, "revealPlaySettingsOnHover", false);
            Scribe_Values.Look(ref hideEditModePlaySettingsButton, "hideEditModePlaySettingsButton", false);
            Scribe_Values.Look(ref defaultNewButtonsToHidden, "defaultNewButtonsToHidden", false);
            Scribe_Values.Look(ref showTimeWidget, "showTimeWidget", false);
            Scribe_Values.Look(ref showTimeIrlWidget, "showTimeIrlWidget", false);
            Scribe_Values.Look(ref showTimeSpeedWidget, "showTimeSpeedWidget", false);
            Scribe_Values.Look(ref showWeatherWidget, "showWeatherWidget", false);
            Scribe_Values.Look(ref showFpsTpsWidget, "showFpsTpsWidget", false);
            Scribe_Values.Look(ref showBatteryWidget, "showBatteryWidget", false);
            Scribe_Values.Look(ref disableVanillaDateReadout, "disableVanillaDateReadout", false);
            Scribe_Values.Look(ref disableVanillaTimeControls, "disableVanillaTimeControls", false);
            Scribe_Values.Look(ref disableVanillaWeatherWidget, "disableVanillaWeatherWidget", false);
            Scribe_Values.Look(ref disableVanillaConditionsWidget, "disableVanillaConditionsWidget", false);
            Scribe_Values.Look(ref revealVanillaResourceReadoutOnHover, "revealVanillaResourceReadoutOnHover", false);
            Scribe_Values.Look(ref disableVanillaResourceReadout, "disableVanillaResourceReadout", false);
            Scribe_Values.Look(ref disableVanillaMouseoverReadout, "disableVanillaMouseoverReadout", false);
            Scribe_Values.Look(ref disableVanillaTemperatureWidget, "disableVanillaTemperatureWidget", false);
            Scribe_Values.Look(ref experimentalMainButtonsAtlasOptimization, "experimentalMainButtonsAtlasOptimization", false);
            Scribe_Collections.Look(ref widgetWidths, "widgetWidths", LookMode.Value, LookMode.Value);
            if (widgetWidths == null)
            {
                widgetWidths = new Dictionary<string, float>();
            }
            Scribe_Collections.Look(ref widgetXPositions, "widgetXPositions", LookMode.Value, LookMode.Value);
            if (widgetXPositions == null)
            {
                widgetXPositions = new Dictionary<string, float>();
            }
            Scribe_Collections.Look(ref knownMainButtonDefNames, "knownMainButtonDefNames", LookMode.Value);
            Scribe_Values.Look(ref gizmoDrawerOffsetX, "gizmoDrawerOffsetX", 0f);
            Scribe_Values.Look(ref gizmoDrawerOffsetY, "gizmoDrawerOffsetY", 0f);
            Scribe_Values.Look(ref gizmoDrawerScale, "gizmoDrawerScale", 1f);
            Scribe_Values.Look(ref gizmoSpacingX, "gizmoSpacingX", 5f);
            Scribe_Values.Look(ref gizmoSpacingY, "gizmoSpacingY", 14f);
            Scribe_Values.Look(ref gizmoScaleMapOnly, "gizmoScaleMapOnly", false);
            fixedButtonWidth = Mathf.Clamp(fixedButtonWidth, 50f, 200f);
            gizmoDrawerScale = Mathf.Clamp(gizmoDrawerScale, 0.5f, 1.5f);
            gizmoSpacingX = Mathf.Clamp(gizmoSpacingX, 0f, 20f);
            gizmoSpacingY = Mathf.Clamp(gizmoSpacingY, -10f, 30f);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (knownMainButtonDefNames == null)
                {
                    knownMainButtonDefNames = MainButtonsCache.AllButtonsInOrder.Select(def => def.defName).ToList();
                }
                else
                {
                    knownMainButtonDefNames = knownMainButtonDefNames
                        .Where(defName => !string.IsNullOrEmpty(defName))
                        .ToList();
                }

                hiddenFromBarDefs = hiddenFromBarDefs.Where(defName => defName != null)
                        .ToList();
                forceShowDefs = new HashSet<MainButtonDef>(forceShowDefs.Where(defName => defName != null));
                blacklistedFromMenuDefs = blacklistedFromMenuDefs.Where(defName => defName != null)
                        .ToList();
                favoriteDefs = favoriteDefs.Where(defName => defName != null)
                        .ToList();
                customOrderDefs = customOrderDefs.Where(defName => defName != null)
                        .ToList();
                freeSizeWidths = freeSizeWidths
                    .Where(kvp => kvp.Key != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                freeSizeXPositions = freeSizeXPositions
                    .Where(kvp => kvp.Key != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                widgetWidths = widgetWidths
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Key) && MainBarWidgetIds.IsKnown(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                widgetXPositions = widgetXPositions
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Key) && MainBarWidgetIds.IsKnown(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                if (useSearchablePlaySettingsMenu && revealPlaySettingsOnHover)
                {
                    revealPlaySettingsOnHover = false;
                }

                if (revealVanillaResourceReadoutOnHover && disableVanillaResourceReadout)
                {
                    disableVanillaResourceReadout = false;
                }

                NormalizeDropdownConfigs();
                RebuildCaches();
                enabledWidgetCacheInitialized = false;
            }
        }

        private static void RebuildCaches()
        {
            // Rebuild hidden set
            hiddenFromBarSet = new HashSet<MainButtonDef>(hiddenFromBarDefs);

            // Mark dropdown cache as dirty so it rebuilds on next access
            dropdownCacheDirty = true;
        }

        public static bool EnsureCustomOrderCoverage()
        {
            if (customOrderDefs == null)
            {
                customOrderDefs = new List<MainButtonDef>();
            }

            List<MainButtonDef> allButtons = MainButtonsCache.AllButtonsInOrder;
            List<MainButtonDef> normalized = new List<MainButtonDef>(allButtons.Count);
            HashSet<MainButtonDef> seen = new HashSet<MainButtonDef>();

            for (int i = 0; i < customOrderDefs.Count; i++)
            {
                MainButtonDef def = customOrderDefs[i];
                if (seen.Add(def))
                {
                    normalized.Add(def);
                }
            }

            for (int i = 0; i < allButtons.Count; i++)
            {
                MainButtonDef def = allButtons[i];
                if (seen.Add(def))
                {
                    normalized.Add(def);
                }
            }

            bool changed = normalized.Count != customOrderDefs.Count;
            if (!changed)
            {
                for (int i = 0; i < normalized.Count; i++)
                {
                    if (normalized[i] != customOrderDefs[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                customOrderDefs = normalized;
                MainButtonsRoot_DoButtons_Patch.InvalidateOrderedVisibleCache();
            }

            return changed;
        }

        private static void RebuildDropdownCache()
        {
            dropdownEntriesCache = new Dictionary<MainButtonDef, List<MainButtonDef>>();

            for (int i = 0; i < dropdownConfigs.Count; i++)
            {
                MainButtonDropdownConfig config = dropdownConfigs[i];
                if (config == null || config.parent == null)
                {
                    continue;
                }

                List<MainButtonDef> entries = config.entries
                    .Where(entry => entry != config.parent && entry.Worker.Visible)
                    .Distinct()
                    .ToList();

                dropdownEntriesCache[config.parent] = entries;
            }

            dropdownCacheDirty = false;
        }

        public static bool IsHiddenFromBar(MainButtonDef def)
        {
            return hiddenFromBarSet.Contains(def);
        }

        public static bool IsForceShown(MainButtonDef def)
        {
            return forceShowDefs.Contains(def);
        }

        public static void SetHiddenFromBar(MainButtonDef def, bool hidden)
        {
            if (hidden)
            {
                if (!hiddenFromBarDefs.Contains(def))
                {
                    hiddenFromBarDefs.Add(def);
                    hiddenFromBarSet.Add(def);
                }
            }
            else
            {
                hiddenFromBarDefs.Remove(def);
                hiddenFromBarSet.Remove(def);
            }

            dropdownCacheDirty = true;
            MainButtonsRoot_DoButtons_Patch.InvalidateOrderedVisibleCache();
        }

        public static void SetForceShown(MainButtonDef def, bool forceShown)
        {
            if (forceShown)
            {
                forceShowDefs.Add(def);
            }
            else
            {
                forceShowDefs.Remove(def);
            }

            MainButtonsRoot_DoButtons_Patch.InvalidateOrderedVisibleCache();
        }

        public static bool IsFavorite(MainButtonDef def)
        {
            return favoriteDefs.Contains(def);
        }

        public static void SetFavorite(MainButtonDef def, bool favorite)
        {
            if (favorite)
            {
                if (!favoriteDefs.Contains(def))
                {
                    favoriteDefs.Add(def);
                }
            }
            else
            {
                favoriteDefs.Remove(def);
            }
        }

        public static bool IsBlacklistedFromMenu(MainButtonDef def)
        {
            return blacklistedFromMenuDefs.Contains(def);
        }

        public static void SetBlacklistedFromMenu(MainButtonDef def, bool blacklisted)
        {
            if (blacklisted)
            {
                if (!blacklistedFromMenuDefs.Contains(def))
                {
                    blacklistedFromMenuDefs.Add(def);
                }
            }
            else
            {
                blacklistedFromMenuDefs.Remove(def);
            }
        }

        public static void SetVanillaResourceReadoutDisabled(bool disabled)
        {
            disableVanillaResourceReadout = disabled;
            if (disabled)
            {
                revealVanillaResourceReadoutOnHover = false;
            }
        }

        public static void SetVanillaResourceReadoutHover(bool revealOnHover)
        {
            revealVanillaResourceReadoutOnHover = revealOnHover;
            if (revealOnHover)
            {
                disableVanillaResourceReadout = false;
            }
        }

        public static void ResetToDefaults()
        {
            hiddenFromBarDefs.Clear();
            forceShowDefs.Clear();
            favoriteDefs.Clear();
            blacklistedFromMenuDefs.Clear();
            dropdownConfigs.Clear();
            customOrderDefs = new List<MainButtonDef>(MainButtonsCache.AllButtonsInOrder);
            freeSizeWidths.Clear();
            freeSizeXPositions.Clear();
            editDropdownsMode = false;
            useAdvancedEditMode = false;
            useFixedWidthMode = false;
            fixedButtonWidth = 120f;
            snapThreshold = 8f;
            centerFixedWidthButtons = false;
            pinMenuButtonRight = false;
            pinMainButtonsMenuWindowRight = false;
            useSearchablePlaySettingsMenu = true;
            revealPlaySettingsOnHover = false;
            hideEditModePlaySettingsButton = false;
            defaultNewButtonsToHidden = false;
            showTimeWidget = false;
            showTimeIrlWidget = false;
            showTimeSpeedWidget = false;
            showWeatherWidget = false;
            showFpsTpsWidget = false;
            showBatteryWidget = false;
            disableVanillaDateReadout = false;
            disableVanillaTimeControls = false;
            disableVanillaWeatherWidget = false;
            disableVanillaConditionsWidget = false;
            revealVanillaResourceReadoutOnHover = false;
            disableVanillaResourceReadout = false;
            disableVanillaMouseoverReadout = false;
            disableVanillaTemperatureWidget = false;
            experimentalMainButtonsAtlasOptimization = false;
            MainButtonsAtlasTextureCache.ClearCache();
            widgetWidths.Clear();
            widgetXPositions.Clear();
            enabledWidgetIdsCache.Clear();
            enabledWidgetCacheInitialized = false;
            enabledWidgetCacheFlags = -1;
            knownMainButtonDefNames = MainButtonsCache.AllButtonsInOrder.Select(def => def.defName).ToList();
            gizmoDrawerOffsetX = 0f;
            gizmoDrawerOffsetY = 0f;
            gizmoDrawerScale = 1f;
            gizmoSpacingX = 5f;
            gizmoSpacingY = 14f;
            gizmoScaleMapOnly = false;
            RebuildCaches();
            EnsureCustomOrderCoverage();
        }

        public static bool DetectAndHideNewButtonsFromBarIfNeeded()
        {
            if (knownMainButtonDefNames.Count == 0)
            {
                knownMainButtonDefNames = MainButtonsCache.AllButtonsInOrder.Select(def => def.defName).ToList();
            }

            HashSet<string> knownDefs = new HashSet<string>(knownMainButtonDefNames);
            bool settingsChanged = false;

           
            for (int i = 0; i < MainButtonsCache.AllButtonsInOrder.Count; i++)
            {
                MainButtonDef def = MainButtonsCache.AllButtonsInOrder[i];
                if (knownDefs.Contains(def.defName))
                {
                    continue;
                }

                if (defaultNewButtonsToHidden && !IsHiddenFromBar(def))
                {
                    SetHiddenFromBar(def, hidden: true);
                }

                settingsChanged = true;
            }

            if (settingsChanged)
            {
                knownMainButtonDefNames = MainButtonsCache.AllButtonsInOrder.Select(def => def.defName).ToList();
            }

            return settingsChanged;
        }

        public static bool IsWidgetEnabled(string widgetId)
        {
            if (widgetId == MainBarWidgetIds.Time)
            {
                return showTimeWidget;
            }
            if (widgetId == MainBarWidgetIds.TimeIrl)
            {
                return showTimeIrlWidget;
            }
            if (widgetId == MainBarWidgetIds.TimeSpeed)
            {
                return showTimeSpeedWidget;
            }
            if (widgetId == MainBarWidgetIds.Weather)
            {
                return showWeatherWidget;
            }
            if (widgetId == MainBarWidgetIds.FpsTps)
            {
                return showFpsTpsWidget;
            }
            if (widgetId == MainBarWidgetIds.Battery)
            {
                return showBatteryWidget;
            }

            return false;
        }

        public static List<string> GetEnabledWidgetIds()
        {
            int currentFlags = BuildEnabledWidgetFlags();
            if (!enabledWidgetCacheInitialized || enabledWidgetCacheFlags != currentFlags)
            {
                RebuildEnabledWidgetIdsCache();
                enabledWidgetCacheFlags = currentFlags;
                enabledWidgetCacheInitialized = true;
            }

            return enabledWidgetIdsCache;
        }

        private static int BuildEnabledWidgetFlags()
        {
            int flags = 0;
            if (showTimeWidget) flags |= 1 << 0;
            if (showTimeIrlWidget) flags |= 1 << 1;
            if (showTimeSpeedWidget) flags |= 1 << 2;
            if (showWeatherWidget) flags |= 1 << 3;
            if (showFpsTpsWidget) flags |= 1 << 4;
            if (showBatteryWidget) flags |= 1 << 5;
            return flags;
        }

        private static void RebuildEnabledWidgetIdsCache()
        {
            enabledWidgetIdsCache.Clear();

            if (showTimeWidget) enabledWidgetIdsCache.Add(MainBarWidgetIds.Time);
            if (showTimeIrlWidget) enabledWidgetIdsCache.Add(MainBarWidgetIds.TimeIrl);
            if (showTimeSpeedWidget) enabledWidgetIdsCache.Add(MainBarWidgetIds.TimeSpeed);
            if (showWeatherWidget) enabledWidgetIdsCache.Add(MainBarWidgetIds.Weather);
            if (showFpsTpsWidget) enabledWidgetIdsCache.Add(MainBarWidgetIds.FpsTps);
            if (showBatteryWidget) enabledWidgetIdsCache.Add(MainBarWidgetIds.Battery);
        }

        public static bool HasDropdown(MainButtonDef def)
        {
            return GetDropdownEntries(def).Count > 0;
        }

        public static List<MainButtonDef> GetDropdownEntries(MainButtonDef def)
        {
            if (dropdownCacheDirty || dropdownEntriesCache == null)
            {
                RebuildDropdownCache();
            }

            if (dropdownEntriesCache.TryGetValue(def, out List<MainButtonDef> cached))
            {
                return cached;
            }

            return new List<MainButtonDef>();
        }

        public static bool IsInDropdown(MainButtonDef parent, MainButtonDef entry)
        {
            MainButtonDropdownConfig config = dropdownConfigs.Find(item => item.parent == parent);

            if (config == null)
            {
                return false;
            }

            return config.entries.Contains(entry);
        }

        public static void SetDropdownEntry(MainButtonDef parent, MainButtonDef entry, bool enabled)
        {
            if (entry == parent)
            {
                return;
            }

            MainButtonDropdownConfig config = GetOrCreateDropdownConfig(parent);
            if (enabled)
            {
                if (!config.entries.Contains(entry))
                {
                    config.entries.Add(entry);
                }
            }
            else
            {
                config.entries.Remove(entry);
                if (config.entries.Count == 0)
                {
                    dropdownConfigs.Remove(config);
                }
            }

            dropdownCacheDirty = true;
        }

        private static MainButtonDropdownConfig GetOrCreateDropdownConfig(MainButtonDef parent)
        {
            MainButtonDropdownConfig config = dropdownConfigs.Find(item => item.parent == parent);
            if (config == null)
            {
                config = new MainButtonDropdownConfig
                {
                    parent = parent,
                    entries = new List<MainButtonDef>()
                };
                dropdownConfigs.Add(config);
            }

            return config;
        }

        private static void NormalizeDropdownConfigs()
        {
            if (dropdownConfigs == null)
            {
                dropdownConfigs = new List<MainButtonDropdownConfig>();
            }

            for (int i = dropdownConfigs.Count - 1; i >= 0; i--)
            {
                MainButtonDropdownConfig config = dropdownConfigs[i];
                if (config == null || config.parent == null)
                {
                    dropdownConfigs.RemoveAt(i);
                    continue;
                }

                if (config.entries == null)
                {
                    config.entries = new List<MainButtonDef>();
                }

                config.entries = config.entries
                    .Where(entry => entry != null && entry != config.parent)
                    .Distinct()
                    .ToList();

                if (config.entries.Count == 0)
                {
                    dropdownConfigs.RemoveAt(i);
                }
            }
        }
    }
}
