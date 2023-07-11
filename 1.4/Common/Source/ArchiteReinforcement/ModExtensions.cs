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
}
