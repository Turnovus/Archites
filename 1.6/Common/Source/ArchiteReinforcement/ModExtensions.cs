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
        public float genChanceFacterOffsetPerTitleSeniority = 1f;

        public FloatRange memberAnyPointFactorRandom = new FloatRange(1f, 1f);
        public FloatRange memberCapacityPointFactorRandom = new FloatRange(1f, 1f);
        public FloatRange memberStatPointFactorRandom = new FloatRange(1f, 1f);

        public bool forceArchiteGen = false;
        public bool allowPlayerFactionArchites = false;
#pragma warning restore CS0649
    }

    public class XenotypeExtension : DefModExtension
    {
#pragma warning disable CS0649
        public float architeGenChanceFactor = 1f;

        public bool architesNeverForbidden = false;
#pragma warning restore CS0649
    }
}
