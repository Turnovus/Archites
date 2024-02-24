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
        public const float ArchiteGenBaseChance = 1f / 75f;
        // This should pretty much guarantee that at least one of the faction leaders has archites.
        public const float ArchiteGenChanceFactorFactionLeader = 70f;

        public static readonly FloatRange CapacityArchiteRandRange = new FloatRange(8f, 35f);
        public static readonly FloatRange StatArchiteRandRange = new FloatRange(10f, 40f);

        public static bool ShouldGrantArchitesTo(Pawn pawn)
        {
            // Prevent all archite spawns if the player has disabled that.
            if (!Mod_ArchiteReinforcement.ActiveMod.Settings.enableArchiteSpawns)
                return false;

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
            FactionExtension extension = faction?.def.GetModExtension<FactionExtension>();
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
            float chance = ArchiteGenBaseChance;
            chance *= ArchiteGenChanceFactorFromFaction(pawn);
            chance *= ArchiteGenChanceFactorFromNobility(pawn);

            return chance;
        }

        private static float ArchiteGenChanceFactorFromFaction(Pawn pawn)
        {
            float factor = 1f;
            Faction faction = pawn.Faction;
            if (faction == null)
                return factor;

            if (faction.leader == pawn)
                factor *= ArchiteGenChanceFactorFactionLeader;

            FactionExtension extension = faction.def.GetModExtension<FactionExtension>();
            if (extension != null)
                factor *= extension.factionMemberArchiteGenChanceFactor;

            return factor;
        }

        private static float ArchiteGenChanceFactorFromNobility(Pawn pawn)
        {
            float factor = 1f;

            foreach (RoyalTitle title in pawn.royalty.AllTitlesForReading)
            {
                FactionExtension extension = title.faction.def.GetModExtension<FactionExtension>();
                if (extension == null)
                    continue;

                factor += extension.genChanceFacterOffsetPerTitleSeniority * title.def.seniority;
            }

            return factor;
        }

        public static float CapacityUpgradePointsFor(Pawn pawn)
        {
            float archites = CapacityArchiteRandRange.RandomInRange;
            archites *= GetRandomCapacityArchiteFactorFor(pawn);
            return (float)Math.Round(archites, 1);
        }

        public static float StatUpgradePointsFor(Pawn pawn)
        {
            float archites = StatArchiteRandRange.RandomInRange;
            archites *= GetRandomStatArchiteFactorFor(pawn);
            return (float)Math.Round(archites, 1);
        }

        private static float GetRandomCapacityArchiteFactorFor(Pawn pawn)
        {
            float factor = GetRandomBaseArchiteFactorFor(pawn);

            FactionExtension faction = pawn.Faction?.def.GetModExtension<FactionExtension>();
            if (faction?.memberAnyPointFactorRandom != null)
                factor *= faction.memberCapacityPointFactorRandom.RandomInRange;

            return factor;
        }

        private static float GetRandomStatArchiteFactorFor(Pawn pawn)
        {
            float factor = GetRandomBaseArchiteFactorFor(pawn);

            FactionExtension faction = pawn.Faction?.def.GetModExtension<FactionExtension>();
            if (faction?.memberAnyPointFactorRandom != null)
                factor *= faction.memberStatPointFactorRandom.RandomInRange;

            return factor;
        }

        private static float GetRandomBaseArchiteFactorFor(Pawn pawn)
        {
            float factor = 1f;

            FactionExtension faction = pawn.Faction?.def.GetModExtension<FactionExtension>();
            if (faction?.memberAnyPointFactorRandom != null)
                factor *= faction.memberAnyPointFactorRandom.RandomInRange;

            return factor;
        }
    }
}
