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

namespace ArchiteReinforcement
{
    
    [HarmonyPatch(typeof(PawnCapacityUtility))]
    [HarmonyPatch(nameof(PawnCapacityUtility.CalculateCapacityLevel))]
    class ArchitesAffectCapacities
    {
        private static readonly ConstructorInfo Ctor_CapacityImpactorGene =
            AccessTools.Constructor(typeof(PawnCapacityUtility.CapacityImpactorGene));
        private static readonly MethodInfo M_AugmentCapacity =
            AccessTools.Method(typeof(ArchitesAffectCapacities), nameof(ArchitesAffectCapacities.AugmentCapacity));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InjectCapacityAugment(
            IEnumerable<CodeInstruction> instructions
        )
        {
            // Flag values:
            // 0 - Looking for CapacityImpactorGene constructor. This is inside of two for loops
            // 1, 2 - looking for two ldc.i4.1 instructions. Once we've found both of them, we'll
            //        know we're almost outside both of the for loops.
            // 3 - Looking for a blt instruction, which means we're completely out of the for loop.
            // 4 - Injecting method.
            // 5 - Patch done.
            int flag = 0;
            
            foreach (CodeInstruction instruction in instructions)
            {
                if (flag == 4 && !instruction.labels.EnumerableNullOrEmpty())
                {
                    flag = 5;

                    List<Label> labels = new List<Label>(instruction.labels);
                    instruction.labels.Clear();

                    CodeInstruction firstInstruction = new CodeInstruction(OpCodes.Ldloc_0); // a
                    firstInstruction.labels = labels;
                    yield return firstInstruction;

                    yield return new CodeInstruction(OpCodes.Ldarg_0); // diffSet
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // capacity
                    yield return new CodeInstruction(OpCodes.Ldarg_2); // impactors
                    yield return new CodeInstruction(OpCodes.Ldarg_3); // forTradePrice

                    yield return new CodeInstruction(OpCodes.Call, M_AugmentCapacity);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }

                yield return instruction;

                switch (flag) {
                    case 0:
                        if (instruction.operand as ConstructorInfo == Ctor_CapacityImpactorGene)
                            flag = 1;
                        break;
                    case 1:
                    case 2:
                        if (instruction.opcode == OpCodes.Ldc_I4_1)
                            flag++;
                        break;
                    case 3:
                        if (instruction.opcode == OpCodes.Blt)
                            flag = 4;
                        break;
                }
            }

            if (flag < 5)
                Log.Error("ArchitesAffectCapacities patch failed!");
        }

        public static float AugmentCapacity(
            float capValue,
            HediffSet diffSet,
            PawnCapacityDef capacity,
            List<PawnCapacityUtility.CapacityImpactor> impactors,
            bool forTradePrice
        )
        {
            if (forTradePrice)
                return capValue;

            bool useImpactors = impactors != null;

            if (capacity.zeroIfCannotBeAwake && !diffSet.pawn.health.capacities.CanBeAwake)
                return capValue;

            CompArchiteTracker comp = diffSet.pawn?.ArchiteTracker();
            if (comp == null || comp.capacityUpgrades.NullOrEmpty())
                return capValue;

            foreach (CapacityArchiteDef archite in comp.capacityUpgrades.Keys)
            {
                if (archite.capacity != capacity)
                    continue;

                archite.ModifyValueAtLevel(ref capValue, comp.capacityUpgrades[archite]);
                if (useImpactors)
                {
                    CapacityImpactor_Archite impactor = new CapacityImpactor_Archite();
                    impactor.archite = archite;
                    impactors.Add(impactor);
                }
            }

            return capValue;
        }

        public class CapacityImpactor_Archite : PawnCapacityUtility.CapacityImpactor
        {
            public CapacityArchiteDef archite;

            public override string Readable(Pawn pawn)
            {
                return "ArchiteReinforcement.CapacityTipListItem".Translate(archite.LabelCap);
            }
        }
    }
}
