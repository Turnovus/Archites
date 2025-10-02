using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    [DefOf]
    public static class MyDefOf
    {
        public static JobDef Turn_Job_ExtractPillarArchites;

        public static RecordDef Turn_Record_ArchitesAcquired_Stat;
        public static RecordDef Turn_Record_ArchitesAcquired_Capacity;
        public static RecordDef Turn_Record_ArchitesSpent_Stat;
        public static RecordDef Turn_Record_ArchitesSpent_Capacity;

        public static ThingDef Turn_ArchiteReinforcement_Chamber;
        public static ThingDef Turn_Building_ArchitePillar;

        static MyDefOf() =>
            DefOfHelper.EnsureInitializedInCtor(typeof(MyDefOf));
    }
}
