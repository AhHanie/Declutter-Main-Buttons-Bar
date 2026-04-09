using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;
using LudeonTK;

namespace Declutter_Main_Buttons_Bar
{
    public static class MainButtonsDrawAtlasOptimizationScope
    {
        private static bool active;

        public static bool Active => active;

        public static void Begin()
        {
            active = true;
        }

        public static void End()
        {
            active = false;
        }
    }

    public static class MainButtonsAtlasTextureCache
    {
        private const int MaxEntries = 256;

        private static readonly Rect AtlasUVTopLeft = new Rect(0f, 0.75f, 0.25f, 0.25f);
        private static readonly Rect AtlasUVTopRight = new Rect(0.75f, 0.75f, 0.25f, 0.25f);
        private static readonly Rect AtlasUVBottomLeft = new Rect(0f, 0f, 0.25f, 0.25f);
        private static readonly Rect AtlasUVBottomRight = new Rect(0.75f, 0f, 0.25f, 0.25f);
        private static readonly Rect AtlasUVTop = new Rect(0.25f, 0.75f, 0.5f, 0.25f);
        private static readonly Rect AtlasUVBottom = new Rect(0.25f, 0f, 0.5f, 0.25f);
        private static readonly Rect AtlasUVLeft = new Rect(0f, 0.25f, 0.25f, 0.5f);
        private static readonly Rect AtlasUVRight = new Rect(0.75f, 0.25f, 0.25f, 0.5f);
        private static readonly Rect AtlasUVCenter = new Rect(0.25f, 0.25f, 0.5f, 0.5f);

        private static readonly Dictionary<AtlasKey, CacheEntry> cache = new Dictionary<AtlasKey, CacheEntry>();

        private struct AtlasKey : IEquatable<AtlasKey>
        {
            public int atlasId;
            public int width;
            public int height;
            public int corner;
            public bool drawTop;

            public bool Equals(AtlasKey other)
            {
                return atlasId == other.atlasId
                    && width == other.width
                    && height == other.height
                    && corner == other.corner
                    && drawTop == other.drawTop;
            }

            public override bool Equals(object obj)
            {
                return obj is AtlasKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                int hash = atlasId;
                hash = (hash * 397) ^ width;
                hash = (hash * 397) ^ height;
                hash = (hash * 397) ^ corner;
                hash = (hash * 397) ^ (drawTop ? 1 : 0);
                return hash;
            }
        }

        private sealed class CacheEntry
        {
            public RenderTexture texture;
            public int lastUsedFrame;
        }

        public static bool TryDrawCached(Rect originalRect, Texture2D atlas, bool drawTop)
        {
            Rect rect = NormalizeRect(originalRect);
            int width = Mathf.RoundToInt(rect.width);
            int height = Mathf.RoundToInt(rect.height);
            if (width <= 0 || height <= 0 || atlas == null)
            {
                return false;
            }

            int corner = CalculateCorner(atlas, rect);
            if (corner <= 0)
            {
                return false;
            }

            AtlasKey key = new AtlasKey
            {
                atlasId = atlas.GetInstanceID(),
                width = width,
                height = height,
                corner = corner,
                drawTop = drawTop
            };

            if (!cache.TryGetValue(key, out CacheEntry entry) || entry.texture == null)
            {
                RenderTexture stitched = CreateStitchedTexture(atlas, width, height, corner, drawTop);
                if (stitched == null)
                {
                    return false;
                }

                entry = new CacheEntry
                {
                    texture = stitched
                };
                cache[key] = entry;
                TrimCacheIfNeeded();
            }

            entry.lastUsedFrame = Time.frameCount;
            GUI.DrawTexture(rect, entry.texture);
            return true;
        }

        public static void ClearCache()
        {
            foreach (KeyValuePair<AtlasKey, CacheEntry> kvp in cache)
            {
                DestroyEntryTexture(kvp.Value);
            }

            cache.Clear();
        }

        private static Rect NormalizeRect(Rect rect)
        {
            rect.x = Mathf.Round(rect.x);
            rect.y = Mathf.Round(rect.y);
            rect.width = Mathf.Round(rect.width);
            rect.height = Mathf.Round(rect.height);
            return UIScaling.AdjustRectToUIScaling(rect);
        }

        private static int CalculateCorner(Texture2D atlas, Rect rect)
        {
            float corner = atlas.width * 0.25f;
            float limit = Mathf.Min(rect.height * 0.5f, rect.width * 0.5f);
            corner = UIScaling.AdjustCoordToUIScalingCeil(Mathf.Min(corner, limit));
            return Mathf.RoundToInt(corner);
        }

