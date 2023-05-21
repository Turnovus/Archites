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

        public static RecordDef Turn_Record_ArchitesAcquired_Stat;
        public static RecordDef Turn_Record_ArchitesAcquired_Capacity;
        public static RecordDef Turn_Record_ArchitesSpent_Stat;
        public static RecordDef Turn_Record_ArchitesSpent_Capacity;

        static MyDefOf() =>
            DefOfHelper.EnsureInitializedInCtor(typeof(MyDefOf));
    }
}
