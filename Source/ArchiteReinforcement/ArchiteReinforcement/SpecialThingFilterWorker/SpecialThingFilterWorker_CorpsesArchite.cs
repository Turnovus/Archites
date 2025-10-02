using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public class SpecialThingFilterWorker_CorpsesArchite : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            if (!(t is Corpse corpse))
                return false;

            CompArchiteTracker tracker = corpse.InnerPawn.ArchiteTracker();
            if (tracker == null)
                return false;

            return AcceptTracker(tracker);
        }

        protected virtual bool AcceptTracker(CompArchiteTracker tracker) => tracker.HasAnyUpgrades;

        public override bool CanEverMatch(ThingDef def)
        {
            return def.IsCorpse;
        }
    }

    public class SpecialThingFilterWorker_CorpsesArchiteInverted : SpecialThingFilterWorker_CorpsesArchite
    {
        protected override bool AcceptTracker(CompArchiteTracker tracker) =>
            !base.AcceptTracker(tracker);
    }
}
