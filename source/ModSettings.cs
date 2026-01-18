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
        public static bool useFixedWidthMode = false;
        public static float fixedButtonWidth = 120f;

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

            Scribe_Values.Look(ref useFixedWidthMode, "useFixedWidthMode", false);
            Scribe_Values.Look(ref fixedButtonWidth, "fixedButtonWidth", 120f);
            fixedButtonWidth = Mathf.Clamp(fixedButtonWidth, 50f, 200f);
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
    }
}
