using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public class HediffComp_ArchiteGiver : HediffComp
    {
        HediffCompProperties_ArchiteGiver Props => (HediffCompProperties_ArchiteGiver)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            float mtbNow = Props.mtbDays;
            if (Props.divideBySeverity)
                mtbNow /= parent.Severity;

            if (Rand.MTBEventOccurs(mtbNow, GenDate.TicksPerDay, 1f))
            {
                CompArchiteTracker comp = Pawn.ArchiteTracker();
                if (comp == null)
                    return;
                if (Rand.Chance(Props.extraArchiteChance))
                {
                    comp.AddCapacityArchiteProgress(1f);
                    comp.AddStatArchiteProgress(1f);
                    return;
                }
                if (Rand.Chance(0.5f))
                {
                    comp.AddCapacityArchiteProgress(1f);
                }
                else
                {
                    comp.AddStatArchiteProgress(1f);
                }
            }
        }
    }

    public class HediffCompProperties_ArchiteGiver : HediffCompProperties
    {
        public float mtbDays;
        public float extraArchiteChance;
        public bool divideBySeverity;

        public HediffCompProperties_ArchiteGiver() => compClass = typeof(HediffComp_ArchiteGiver);
    }
}
