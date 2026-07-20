using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public class MainButtonDropdownConfig : IExposable
    {
        public MainButtonDef parent;
        public List<MainButtonDef> entries = new List<MainButtonDef>();

        private string parentName;
        private List<string> entryNames = new List<string>();

        public bool ResolvedWithDroppedData { get; private set; }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                parentName = parent != null ? parent.defName : null;
                entryNames = entries != null
                    ? entries.Where(entry => entry != null && !string.IsNullOrEmpty(entry.defName))
                        .Select(entry => entry.defName)
                        .ToList()
                    : new List<string>();
            }

            Scribe_Values.Look(ref parentName, "parent");
            Scribe_Collections.Look(ref entryNames, "entries", LookMode.Value);
            if (entryNames == null)
            {
                entryNames = new List<string>();
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                bool dropped = false;

                parent = ModSettings.ResolveMainButtonDef(parentName);
                if (parent == null && !string.IsNullOrWhiteSpace(parentName))
                {
                    dropped = true;
                }

                List<MainButtonDef> resolvedEntries = new List<MainButtonDef>();
                HashSet<MainButtonDef> seen = new HashSet<MainButtonDef>();
                for (int i = 0; i < entryNames.Count; i++)
                {
                    MainButtonDef entry = ModSettings.ResolveMainButtonDef(entryNames[i]);
                    if (entry == null)
                    {
                        dropped = true;
                        continue;
                    }

                    if (seen.Add(entry))
                    {
                        resolvedEntries.Add(entry);
                    }
                    else
                    {
                        dropped = true;
                    }
                }

                entries = resolvedEntries;
                ResolvedWithDroppedData = dropped;
            }
        }
    }
}
