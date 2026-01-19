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
        public static bool useFixedWidthMode = false;
        public static float fixedButtonWidth = 120f;
        public static bool centerFixedWidthButtons = false;
        public static bool pinMenuButtonRight = false;
        public static bool drawGizmosAtBottom = false;
        public static float gizmoBottomOffset = 35f;

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

            Scribe_Values.Look(ref useFixedWidthMode, "useFixedWidthMode", false);
            Scribe_Values.Look(ref fixedButtonWidth, "fixedButtonWidth", 120f);
            Scribe_Values.Look(ref centerFixedWidthButtons, "centerFixedWidthButtons", false);
            Scribe_Values.Look(ref pinMenuButtonRight, "pinMenuButtonRight", false);
            Scribe_Values.Look(ref drawGizmosAtBottom, "drawGizmosAtBottom", false);
            Scribe_Values.Look(ref gizmoBottomOffset, "gizmoBottomOffset", 35f);
            fixedButtonWidth = Mathf.Clamp(fixedButtonWidth, 50f, 200f);
            gizmoBottomOffset = Mathf.Clamp(gizmoBottomOffset, 0f, 120f);
        }

        public static bool IsHiddenFromBar(MainButtonDef def)
        {
            return hiddenFromBarDefs.Contains(def);
        }

        public static void SetHiddenFromBar(MainButtonDef def, bool hidden)
        {
            if (hidden)
            {
                if (!hiddenFromBarDefs.Contains(def))
                {
                    hiddenFromBarDefs.Add(def);
                }
            }
            else
            {
                hiddenFromBarDefs.Remove(def);
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
    }
}
