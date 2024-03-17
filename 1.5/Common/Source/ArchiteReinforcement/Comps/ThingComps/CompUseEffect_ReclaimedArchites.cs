﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    class CompUseEffect_ReclaimedArchites : CompUseEffect
    {
        public float capacityArchites;
        public float statArchites;

        public override string CompInspectStringExtra()
        {
            return "ArchiteReinforcement.InspectCapacityCount".Translate(capacityArchites.ToString()) + "\n" +
                "ArchiteReinforcement.InspectStatCount".Translate(statArchites.ToString());
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            CompArchiteTracker comp = p.ArchiteTracker();
            if (comp == null)
                return "ArchiteReinforcement.UseFailReasonNoComp".Translate();

            return true;
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            CompArchiteTracker comp = usedBy.ArchiteTracker();
            if (comp == null)
                return;

            comp.AddCapacityArchiteProgress(capacityArchites);
            comp.AddStatArchiteProgress(statArchites);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref capacityArchites, "capacityArchites");
            Scribe_Values.Look(ref statArchites, "statArchites");
        }
    }
}
