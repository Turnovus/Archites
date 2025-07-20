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
    [HarmonyPatch(typeof(HealthCardUtility))]
    [HarmonyPatch(nameof(HealthCardUtility.GetPawnCapacityTip))]
    class CapacityTipPatch
    {
        public static readonly MethodInfo M_AppendArchiteTipInfo =
            typeof(CapacityTipPatch).GetMethod(nameof(CapacityTipPatch.AppendArchiteTipInfo));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AppendArchiteReadout(
            IEnumerable<CodeInstruction> instructions
        )
        {
            // Nothing too fancy, we just need to grab the right internal variables and then
            // modify the StringBuilder right before it is returned.
            int numInstructions = instructions.Count();

            for (int index = 0; index < numInstructions; ++index)
            {
                // Inject our method right before the string builder is pushed onto the stack and
                // has the result of its ToString method returned.
                if (index == numInstructions - 3)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_3); // stringBuilder
                    yield return new CodeInstruction(OpCodes.Ldloc_1); // impactors
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // pawn
                    yield return new CodeInstruction(OpCodes.Call, M_AppendArchiteTipInfo);
                }
                yield return instructions.ElementAt(index);
            }


        }

        public static void AppendArchiteTipInfo(StringBuilder result, List<PawnCapacityUtility.CapacityImpactor> impactors, Pawn pawn)
        {
            List<ArchitesAffectCapacities.CapacityImpactor_Archite> archites =
                new List<ArchitesAffectCapacities.CapacityImpactor_Archite>();

            foreach (PawnCapacityUtility.CapacityImpactor impactor in impactors)
            {
                if (impactor is ArchitesAffectCapacities.CapacityImpactor_Archite archite)
                    archites.Add(archite);
            }

            if (archites.Count <= 0)
                return;

            result.AppendLine("ArchiteReinforcement.CapacityImpactorArchites".Translate());

            foreach (ArchitesAffectCapacities.CapacityImpactor_Archite archite in archites)
            {
                result.AppendLine(archite.Readable(pawn));
            }
        }
    }
}
