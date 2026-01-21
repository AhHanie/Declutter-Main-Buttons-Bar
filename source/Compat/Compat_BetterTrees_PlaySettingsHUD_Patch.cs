using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;

namespace Declutter_Main_Buttons_Bar
{
    [HarmonyPatch]
    public static class Compat_BetterTrees_PlaySettingsHUD_Patch
    {
        private const string PatchTypeName = "BetterTrees.PlaySettings_HUD_Patch";
        private const string BetterTreesPackageId = "chaoticenrico.bettertrees";

        static bool Prepare()
        {
            return ModsConfig.IsActive(BetterTreesPackageId);
        }

        static System.Reflection.MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName(PatchTypeName);
            return type == null ? null : AccessTools.Method(type, "Postfix");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            var matcher = new CodeMatcher(codes, generator);
            var buttonIconRect = AccessTools.Method(typeof(WidgetRow), "ButtonIconRect", new[] { typeof(float) });
            var drawTexture = AccessTools.Method(typeof(GUI), "DrawTexture", new[] { typeof(Rect), typeof(Texture) });
            var toggleableIcon = AccessTools.Method(typeof(WidgetRow), "ToggleableIcon", new[] { typeof(bool).MakeByRefType(), typeof(Texture2D), typeof(string), typeof(SoundDef), typeof(string) });
            var toggleTransparency = AccessTools.Method("BetterTrees.TransparencyController:ToggleTransparency");
            var toggleIcon = AccessTools.Field("BetterTrees.TransparencyController:ToggleIcon");
            var mouseoverSound = AccessTools.Field(typeof(SoundDefOf), "Mouseover_ButtonToggle");

            if (buttonIconRect == null || drawTexture == null || toggleableIcon == null || toggleTransparency == null || toggleIcon == null || mouseoverSound == null)
            {
                Logger.Warning("BetterTrees compat transpiler: required members not found.");
                return codes;
            }

            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldc_R4),
                new CodeMatch(OpCodes.Callvirt, buttonIconRect)
            );

            if (!matcher.IsValid)
            {
                Logger.Warning("BetterTrees compat transpiler: ButtonIconRect block start not found.");
                return codes;
            }

            int startIndex = matcher.Pos;
            matcher.MatchStartForward(new CodeMatch(OpCodes.Call, drawTexture));

            if (!matcher.IsValid)
            {
                Logger.Warning("BetterTrees compat transpiler: GUI.DrawTexture call not found.");
                return codes;
            }

            int endIndex = matcher.Pos;
            if (endIndex < startIndex)
            {
                Logger.Warning("BetterTrees compat transpiler: invalid target block range.");
                return codes;
            }

            var inheritedLabels = codes[startIndex].labels;
            matcher.Start().Advance(startIndex).RemoveInstructions(endIndex - startIndex + 1);

            var skipToggleCall = generator.DefineLabel();
            var instructionsToInsert = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Stloc_S, (byte)7),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloca_S, (byte)0),
                new CodeInstruction(OpCodes.Ldsfld, toggleIcon),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldsfld, mouseoverSound),
                new CodeInstruction(OpCodes.Ldnull),
                new CodeInstruction(OpCodes.Callvirt, toggleableIcon),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldloc_S, (byte)7),
                new CodeInstruction(OpCodes.Ceq),
                new CodeInstruction(OpCodes.Brtrue_S, skipToggleCall),
                new CodeInstruction(OpCodes.Call, toggleTransparency),
                new CodeInstruction(OpCodes.Nop) { labels = new List<Label> { skipToggleCall } },
            };

            if (inheritedLabels != null && inheritedLabels.Count > 0)
            {
                instructionsToInsert[0].labels.AddRange(inheritedLabels);
            }

            matcher.Start().Advance(startIndex).Insert(instructionsToInsert);
            Logger.Message("BetterTrees compat transpiler: replaced checkbox draw block with ToggleableIcon.");

            return matcher.InstructionEnumeration();
        }
    }
}
