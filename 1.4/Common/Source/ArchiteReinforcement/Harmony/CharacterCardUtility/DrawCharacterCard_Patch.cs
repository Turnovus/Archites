using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace ArchiteReinforcement
{
    [HarmonyPatch(typeof(CharacterCardUtility))]
    [HarmonyPatch(nameof(CharacterCardUtility.DrawCharacterCard))]
    class DrawCharacterCard_Patch
    {
        public const float ButtonHorizontalOffset = 32f;

        private static readonly MethodInfo M_InfoCardButton =
            AccessTools.Method(typeof(Widgets), nameof(Widgets.InfoCardButton), new Type[] {
                typeof(float),
                typeof(float),
                typeof(Thing),
            });
        private static readonly MethodInfo M_TryDrawUpgradeButton =
            AccessTools.Method(typeof(DrawCharacterCard_Patch), nameof(DrawCharacterCard_Patch.TryDrawUpgradeButton));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InjectUpgradeButton(
            IEnumerable<CodeInstruction> instructions
        )
        {
            // Flag values:
            // 0 - Looking for call to Widgets.InfoCardButton(float, float, thing).
            // 1 - Waiting for pop to inject method.
            // 2 - Patch done.
            int flag = 0;

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                switch(flag)
                {
                    case 0:
                        if (instruction.operand as MethodInfo == M_InfoCardButton)
                            flag = 1;
                        break;
                    case 1:
                        if (instruction.opcode != OpCodes.Pop)
                            break;

                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Call, M_TryDrawUpgradeButton);

                        flag = 2;
                        break;
                }
            }

            if (flag < 2)
                Log.Error("InjectUpgradeButton patch failed!");
        }

        public static void TryDrawUpgradeButton(Pawn forPawn, Rect nameRect)
        {
            CompArchiteTracker tracker = forPawn.ArchiteTracker();
            if (tracker == null || !tracker.CanOpenMenu)
                return;

            float x = nameRect.xMax + ButtonHorizontalOffset;
            ArchiteUpgradesWindowDrawer.UpgradeViewerButton(x, nameRect.y, forPawn);
        }
    }
}
