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
    
    [HarmonyPatch(typeof(PawnCapacityUtility))]
    [HarmonyPatch(nameof(PawnCapacityUtility.CalculateCapacityLevel))]
    class ArchitesAffectCapacities
    {
        [HarmonyPostfix]
        public static void AugmentCapacity(
            ref float __result,
            HediffSet diffSet,
            PawnCapacityDef capacity,
            List<PawnCapacityUtility.CapacityImpactor> impactors,
            bool forTradePrice
        )
        {
            if (forTradePrice)
                return;

            bool useImpactors = impactors != null;

            if (capacity.zeroIfCannotBeAwake && !diffSet.pawn.health.capacities.CanBeAwake)
                return;

            CompArchiteTracker comp = diffSet.pawn?.ArchiteTracker();
            if (comp == null || comp.capacityUpgrades.NullOrEmpty())
                return;

            foreach (CapacityArchiteDef archite in comp.capacityUpgrades.Keys)
            {
                if (archite.capacity != capacity)
                    continue;

                archite.ModifyValueAtLevel(ref __result, comp.capacityUpgrades[archite]);
                if (useImpactors)
                {
                    CapacityImpactor_Archite impactor = new CapacityImpactor_Archite();
                    impactor.archite = archite;
                    impactors.Add(impactor);
                }
            }
        }

        // TODO: Patch HealthCardUtility.GetPawnCapacityTip to write our custom impactor
        public class CapacityImpactor_Archite : PawnCapacityUtility.CapacityImpactor
        {
            public CapacityArchiteDef archite;

            public override string Readable(Pawn pawn)
            {
                return string.Format("{0}", archite.LabelCap); //TODO
            }
        }
    }
}
