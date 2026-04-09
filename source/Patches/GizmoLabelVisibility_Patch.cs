using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch(typeof(Command), "GizmoOnGUIInt")]
    public static class Command_GizmoOnGUIInt_HideLabels_Patch
    {
        public static bool Prepare()
        {
            return ModSettings.enableGizmoLabelVisibilityPatches;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo shrunkField = AccessTools.Field(typeof(GizmoRenderParms), nameof(GizmoRenderParms.shrunk));
            FieldInfo iconField = AccessTools.Field(typeof(Command), nameof(Command.icon));
            MethodInfo topRightLabelGetter = AccessTools.PropertyGetter(typeof(Command), nameof(Command.TopRightLabel));
            MethodInfo helperMethod = AccessTools.Method(typeof(Command_GizmoOnGUIInt_HideLabels_Patch), nameof(ShouldSkipLabelBlock));
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (!codes[i].Calls(topRightLabelGetter))
                {
                    continue;
                }

                for (int j = i - 1; j >= 0 && j >= i - 12; j--)
                {
                    if (codes[j].opcode != OpCodes.Ldfld || !Equals(codes[j].operand, shrunkField))
                    {
                        continue;
                    }

                    if (codes[j + 1].opcode != OpCodes.Brtrue && codes[j + 1].opcode != OpCodes.Brtrue_S)
                    {
                        continue;
                    }

                    codes.InsertRange(j + 1, new[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, iconField),
                        new CodeInstruction(OpCodes.Call, helperMethod)
                    });

                    Logger.Message("Command.GizmoOnGUIInt hide labels patch applied.");
                    return codes;
                }
            }

            Logger.Warning("Command.GizmoOnGUIInt hide labels patch failed to apply.");
            return codes;
        }

        public static bool ShouldSkipLabelBlock(bool shrunk, Command command, Texture icon)
        {
            bool hideLabels = ModSettings.hideAllGizmoLabels
                || (ModSettings.hideGizmoLabelsForSelectedColonistsOnly
                    && SelectorSelectionState.AllSelectedObjectsAreColonists);
            return shrunk || (hideLabels && icon != null && icon != BaseContent.BadTex);
        }
    }

    [HarmonyPatch(typeof(Command_Ability), nameof(Command_Ability.GizmoOnGUI))]
    public static class Command_Ability_GizmoOnGUI_HideCooldownText_Patch
    {
        public static bool Prepare()
        {
            return ModSettings.enableGizmoLabelVisibilityPatches;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo abilityField = AccessTools.Field(typeof(Command_Ability), "ability");
            MethodInfo cooldownGetter = AccessTools.PropertyGetter(typeof(Ability), nameof(Ability.CooldownTicksRemaining));
            MethodInfo helperMethod = AccessTools.Method(
                typeof(Command_Ability_GizmoOnGUI_HideCooldownText_Patch),
                nameof(ShouldDrawCooldownText));

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int cooldownCheckCount = 0;

            for (int i = 0; i < codes.Count - 4; i++)
            {
                if (codes[i].opcode != OpCodes.Ldarg_0)
                {
                    continue;
                }

                if (codes[i + 1].opcode != OpCodes.Ldfld || !Equals(codes[i + 1].operand, abilityField))
                {
                    continue;
                }

                if (!codes[i + 2].Calls(cooldownGetter) || codes[i + 3].opcode != OpCodes.Ldc_I4_0)
                {
                    continue;
                }

                if (codes[i + 4].opcode != OpCodes.Ble && codes[i + 4].opcode != OpCodes.Ble_S)
                {
                    continue;
                }

                cooldownCheckCount++;
                if (cooldownCheckCount != 2)
                {
                    continue;
                }

                CodeInstruction loadInstance = new CodeInstruction(OpCodes.Ldarg_0);
                loadInstance.labels.AddRange(codes[i].labels);
                loadInstance.labels.AddRange(codes[i + 1].labels);
                loadInstance.labels.AddRange(codes[i + 2].labels);
                loadInstance.labels.AddRange(codes[i + 3].labels);
                loadInstance.blocks.AddRange(codes[i].blocks);
                loadInstance.blocks.AddRange(codes[i + 1].blocks);
                loadInstance.blocks.AddRange(codes[i + 2].blocks);
                loadInstance.blocks.AddRange(codes[i + 3].blocks);

                CodeInstruction callHelper = new CodeInstruction(OpCodes.Call, helperMethod);

                codes[i] = loadInstance;
                codes[i + 1] = callHelper;
                codes[i + 2] = new CodeInstruction(OpCodes.Nop);
                codes[i + 3] = new CodeInstruction(OpCodes.Nop);
                codes[i + 4].opcode = codes[i + 4].opcode == OpCodes.Ble_S ? OpCodes.Brfalse_S : OpCodes.Brfalse;

                Logger.Message("Command_Ability.GizmoOnGUI hide cooldown text patch applied.");
                return codes;
            }

            Logger.Warning("Command_Ability.GizmoOnGUI hide cooldown text patch failed to apply.");
            return codes;
        }

        public static bool ShouldDrawCooldownText(Command_Ability command)
        {
            bool hideLabels = ModSettings.hideAllGizmoLabels
                || (ModSettings.hideGizmoLabelsForSelectedColonistsOnly
                    && SelectorSelectionState.AllSelectedObjectsAreColonists);

            return !hideLabels && command.Ability.CooldownTicksRemaining > 0;
        }
    }
}
