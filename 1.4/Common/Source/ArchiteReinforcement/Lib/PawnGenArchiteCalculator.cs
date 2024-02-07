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
    /// This class encapsulates all of the calculations that determine if a pawn should be
    /// generated with archites, and how many archites they should generate with. 
    /// </summary>
    public static class PawnGenArchiteCalculator
    {
        public const float ArchiteGenBaseChance = 1f / 5_000f;

        public static readonly FloatRange CapacityArchiteRandRange = new FloatRange(8f, 35f);
        public static readonly FloatRange StatArchiteRandRange = new FloatRange(10f, 40f);

        public static bool ShouldGrantArchitesTo(Pawn pawn)
        {
            // N.B.: Forcing archites takes precedence over forbidding them. This means that 
            // individuals with archites are more important than entire communities without, or
            // vice-versa.
            if (ArchitesForcedFor(pawn))
                return true;

            if (ArchitesForbiddenFor(pawn))
                return false;

            return Rand.Chance(ArchiteGenChanceFor(pawn));
        }

        private static bool ArchitesForcedFor(Pawn pawn)
        {
            if (FactionForcesArchites(pawn.Faction))
                return true;

            return false;
        }

        private static bool FactionForcesArchites(Faction faction)
        {
            FactionExtension extension = faction.def.GetModExtension<FactionExtension>();
            return extension == null ? false : extension.forceArchiteGen;
        }

        private static bool ArchitesForbiddenFor(Pawn pawn)
        {
            if (PlayerFactionPreventsArchites(pawn.Faction))
                return true;

            return false;
        }

        private static bool PlayerFactionPreventsArchites(Faction faction)
        {
            if (faction == null || !faction.IsPlayer)
                return false;

            FactionExtension extension = faction.def.GetModExtension<FactionExtension>();
            if (extension == null)
                return true; // We are the player faction, and no explicit override was given.

            return !extension.allowPlayerFactionArchites;
        }

        public static float ArchiteGenChanceFor(Pawn pawn)
        {
            return 1f; // TODO: Use the actual base chance instead of just giving everybody archites.
        }

        public static float CapacityUpgradePointsFor(Pawn pawn)
        {
            float archites = CapacityArchiteRandRange.RandomInRange;
            return (float)Math.Round(archites, 1);
        }

        public static float StatUpgradePointsFor(Pawn pawn)
        {
            float archites = StatArchiteRandRange.RandomInRange;
            return (float)Math.Round(archites, 1);
        }
    }
}
