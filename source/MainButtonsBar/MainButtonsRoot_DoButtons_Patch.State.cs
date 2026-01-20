using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public static partial class MainButtonsRoot_DoButtons_Patch
    {
        private const float BarHeight = 36f;
        private const float BarBottomOffset = 35f;
        private const float DropdownRowHeight = BarHeight;
        private const float EditPlusSize = 16f;
        private const float ResizeHandleSize = 12f;
        private const float MinFreeSizeWidth = 16f;

        private static MainButtonDef openDropdownDef;
        private static Rect openDropdownButtonRect;
        private static Rect openDropdownRect;

        private static MainButtonDef draggingDef;
        private static float dragOffsetX;
        private static MainButtonDef resizingDef;
        private static float resizeStartWidth;
        private static float resizeStartMouseX;
        private static List<MainButtonDef> currentDragOrder = new List<MainButtonDef>();

        private static bool gizmoDragActive;
        private static Vector2 gizmoDragStartMouse;
        private static Vector2 gizmoDragStartOffset;
        private static Rect lastPreviewGizmoRect;
        private static List<Gizmo> previewGizmos = new List<Gizmo>();

        // Performance: Cache for GetOrderedVisibleDefs
        private static List<MainButtonDef> cachedOrderedVisible;
        private static List<MainButtonDef> cachedOrderedVisibleNoPinnedMenu;
        private static int lastVisibleCheckFrame = -1;

        public static void ClearDropdownState()
        {
            openDropdownDef = null;
            openDropdownButtonRect = default;
            openDropdownRect = default;
        }

        public static void InvalidateOrderedVisibleCache()
        {
            cachedOrderedVisible = null;
            cachedOrderedVisibleNoPinnedMenu = null;
            lastVisibleCheckFrame = -1;
        }
    }
}
