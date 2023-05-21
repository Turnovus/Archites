using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public class CompUseEffect_ArchiteProgress : CompUseEffect
    {
        public float? storedArchites = null;

        public CompProperties_UseEffectArchiteProgress Props =>
            props as CompProperties_UseEffectArchiteProgress;

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            CompArchiteTracker comp = p.ArchiteTracker();
            if (comp == null)
            {
                failReason = "ArchiteReinforcement.UseFailReasonNoComp".Translate();
                return false;
            }

            failReason = string.Empty;
            return true;
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            CompArchiteTracker comp = usedBy.ArchiteTracker();
            if (comp == null)
            {
                Log.Error("Pawn {0} used archite item without archite comp".Formatted(usedBy.ToString()));
                return;
            }

            comp.AddTotalArchiteProgress(storedArchites ?? Props.defaultStoredArchites);
        }
    }

    public class CompProperties_UseEffectArchiteProgress : CompProperties_UseEffect
    {
        public float defaultStoredArchites = 1f;

        public CompProperties_UseEffectArchiteProgress()
        {
            compClass = typeof(CompUseEffect_ArchiteProgress);
        }
    }
}
