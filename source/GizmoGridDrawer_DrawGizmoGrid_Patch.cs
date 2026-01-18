using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch(typeof(GizmoGridDrawer), "DrawGizmoGrid")]
    public static class GizmoGridDrawer_DrawGizmoGrid_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo adjustMethod = AccessTools.Method(typeof(GizmoGridDrawer_DrawGizmoGrid_Patch), nameof(AdjustVectorY));
            ConstructorInfo vectorCtor = AccessTools.Constructor(typeof(Vector2), new[] { typeof(float), typeof(float) });
            var codes = new List<CodeInstruction>(instructions);
            bool patched = false;

            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (codes[i].opcode != OpCodes.Ldloca_S)
                {
                    continue;
                }

                LocalBuilder local = codes[i].operand as LocalBuilder;
                if (local == null || (local.LocalIndex != 3 && local.LocalIndex != 6))
                {
                    continue;
                }

                if (codes[i + 1].opcode != OpCodes.Ldarg_1)
                {
                    continue;
                }

                if (codes[i + 2].opcode != OpCodes.Ldloc_2)
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
                patched = true;
                i += 5;
            }

            if (patched)
            {
                Logger.Message("GizmoGridDrawer.DrawGizmoGrid patch applied.");
            }
            else
            {
                Logger.Warning("GizmoGridDrawer.DrawGizmoGrid patch failed to apply.");
            }

            return codes;
        }

        private static void AdjustVectorY(ref Vector2 value)
        {
            if (!ModSettings.drawGizmosAtBottom)
            {
                return;
            }

            value.y += ModSettings.gizmoBottomOffset;
        }
    }
}
