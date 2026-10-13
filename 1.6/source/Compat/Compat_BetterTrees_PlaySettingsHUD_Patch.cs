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
    public static class Compat_BetterTrees_PlaySettingsHUD_Patch
    {
        private const string BetterTreesPackageId = "chaoticenrico.bettertrees";
        private const string PatchTypeName = "BetterTrees.PlaySettings_HUD_Patch";
        private const string ModTypeName = "BetterTrees.BetterTreesMod";
        private const string SettingsFieldName = "settings";
        private const string TransparencyEnabledFieldName = "transparencyEnabled";
        private const string TransparencyControllerTypeName = "BetterTrees.TransparencyController";
        private const string ToggleIconFieldName = "ToggleIcon";
        private const string ToggleTransparencyMethodName = "ToggleTransparency";
        private const string KeyBindingDefOfTypeName = "BetterTrees.KeyBindingDefOf";
        private const string ToggleTransparentTreesFieldName = "ToggleTransparentTrees";
        private const string FallbackTooltip = "Makes trees near your cursor transparent.\n\nLeft-click to toggle.\nRight-click for transparency settings.";

        private static bool resolutionAttempted;
        private static bool resolutionAvailable;

        private static Func<object> settingsGetter;
        private static Func<object, bool> transparencyEnabledGetter;
        private static Texture2D toggleIcon;
        private static Action toggleTransparency;
        private static KeyBindingDef toggleTransparentTreesKey;

        static bool Prepare()
        {
            return ModsConfig.IsActive(BetterTreesPackageId);
        }

        static MethodBase TargetMethod()
        {
            Type type = AccessTools.TypeByName(PatchTypeName);
            return type == null ? null : AccessTools.Method(type, "Postfix");
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

            if (!TryResolve())
            {
                return false;
            }

            object settingsInstance = settingsGetter();
            if (settingsInstance == null)
            {
                // Better Trees hasn't finished initializing its settings yet; skip this frame's row only.
                return false;
            }

            bool transparencyEnabled = transparencyEnabledGetter(settingsInstance);
            string tooltip = BuildTooltip();

            if (!MapControlsTableContext.MatchesFilter(tooltip))
            {
                return false;
            }

            if (MapControlsTableContext.Measuring)
            {
                MapControlsTableRenderer.MeasureRowHeight(tooltip, true);
                MapControlsTableContext.TotalRows++;
                return false;
            }

            bool newValue = transparencyEnabled;
            MapControlsTableRenderer.DrawCustomToggleRow(ref newValue, toggleIcon, tooltip);
            if (newValue != transparencyEnabled)
            {
                toggleTransparency();
            }

            return false;
        }

        private static string BuildTooltip()
        {
            string hotkeyLabel = toggleTransparentTreesKey != null ? toggleTransparentTreesKey.MainKeyLabel : null;
            if (string.IsNullOrEmpty(hotkeyLabel))
            {
                return FallbackTooltip;
            }

            return "Makes trees near your cursor transparent.\n\nLeft-click or " + hotkeyLabel + " to toggle.\nRight-click for transparency settings.";
        }

        private static bool TryResolve()
        {
            if (resolutionAttempted)
            {
                return resolutionAvailable;
            }

            resolutionAttempted = true;

            try
            {
                Type modType = AccessTools.TypeByName(ModTypeName);
                FieldInfo settingsField = modType == null ? null : AccessTools.Field(modType, SettingsFieldName);
                if (settingsField == null || !settingsField.IsStatic)
                {
                    Log.Warning("[DeclutterMainButtonsBar] BetterTrees compat: could not resolve BetterTreesMod.settings field. Tree-transparency row will not be shown.");
                    return false;
                }

                FieldInfo transparencyEnabledField = AccessTools.Field(settingsField.FieldType, TransparencyEnabledFieldName);
                if (transparencyEnabledField == null || transparencyEnabledField.IsStatic || transparencyEnabledField.FieldType != typeof(bool))
                {
                    Log.Warning("[DeclutterMainButtonsBar] BetterTrees compat: could not resolve settings.transparencyEnabled field. Tree-transparency row will not be shown.");
                    return false;
                }

                Type controllerType = AccessTools.TypeByName(TransparencyControllerTypeName);
                FieldInfo toggleIconField = controllerType == null ? null : AccessTools.Field(controllerType, ToggleIconFieldName);
                if (toggleIconField == null || !toggleIconField.IsStatic || !typeof(Texture2D).IsAssignableFrom(toggleIconField.FieldType))
                {
                    Log.Warning("[DeclutterMainButtonsBar] BetterTrees compat: could not resolve TransparencyController.ToggleIcon field. Tree-transparency row will not be shown.");
                    return false;
                }

                MethodInfo toggleTransparencyMethod = controllerType == null ? null : AccessTools.Method(controllerType, ToggleTransparencyMethodName, Type.EmptyTypes);
                if (toggleTransparencyMethod == null)
                {
                    Log.Warning("[DeclutterMainButtonsBar] BetterTrees compat: could not resolve TransparencyController.ToggleTransparency method. Tree-transparency row will not be shown.");
                    return false;
                }

                if (!TryBuildStaticFieldGetter<object>(settingsField, "settings", out settingsGetter))
                {
                    return false;
                }

                if (!TryBuildInstanceFieldGetter<bool>(transparencyEnabledField, "transparencyEnabled", out transparencyEnabledGetter))
                {
                    return false;
                }

                toggleIcon = toggleIconField.GetValue(null) as Texture2D;
                toggleTransparency = (Action)Delegate.CreateDelegate(typeof(Action), toggleTransparencyMethod);

                Type keyBindingDefOfType = AccessTools.TypeByName(KeyBindingDefOfTypeName);
                FieldInfo keyField = keyBindingDefOfType == null ? null : AccessTools.Field(keyBindingDefOfType, ToggleTransparentTreesFieldName);
                if (keyField != null && keyField.IsStatic && typeof(KeyBindingDef).IsAssignableFrom(keyField.FieldType))
                {
                    toggleTransparentTreesKey = keyField.GetValue(null) as KeyBindingDef;
                }

                resolutionAvailable = true;
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning("[DeclutterMainButtonsBar] BetterTrees compat: failed to resolve API members, tree-transparency row will not be shown. " + ex);
                resolutionAvailable = false;
                return false;
            }
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
                Logger.Exception(ex, "BetterTrees compat: failed to build " + debugName + " field accessor");
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
                Logger.Exception(ex, "BetterTrees compat: failed to build " + debugName + " field accessor");
                getter = null;
                return false;
            }
        }
    }
}
