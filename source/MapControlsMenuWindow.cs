using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public class MapControlsTableWindow : Window
    {
        public static bool IsOpen;
        private const float RowHeight = 56f;
        private const float SearchHeight = 26f;
        private static readonly Color PanelBg = new Color(0.11f, 0.11f, 0.11f, 1f);
        private readonly bool worldView;

        private Vector2 scrollPosition = Vector2.zero;
        private static string searchText = string.Empty;
        private bool triedToFocus;
        private int openFrames;

        public override Vector2 InitialSize
        {
            get
            {
                float height = Mathf.Min(480f, UI.screenHeight - 35f);
                return new Vector2(420f, height);
            }
        }

        public MapControlsTableWindow(bool worldView)
        {
            this.worldView = worldView;
            draggable = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            closeOnCancel = false;
            optionalTitle = "DMMB.PlaySettingsMenuTitle".Translate();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            IsOpen = true;
            triedToFocus = false;
            openFrames = 0;
        }

        public override void PostClose()
        {
            base.PostClose();
            IsOpen = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            if (!triedToFocus && openFrames == 2)
            {
                UI.FocusControl("DMMB_MapControlsSearch", this);
                triedToFocus = true;
            }

            Rect topBarRect = new Rect(0f, 0f, inRect.width, SearchHeight + 4f);
            Widgets.DrawBoxSolid(topBarRect, PanelBg);

            Rect searchRect = new Rect(6f, 4f, inRect.width - 12f, SearchHeight);
            Rect listRect = new Rect(0f, topBarRect.height, inRect.width, inRect.height - topBarRect.height);
            Widgets.DrawBoxSolid(listRect, PanelBg);

            GUI.SetNextControlName("DMMB_MapControlsSearch");
            searchText = Widgets.TextField(searchRect, searchText ?? string.Empty);

            MapControlsTableContext.BeginMeasure(searchText, RowHeight, listRect.width - 16f);
            DrawMapControls();
            AddEditToggleRow(measureOnly: true);
            int totalRows = MapControlsTableContext.TotalRows;
            float measuredRowHeight = MapControlsTableContext.MaxRowHeight;
            MapControlsTableContext.End();

            float viewHeight = totalRows * measuredRowHeight;
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, Mathf.Max(viewHeight, listRect.height));

            Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);

            MapControlsTableContext.BeginRender(searchText, measuredRowHeight, viewRect.width);
            DrawMapControls();
            AddEditToggleRow(measureOnly: false);
            MapControlsTableContext.End();

            Widgets.EndScrollView();
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();
            openFrames++;
        }

        public override void Notify_ClickOutsideWindow()
        {
            base.Notify_ClickOutsideWindow();
        }

        private void DrawMapControls()
        {
            PlaySettings settings = Find.PlaySettings;
            if (settings == null)
            {
                return;
            }

            var row = new WidgetRow(0f, 0f, UIDirection.RightThenDown, 9999f);
            settings.DoPlaySettingsGlobalControls(row, worldView: worldView);
        }

        private static void AddEditToggleRow(bool measureOnly)
        {
            string tooltip = "DMMB.PlaySettingsEditDropdowns".Translate();
            if (!MapControlsTableContext.MatchesFilter(tooltip))
            {
                return;
            }

            if (measureOnly)
            {
                MapControlsTableRenderer.MeasureRowHeight(tooltip, true);
                MapControlsTableContext.TotalRows++;
                return;
            }

            bool editMode = ModSettings.editDropdownsMode;
            MapControlsTableRenderer.DrawCustomToggleRow(ref editMode, DMMBTextures.UiToggle.Texture, tooltip);
            if (editMode != ModSettings.editDropdownsMode)
            {
                ModSettings.editDropdownsMode = editMode;
                if (!editMode)
                {
                    MainButtonsRoot_DoButtons_Patch.ClearDropdownState();
                    Mod.Settings.Write();
                }
            }
        }

        public static string GetDisplayLabel(string tooltip)
        {
            if (string.IsNullOrEmpty(tooltip))
            {
                return "DMMB.PlaySettingsMenuUnknown".Translate();
            }

            int splitIndex = tooltip.LastIndexOf("\n\n", System.StringComparison.Ordinal);
            if (splitIndex >= 0 && splitIndex + 2 < tooltip.Length)
            {
                return tooltip.Substring(splitIndex + 2).Trim();
            }

            return tooltip.Trim();
        }
    }
}
