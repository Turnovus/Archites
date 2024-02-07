using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    /// <summary>
    /// This class adds archite upgrades to a pawn given the necessary parameters. Its purpose is
    /// to decide how archites are spent, but not how many there are to spend.
    /// </summary>
    public static class PawnGenArchiteAllocator
    {
        public static void AllocateAllUnspentPoints(CompArchiteTracker tracker)
        {
            AllocateUnspentCapacityPoints(tracker);
            AllocateUnspentStatPoints(tracker);
        }

        private static void AllocateUnspentCapacityPoints(CompArchiteTracker tracker)
        {

        }

        private static void AllocateUnspentStatPoints(CompArchiteTracker tracker)
        {

        }
    }
}
