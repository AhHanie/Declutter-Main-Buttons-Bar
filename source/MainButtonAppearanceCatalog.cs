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

        private static List<string> iconPaths;
        private static HashSet<string> iconPathSet;
        private static readonly Dictionary<string, Texture2D> TextureCache = new Dictionary<string, Texture2D>();

        public static IReadOnlyList<string> IconPaths
        {
            get
            {
                EnsureBuilt();
                return iconPaths;
            }
        }

        public static bool IsKnownPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            EnsureBuilt();
            return iconPathSet.Contains(path);
        }

        public static Texture2D GetTexture(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (TextureCache.TryGetValue(path, out Texture2D cached))
            {
                return cached;
            }

            if (!IsKnownPath(path))
            {
                return null;
            }

            Texture2D texture = ContentFinder<Texture2D>.Get(path, false) ?? BaseContent.BadTex;
            TextureCache[path] = texture;
            return texture;
        }

        private static void EnsureBuilt()
        {
            if (iconPaths != null)
            {
                return;
            }

            iconPaths = new List<string>(Names.Count);
            iconPathSet = new HashSet<string>();
            for (int i = 0; i < Names.Count; i++)
            {
                string path = TextureRoot + Names[i];
                iconPaths.Add(path);
                iconPathSet.Add(path);
            }
        }
    }
}
