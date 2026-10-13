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

        // Explicit render order for this dropdown's entries. Distinct from `entries`
        // (membership) so a legacy config with no saved order can be told apart from one
        // whose order was intentionally set once already.
        public List<MainButtonDef> entryOrder = new List<MainButtonDef>();

        private string parentName;
        private List<string> entryNames = new List<string>();
        private List<string> entryOrderNames = new List<string>();

        // Defaults true so a dropdown created and reordered this session (never round-tripped
        // through Scribe loading) saves its live order as-is. A legacy save with no such node
        // makes Scribe_Values.Look overwrite this to false before PostLoadInit runs, which is
        // what marks it for stable-order migration below.
        private bool hasExplicitEntryOrder = true;

        public bool RequiresSettingsRewrite { get; private set; }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                parentName = parent != null ? parent.defName : null;
                entryNames = ProjectDefNames(entries);
                entryOrderNames = ProjectDefNames(entryOrder);
            }

            Scribe_Values.Look(ref parentName, "parent");
            Scribe_Collections.Look(ref entryNames, "entries", LookMode.Value);
            if (entryNames == null)
            {
                entryNames = new List<string>();
            }

            Scribe_Values.Look(ref hasExplicitEntryOrder, "hasExplicitEntryOrder", false);
            Scribe_Collections.Look(ref entryOrderNames, "entryOrder", LookMode.Value);
            if (entryOrderNames == null)
            {
                entryOrderNames = new List<string>();
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
                HashSet<MainButtonDef> seenEntries = new HashSet<MainButtonDef>();
                for (int i = 0; i < entryNames.Count; i++)
                {
                    MainButtonDef entry = ModSettings.ResolveMainButtonDef(entryNames[i]);
                    if (entry == null || entry == parent)
                    {
                        dropped = true;
                        continue;
                    }

                    if (seenEntries.Add(entry))
                    {
                        resolvedEntries.Add(entry);
                    }
                    else
                    {
                        dropped = true;
                    }
                }

                entries = resolvedEntries;

                List<MainButtonDef> normalizedOrder;
                if (hasExplicitEntryOrder)
                {
                    normalizedOrder = new List<MainButtonDef>();
                    HashSet<MainButtonDef> seenOrder = new HashSet<MainButtonDef>();
                    for (int i = 0; i < entryOrderNames.Count; i++)
                    {
                        MainButtonDef orderedEntry = ModSettings.ResolveMainButtonDef(entryOrderNames[i]);
                        if (orderedEntry == null || orderedEntry == parent || !seenEntries.Contains(orderedEntry) || !seenOrder.Add(orderedEntry))
                        {
                            dropped = true;
                            continue;
                        }

                        normalizedOrder.Add(orderedEntry);
                    }

                    if (normalizedOrder.Count != resolvedEntries.Count)
                    {
                        dropped = true;
                        List<MainButtonDef> missing = resolvedEntries.Where(entry => !seenOrder.Contains(entry)).ToList();
                        normalizedOrder.AddRange(ModSettings.GetStableDefaultDropdownOrder(missing));
                    }
                }
                else
                {
                    // Legacy config saved before dropdown ordering existed: derive a
                    // deterministic starting order instead of relying on save-file sequence.
                    dropped = true;
                    normalizedOrder = ModSettings.GetStableDefaultDropdownOrder(resolvedEntries);
                }

                entryOrder = normalizedOrder;
                hasExplicitEntryOrder = true;
                RequiresSettingsRewrite = dropped;
            }
        }

        private static List<string> ProjectDefNames(List<MainButtonDef> defs)
        {
            return defs != null
                ? defs.Where(def => def != null && !string.IsNullOrEmpty(def.defName))
                    .Select(def => def.defName)
                    .ToList()
                : new List<string>();
        }
    }
}
