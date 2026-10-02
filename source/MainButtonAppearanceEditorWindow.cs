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
        private const float SectionHeaderPadding = 4f;

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
            curY = DrawUnavailableSelectionNote(inRect, curY);

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
                // Unavailable optional-pack choices preview the original icon, matching
                // ModSettings.GetDisplayIcon; the editor still surfaces BadTex for built-ins.
                Texture2D texture = MainButtonAppearanceCatalog.GetTexture(workingIconPath);
                if (texture != null)
                {
                    return texture;
                }
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

        private float DrawUnavailableSelectionNote(Rect inRect, float curY)
        {
            if (workingIconPath == null
                || MainButtonAppearanceCatalog.GetTexture(workingIconPath) != null
                || !MainButtonAppearanceCatalog.TryDescribe(workingIconPath, out string sourceLabel, out string leafName))
            {
                return curY;
            }

            Text.Font = GameFont.Tiny;
            string note = "DMMB.AppearanceSelectionUnavailable".Translate(leafName, sourceLabel);
            float height = Text.CalcHeight(note, inRect.width);
            Rect noteRect = new Rect(0f, curY, inRect.width, height);

            Color prevColor = GUI.color;
            GUI.color = new Color(1f, 0.8f, 0.4f, 1f);
            Widgets.Label(noteRect, note);
            GUI.color = prevColor;
            Text.Font = GameFont.Small;

            return noteRect.yMax + 2f;
        }

        private void DrawIconGrid(Rect outRect)
        {
            IReadOnlyList<MainButtonIconSource> sources = MainButtonAppearanceCatalog.GetSources();
            float viewWidth = outRect.width - 16f;
            int columns = Mathf.Max(1, Mathf.FloorToInt(viewWidth / IconCellSize));

            // Section headers are only needed when more than one source is available.
            bool showHeaders = sources.Count > 1;
            float headerHeight = showHeaders ? Text.LineHeightOf(GameFont.Tiny) + SectionHeaderPadding : 0f;

            float contentHeight = 0f;
            for (int s = 0; s < sources.Count; s++)
            {
                int rows = Mathf.CeilToInt(sources[s].Entries.Count / (float)columns);
                contentHeight += headerHeight + rows * IconCellSize;
            }

            Rect viewRect = new Rect(0f, 0f, viewWidth, Mathf.Max(outRect.height, contentHeight));

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float curY = 0f;
            for (int s = 0; s < sources.Count; s++)
            {
                MainButtonIconSource source = sources[s];
                if (showHeaders)
                {
                    Text.Font = GameFont.Tiny;
                    Rect headerRect = new Rect(0f, curY, viewWidth, headerHeight);
                    GUI.color = Color.gray;
                    Widgets.Label(headerRect, source.LabelKey.Translate() + " (" + source.Entries.Count + ")");
                    GUI.color = Color.white;
                    Text.Font = GameFont.Small;
                    curY += headerHeight;
                }

                for (int i = 0; i < source.Entries.Count; i++)
                {
                    int col = i % columns;
                    int row = i / columns;
                    Rect cellRect = new Rect(col * IconCellSize, curY + row * IconCellSize, IconCellSize, IconCellSize);
                    DrawIconCell(cellRect, source.Entries[i]);
                }

                curY += Mathf.CeilToInt(source.Entries.Count / (float)columns) * IconCellSize;
            }

            Widgets.EndScrollView();
        }

        private void DrawIconCell(Rect cellRect, MainButtonIconEntry entry)
        {
            string path = entry.Key;
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

            TooltipHandler.TipRegion(cellRect, entry.Name + "\n" + entry.SourceLabelKey.Translate());

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
