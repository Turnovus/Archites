using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public class ArchiteStatUpgradeExtension : DefModExtension
    {
#pragma warning disable CS0649
        public StatArchiteDef upgrade;
#pragma warning restore CS0649

        public override IEnumerable<string> ConfigErrors()
        {
            if (upgrade == null)
                yield return "No upgrade set.";
        }
    }

    public class PlayerPossessionCountsMinified : DefModExtension { }

    public class FactionExtension : DefModExtension
    {
#pragma warning disable CS0649
        public float factionMemberArchiteGenChanceFactor = 1f;
        public float grantedTitleSeniorityArchiteGenChanceFactor = 1f;

        public FloatRange memberAnyPointFactorRandom;
        public FloatRange memberCapacityPointFactorRandom;
        public FloatRange memberStatPointFactorRandom;

        public bool forceArchiteGen = false;
        public bool allowPlayerFactionArchites = false;
#pragma warning restore CS0649
    }


}
