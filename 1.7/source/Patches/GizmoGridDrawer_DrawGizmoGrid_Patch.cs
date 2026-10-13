using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    // Harmony 3 infix patches cannot replace this transpiler: `new Vector2(startX, y)` is a struct constructor call on a
    // local (infix constructors only match `newobj`), and `GizmoSpacing.x` is read through `ldsflda` (infix field reads
    // only match `ldsfld`). The transpiler therefore stays, but it matches on IL shape and local *type* rather than on
    // local indexes, and it reports how many sites it patched.
    [HarmonyPatch(typeof(GizmoGridDrawer), "DrawGizmoGrid")]
    public static class GizmoGridDrawer_DrawGizmoGrid_Patch
    {
        // Vanilla 1.7 builds the start position twice (measuring pass, drawing pass) and reads GizmoSpacing six times.
        private const int ExpectedOffsetSites = 2;
        private const int ExpectedSpacingSites = 6;

        private static bool previewDrawActive;
        private static Vector2? lastPreviewPosition;
        private static bool previewPrevApplyOffset;
        public static bool ApplyOffset;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo adjustMethod = AccessTools.Method(typeof(GizmoGridDrawer_DrawGizmoGrid_Patch), nameof(AdjustVector));
            MethodInfo spacingXMethod = AccessTools.Method(typeof(GizmoGridDrawer_DrawGizmoGrid_Patch), nameof(GetSpacingX));
            MethodInfo spacingYMethod = AccessTools.Method(typeof(GizmoGridDrawer_DrawGizmoGrid_Patch), nameof(GetSpacingY));
            ConstructorInfo vectorCtor = AccessTools.Constructor(typeof(Vector2), new[] { typeof(float), typeof(float) });
            FieldInfo gizmoSpacingField = AccessTools.Field(typeof(GizmoGridDrawer), "GizmoSpacing");
            FieldInfo vectorXField = AccessTools.Field(typeof(Vector2), "x");
            FieldInfo vectorYField = AccessTools.Field(typeof(Vector2), "y");
            var codes = new List<CodeInstruction>(instructions);
            int offsetSites = 0;
            int spacingSites = 0;

            // new Vector2(startX, y): ldloca <Vector2 local>; ldarg.1 (startX); ldloc* y; call Vector2::.ctor(float, float)
            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (codes[i].opcode != OpCodes.Ldloca_S && codes[i].opcode != OpCodes.Ldloca)
                {
                    continue;
                }

                LocalBuilder local = codes[i].operand as LocalBuilder;
                if (local == null || local.LocalType != typeof(Vector2))
                {
                    continue;
                }

                if (codes[i + 1].opcode != OpCodes.Ldarg_1 || !IsLoadLocal(codes[i + 2]))
                {
                    continue;
                }

                if (codes[i + 3].opcode != OpCodes.Call || !Equals(codes[i + 3].operand, vectorCtor))
                {
                    continue;
                }

                codes.InsertRange(i + 4, new[]
                {
                    new CodeInstruction(OpCodes.Ldloca_S, local),
                    new CodeInstruction(OpCodes.Call, adjustMethod)
                });
                offsetSites++;
                i += 5;
            }

            // GizmoSpacing.x / GizmoSpacing.y: ldsflda GizmoSpacing; ldfld Vector2::x|y
            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (codes[i].opcode != OpCodes.Ldsflda || !Equals(codes[i].operand, gizmoSpacingField))
                {
                    continue;
                }

                if (codes[i + 1].opcode != OpCodes.Ldfld)
                {
                    continue;
                }

                MethodInfo spacingMethod = null;
                if (Equals(codes[i + 1].operand, vectorXField))
                {
                    spacingMethod = spacingXMethod;
                }
                else if (Equals(codes[i + 1].operand, vectorYField))
                {
                    spacingMethod = spacingYMethod;
                }

                if (spacingMethod == null)
                {
                    continue;
                }

                var callInstruction = new CodeInstruction(OpCodes.Call, spacingMethod);
                callInstruction.labels.AddRange(codes[i].labels);
                callInstruction.blocks.AddRange(codes[i].blocks);
                codes[i] = callInstruction;
                codes.RemoveAt(i + 1);
                spacingSites++;
            }

            if (offsetSites == 0 || spacingSites == 0)
            {
                Log.Warning($"[DeclutterMainButtonsBar] GizmoGridDrawer.DrawGizmoGrid patch did not match (start offset sites: {offsetSites}, spacing sites: {spacingSites}). Gizmo offset and spacing settings will have no effect.");
            }
            else if (offsetSites != ExpectedOffsetSites || spacingSites != ExpectedSpacingSites)
            {
                Log.Warning($"[DeclutterMainButtonsBar] GizmoGridDrawer.DrawGizmoGrid patch matched an unexpected number of sites (start offset: {offsetSites}/{ExpectedOffsetSites}, spacing: {spacingSites}/{ExpectedSpacingSites}). The game or another mod changed this method.");
            }
            else
            {
                Logger.Message("GizmoGridDrawer.DrawGizmoGrid patch applied.");
            }

            return codes;
        }

        private static bool IsLoadLocal(CodeInstruction instruction)
        {
            OpCode op = instruction.opcode;
            return op == OpCodes.Ldloc_0 || op == OpCodes.Ldloc_1 || op == OpCodes.Ldloc_2 || op == OpCodes.Ldloc_3
                || op == OpCodes.Ldloc_S || op == OpCodes.Ldloc;
        }

        public static void BeginPreviewDraw()
        {
            previewDrawActive = true;
            lastPreviewPosition = null;
            previewPrevApplyOffset = ApplyOffset;
            ApplyOffset = true;
        }

        public static void EndPreviewDraw()
        {
            previewDrawActive = false;
            ApplyOffset = previewPrevApplyOffset;
        }

        public static bool TryGetLastPreviewPosition(out Vector2 position)
        {
            if (lastPreviewPosition.HasValue)
            {
                position = lastPreviewPosition.Value;
                return true;
            }

            position = default;
            return false;
        }

        private static void AdjustVector(ref Vector2 value)
        {
            if (ApplyOffset)
            {
                value.x += ModSettings.gizmoDrawerOffsetX;
                value.y += ModSettings.gizmoDrawerOffsetY;
            }

            if (previewDrawActive)
            {
                lastPreviewPosition = value;
            }
        }

        private static float GetSpacingX()
        {
            bool apply = !ModSettings.gizmoScaleMapOnly || ApplyOffset;
            float value = apply ? ModSettings.gizmoSpacingX : GizmoGridDrawer.GizmoSpacing.x;
            return apply && !previewDrawActive ? value * ModSettings.gizmoDrawerScale : value;
        }

        private static float GetSpacingY()
        {
            bool apply = !ModSettings.gizmoScaleMapOnly || ApplyOffset;
            float value = apply ? ModSettings.gizmoSpacingY : GizmoGridDrawer.GizmoSpacing.y;
            return apply && !previewDrawActive ? value * ModSettings.gizmoDrawerScale : value;
        }
    }
}
