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
    [HarmonyPatch(typeof(HealthCardUtility))]
    [HarmonyPatch(nameof(HealthCardUtility.GetPawnCapacityTip))]
    class CapacityTipPatch
    {
        [HarmonyPostfix]
        public static void AppendArchiteReadout(
            Pawn pawn,
            PawnCapacityDef capacity,
            ref string __result
        )
        {
            if (pawn.ArchiteTracker()?.HasAnyUpgradeForCapacity(capacity) == true)
                // re-use the string for the archites ITab, I'm sure this won't cause problems later.
                __result = __result + "  " + "ArchiteReinforcement.ITab_Pawn_ArchiteUpgrades".Translate();
        }
    }
}