        private static RenderTexture CreateStitchedTexture(Texture2D atlas, int width, int height, int corner, bool drawTop)
        {
            RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = atlas.filterMode;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.Create();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0f, width, height, 0f);
            GL.Clear(true, true, Color.clear);

            DrawAtlasParts(atlas, width, height, corner, drawTop);

            GL.PopMatrix();
            RenderTexture.active = previous;
            return rt;
        }

        private static void DrawAtlasParts(Texture2D atlas, int width, int height, int corner, bool drawTop)
        {
            float a = corner;
            float w = width;
            float h = height;

            if (drawTop)
            {
                DrawPart(atlas, new Rect(0f, 0f, a, a), AtlasUVTopLeft);
                DrawPart(atlas, new Rect(w - a, 0f, a, a), AtlasUVTopRight);
                DrawPart(atlas, new Rect(a, 0f, w - a * 2f, a), AtlasUVTop);
            }

            DrawPart(atlas, new Rect(0f, h - a, a, a), AtlasUVBottomLeft);
            DrawPart(atlas, new Rect(w - a, h - a, a, a), AtlasUVBottomRight);
            DrawPart(atlas, new Rect(a, h - a, w - a * 2f, a), AtlasUVBottom);

            if (drawTop)
            {
                DrawPart(atlas, new Rect(0f, a, a, h - a * 2f), AtlasUVLeft);
                DrawPart(atlas, new Rect(w - a, a, a, h - a * 2f), AtlasUVRight);
                DrawPart(atlas, new Rect(a, a, w - a * 2f, h - a * 2f), AtlasUVCenter);
                return;
            }

            DrawPart(atlas, new Rect(0f, 0f, a, h - a), AtlasUVLeft);
            DrawPart(atlas, new Rect(w - a, 0f, a, h - a), AtlasUVRight);
            DrawPart(atlas, new Rect(a, 0f, w - a * 2f, h - a), AtlasUVCenter);
        }

        private static void DrawPart(Texture2D atlas, Rect dest, Rect uv)
        {
            if (dest.width <= 0f || dest.height <= 0f)
            {
                return;
            }

            Graphics.DrawTexture(dest, atlas, uv, 0, 0, 0, 0);
        }

        private static void TrimCacheIfNeeded()
        {
            while (cache.Count > MaxEntries)
            {
                AtlasKey oldestKey = default;
                CacheEntry oldestEntry = null;
                bool found = false;
                int oldestFrame = int.MaxValue;

                foreach (KeyValuePair<AtlasKey, CacheEntry> kvp in cache)
                {
                    CacheEntry entry = kvp.Value;
                    if (entry == null || entry.texture == null)
                    {
                        oldestKey = kvp.Key;
                        oldestEntry = entry;
                        found = true;
                        break;
                    }

                    if (entry.lastUsedFrame < oldestFrame)
                    {
                        oldestFrame = entry.lastUsedFrame;
                        oldestKey = kvp.Key;
                        oldestEntry = entry;
                        found = true;
                    }
                }

                if (!found)
                {
                    return;
                }

                DestroyEntryTexture(oldestEntry);
                cache.Remove(oldestKey);
            }
        }

        private static void DestroyEntryTexture(CacheEntry entry)
        {
            if (entry == null || entry.texture == null)
            {
                return;
            }

            if (entry.texture.IsCreated())
            {
                entry.texture.Release();
            }

            UnityEngine.Object.Destroy(entry.texture);
            entry.texture = null;
        }
    }

    [HarmonyPatch(typeof(Widgets), nameof(Widgets.DrawAtlas), new Type[] { typeof(Rect), typeof(Texture2D), typeof(bool) })]
    public static class Widgets_DrawAtlas_Optimization_Patch
    {
        public static bool Prepare()
        {
            return ModSettings.enableAtlasOptimizationPatch;
        }

        public static bool Prefix(ref Rect rect, Texture2D atlas, bool drawTop)
        {
            if (!ModSettings.experimentalMainButtonsAtlasOptimization)
            {
                return true;
            }

            if (!MainButtonsDrawAtlasOptimizationScope.Active)
            {
                return true;
            }

            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return false;
            }

            return !MainButtonsAtlasTextureCache.TryDrawCached(rect, atlas, drawTop);
        }
    }
}
