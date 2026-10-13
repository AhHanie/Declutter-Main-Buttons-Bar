using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    // Explicit, deterministic catalog of the DMMB main-button icon set. RimWorld cannot safely
    // enumerate a packed mod's texture folder at runtime, so the 71 shipped PNGs are listed here
    // by hand; this list must stay in sync with Textures/DMMB/UI/MainButton.
    public static class MainButtonAppearanceCatalog
    {
        private const string TextureRoot = "DMMB/UI/MainButton/";

        private static readonly List<string> Names = new List<string>
        {
            "alert-circle-filled",
            "alert-hexagon-filled",
            "ambulance",
            "apps-filled",
            "battery-vertical-2",
            "bed-filled",
            "binary-tree-2-filled",
            "book-filled",
            "bug-filled",
            "building-bank",
            "building-lighthouse",
            "building-warehouse",
            "bulb-filled",
            "calendar",
            "calendar-month",
            "car-garage",
            "cat",
            "category-filled",
            "chef-hat-filled",
            "christmas-tree-filled",
            "clipboard-filled",
            "clipboard-text-filled",
            "clock-filled",
            "cloud-filled",
            "coffin",
            "compass-filled",
            "cookie-filled",
            "cookie-man-filled",
            "credit-card-filled",
            "cup",
            "dog",
            "file-description-filled",
            "flag-2-filled",
            "flask-filled",
            "folder-filled",
            "folder-open-filled",
            "graph",
            "hammer",
            "hammer-drill",
            "headphones-filled",
            "home-2-filled",
            "home-filled",
            "leaf-filled",
            "library-filled",
            "lighter",
            "map",
            "message-chatbot-filled",
            "music",
            "needle",
            "palette-filled",
            "paw-filled",
            "pill-filled",
            "plane-filled",
            "planet",
            "robot",
            "ruler-2",
            "scale-filled",
            "script",
            "settings-cog",
            "shield-checkered-filled",
            "shield-filled",
            "shovel-pitchforks",
            "sitemap-filled",
            "skull",
            "sparkle",
            "teapot",
            "tool",
            "tools-kitchen-2-filled",
            "user-filled",
            "users-group",
            "wheat",
        };

        // Optional icon libraries from other mods. A saved choice from one of these is stored as
        // "mod:<packageId>|<texture path>" so it is resolved from that mod's own loaded textures
        // rather than through ContentFinder, which would pick a same-named texture from any mod.
        private const string ExternalPrefix = "mod:";
        private const char ExternalSeparator = '|';

        private sealed class Provider
        {
            public string PackageId;
            public string Folder;
            public string LabelKey;
        }

        private static readonly Provider[] Providers =
        {
            new Provider
            {
                PackageId = "bs.mbifvte",
                Folder = "UI/Buttons/MainButtons/",
                LabelKey = "DMMB.AppearanceSourceBradson",
            },
            new Provider
            {
                PackageId = "vanillaexpanded.vtexe",
                Folder = "UI/Buttons/MainButtons/",
                LabelKey = "DMMB.AppearanceSourceVTE",
            },
        };

        private sealed class ProviderState
        {
            public Provider Provider;
            public ModContentPack Pack;
            public int PackIndex = -1;
            public ModContentHolder<Texture2D> Holder;
            public int HolderCount;
            public MainButtonIconSource Source;
        }

        private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;
        private static readonly Dictionary<string, Texture2D> TextureCache = new Dictionary<string, Texture2D>();

        private static MainButtonIconSource builtInSource;
        private static HashSet<string> builtInKeySet;
        private static ProviderState[] providerStates;
        private static List<MainButtonIconSource> sources;
        private static List<ModContentPack> lastRunningMods;
        private static int lastRunningModsCount;

        // Fixed order: built-in, Bradson's Main Button Icons, Vanilla Textures Expanded. Optional
        // groups appear only while their mod is active and has loaded textures in its icon folder.
        public static IReadOnlyList<MainButtonIconSource> GetSources()
        {
            EnsureProviderState();
            return sources;
        }

        // True when the value may be kept in settings, regardless of whether its provider mod is
        // currently active. Rejects unknown providers and malformed paths.
        public static bool IsPersistableKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!IsExternalKey(key))
            {
                EnsureBuiltIn();
                return builtInKeySet.Contains(key);
            }

            return TryParseExternalKey(key, out _, out _);
        }

        public static bool IsExternalKey(string key)
        {
            return key != null && key.StartsWith(ExternalPrefix, StringComparison.Ordinal);
        }

        // Source label and icon name for tooltips and the unavailable-selection message.
        public static bool TryDescribe(string key, out string sourceLabel, out string leafName)
        {
            sourceLabel = null;
            leafName = null;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (IsExternalKey(key))
            {
                if (!TryParseExternalKey(key, out Provider provider, out string path))
                {
                    return false;
                }

                sourceLabel = provider.LabelKey.Translate();
                leafName = path.Substring(path.LastIndexOf('/') + 1);
                return true;
            }

            if (!IsPersistableKey(key))
            {
                return false;
            }

            sourceLabel = "DMMB.AppearanceSourceBuiltIn".Translate();
            leafName = key.Substring(key.LastIndexOf('/') + 1);
            return true;
        }

        // Returns the loaded texture for a saved key. Built-in keys fall back to BadTex when the
        // file failed to load; optional keys return null whenever the provider mod or texture
        // is unavailable, so callers can fall back to the button's original icon.
        public static Texture2D GetTexture(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (IsExternalKey(key))
            {
                return GetExternalTexture(key);
            }

            if (TextureCache.TryGetValue(key, out Texture2D cached))
            {
                return cached;
            }

            if (!IsPersistableKey(key))
            {
                return null;
            }

            Texture2D texture = ContentFinder<Texture2D>.Get(key, false) ?? BaseContent.BadTex;
            TextureCache[key] = texture;
            return texture;
        }

        private static Texture2D GetExternalTexture(string key)
        {
            if (!TryParseExternalKey(key, out Provider provider, out string path))
            {
                return null;
            }

            // Refresh first: a rebuild clears the cache, so a texture from a provider that has
            // since gone away or reloaded is never returned. This is cheap enough for IMGUI.
            EnsureProviderState();

            // A destroyed Unity object compares equal to null, so a stale entry is re-resolved.
            if (TextureCache.TryGetValue(key, out Texture2D cached) && cached != null)
            {
                return cached;
            }

            TextureCache.Remove(key);
            for (int i = 0; i < providerStates.Length; i++)
            {
                ProviderState state = providerStates[i];
                if (state.Provider != provider || state.Holder == null)
                {
                    continue;
                }

                Texture2D texture = state.Holder.Get(path);
                if (texture == null || texture == BaseContent.BadTex)
                {
                    return null;
                }

                TextureCache[key] = texture;
                return texture;
            }

            return null;
        }

        private static bool TryParseExternalKey(string key, out Provider provider, out string path)
        {
            provider = null;
            path = null;
            if (!IsExternalKey(key))
            {
                return false;
            }

            int separator = key.IndexOf(ExternalSeparator);
            if (separator < 0)
            {
                return false;
            }

            string packageId = key.Substring(ExternalPrefix.Length, separator - ExternalPrefix.Length);
            string texturePath = key.Substring(separator + 1);
            for (int i = 0; i < Providers.Length; i++)
            {
                Provider candidate = Providers[i];
                if (!string.Equals(candidate.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!texturePath.StartsWith(candidate.Folder, StringComparison.Ordinal))
                {
                    return false;
                }

                string leaf = texturePath.Substring(candidate.Folder.Length);
                if (leaf.Length == 0
                    || leaf.IndexOf('/') >= 0
                    || leaf.IndexOf('\\') >= 0
                    || leaf.IndexOf(ExternalSeparator) >= 0
                    || leaf == "."
                    || leaf == "..")
                {
                    return false;
                }

                provider = candidate;
                path = texturePath;
                return true;
            }

            return false;
        }

        private static string MakeKey(Provider provider, string texturePath)
        {
            return ExternalPrefix + provider.PackageId + ExternalSeparator + texturePath;
        }

        private static void EnsureBuiltIn()
        {
            if (builtInSource != null)
            {
                return;
            }

            List<MainButtonIconEntry> entries = new List<MainButtonIconEntry>(Names.Count);
            HashSet<string> keys = new HashSet<string>();
            for (int i = 0; i < Names.Count; i++)
            {
                string path = TextureRoot + Names[i];
                keys.Add(path);
                entries.Add(new MainButtonIconEntry(path, Names[i], "DMMB.AppearanceSourceBuiltIn"));
            }

            builtInKeySet = keys;
            builtInSource = new MainButtonIconSource("DMMB.AppearanceSourceBuiltIn", entries);
        }

        // Rebuilds the optional groups when the active mod list or a provider's loaded texture
        // set changes (e.g. content reload), otherwise returns immediately: this runs from IMGUI.
        private static void EnsureProviderState()
        {
            EnsureBuiltIn();

            List<ModContentPack> running = LoadedModManager.RunningModsListForReading;
            if (providerStates != null
                && ReferenceEquals(running, lastRunningMods)
                && running.Count == lastRunningModsCount
                && ProviderStatesCurrent(running))
            {
                return;
            }

            lastRunningMods = running;
            lastRunningModsCount = running.Count;
            TextureCache.Clear();

            ProviderState[] states = new ProviderState[Providers.Length];
            List<MainButtonIconSource> result = new List<MainButtonIconSource> { builtInSource };
            for (int i = 0; i < Providers.Length; i++)
            {
                states[i] = BuildProviderState(Providers[i], running);
                if (states[i].Source != null)
                {
                    result.Add(states[i].Source);
                }
            }

            providerStates = states;
            sources = result;
        }

        private static bool ProviderStatesCurrent(List<ModContentPack> running)
        {
            for (int i = 0; i < providerStates.Length; i++)
            {
                ProviderState state = providerStates[i];
                if (state.Holder == null)
                {
                    continue;
                }

                // The list can be reused with the same count while its contents change, and a
                // reloaded pack swaps its holder contents.
                if (state.PackIndex >= running.Count
                    || running[state.PackIndex] != state.Pack
                    || state.Pack.GetContentHolder<Texture2D>() != state.Holder
                    || state.Holder.contentList.Count != state.HolderCount)
                {
                    return false;
                }
            }

            return true;
        }

        private static ProviderState BuildProviderState(Provider provider, List<ModContentPack> running)
        {
            ProviderState state = new ProviderState { Provider = provider };
            for (int i = 0; i < running.Count; i++)
            {
                ModContentPack pack = running[i];
                if (pack == null || !string.Equals(pack.PackageIdPlayerFacing, provider.PackageId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                state.Pack = pack;
                state.PackIndex = i;
                state.Holder = pack.GetContentHolder<Texture2D>();
                break;
            }

            if (state.Holder == null)
            {
                return state;
            }

            state.HolderCount = state.Holder.contentList.Count;
            List<MainButtonIconEntry> entries = new List<MainButtonIconEntry>();
            HashSet<string> seen = new HashSet<string>();
            foreach (KeyValuePair<string, Texture2D> pair in state.Holder.contentList)
            {
                string texturePath = pair.Key;
                if (!texturePath.StartsWith(provider.Folder, StringComparison.Ordinal)
                    || pair.Value == null
                    || pair.Value == BaseContent.BadTex
                    || !seen.Add(texturePath))
                {
                    continue;
                }

                string leaf = texturePath.Substring(provider.Folder.Length);
                if (leaf.Length == 0 || leaf.IndexOf('/') >= 0)
                {
                    continue;
                }

                entries.Add(new MainButtonIconEntry(MakeKey(provider, texturePath), leaf, provider.LabelKey));
            }

            if (entries.Count > 0)
            {
                entries.Sort((a, b) =>
                {
                    int byName = NameComparer.Compare(a.Name, b.Name);
                    return byName != 0 ? byName : string.CompareOrdinal(a.Name, b.Name);
                });
                state.Source = new MainButtonIconSource(provider.LabelKey, entries);
            }

            return state;
        }
    }

    public sealed class MainButtonIconEntry
    {
        public readonly string Key;
        public readonly string Name;
        public readonly string SourceLabelKey;

        public MainButtonIconEntry(string key, string name, string sourceLabelKey)
        {
            Key = key;
            Name = name;
            SourceLabelKey = sourceLabelKey;
        }
    }

    public sealed class MainButtonIconSource
    {
        public readonly string LabelKey;
        public readonly IReadOnlyList<MainButtonIconEntry> Entries;

        public MainButtonIconSource(string labelKey, IReadOnlyList<MainButtonIconEntry> entries)
        {
            LabelKey = labelKey;
            Entries = entries;
        }
    }
}
