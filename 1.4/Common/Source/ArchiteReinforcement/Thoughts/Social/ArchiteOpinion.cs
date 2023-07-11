using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public abstract class Thought_ArchiteOpinion : Thought_SituationalSocial
    {
        public virtual float LevelOf(Pawn p)
        {
            ArchiteStatUpgradeExtension ext = def.GetModExtension<ArchiteStatUpgradeExtension>();
            return pawn.ArchiteTracker()?.LevelForImpliedUpgrade(ext.upgrade) ?? 0f;
        }
    }

    // Pod user has this thought about everyone else.
    public class Thought_ArchiteOpinionOutgoing : Thought_ArchiteOpinion
    {
        public override float OpinionOffset()
        {
            // My level affects how much I like the other pawn.
            return LevelOf(pawn);
        }
    }

    // Everyone else has this thought about pod user.
    public class Thought_ArchiteOpinionIncoming : Thought_ArchiteOpinion
    {
        public override float OpinionOffset()
        {
            // The other pawn's level affects how much I like the other pawn.
            return LevelOf(otherPawn);
        }
    }

    public class ThoughtWorker_ArchiteOpinionOutgoing : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            ArchiteStatUpgradeExtension ext = def.GetModExtension<ArchiteStatUpgradeExtension>();
            return otherPawn.IsColonist && p.ArchiteTracker()?.HasAnyLevelOfImpliedUpgrade(ext.upgrade) == true;
        }
    }

    public class ThoughtWorker_ArchiteOpinionIncoming : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            ArchiteStatUpgradeExtension ext = def.GetModExtension<ArchiteStatUpgradeExtension>();
            return otherPawn.ArchiteTracker()?.HasAnyLevelOfImpliedUpgrade(ext.upgrade) == true;
        }
    }
}
