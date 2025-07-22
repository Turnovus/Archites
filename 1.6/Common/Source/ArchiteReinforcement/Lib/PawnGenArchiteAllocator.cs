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
        private const float AddUpgradeBaseChance = 0.1f;
        private const float AddUpgradeFactorFromLevelSpread = 0.8f;
        private const float AddUpgradeFactorFromLowestLevel = 1.1f;
        private const float MinBudgetToContinueSelection = 1.1f;
        private const int MaxUpgradeSelectionIterations = 1_000;

        public static void AllocateAllUnspentPoints(Pawn pawn)
        {
            AllocateUnspentCapacityPoints(pawn);
            AllocateUnspentStatPoints(pawn);
        }

        private static void AllocateUnspentCapacityPoints(Pawn pawn)
        {
            CompArchiteTracker tracker = pawn.ArchiteTracker();
            if (tracker == null)
                return;

            ApplyRandomUpgradesUsingStoredPoints(
                pawn,
                DefDatabase<CapacityArchiteDef>.AllDefs,
                tracker.capacityArchitesToSpend
            );
        }

        private static void AllocateUnspentStatPoints(Pawn pawn)
        {
            CompArchiteTracker tracker = pawn.ArchiteTracker();
            if (tracker == null)
                return;

            ApplyRandomUpgradesUsingStoredPoints(
                pawn,
                DefDatabase<StatArchiteDef>.AllDefs,
                tracker.statArchitesToSpend
            );
        }

        private static void ApplyRandomUpgradesUsingStoredPoints(
            Pawn pawn,
            IEnumerable<ArchiteDef> possibleUpgrades,
            float upgradePointBudget
        )
        {
            CompArchiteTracker tracker = pawn.ArchiteTracker();

            Dictionary<ArchiteDef, int> selectedUpgrades = SelectUpgradesFromPool(pawn, possibleUpgrades, upgradePointBudget);

            foreach (ArchiteDef selectedUpgrade in selectedUpgrades.Keys)
            {
                for (int i = 0; i < selectedUpgrades[selectedUpgrade]; i++)
                {
                    tracker.TryBuyUpgrade(selectedUpgrade);
                }
            }
        }

        private static Dictionary<ArchiteDef, int> SelectUpgradesFromPool(
            Pawn pawn,
            IEnumerable<ArchiteDef> possibleUpgrades,
            float upgradePointBudget
        )
        {
            CompArchiteTracker tracker = pawn.ArchiteTracker();
            Dictionary<ArchiteDef, int> selectedUpgrades = new Dictionary<ArchiteDef, int>();
            List<string> selectedExclusionTags = new List<string>(tracker.ActiveExclusionTags);
            int iterations = 0;

            while (iterations < MaxUpgradeSelectionIterations)
            {
                if (upgradePointBudget <= MinBudgetToContinueSelection)
                    break;

                ArchiteDef upgrade;

                if (Rand.Chance(AddNewUpgradeChance()))
                {
                    upgrade = PickRandomUpgrade(possibleUpgrades);
                    if (upgrade != null)
                    {
                        IncreaseUpgrade(upgrade);
                        continue;
                    }
                }

                upgrade = PickRandomUpgrade(selectedUpgrades.Keys);
                if (upgrade != null)
                    IncreaseUpgrade(upgrade);

                iterations++;
            }

            return selectedUpgrades;


            float AddNewUpgradeChance()
            {
                if (selectedUpgrades.NullOrEmpty())
                    return 1f;

                float chance = AddUpgradeBaseChance;

                int lowest = -1;
                int highest = -1;

                foreach (ArchiteDef key in selectedUpgrades.Keys)
                {
                    int level = selectedUpgrades[key];
                    if (lowest == -1 || level < lowest)
                        lowest = level;
                    if (highest == -1 || level > highest)
                        highest = level;
                }

                int levelSpread = highest - lowest;
                chance *= (float)Math.Pow(AddUpgradeFactorFromLevelSpread, levelSpread);
                chance *= (float)Math.Pow(AddUpgradeFactorFromLowestLevel, lowest);

                return chance;
            }

            ArchiteDef PickRandomUpgrade(IEnumerable<ArchiteDef>options)
            {
                IEnumerable<ArchiteDef> acceptableOptions = options.Where(a => CanPickUpgrade(a));
                if (acceptableOptions.EnumerableNullOrEmpty())
                    return null;

                return acceptableOptions.RandomElementByWeight(a => UpgradeWeightForPawn(a));
            }

            bool CanPickUpgrade(ArchiteDef upgrade)
            {
                if (!selectedUpgrades.Keys.Contains(upgrade) && !upgrade.exclusionTags.NullOrEmpty())
                {
                    foreach (string tag in upgrade.exclusionTags)
                    {
                        if (selectedExclusionTags.Contains(tag))
                            return false;
                    }
                }

                if (upgradePointBudget < upgrade.upgradeValue)
                    return false;

                int currentLevel = ValueOfSelectedUpgrade(upgrade) + tracker.LevelForUpgrade(upgrade);
                if (upgrade.maxUses <= currentLevel)
                    return false;

                return true;
            }

            int ValueOfSelectedUpgrade(ArchiteDef upgrade)
            {
                if (selectedUpgrades.ContainsKey(upgrade))
                    return selectedUpgrades[upgrade];
                return 0;
            }

            void IncreaseUpgrade(ArchiteDef upgrade)
            {
                if (!selectedUpgrades.ContainsKey(upgrade))
                    selectedUpgrades[upgrade] = 0;
                selectedUpgrades[upgrade] += 1;

                foreach (string tag in upgrade.exclusionTags)
                    if (!selectedExclusionTags.Contains(tag))
                        selectedExclusionTags.Add(tag);
                
                upgradePointBudget -= upgrade.upgradeValue;
            }

            float UpgradeWeightForPawn(ArchiteDef upgrade)
            {
                // TODO
                return 1f;
            }
        }
    }
}
