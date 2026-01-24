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
        public static List<MainButtonDef> favoriteDefs = new List<MainButtonDef>();
        public static List<MainButtonDef> blacklistedFromMenuDefs = new List<MainButtonDef>();
        public static List<MainButtonDropdownConfig> dropdownConfigs = new List<MainButtonDropdownConfig>();
        public static List<MainButtonDef> customOrderDefs = new List<MainButtonDef>();
        public static Dictionary<MainButtonDef, float> freeSizeWidths = new Dictionary<MainButtonDef, float>();
        public static Dictionary<MainButtonDef, float> freeSizeXPositions = new Dictionary<MainButtonDef, float>();
        public static float snapThreshold = 8f;
        public static bool editDropdownsMode = false;
        public static bool useFreeSizeMode = false;
        public static bool useFixedWidthMode = false;
        public static float fixedButtonWidth = 120f;
        public static bool centerFixedWidthButtons = false;
        public static bool pinMenuButtonRight = false;
        public static bool useSearchablePlaySettingsMenu = true;
        public static bool revealPlaySettingsOnHover = false;
        public static float gizmoDrawerOffsetX = 0f;
        public static float gizmoDrawerOffsetY = 0f;
        public static float gizmoDrawerScale = 1f;
        public static bool gizmoScaleMapOnly = false;

        private static HashSet<MainButtonDef> hiddenFromBarSet;
        private static Dictionary<MainButtonDef, List<MainButtonDef>> dropdownEntriesCache;
        private static bool dropdownCacheDirty = true;

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref hiddenFromBarDefs, "hiddenFromBarDefs", LookMode.Def);
            if (hiddenFromBarDefs == null)
            {
                hiddenFromBarDefs = new List<MainButtonDef>();
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

            Scribe_Values.Look(ref useFreeSizeMode, "useFreeSizeMode", false);
            Scribe_Values.Look(ref useFixedWidthMode, "useFixedWidthMode", false);
            Scribe_Values.Look(ref fixedButtonWidth, "fixedButtonWidth", 120f);
            Scribe_Values.Look(ref snapThreshold, "snapThreshold", 8f);
            Scribe_Values.Look(ref centerFixedWidthButtons, "centerFixedWidthButtons", false);
            Scribe_Values.Look(ref pinMenuButtonRight, "pinMenuButtonRight", false);
            Scribe_Values.Look(ref useSearchablePlaySettingsMenu, "useSearchablePlaySettingsMenu", true);
            Scribe_Values.Look(ref revealPlaySettingsOnHover, "revealPlaySettingsOnHover", false);
            Scribe_Values.Look(ref gizmoDrawerOffsetX, "gizmoDrawerOffsetX", 0f);
            Scribe_Values.Look(ref gizmoDrawerOffsetY, "gizmoDrawerOffsetY", 0f);
            Scribe_Values.Look(ref gizmoDrawerScale, "gizmoDrawerScale", 1f);
            Scribe_Values.Look(ref gizmoScaleMapOnly, "gizmoScaleMapOnly", false);
            fixedButtonWidth = Mathf.Clamp(fixedButtonWidth, 50f, 200f);
            gizmoDrawerScale = Mathf.Clamp(gizmoDrawerScale, 0.5f, 1.5f);
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (useSearchablePlaySettingsMenu && revealPlaySettingsOnHover)
                {
                    revealPlaySettingsOnHover = false;
                }
                NormalizeDropdownConfigs();
                RebuildCaches();
            }
        }

        private static void RebuildCaches()
        {
            // Rebuild hidden set
            hiddenFromBarSet = new HashSet<MainButtonDef>(hiddenFromBarDefs);

            // Mark dropdown cache as dirty so it rebuilds on next access
            dropdownCacheDirty = true;
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
                    .Where(entry => entry != config.parent && !IsHiddenFromBar(entry) && entry.Worker.Visible)
                    .Distinct()
                    .OrderBy(entry => entry.order)
                    .ToList();

                dropdownEntriesCache[config.parent] = entries;
            }

            dropdownCacheDirty = false;
        }

        public static bool IsHiddenFromBar(MainButtonDef def)
        {
            if (hiddenFromBarSet == null)
            {
                hiddenFromBarSet = new HashSet<MainButtonDef>(hiddenFromBarDefs);
            }
            return hiddenFromBarSet.Contains(def);
        }

        public static void SetHiddenFromBar(MainButtonDef def, bool hidden)
        {
            if (hidden)
            {
                if (!hiddenFromBarDefs.Contains(def))
                {
                    hiddenFromBarDefs.Add(def);
                    if (hiddenFromBarSet != null)
                    {
                        hiddenFromBarSet.Add(def);
                    }
                }
            }
            else
            {
                hiddenFromBarDefs.Remove(def);
                if (hiddenFromBarSet != null)
                {
                    hiddenFromBarSet.Remove(def);
                }
            }

            dropdownCacheDirty = true;
            MainButtonsRoot_DoButtons_Patch.InvalidateOrderedVisibleCache();

            if (useFreeSizeMode && !hidden)
            {
                MainButtonsRoot_DoButtons_Patch.ReconcileFreeSizeAfterVisibilityChange();
            }
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

        public static void ResetToDefaults()
        {
            hiddenFromBarDefs.Clear();
            favoriteDefs.Clear();
            blacklistedFromMenuDefs.Clear();
            dropdownConfigs.Clear();
            customOrderDefs.Clear();
            freeSizeWidths.Clear();
            freeSizeXPositions.Clear();
            editDropdownsMode = false;
            useFreeSizeMode = false;
            useFixedWidthMode = false;
            fixedButtonWidth = 120f;
            snapThreshold = 8f;
            centerFixedWidthButtons = false;
            pinMenuButtonRight = false;
            useSearchablePlaySettingsMenu = true;
            revealPlaySettingsOnHover = false;
            gizmoDrawerOffsetX = 0f;
            gizmoDrawerOffsetY = 0f;
            gizmoDrawerScale = 1f;
            gizmoScaleMapOnly = false;
            RebuildCaches();
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
