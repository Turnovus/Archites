using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public class Thought_ArchiteMood : Thought_Situational
    {
        public override int CurStageIndex => 0;

        protected override float BaseMoodOffset
        {
            get
            {
                ArchiteStatUpgradeExtension ext = def.GetModExtension<ArchiteStatUpgradeExtension>();
                return pawn.ArchiteTracker()?.LevelForImpliedUpgrade(ext.upgrade) ?? 0f;
            }
        }
    }

    public class ThoughtWorker_ArchiteMood : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            ArchiteStatUpgradeExtension ext = def.GetModExtension<ArchiteStatUpgradeExtension>();
            return p.ArchiteTracker()?.HasAnyLevelOfImpliedUpgrade(ext.upgrade) == true;
        }
    }
}
