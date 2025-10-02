using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace ArchiteReinforcement
{
    [HarmonyPatch(typeof(PawnTechHediffsGenerator))]
    [HarmonyPatch(nameof(PawnTechHediffsGenerator.GenerateTechHediffsFor))]
    class GenerateTechHediffsFor_Patch
    {
        [HarmonyPostfix]
        public static void TryApplyArchiteUpgradesTo(Pawn pawn)
        {
            ProcessNewlyGeneratedPawn(pawn);
        }

        public static void ProcessNewlyGeneratedPawn(Pawn pawn)
        {
            CompArchiteTracker tracker = pawn.GetComp<CompArchiteTracker>();
            if (tracker == null)
                return;

            if (!PawnGenArchiteCalculator.ShouldGrantArchitesTo(pawn))
                return;

            tracker.capacityArchitesToSpend = PawnGenArchiteCalculator.CapacityUpgradePointsFor(pawn);
            tracker.statArchitesToSpend = PawnGenArchiteCalculator.StatUpgradePointsFor(pawn);

            PawnGenArchiteAllocator.AllocateAllUnspentPoints(pawn);
        }
    }
}
