using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    class StatPart_SpentArchites : StatPart
    {
        public static float OffsetForPawn(Pawn pawn)
        {
            CompArchiteTracker comp = pawn.ArchiteTracker();
            if (comp == null)
                return 0f;

            return comp.MarketValueOffsetFromArchites;
        }

        public override string ExplanationPart(StatRequest req)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null)
                return string.Empty;

            float offset = OffsetForPawn(pawn);
            if (offset == 0f)
                return string.Empty;

            return "ArchiteReinforcement.ArchiteUsedListExplanation".Translate(offset.ToStringWithSign());
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null)
                return;

            float offset = OffsetForPawn(pawn);
            if (offset != 0f)
                val += offset;
        }
    }
}
