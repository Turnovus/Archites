using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public class CompArchiteTracker : ThingComp
    {
        public Dictionary<StatArchiteDef, int> statUpgrades = new Dictionary<StatArchiteDef, int>();
        public Dictionary<CapacityArchiteDef, int> capacityUpgrades = new Dictionary<CapacityArchiteDef, int>();
        public float statArchiteProgress = 0f;
        public float capacityArchiteProgress = 0f;
        public float statArchitesToSpend = 0f;
        public float capacityArchitesToSpend = 0f;

        private float? cachedTotalStatArchites = null;
        private float? cachedTotalCapacityArchites = null;

        public List<string> ActiveExclusionTags
        {
            get
            {
                List<string> result = new List<string>();
                foreach (CapacityArchiteDef cap in capacityUpgrades.Keys)
                    foreach (string tag in cap.exclusionTags)
                        if (!result.Contains(tag))
                            result.Add(tag);
                foreach (StatArchiteDef stat in statUpgrades.Keys)
                    foreach (string tag in stat.exclusionTags)
                        if (!result.Contains(tag))
                            result.Add(tag);
                return result;
            }
        }

        public float TotalStatArchiteUpgradeValue
        {
            get
            {
                if (cachedTotalStatArchites == null)
                {
                    float n = statArchitesToSpend;
                    foreach (StatArchiteDef def in statUpgrades.Keys)
                        n += def.upgradeValue * statUpgrades[def];
                    cachedTotalStatArchites = n;
                }
                return cachedTotalStatArchites ?? 0;
            }
        }

        public float TotalCapacityArchiteUpgradeValue
        {
            get
            {
                if (cachedTotalCapacityArchites == null)
                {
                    float n = capacityArchitesToSpend;
                    foreach (CapacityArchiteDef def in capacityUpgrades.Keys)
                        n += def.upgradeValue * capacityUpgrades[def];
                    cachedTotalCapacityArchites = n;
                }
                return cachedTotalCapacityArchites ?? 0;
            }
        }

        public float StatUpgradeCost =>
            Math.Max(50, TotalStatArchiteUpgradeValue * 10);

        public float CapacityUpgradeCost =>
            Math.Max(250, TotalCapacityArchiteUpgradeValue * 50);

        public Pawn ParentPawn => parent as Pawn;

        public float MarketValueOffsetFromArchites
        {
            get
            {
                float n = 0f;

                foreach (StatArchiteDef stat in statUpgrades.Keys)
                    n += stat.marketValuePerLevel * statUpgrades[stat];
                foreach (CapacityArchiteDef cap in capacityUpgrades.Keys)
                    n += cap.marketValuePerLevel * capacityUpgrades[cap];

                return n;
            }
        }

        public bool IsUpgradeMaxLevel(ArchiteDef upgrade)
        {
            if (upgrade.maxUses == null)
                return false;

            if (upgrade is StatArchiteDef stat)
            {
                if (!statUpgrades.ContainsKey(stat))
                    return false;
                return statUpgrades[stat] >= stat.maxUses;
            }

            if (upgrade is CapacityArchiteDef cap)
            {
                if (!capacityUpgrades.ContainsKey(cap))
                    return false;
                return capacityUpgrades[cap] >= cap.maxUses;
            }

            return false;
        }

        public bool TryModifyStatValue(StatArchiteDef stat, ref float value)
        {
            if (!statUpgrades.ContainsKey(stat) || statUpgrades[stat] <= 0)
                return false;

            stat.ModifyValueAtLevel(ref value, statUpgrades[stat]);
            return true;
        }

        public bool CanAcceptUpgrade(ArchiteDef upgrade, out string failReason)
        {
            if (upgrade == null)
            {
                failReason = "ArchiteReinforcement.UseFailReasonOther".Translate();
                return false;
            }

            if (IsUpgradeMaxLevel(upgrade))
            {
                failReason = "ArchiteReinforcement.UseFailReasonMaxUsed".Translate();
                return false;
            }

            if (!upgrade.exclusionTags.NullOrEmpty() && !HasAnyLevelOfUpgrade(upgrade))
            {
                List<string> exclusions = ActiveExclusionTags;
                if (!exclusions.NullOrEmpty())
                {
                    foreach (string tag in upgrade.exclusionTags)
                    {
                        if (exclusions.Contains(tag))
                        {
                            failReason = "ArchiteReinforcement.UseFailReasonExcluded".Translate();
                            return false;
                        }
                    }
                }
            }

            failReason = string.Empty;
            return true;
        }

        public bool HasAnyLevelOfUpgrade(ArchiteDef upgrade)
        {
            if (upgrade is StatArchiteDef stat)
                return statUpgrades.ContainsKey(stat);
            if (upgrade is CapacityArchiteDef cap)
                return capacityUpgrades.ContainsKey(cap);
            return false;
        }

        public int LevelForStat(StatArchiteDef stat)
        {
            if (!statUpgrades.ContainsKey(stat))
                return 0;
            return statUpgrades[stat];
        }

        public static CompProperties NewPropsForDefPatch() =>
            new CompProperties()
            {
                compClass = typeof(CompArchiteTracker),
            };

        public void DoUpgrade(ArchiteDef def)
        {
            if (def is StatArchiteDef stat)
            {
                if (!statUpgrades.ContainsKey(stat))
                    statUpgrades[stat] = 0;
                statUpgrades[stat] += 1;
            }

            if (def is CapacityArchiteDef cap)
            {
                if (!capacityUpgrades.ContainsKey(cap))
                    capacityUpgrades[cap] = 0;
                capacityUpgrades[cap] += 1;

                if (parent is Pawn parentPawn)
                    parentPawn.health.capacities.Notify_CapacityLevelsDirty();
            }

            cachedTotalStatArchites = null;
            cachedTotalCapacityArchites = null;
        }

        public void TryBuyUpgrade(ArchiteDef def)
        {
            if (def is StatArchiteDef stat)
            {
                if (statArchitesToSpend < stat.upgradeValue)
                    return;

                statArchitesToSpend -= stat.upgradeValue;
                ParentPawn.records.AddTo(MyDefOf.Turn_Record_ArchitesSpent_Stat, stat.upgradeValue);
            }
            else if (def is CapacityArchiteDef cap)
            {
                if (capacityArchitesToSpend < cap.upgradeValue)
                    return;

                capacityArchitesToSpend -= cap.upgradeValue;
                ParentPawn.records.AddTo(MyDefOf.Turn_Record_ArchitesSpent_Capacity, cap.upgradeValue);
            }

            DoUpgrade(def);
        }

        public void AddStatArchiteProgress(float amount)
        {
            int levelsGained = 0;

            statArchiteProgress += amount;
            while (statArchiteProgress >= StatUpgradeCost)
            {
                statArchiteProgress -= StatUpgradeCost;
                statArchitesToSpend += 1f;
                cachedTotalStatArchites += 1f;
                ParentPawn.records.AddTo(MyDefOf.Turn_Record_ArchitesAcquired_Stat, 1f);
                levelsGained += 1;
            }

            cachedTotalStatArchites = null;
            if (levelsGained > 0)
                NotifyGotUpgrades(parent, levelsGained, "ArchiteReinforcement.UpgradeTypeStat".Translate());
        }

        public void AddCapacityArchiteProgress(float amount)
        {
            int levelsGained = 0;

            capacityArchiteProgress += amount;
            while (capacityArchiteProgress >= CapacityUpgradeCost)
            {
                capacityArchiteProgress -= CapacityUpgradeCost;
                capacityArchitesToSpend += 1f;
                cachedTotalCapacityArchites += 1f;
                ParentPawn.records.AddTo(MyDefOf.Turn_Record_ArchitesAcquired_Capacity, 1f);
                levelsGained += 1;
            }

            cachedTotalCapacityArchites = null;
            if (levelsGained > 0)
                NotifyGotUpgrades(parent, levelsGained, "ArchiteReinforcement.UpgradeTypeCapacity".Translate());
        }

        public void AddTotalArchiteProgress(float amount)
        {
            AddStatArchiteProgress(amount);
            AddCapacityArchiteProgress(amount);
        }

        public void AddCapacityLevel() =>
            AddCapacityArchiteProgress(CapacityUpgradeCost);

        public void AddStatLevel() =>
            AddStatArchiteProgress(StatUpgradeCost);

        public static void NotifyGotUpgrades(Thing pawn, int levels, string upgradeTypeLabel)
        {
            string fullString = levels == 1 ? "ArchiteReinforcement.PawnGainedUpgrade" : "ArchiteReinforcement.PawnGainedUpgrade.Plural";
            fullString = fullString.Translate(pawn.LabelCap, upgradeTypeLabel, levels);

            Messages.Message(fullString, pawn, MessageTypeDefOf.PositiveEvent);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!DebugSettings.ShowDevGizmos)
                yield break;

            yield return new Command_Action()
            {
                defaultLabel = "DEV: Add Capacity Archite",
                action = delegate
                {
                    AddCapacityLevel();
                },
            };

            yield return new Command_Action()
            {
                defaultLabel = "DEV: Add Stat Archite",
                action = delegate
                {
                    AddStatLevel();
                },
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(ref statUpgrades, "statUpgrades");
            Scribe_Collections.Look(ref capacityUpgrades, "capacityUpgrades");
            Scribe_Values.Look(ref statArchiteProgress, "statArchiteProgress");
            Scribe_Values.Look(ref capacityArchiteProgress, "capacityArchiteProgress");
            Scribe_Values.Look(ref statArchitesToSpend, "statArchitesToSpend");
            Scribe_Values.Look(ref capacityArchitesToSpend, "capacityArchitesToSpend");
        }
    }
}
