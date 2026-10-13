using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static class TimeSpeedWidgetRenderer
    {
        private static Action<Rect> cachedTimeControlsDrawer = DrawDefaultTimeControls;
        private static float extraPreferredWidth;

        public static void SetSmartSpeedMode()
        {
            cachedTimeControlsDrawer = DrawSmartSpeedTimeControls;
            extraPreferredWidth = 8f;
        }

        public static float GetPreferredWidth()
        {
            return 140f + extraPreferredWidth;
        }

        public static void Draw(Rect rect)
        {
            WidgetRenderUtility.DrawBackground(rect);
            Rect inner = rect.ContractedBy(4f);
            inner.y = rect.center.y - 12f;
            inner.height = 24f;
            EnsureTimeControlsDrawer();
            cachedTimeControlsDrawer(inner);
        }

        private static void EnsureTimeControlsDrawer()
        {
            if (cachedTimeControlsDrawer == null)
            {
                cachedTimeControlsDrawer = DrawDefaultTimeControls;
            }
        }

        private static void DrawDefaultTimeControls(Rect timerRect)
        {
            TimeControls.DoTimeControlsGUI(timerRect);
        }

        private static void DrawSmartSpeedTimeControls(Rect timerRect)
        {
            timerRect.x += 14f;
            TimeControls.DoTimeControlsGUI(timerRect);
        }
    }
}
