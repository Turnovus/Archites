using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public class CompUseEffect_AddToRecord : CompUseEffect
    {
        public CompProperties_UseEffectAddToRecord Props =>
            props as CompProperties_UseEffectAddToRecord;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            usedBy.records.AddTo(Props.record, Props.amount);
        }
    }

    public class CompProperties_UseEffectAddToRecord : CompProperties_UseEffect
    {
        public RecordDef record;
        public float amount;

        public CompProperties_UseEffectAddToRecord() =>
            compClass = typeof(CompUseEffect_AddToRecord);
    }
}
