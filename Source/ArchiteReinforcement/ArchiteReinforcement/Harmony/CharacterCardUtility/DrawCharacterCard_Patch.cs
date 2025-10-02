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
        public const float CreationMenuButtonSize = 24f;
        public const float BiographyButtonSize = 30f;

        private static readonly MethodInfo M_InfoCardButton =
            AccessTools.Method(typeof(Widgets), nameof(Widgets.InfoCardButton), new Type[] {
                typeof(float),
                typeof(float),
                typeof(Thing),
            });
        private static readonly MethodInfo M_WindowStackAdd =
            AccessTools.Method(typeof(WindowStack), nameof(WindowStack.Add));
        private static readonly MethodInfo M_TryDrawCreationModeButton =
            AccessTools.Method(typeof(DrawCharacterCard_Patch), nameof(DrawCharacterCard_Patch.TryDrawCreationModeButton));
        private static readonly MethodInfo M_TryDrawBiographyButton =
            AccessTools.Method(typeof(DrawCharacterCard_Patch), nameof(DrawCharacterCard_Patch.TryDrawBiographyButton));


        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InjectUpgradeButton(
            IEnumerable<CodeInstruction> instructions
        )
        {
            // Flag values:
            // 0 - Looking for call to Widgets.InfoCardButton(float, float, thing).
            // 1 - Waiting for pop to inject method.
            // 2 - Looking for the next stloc.s. This will contain the field for local variable x.
            // 3 - Looking for Verse.WindowStack.Add. This will place our injection
            //       between drawing the banish button and the renounce title button.
            // 4 - Looking for an stloc.s, which means that the game has finished drawing the
            //     banish button.
            // 5 - Patch done.
            int flag = 0;
            object x = null;

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

                        yield return new CodeInstruction(OpCodes.Ldarg_1); // pawn
                        yield return new CodeInstruction(OpCodes.Ldloc_3); // rect2
                        yield return new CodeInstruction(OpCodes.Call, M_TryDrawCreationModeButton);

                        flag = 2;
                        break;
                    case 2:
                        if (instruction.opcode == OpCodes.Stloc_S)
                        {
                            x = instruction.operand;
                            flag = 3;
                        }
                        break;
                    case 3:
                        if (instruction.operand as MethodInfo == M_WindowStackAdd)
                            flag = 4;
                        break;
                    case 4:
                        if (instruction.opcode != OpCodes.Stloc_S)
                            break;

                        yield return new CodeInstruction(OpCodes.Ldloca_S, x); // x
                        yield return new CodeInstruction(OpCodes.Ldarg_1); // pawn
                        yield return new CodeInstruction(OpCodes.Call, M_TryDrawBiographyButton);

                        flag = 5;
                        break;
                }
            }

            if (flag < 5)
                Log.Error("InjectUpgradeButton patch failed! Flag=" + flag.ToString());
        }

        public static void TryDrawCreationModeButton(Pawn forPawn, Rect nameRect)
        {
            CompArchiteTracker tracker = forPawn.ArchiteTracker();
            if (tracker == null || !tracker.CanOpenMenu)
                return;

            float x = nameRect.xMax + ButtonHorizontalOffset;
            float y = nameRect.y + (0.5f * (nameRect.height - ArchiteUpgradesWindowDrawer.UpgradeButtonSize));
            Rect rect = new Rect(x, y, CreationMenuButtonSize, CreationMenuButtonSize);
            ArchiteUpgradesWindowDrawer.UpgradeViewerButton(rect, forPawn, "ArchiteReinforcement.UpgradeButtonTip.CreationMenu");
        }

        public static void TryDrawBiographyButton(ref float x, Pawn forPawn)
        {
            CompArchiteTracker tracker = forPawn.ArchiteTracker();
            if (tracker == null || !tracker.CanOpenMenu)
                return;

            Rect rect = new Rect(x, 0f, BiographyButtonSize, BiographyButtonSize);
            ArchiteUpgradesWindowDrawer.UpgradeViewerButton(rect, forPawn, "ArchiteReinforcement.UpgradeButtonTip.CreationMenu");
            x -= BiographyButtonSize;
        }
    }
}
