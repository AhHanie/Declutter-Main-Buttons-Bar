using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch]
    [StaticConstructorOnStartup]
    public static class Compat_QualityJobs_PlaySettings_Patch
    {
        private const string PackageId = "EPrime.QualityJobs";
        private const string PatchTypeName = "QualityJobs.Patches.Patch_PlaySettings";
        private const string PostfixMethodName = "Postfix";
        private const string ModTypeName = "QualityJobs.QualityJobsMod";
        private const string SettingsFieldName = "Settings";
        private const string ShowToolbarButtonFieldName = "showToolbarButton";
        private const string InstanceFieldName = "Instance";
        private const string TooltipTranslationKey = "QJ_ToolbarButtonTip";
        private const string IconPath = "QualityJobs/ToolbarButton";

        private static Type modType;

        // Compiled once from the reflected FieldInfo, then invoked like a normal
        // delegate on every render call — avoids FieldInfo.GetValue reflection
        // and boxing on this render-hot path.
        private static Func<object> settingsGetter;
        private static Func<object, bool> showToolbarButtonGetter;
        private static Func<Mod> instanceGetter;

        // Loaded via the static constructor (StaticConstructorOnStartup) rather than
        // lazily, since Texture2D/asset loading must happen on the main thread.
        private static readonly Texture2D icon = ContentFinder<Texture2D>.Get(IconPath, false);

        static bool Prepare()
        {
            return ModsConfig.IsActive(PackageId);
        }

        static MethodBase TargetMethod()
        {
            Type patchType = AccessTools.TypeByName(PatchTypeName);
            if (patchType == null)
            {
                return null;
            }

            return AccessTools.Method(patchType, PostfixMethodName, new[] { typeof(WidgetRow), typeof(bool) });
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(WidgetRow row, bool worldView)
        {
            if (!MapControlsTableContext.Active)
            {
                return !MapControlsTableContext.SuppressExternal;
            }

            if (worldView)
            {
                return false;
            }

            if (!TryGetShowToolbarButton(out bool enabled) || !enabled)
            {
                return false;
            }

            if (!TryGetIcon(out Texture2D tex))
            {
                return false;
            }

            string tooltip = TooltipTranslationKey.Translate();
            if (!MapControlsTableContext.MatchesFilter(tooltip))
            {
                return false;
            }

            if (MapControlsTableContext.Measuring)
            {
                MapControlsTableRenderer.MeasureRowHeight(tooltip, false);
                MapControlsTableContext.TotalRows++;
                return false;
            }

            if (MapControlsTableRenderer.DrawButtonRow(tex, tooltip) && TryGetInstanceMod(out Mod mod))
            {
                Find.WindowStack.Add(new Dialog_ModSettings(mod));
            }

            return false;
        }

        private static bool TryResolveModType()
        {
            if (modType != null)
            {
                return true;
            }

            modType = AccessTools.TypeByName(ModTypeName);
            return modType != null;
        }

        private static bool TryGetSettingsGetter(out Func<object> getter)
        {
            if (settingsGetter != null)
            {
                getter = settingsGetter;
                return true;
            }

            getter = null;
            if (!TryResolveModType())
            {
                return false;
            }

            FieldInfo field = AccessTools.Field(modType, SettingsFieldName);
            if (field == null || !field.IsStatic)
            {
                return false;
            }

            if (!TryBuildStaticFieldGetter<object>(field, "Settings", out settingsGetter))
            {
                return false;
            }

            getter = settingsGetter;
            return true;
        }

        private static bool TryGetShowToolbarButton(out bool enabled)
        {
            enabled = false;

            if (!TryGetSettingsGetter(out Func<object> getSettings))
            {
                return false;
            }

            object settings = getSettings();
            if (settings == null)
            {
                return false;
            }

            if (showToolbarButtonGetter == null)
            {
                FieldInfo field = AccessTools.Field(settings.GetType(), ShowToolbarButtonFieldName);
                if (field == null || field.IsStatic || field.FieldType != typeof(bool))
                {
                    return false;
                }

                if (!TryBuildInstanceFieldGetter<bool>(field, "showToolbarButton", out showToolbarButtonGetter))
                {
                    return false;
                }
            }

            enabled = showToolbarButtonGetter(settings);
            return true;
        }

        private static bool TryGetInstanceMod(out Mod mod)
        {
            mod = null;

            if (instanceGetter == null)
            {
                if (!TryResolveModType())
                {
                    return false;
                }

                FieldInfo field = AccessTools.Field(modType, InstanceFieldName);
                if (field == null || !field.IsStatic || !typeof(Mod).IsAssignableFrom(field.FieldType))
                {
                    return false;
                }

                if (!TryBuildStaticFieldGetter<Mod>(field, "Instance", out instanceGetter))
                {
                    return false;
                }
            }

            mod = instanceGetter();
            return mod != null;
        }

        private static bool TryGetIcon(out Texture2D texture)
        {
            texture = icon;
            return icon != null;
        }

        private static bool TryBuildStaticFieldGetter<TResult>(FieldInfo field, string debugName, out Func<TResult> getter)
            where TResult : class
        {
            try
            {
                MemberExpression fieldAccess = Expression.Field(null, field);
                UnaryExpression converted = Expression.Convert(fieldAccess, typeof(TResult));
                getter = Expression.Lambda<Func<TResult>>(converted).Compile();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "QualityJobs compat: failed to build " + debugName + " field accessor");
                getter = null;
                return false;
            }
        }

        private static bool TryBuildInstanceFieldGetter<TField>(FieldInfo field, string debugName, out Func<object, TField> getter)
        {
            try
            {
                ParameterExpression instanceParam = Expression.Parameter(typeof(object), "instance");
                UnaryExpression castInstance = Expression.Convert(instanceParam, field.DeclaringType);
                MemberExpression fieldAccess = Expression.Field(castInstance, field);
                getter = Expression.Lambda<Func<object, TField>>(fieldAccess, instanceParam).Compile();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "QualityJobs compat: failed to build " + debugName + " field accessor");
                getter = null;
                return false;
            }
        }
    }
}
