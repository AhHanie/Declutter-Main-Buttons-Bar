using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;

namespace Declutter_Main_Buttons_Bar
{
    public static class Compat_OmniTab_RightAlign
    {
        public const string OmniTabPackageId = "scheigster.omnitab";
        private const string OmniTabLayoutTypeName = "OmniTab.UI.Windows.OmniTab.OmniTabWindowLayout";
        private const string OmniTabWindowTypeName = "OmniTab.UI.Windows.OmniTabWindow";
        private const string OmniTabWindowTypeNameFallback = "OmniTabWindow";

        private static readonly Type OmniTabLayoutType = AccessTools.TypeByName(OmniTabLayoutTypeName);

        private static FieldInfo cachedStateField;
        private static FieldInfo cachedIsCompactModeField;
        private static FieldInfo cachedWindowChromeWidthField;
        private static PropertyInfo cachedMarginProperty;

        private static MethodInfo cachedCalculateWindowWidthMethod;


        public static bool TryAlign(MainTabWindow window)
        {
            if (!IsOmniTabWindow(window))
            {
                return false;
            }

            Rect rect = window.windowRect;

            // OmniTab updates compact mode during width calculation. Use that state to decide if we
            // should pre-expand width (non-compact) before pinning so it stays flush-right.
            FieldInfo stateField = GetStateField(window.GetType());
            object state = stateField.GetValue(window);
            float calculatedWidth = TryCalculateWindowWidth(state);

            if (calculatedWidth > 0f && TryGetIsCompactMode(state, out bool isCompactMode) && !isCompactMode)
            {
                float targetWidth = calculatedWidth;
                if (TryGetWindowChromeWidth(state, out float windowChromeWidth) && windowChromeWidth <= 0f)
                {
                    if (TryGetCalculatedWindowChromeWidth(window, out float calculatedChromeWidth))
                    {
                        targetWidth = Mathf.Min(targetWidth + calculatedChromeWidth, UI.screenWidth - 40f);
                    }
                }

                rect.width = Mathf.Max(rect.width, targetWidth);
            }
            rect.x = Mathf.Max(0f, UI.screenWidth - rect.width);
            window.windowRect = rect;
            return true;
        }

        private static float TryCalculateWindowWidth(object state)
        {
            MethodInfo calculateWidthMethod = GetCalculateWindowWidthMethod(state.GetType());

            object value = calculateWidthMethod.Invoke(null, new[] { state });

            return Mathf.Min((float)value, UI.screenWidth - 40f);
        }

        private static bool TryGetIsCompactMode(object instance, out bool value)
        {
            value = false;

            Type stateType = instance.GetType();
            EnsureCompactModeAccessors(stateType);

            value = (bool)cachedIsCompactModeField.GetValue(instance);
            return true;
        }

        private static bool TryGetWindowChromeWidth(object instance, out float value)
        {
            value = 0f;
            Type stateType = instance.GetType();
            EnsureWindowChromeWidthAccessor(stateType);
            if (cachedWindowChromeWidthField == null)
            {
                return false;
            }

            object raw = cachedWindowChromeWidthField.GetValue(instance);
            if (!(raw is float chromeWidth))
            {
                return false;
            }

            value = chromeWidth;
            return true;
        }

        private static bool TryGetCalculatedWindowChromeWidth(MainTabWindow window, out float value)
        {
            value = 0f;
            EnsureMarginAccessor(window.GetType());
            if (cachedMarginProperty == null)
            {
                return false;
            }

            object raw = cachedMarginProperty.GetValue(window, null);
            if (!(raw is float margin))
            {
                return false;
            }

            value = Mathf.Max(0f, margin * 2f);
            return value > 0f;
        }

        private static FieldInfo GetStateField(Type windowType)
        {
            if (cachedStateField == null)
            {
                cachedStateField = AccessTools.Field(windowType, "_state");
            }

            return cachedStateField;
        }

        private static MethodInfo GetCalculateWindowWidthMethod(Type stateType)
        {
            if (cachedCalculateWindowWidthMethod == null)
            {
                cachedCalculateWindowWidthMethod = AccessTools.Method(
                    OmniTabLayoutType,
                    "CalculateWindowWidth",
                    new[] { stateType });
            }

            return cachedCalculateWindowWidthMethod;
        }

        private static void EnsureCompactModeAccessors(Type stateType)
        {
            if (cachedIsCompactModeField != null)
            {
                return;
            }

            cachedIsCompactModeField = AccessTools.Field(stateType, "IsCompactMode");
        }

        private static void EnsureWindowChromeWidthAccessor(Type stateType)
        {
            if (cachedWindowChromeWidthField != null)
            {
                return;
            }

            cachedWindowChromeWidthField = AccessTools.Field(stateType, "WindowChromeWidth");
        }

        private static void EnsureMarginAccessor(Type windowType)
        {
            if (cachedMarginProperty != null)
            {
                return;
            }

            cachedMarginProperty = AccessTools.Property(windowType, "Margin");
        }

        private static bool IsOmniTabWindow(MainTabWindow window)
        {
            if (!ModsConfig.IsActive(OmniTabPackageId))
            {
                return false;
            }

            Type windowType = window.GetType();

            string fullName = windowType.FullName;
            if (fullName == OmniTabWindowTypeName || fullName == OmniTabWindowTypeNameFallback)
            {
                return true;
            }

            return windowType.Name == OmniTabWindowTypeNameFallback;
        }
    }

    [HarmonyPatch("OmniTab.UI.Windows.OmniTab.OmniTabWindowActions", "ToggleCompareMode")]
    public static class Compat_OmniTab_ToggleCompareMode_RightAlign_Patch
    {
        static bool Prepare()
        {
            return ModsConfig.IsActive(Compat_OmniTab_RightAlign.OmniTabPackageId);
        }

        static void Postfix()
        {
            if (!ModSettings.pinMainButtonsMenuWindowRight)
            {
                return;
            }

            MainTabWindow window = Find.WindowStack.WindowOfType<MainTabWindow>();

            if (window.GetType() != MainTabWindow_RightAlign_Eligibility.lastWindowOpenedFromMenuType)
            {
                return;
            }

            Compat_OmniTab_RightAlign.TryAlign(window);
        }
    }
}
