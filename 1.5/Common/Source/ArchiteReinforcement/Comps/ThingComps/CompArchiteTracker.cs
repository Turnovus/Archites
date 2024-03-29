﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace ArchiteReinforcement
{
    public class CompArchiteTracker : ThingComp
    {
        public const float StartStatCost = 40f;
        public const float StartCapCost = 80f;
        public const float StatCostPerArchite = 8f;
        public const float CapCostPerArchite = 12f;
        // The first n upgrade points earned don't count towards the archite cost of additional points
        public const float CapUpgradeGrace = 15f;
        public const float StatUpgradeGrace = 15f;

        public Dictionary<StatArchiteDef, int> statUpgrades = new Dictionary<StatArchiteDef, int>();
        public Dictionary<CapacityArchiteDef, int> capacityUpgrades = new Dictionary<CapacityArchiteDef, int>();
        public float statArchiteProgress = 0f;
        public float capacityArchiteProgress = 0f;
        public float statArchitesToSpend = 0f;
        public float capacityArchitesToSpend = 0f;

        private float? cachedTotalStatArchites = null;
        private float? cachedTotalCapacityArchites = null;
        // There are only going to be a small handful of implied archite defs, so we can set them
        // aside into their own collection so that other parts of code can find them faster on pawns
        // that have a ton of unique upgrade types.
        // This collection can only store stat archite defs, because only those can be implicit.
        private Dictionary<StatArchiteDef, int> impliedLevelsCached = null;

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

        public Dictionary<StatArchiteDef, int> ImpliedLevels
        {
            get
            {
                if(impliedLevelsCached == null)
                {
                    impliedLevelsCached = new Dictionary<StatArchiteDef, int>();
                    foreach (StatArchiteDef def in statUpgrades.Keys)
                    {
                        if (def.IsImplied)
                            impliedLevelsCached[def] = statUpgrades[def];
                    }
                }
                return impliedLevelsCached;
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

        public bool HasAnyUpgrades =>
            TotalCapacityArchiteUpgradeValue > 0 ||
            TotalStatArchiteUpgradeValue > 0;

        public float StatUpgradeCost =>
            CalculateUpgradeCost(TotalStatArchiteUpgradeValue, StatCostPerArchite, StartStatCost, StatUpgradeGrace);

        public float CapacityUpgradeCost =>
            CalculateUpgradeCost(TotalCapacityArchiteUpgradeValue, CapCostPerArchite, StartCapCost, CapUpgradeGrace);

        public float TotalCapacityArchiteProgressFromFullLevel
        {
            get
            {
                float archites = 0;
                int levels = (int)TotalCapacityArchiteUpgradeValue;
                for (int i = 1; i <= levels; i++)
                    archites += CalculateUpgradeCost(i, CapCostPerArchite, StartCapCost, CapUpgradeGrace);
                return archites;
            }
        }

        public float TotalStatArchiteProgressFromFullLevel
        {
            get
            {
                float archites = 0;
                int levels = (int)TotalStatArchiteUpgradeValue;
                for (int i = 1; i <= levels; i++)
                    archites += CalculateUpgradeCost(i, StatCostPerArchite, StartStatCost, StatUpgradeGrace);
                return archites;
            }
        }

        private float CalculateUpgradeCost(float levelNow, float costPerLevel, float minCost, float grace) =>
            Math.Max(minCost,
                (levelNow - grace) * costPerLevel
            );

        public Pawn ParentPawn => parent as Pawn;

        // Maybe cache this later if we're dying for performance, but this should only be a problem
        // for absolutely stupid edge cases, like if all 50 of the player's colonists have each
        // individual upgrade type.
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

        // FIXME: Replace calls to this with HasAnyUpgrades
        public bool CanOpenMenu =>
            HasAnyUpgrades ||
            capacityArchiteProgress > 0 ||
            statArchiteProgress > 0 ||
            capacityArchitesToSpend > 0 ||
            statArchitesToSpend > 0;


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

        public AcceptanceReport CanAcceptUpgrade(ArchiteDef upgrade)
        {
            if (upgrade == null)
                return "ArchiteReinforcement.UseFailReasonOther".Translate();

            if (IsUpgradeMaxLevel(upgrade))
                return "ArchiteReinforcement.UseFailReasonMaxUsed".Translate();

            if (!upgrade.exclusionTags.NullOrEmpty() && !HasAnyLevelOfUpgrade(upgrade))
            {
                List<string> exclusions = ActiveExclusionTags;
                if (!exclusions.NullOrEmpty())
                {
                    foreach (string tag in upgrade.exclusionTags)
                    {
                        if (exclusions.Contains(tag))
                        {
                            return "ArchiteReinforcement.UseFailReasonExcluded".Translate();
                        }
                    }
                }
            }

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

        public bool HasAnyLevelOfImpliedUpgrade(StatArchiteDef upgrade)
            => ImpliedLevels.ContainsKey(upgrade);

        public int LevelForUpgrade(ArchiteDef upgrade)
        {
            if (upgrade is CapacityArchiteDef capacity)
                return LevelForCapacity(capacity);
            if (upgrade is StatArchiteDef stat)
                return LevelForStat(stat);

            return 0;
        }

        public int LevelForCapacity(CapacityArchiteDef capacity)
        {
            if (!capacityUpgrades.ContainsKey(capacity))
                return 0;
            return capacityUpgrades[capacity];
        }

        public int LevelForStat(StatArchiteDef stat)
        {
            if (!statUpgrades.ContainsKey(stat))
                return 0;
            return statUpgrades[stat];
        }

        public bool HasAnyUpgradeForCapacity(PawnCapacityDef capacity)
        {
            foreach (CapacityArchiteDef def in capacityUpgrades.Keys)
            {
                if (def.capacity == capacity)
                    return true;
            }
            return false;
        }

        public int LevelForImpliedUpgrade(StatArchiteDef upgrade)
        {
            if (!upgrade.IsImplied)
            {
                Log.Error("Called LevelForImpliedUpgrade with " + upgrade.ToString() + ", but it is not an implied upgrade!");
                return 0;
            }
            return ImpliedLevels.ContainsKey(upgrade) ? ImpliedLevels[upgrade] : 0;
        }           

        // Any time a pawn acquires any sort of upgrade, it should take place here. It DOES NOT
        // handle the transaction of upgrade points, but it DOES dirty all of the upgrade caches.
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
            impliedLevelsCached = null;
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

        public override void PostDraw()
        {
            ArchiteBadgeDrawer.TryDrawBadge(this);
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

            if (Scribe.mode == LoadSaveMode.Inactive || Scribe.mode == LoadSaveMode.Saving)
                return;
            // Ensure that we have our required collections initialized, in case the player is
            // loading a save file that did not previously have AR enabled.
            if (statUpgrades == null)
                statUpgrades = new Dictionary<StatArchiteDef, int>();
            if (capacityUpgrades == null)
                capacityUpgrades = new Dictionary<CapacityArchiteDef, int>();
        }
    }

    [StaticConstructorOnStartup]
    public static class ArchiteBadgeDrawer
    {
        public const float MinAlpha = 0.25f;
        public const float MaxAlpha = 3f;
        public const int FadeIntervalTicks = 180;
        public const float BadgeSize = 0.5f;

        public static readonly Vector3 BadgePositionRelative = new Vector3(
            0.5f,
            AltitudeLayer.MoteOverhead.AltitudeFor(),
            0.7f
        );
        public static readonly Material BadgeMaterial = MaterialPool.MatFrom("ArchiteReinforcement/Other/ArchiteBadge", ShaderDatabase.Transparent);

        public static void TryDrawBadge(CompArchiteTracker tracker)
        {
            Pawn pawn = tracker.ParentPawn;

            if (!ShouldDrawBadge())
                return;

            DrawBadge(pawn.DrawPos, BadgeOpacityNow());


            bool ShouldDrawBadge()
            {
                if (!tracker.HasAnyUpgrades)
                    return false;

                if (pawn.Dead)
                    return false; // Maybe consider adding badge to corpses for reclamation?

                return SettingsAllowBadge();
            }

            bool SettingsAllowBadge()
            {
                ModSettings_ArchiteReinforcement settings = Mod_ArchiteReinforcement.ActiveMod.Settings;

                if (pawn.IsSlaveOfColony)
                    return settings.drawBadgeForSlaves;

                if (pawn.HostileTo(Faction.OfPlayer))
                    return settings.drawBadgeForHostiles;

                if (pawn.IsPrisonerOfColony)
                    return settings.drawBadgeForPrisoners;

                if (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer)
                    return settings.drawBadgeForColonists;

                return settings.drawBadgeForNeutrals;
            }

            float BadgeOpacityNow()
            {

                float intervalTick = Math.Abs(pawn.HashOffsetTicks() % FadeIntervalTicks);
                float alphaFactor = intervalTick / FadeIntervalTicks;
                alphaFactor = Math.Abs(alphaFactor - 0.5f) * 2f;

                return Mathf.Lerp(MinAlpha, MaxAlpha, alphaFactor);
            }
        }

        private static void DrawBadge(Vector3 position, float opacity)
        {
            Vector3 drawPosition = new Vector3(position.x, 0f, position.z);
            drawPosition += BadgePositionRelative;

            Vector3 dimensions = new Vector3(BadgeSize, 1f, BadgeSize);
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(drawPosition, Quaternion.AngleAxis(0f, Vector3.up), dimensions);

            Material material = FadedMaterialPool.FadedVersionOf(BadgeMaterial, opacity);
            Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
        }
    }
}
