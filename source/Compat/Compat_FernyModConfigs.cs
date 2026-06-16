using System;
using System.IO;
using System.Xml;
using Verse;

namespace Declutter_Main_Buttons_Bar.Compat
{
    public static class Compat_FernyModConfigs
    {
        private const string FernyPackageId = "ferny.modconfigs";
        private const string DeclutterClassAttr = "Declutter_Main_Buttons_Bar.ModSettings";
        private const string DeclutterWorkshopId = "3649094165";

        public static bool IsEnabled()
        {
            return ModsConfig.IsActive(FernyPackageId);
        }

        public static bool TryApplyAfterGetSettings(ModSettings settings)
        {
            try
            {
                if (!ModSettings.autoLoadFernyModConfigs)
                {
                    return false;
                }

                ModContentPack fernyPack = FindFernyPack();
                if (fernyPack == null)
                {
                    return false;
                }

                string settingsFile = FindDeclutterSettingsFile(fernyPack.RootDir);
                if (settingsFile == null)
                {
                    Log.Warning("[DeclutterMainButtonsBar] Ferny's Mod Configs is active but no Declutter UI settings file was found in its Settings folder.");
                    return false;
                }

                PresetMeta preset = SelectBestPreset(fernyPack.RootDir);

                // Save the opt-out flag — Ferny's preset won't have it so Scribe would reset it to true,
                // but we preserve it explicitly in case a future preset version includes the field.
                bool savedOptOut = ModSettings.autoLoadFernyModConfigs;

                // Use RimWorld's own Scribe machinery to deserialize the preset into the static fields.
                // This mirrors LoadedModManager.ReadModSettings exactly, and the PostLoadInit pass
                // in FinalizeLoading runs ExposeData's cleanup/invariant block automatically.
                ModSettings loaded = null;
                Scribe.loader.InitLoading(settingsFile);
                try
                {
                    Scribe_Deep.Look(ref loaded, "ModSettings");
                }
                finally
                {
                    Scribe.loader.FinalizeLoading();
                }

                ModSettings.autoLoadFernyModConfigs = savedOptOut;

                if (preset != null)
                {
                    ModSettings.lastFernyPresetName = preset.DefName;
                    ModSettings.lastFernyPresetVersion = preset.Version;
                }
                ModSettings.lastFernyPresetAppliedUtc = DateTime.UtcNow.ToString("o");

                MainButtonsRoot_DoButtons_Patch.InvalidateOrderedVisibleCache();
                MainButtonsRoot_DoButtons_Patch.ClearDropdownState();
                MainButtonsAtlasTextureCache.ClearCache();

                Logger.Message("Applied Ferny's Mod Configs preset: " + (preset?.DefName ?? "?") + " v" + (preset?.Version ?? "?"));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "Ferny's Mod Configs compatibility failed");
                return false;
            }
        }

        private static ModContentPack FindFernyPack()
        {
            foreach (ModContentPack mod in LoadedModManager.RunningMods)
            {
                if (string.Equals(mod.PackageId, FernyPackageId, StringComparison.OrdinalIgnoreCase))
                {
                    return mod;
                }
            }
            return null;
        }

        private class PresetMeta
        {
            public string DefName;
            public string Version;
        }

        private static PresetMeta SelectBestPreset(string fernyRoot)
        {
            string defsDir = Path.Combine(fernyRoot, "Defs");
            if (!Directory.Exists(defsDir))
            {
                return null;
            }

            string[] xmlFiles = Directory.GetFiles(defsDir, "*.xml", SearchOption.AllDirectories);
            PresetMeta best = null;
            Version bestVersion = null;

            foreach (string file in xmlFiles)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);
                    foreach (XmlNode node in doc.GetElementsByTagName("ModlistConfigurator.ModlistPresetDef"))
                    {
                        XmlElement el = node as XmlElement;
                        if (el == null) continue;

                        PresetMeta meta = new PresetMeta
                        {
                            DefName = el["defName"]?.InnerText?.Trim(),
                            Version = el["version"]?.InnerText?.Trim()
                        };

                        if (best == null)
                        {
                            best = meta;
                            Version.TryParse(meta.Version, out bestVersion);
                            continue;
                        }

                        bool parsedCandidate = Version.TryParse(meta.Version, out Version candidateVersion);
                        bool parsedBest = bestVersion != null;

                        if (parsedCandidate && parsedBest)
                        {
                            if (candidateVersion > bestVersion)
                            {
                                best = meta;
                                bestVersion = candidateVersion;
                            }
                        }
                        else if (parsedCandidate)
                        {
                            best = meta;
                            bestVersion = candidateVersion;
                        }
                        else if (!parsedBest)
                        {
                            if (!string.IsNullOrEmpty(meta.Version) && string.IsNullOrEmpty(best.Version))
                            {
                                best = meta;
                            }
                            else if (!string.IsNullOrEmpty(meta.Version) && !string.IsNullOrEmpty(best.Version)
                                && string.Compare(meta.Version, best.Version, StringComparison.Ordinal) > 0)
                            {
                                best = meta;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "Failed to parse Ferny preset def: " + file);
                }
            }

            return best;
        }

        private static string FindDeclutterSettingsFile(string fernyRoot)
        {
            string settingsDir = Path.Combine(fernyRoot, "Settings");
            if (!Directory.Exists(settingsDir))
            {
                return null;
            }

            string preferred = null;
            string fallbackByPackageId = null;
            string fallbackFirst = null;

            foreach (string file in Directory.GetFiles(settingsDir, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);
                    if (doc.SelectNodes("//ModSettings[@Class='" + DeclutterClassAttr + "']")?.Count == 0)
                    {
                        continue;
                    }

                    string fileName = Path.GetFileName(file);
                    if (fileName.Contains(DeclutterWorkshopId))
                    {
                        if (preferred == null) preferred = file;
                    }
                    else if (fileName.Contains("sk.dmbb"))
                    {
                        if (fallbackByPackageId == null) fallbackByPackageId = file;
                    }
                    else if (fallbackFirst == null)
                    {
                        fallbackFirst = file;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "Failed to parse Ferny settings file: " + file);
                }
            }

            return preferred ?? fallbackByPackageId ?? fallbackFirst;
        }
    }
}
