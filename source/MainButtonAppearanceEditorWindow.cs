using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    public class MainButtonAppearanceEditorWindow : Window
    {
        private const float PreviewHeight = 36f;
        private const float IconCellSize = 50f;
        private const float IconCellPadding = 4f;
        private const float IconThumbnailSize = 32f;
        private const float RowGap = 8f;
        private const float FooterHeight = 34f;

        private static readonly Color PreviewBg = new Color(0.08f, 0.08f, 0.08f, 1f);
        private static readonly Color SelectedIconBg = new Color(1f, 0.85f, 0.3f, 0.25f);

        private const float DescriptionFieldHeight = 54f;

        private readonly MainButtonDef def;
        private string workingLabel;
        private string workingDescription;
        private string workingIconPath;
        private bool workingShowIcon;
        private bool workingPreferIconOnly;
        private Vector2 scrollPosition;

        public override Vector2 InitialSize => new Vector2(480f, 616f + DescriptionFieldHeight + RowGap);

        public MainButtonAppearanceEditorWindow(MainButtonDef def)
        {
            this.def = def;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            closeOnCancel = false;
            draggable = true;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            LoadWorkingCopy();
        }

        private void LoadWorkingCopy()
        {
            MainButtonAppearanceConfig existing = ModSettings.GetAppearance(def);
            workingLabel = existing?.customLabel ?? def.LabelCap.ToString();
            workingDescription = existing?.customDescription ?? def.description ?? string.Empty;
            workingIconPath = existing?.iconPath;
            workingShowIcon = existing == null || existing.showIcon;
            workingPreferIconOnly = existing != null && existing.preferIconOnly;
            scrollPosition = Vector2.zero;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Rect titleRect = new Rect(0f, 0f, inRect.width, Text.LineHeight);
            Widgets.Label(titleRect, "DMMB.AppearanceEditorTitle".Translate(def.LabelCap));
            float curY = titleRect.yMax + 6f;

            curY = DrawPreviewSection(inRect, curY) + RowGap;

            if (!MainButtonAppearanceRenderer.Supports(def))
            {
                curY = DrawCompatibilityWarning(inRect, curY) + RowGap;
            }

            curY = DrawNameField(inRect, curY) + RowGap;
            curY = DrawDescriptionField(inRect, curY) + RowGap;
            curY = DrawShowIconRow(inRect, curY) + RowGap;
            curY = DrawIconOnlyRow(inRect, curY) + RowGap;

            Text.Font = GameFont.Tiny;
            Rect gridLabelRect = new Rect(0f, curY, inRect.width, Text.LineHeight);
            Widgets.Label(gridLabelRect, "DMMB.AppearanceIconGridTitle".Translate());
            Text.Font = GameFont.Small;
            curY = gridLabelRect.yMax + 2f;

            Rect footerRect = new Rect(0f, inRect.height - FooterHeight, inRect.width, FooterHeight);
            Rect gridRect = new Rect(0f, curY, inRect.width, footerRect.y - RowGap - curY);
            DrawIconGrid(gridRect);

            DrawFooter(footerRect);
        }

        private float DrawPreviewSection(Rect inRect, float curY)
        {
            Text.Font = GameFont.Tiny;
            Rect labelRect = new Rect(0f, curY, inRect.width, Text.LineHeight);
            Widgets.Label(labelRect, "DMMB.AppearancePreviewLabel".Translate());
            curY = labelRect.yMax + 2f;
            Text.Font = GameFont.Small;

            float previewWidth = Mathf.Clamp(MainButtonsRoot_DoButtons_Patch.GetCurrentButtonWidth(def), 40f, inRect.width);
            Rect previewRect = new Rect(0f, curY, previewWidth, PreviewHeight);
            Widgets.DrawBoxSolid(previewRect, PreviewBg);

            string previewLabel = MainButtonAppearanceConfig.NormalizeLabel(workingLabel);
            Texture2D previewIcon = GetWorkingIcon();
            MainButtonAppearanceRenderer.DrawPreview(def, previewRect, previewLabel, previewIcon, workingPreferIconOnly);

            return previewRect.yMax;
        }

        private float DrawCompatibilityWarning(Rect inRect, float curY)
        {
            Text.Font = GameFont.Tiny;
            string warning = "DMMB.AppearanceCompatibilityWarning".Translate();
            float height = Text.CalcHeight(warning, inRect.width);
            Rect warningRect = new Rect(0f, curY, inRect.width, height);

            Color prevColor = GUI.color;
            GUI.color = new Color(1f, 0.8f, 0.4f, 1f);
            Widgets.Label(warningRect, warning);
            GUI.color = prevColor;
            Text.Font = GameFont.Small;

            return warningRect.yMax;
        }

        private Texture2D GetWorkingIcon()
        {
            if (!workingShowIcon)
            {
                return null;
            }

            if (workingIconPath != null)
            {
                return MainButtonAppearanceCatalog.GetTexture(workingIconPath);
            }

            return def.Icon;
        }

        private float DrawNameField(Rect inRect, float curY)
        {
            float rowHeight = Text.LineHeight + 4f;
            Rect labelRect = new Rect(0f, curY, 90f, rowHeight);
            TextAnchor prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, "DMMB.AppearanceNameLabel".Translate());
            Text.Anchor = prevAnchor;

            Rect fieldRect = new Rect(labelRect.xMax + 6f, curY, inRect.width - labelRect.width - 6f, rowHeight);
            string newLabel = Widgets.TextField(fieldRect, workingLabel);
            if (newLabel.Length > MainButtonAppearanceConfig.MaxLabelLength)
            {
                newLabel = newLabel.Substring(0, MainButtonAppearanceConfig.MaxLabelLength);
            }

            workingLabel = newLabel;
            return fieldRect.yMax;
        }

        private float DrawDescriptionField(Rect inRect, float curY)
        {
            Text.Font = GameFont.Tiny;
            Rect labelRect = new Rect(0f, curY, inRect.width, Text.LineHeight);
            Widgets.Label(labelRect, "DMMB.AppearanceDescriptionLabel".Translate());
            Text.Font = GameFont.Small;

            Rect fieldRect = new Rect(0f, labelRect.yMax + 2f, inRect.width, DescriptionFieldHeight);
            string newDescription = Widgets.TextArea(fieldRect, workingDescription);
            if (newDescription.Length > MainButtonAppearanceConfig.MaxDescriptionLength)
            {
                newDescription = newDescription.Substring(0, MainButtonAppearanceConfig.MaxDescriptionLength);
            }

            workingDescription = newDescription;
            return fieldRect.yMax;
        }

        private float DrawShowIconRow(Rect inRect, float curY)
        {
            const float rowHeight = 28f;
            Rect checkboxRect = new Rect(0f, curY, inRect.width * 0.5f, rowHeight);
            Widgets.CheckboxLabeled(checkboxRect, "DMMB.AppearanceShowIcon".Translate(), ref workingShowIcon);
            TooltipHandler.TipRegion(checkboxRect, "DMMB.AppearanceShowIconDesc".Translate());

            Rect restoreRect = new Rect(checkboxRect.xMax + 8f, curY, inRect.width - checkboxRect.width - 8f, rowHeight);
            if (Widgets.ButtonText(restoreRect, "DMMB.AppearanceRestoreOriginalIcon".Translate()))
            {
                workingIconPath = null;
            }

            return checkboxRect.yMax;
        }

        private float DrawIconOnlyRow(Rect inRect, float curY)
        {
            const float rowHeight = 28f;
            Rect rowRect = new Rect(0f, curY, inRect.width, rowHeight);
            Widgets.CheckboxLabeled(rowRect, "DMMB.AppearanceIconOnly".Translate(), ref workingPreferIconOnly);
            TooltipHandler.TipRegion(rowRect, "DMMB.AppearanceIconOnlyDesc".Translate());
            return rowRect.yMax;
        }

        private void DrawIconGrid(Rect outRect)
        {
            IReadOnlyList<string> paths = MainButtonAppearanceCatalog.IconPaths;
            float viewWidth = outRect.width - 16f;
            int columns = Mathf.Max(1, Mathf.FloorToInt(viewWidth / IconCellSize));
            int rows = Mathf.CeilToInt(paths.Count / (float)columns);
            float viewHeight = Mathf.Max(outRect.height, rows * IconCellSize);
            Rect viewRect = new Rect(0f, 0f, viewWidth, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            for (int i = 0; i < paths.Count; i++)
            {
                int col = i % columns;
                int row = i / columns;
                Rect cellRect = new Rect(col * IconCellSize, row * IconCellSize, IconCellSize, IconCellSize);
                DrawIconCell(cellRect, paths[i]);
            }

            Widgets.EndScrollView();
        }

        private void DrawIconCell(Rect cellRect, string path)
        {
            bool selected = path == workingIconPath;
            bool hovered = Mouse.IsOver(cellRect);

            if (selected)
            {
                Widgets.DrawBoxSolid(cellRect, SelectedIconBg);
            }
            else if (hovered)
            {
                Widgets.DrawHighlight(cellRect);
            }

            Texture2D texture = MainButtonAppearanceCatalog.GetTexture(path);
            if (texture != null)
            {
                Rect inner = cellRect.ContractedBy(IconCellPadding);
                Rect iconRect = new Rect(
                    inner.x + (inner.width - IconThumbnailSize) / 2f,
                    inner.y + (inner.height - IconThumbnailSize) / 2f,
                    IconThumbnailSize,
                    IconThumbnailSize);
                Widgets.DrawTextureFitted(iconRect, texture, 1f);
            }

            if (selected)
            {
                Widgets.DrawBox(cellRect, 2);
            }

            string fileName = path.Substring(path.LastIndexOf('/') + 1);
            TooltipHandler.TipRegion(cellRect, fileName);

            if (Widgets.ButtonInvisible(cellRect))
            {
                workingIconPath = path;
            }
        }

        private void DrawFooter(Rect rect)
        {
            float buttonWidth = (rect.width - RowGap) / 2f;
            Rect resetRect = new Rect(0f, rect.y, buttonWidth, rect.height);
            Rect doneRect = new Rect(resetRect.xMax + RowGap, rect.y, buttonWidth, rect.height);

            if (Widgets.ButtonText(resetRect, "DMMB.AppearanceResetThisButton".Translate()))
            {
                ModSettings.ResetAppearance(def);
                Mod.Settings.Write();
                LoadWorkingCopy();
            }

            if (Widgets.ButtonText(doneRect, "DMMB.AppearanceDone".Translate()))
            {
                Commit();
                Close();
            }
        }

        private void Commit()
        {
            string normalizedLabel = MainButtonAppearanceConfig.NormalizeLabel(workingLabel);
            if (normalizedLabel == def.LabelCap.ToString())
            {
                normalizedLabel = null;
            }

            string normalizedDescription = MainButtonAppearanceConfig.NormalizeDescription(workingDescription);
            if (normalizedDescription == (def.description ?? string.Empty))
            {
                normalizedDescription = null;
            }

            MainButtonAppearanceConfig config = new MainButtonAppearanceConfig
            {
                customLabel = normalizedLabel,
                customDescription = normalizedDescription,
                iconPath = workingIconPath,
                showIcon = workingShowIcon,
                preferIconOnly = workingPreferIconOnly,
            };

            ModSettings.SetAppearance(def, config);
            Mod.Settings.Write();
        }
    }
}
