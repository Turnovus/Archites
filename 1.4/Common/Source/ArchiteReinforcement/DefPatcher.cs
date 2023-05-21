using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    [StaticConstructorOnStartup]
    public static class DefPatcher
    {
        public static Dictionary<string, List<ArchiteDef>> exclusionGroups;

        static DefPatcher()
        {
            exclusionGroups = new Dictionary<string, List<ArchiteDef>>();

            foreach (StatArchiteDef statArchiteDef in DefDatabase<StatArchiteDef>.AllDefs)
            {
                if (statArchiteDef.exclusionTags.NullOrEmpty())
                    continue;

                foreach (string tag in statArchiteDef.exclusionTags)
                {
                    if (!exclusionGroups.ContainsKey(tag))
                        exclusionGroups[tag] = new List<ArchiteDef>();
                    exclusionGroups[tag].Add(statArchiteDef);
                }
            }

            foreach (CapacityArchiteDef capacityArchiteDef in DefDatabase<CapacityArchiteDef>.AllDefs)
            {
                if (capacityArchiteDef.exclusionTags.NullOrEmpty())
                    continue;

                foreach (string tag in capacityArchiteDef.exclusionTags)
                {
                    if (!exclusionGroups.ContainsKey(tag))
                        exclusionGroups[tag] = new List<ArchiteDef>();
                    exclusionGroups[tag].Add(capacityArchiteDef);
                }
            }
        }

        public static CompArchiteTracker ArchiteTracker(this Pawn pawn) =>
            pawn.TryGetComp<CompArchiteTracker>();
    }
}
