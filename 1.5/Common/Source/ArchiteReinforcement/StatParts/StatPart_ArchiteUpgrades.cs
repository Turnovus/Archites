using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    class StatPart_ArchiteUpgrades : StatPart
    {
#pragma warning disable CS0649
        public StatArchiteDef architeDef;
#pragma warning restore CS0649

        public static Pawn ReqPawn(StatRequest req) =>
            req.Thing as Pawn;

        public override string ExplanationPart(StatRequest req)
        {
            Pawn pawn = ReqPawn(req);

            if (pawn == null)
                return string.Empty;

            CompArchiteTracker comp = pawn.ArchiteTracker();
            if (comp == null)
                return string.Empty;

            if (comp.LevelForStat(architeDef) <= 0)
                return string.Empty;

            return "ArchiteReinforcement.ArchiteListExplanation".Translate(architeDef.ValueReadoutAtLevel(comp.LevelForStat(architeDef)));
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            Pawn pawn = ReqPawn(req);
            if (pawn == null)
                return;

            CompArchiteTracker comp = pawn.TryGetComp<CompArchiteTracker>();
            if (comp == null)
                return;

            comp.TryModifyStatValue(architeDef, ref val);
        }
    }
}
