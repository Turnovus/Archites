using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    class DebugActions
    {
        [DebugAction("Archite", "One of every artifact", false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static DebugActionNode SpawnAllPods() =>
            new DebugActionNode("Spawn One of Every Artifact", DebugActionType.ToolMap, () =>
            {
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def.comps.Any((x) => x.compClass == typeof(CompUseEffect_ArchiteUpgrade)))
                        GenPlace.TryPlaceThing(ThingMaker.MakeThing(def), UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);
                }
            });

        [DebugAction("Archite", null, false, false, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void GiveRandomUpgrades(Pawn p)
        {
            CompArchiteTracker tracker = p.ArchiteTracker();
            if (tracker == null)
                return;

            tracker.capacityArchitesToSpend += 100f;
            tracker.statArchitesToSpend += 100f;
            PawnGenArchiteAllocator.AllocateAllUnspentPoints(p);
        }

    }
}
