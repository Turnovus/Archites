using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace ArchiteReinforcement
{
    public class JobDriver_ExtractArchites : JobDriver
    {
        public const TargetIndex PillarIndex = TargetIndex.A;
        public const int Duration = 200;

        private Building_ArchitePillar Pillar =>
            job.GetTarget(PillarIndex).Thing as Building_ArchitePillar;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Pillar, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(PillarIndex);
            this.FailOnBurningImmobile(PillarIndex);
            yield return Toils_Goto.GotoThing(PillarIndex, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration)
                .FailOnDestroyedNullOrForbidden(PillarIndex)
                .FailOnCannotTouch(PillarIndex, PathEndMode.Touch);

            Toil extract = ToilMaker.MakeToil("MakeNewToils");
            extract.initAction = delegate { Pillar.TransferArchites(pawn); };
            extract.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return extract;
        }
    }

    public class JobGiver_ExtractArchites : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Predicate<Thing> validator = delegate(Thing t)
            {
                if (!(t is Building_ArchitePillar pillar))
                    return false;
                if (pillar.IsForbidden(pawn))
                    return false;
                if (pillar.IsBurning())
                    return false;
                return pillar.AssignedPawn == pawn && pillar.CanWithdraw;
            };
            Thing thing = GenClosest.ClosestThing_Global_Reachable(
                pawn.Position,
                pawn.Map,
                pawn.Map.listerBuildings.allBuildingsColonist,
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                9999f,
                validator
            );
            if (thing == null)
                return null;
            return JobMaker.MakeJob(MyDefOf.Turn_Job_ExtractPillarArchites, thing);
        }
    }
}
