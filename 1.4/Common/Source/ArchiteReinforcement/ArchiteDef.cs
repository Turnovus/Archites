﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public abstract class ArchiteDef : Def, IComparable<ArchiteDef>
    {
        public int? maxUses;
        public EUpgradeType upgradeType = EUpgradeType.Offset;
        public float effectPerLevel;
        public float levelProgressiveRate = 0f;
        public float baseOffset = 0f;
        public float marketValuePerLevel = 0f;
        public List<string> exclusionTags = new List<string>();
        public float upgradeValue = 1f;
        public string effectDescriptionOverride = null;
        public string progressiveKeyOverride = null;

        public virtual bool IsImplied => false;

        public abstract string NameOfThingToUpgradeLower { get; }
        public abstract string NameOfThingToUpgrade { get; }

        public float ModAtLevel(int level)
        {
            if (maxUses != null)
                level = Math.Min(level, (int)maxUses);

            float mod;

            // No negative progressive rates, because that will cause upgrades to turn into
            // downgrades after a certain level.
            if (levelProgressiveRate <= 0)
            {
                mod = baseOffset + (effectPerLevel * level);
            }
            else
            {
                // nx( 1 - m/2 + x(m/2) ) + b
                // Where x = level, n = bonus per level, m = progressive bonus, b = base offset
                mod = 1;
                mod = mod - (levelProgressiveRate / 2f);
                mod = mod + (level * (levelProgressiveRate / 2f));
                mod = mod * level * effectPerLevel;
                mod += baseOffset;
            }

            return mod;
        }

        public void ModifyValueAtLevel(ref float value, int level)
        {
            if (level <= 0)
                return;

            float mod = ModAtLevel(level);

            switch (upgradeType)
            {
                case EUpgradeType.Offset:
                    value += mod;
                    return;
                case EUpgradeType.Factor:
                    value *= mod;
                    return;
                case EUpgradeType.Exponent:
                    value = (float)Math.Pow(value, mod);
                    return;
                case EUpgradeType.ExponentialBase:
                    value = (float)Math.Pow(mod, value);
                    return;
                case EUpgradeType.Denominator:
                    if (level != 0)
                        value /= mod;
                    return;
            }
        }

        public string ValueReadoutAtLevel(float level)
        {
            float mod = ModAtLevel((int)level);

            switch (upgradeType)
            {
                case EUpgradeType.Offset:
                    return mod.ToStringWithSign();
                case EUpgradeType.Factor:
                    return "x" + mod.ToString();
                case EUpgradeType.Exponent:
                    return "^" + mod.ToString();
                case EUpgradeType.ExponentialBase:
                    return mod.ToString() + "^n";
                case EUpgradeType.Denominator:
                    return "/" + mod.ToString();
            }

            return string.Empty;
        }

        public string ValueModAtLevel(float level, string modifiedValue)
        {
            if (maxUses != null)
                level = Math.Min(level, (int)maxUses);

            float mod = ModAtLevel((int)level);

            switch (upgradeType)
            {
                case EUpgradeType.Offset:
                    return modifiedValue + (mod >= 0 ? " + " : " - ") + Math.Abs(mod).ToString();
                case EUpgradeType.Factor:
                    return modifiedValue + " x " + mod.ToString();
                case EUpgradeType.Exponent:
                    return "^" + mod.ToString();
                case EUpgradeType.ExponentialBase:
                    return mod.ToString() + "^" + modifiedValue;
                case EUpgradeType.Denominator:
                    return modifiedValue + " / " + mod.ToString();
            }

            return string.Empty;
        }

        public virtual int CompareTo(ArchiteDef other)
        {
            // More expensive upgrades come first because they tend to be more meaningful.
            if (other.upgradeValue != upgradeValue)
                return upgradeValue > other.upgradeValue ? -1 : 1;

            // Implied stats come before actual ones because they have special behaviors.
            if (IsImplied && !other.IsImplied)
                return -1;

            if (other.IsImplied && !IsImplied)
                return 1;

            return 0;
        }

        public string UpgradeBreakdown()
        {
            string str;

            str = effectDescriptionOverride.NullOrEmpty() ?
                (string)"ArchiteReinforcement.WillUpgrade".Translate(NameOfThingToUpgrade) :
                effectDescriptionOverride.CapitalizeFirst() + " " + NameOfThingToUpgrade;

            str += "\n\n";
            str += "ArchiteReinforcement.PerLevelHeader".Translate();

            int iterations = 10;
            if (maxUses != null)
                iterations = Math.Min(10, (int)maxUses);

            for (int i = 1; i <= iterations; i++)
            {
                str += "\n";
                str += "ArchiteReinforcement.PerLevelItem".Translate(i.ToString(), ValueReadoutAtLevel(i));
            }

            if (maxUses == null || iterations < maxUses)
                str += "\n" + "ArchiteReinforcement.PerLevelItem.EtCetera".Translate();

            if (levelProgressiveRate > 0f)
            {
                str += "\n\n";
                str += ProgressiveExplanation();
            }

            str += "\n\n";
            str += "ArchiteReinforcement.DescriptionMaxLevel".Translate(
                maxUses == null ?
                    (string)"ArchiteReinforcement.DescriptionMaxLevel.Unlimited".Translate() :
                    maxUses.ToString()
            );

            return str;
        }

        public string DescriptionWithBreakdown()
        {
            string str = description;
            string str2 = UpgradeBreakdown();

            if (!str.NullOrEmpty())
                str += "\n\n" + str2;

            return str;
        }

        public string ProgressiveExplanation()
        {
            string enhances = effectDescriptionOverride ?? "ArchiteReinforcement.EffectDescriptionSimple".Translate();
            string key = progressiveKeyOverride ?? "ArchiteReinforcement.ProgressiveExplanation";
            float progressive = levelProgressiveRate * effectPerLevel;
            return key.Translate(enhances, NameOfThingToUpgradeLower, progressive.ToStringByStyle(ToStringStyle.FloatMaxThree));
        }
    }
    
    public class StatArchiteDef : ArchiteDef
    {
        // Lets us set custom names for pods with implied effects
        public string statLabelOverride = null;
        public StatDef stat;

        // This def is the only one that overrides IsImplied, so it's the only one that can actually
        // *be* implied. Because I don't really have any need for capacity archite defs to 
        // ever be implied.
        public override bool IsImplied => stat == null;

        public override string NameOfThingToUpgradeLower => statLabelOverride ?? stat?.label ?? "ArchiteReinforcement.UpgradeNameFallback.Lower".Translate();
        public override string NameOfThingToUpgrade => statLabelOverride?.CapitalizeFirst() ?? stat?.LabelCap ?? "ArchiteReinforcement.UpgradeNameFallback".Translate();

        public override int CompareTo(ArchiteDef other)
        {
            int n = base.CompareTo(other);
            if (n != 0)
                return n;

            if (!(other is StatArchiteDef))
                return 1;

            StatArchiteDef otherStat = other as StatArchiteDef;

            if (otherStat == null)
                return -1;

            if (otherStat.stat == stat)
                return label.CompareTo(otherStat.label);

            if (otherStat.stat.category != stat.category)
                return otherStat.stat.category.displayOrder < stat.category.displayOrder ? 1 : -1;

            if (otherStat.stat.displayPriorityInCategory == stat.displayPriorityInCategory)
                return stat.label.CompareTo(otherStat.stat.label);

            return otherStat.stat.displayPriorityInCategory > stat.displayPriorityInCategory ? 1 : -1;
        }
    }

    public class CapacityArchiteDef : ArchiteDef
    {
        public PawnCapacityDef capacity;

        public override string NameOfThingToUpgradeLower => capacity.label;
        public override string NameOfThingToUpgrade => capacity.LabelCap;

        public override int CompareTo(ArchiteDef other)
        {
            int n = base.CompareTo(other);
            if (n != 0) // Base sorting behavior takes priority
                return n;

            if (!(other is CapacityArchiteDef))
                return -1;

            CapacityArchiteDef otherCap = other as CapacityArchiteDef;

            if (other == null)
                return 1;

            if (otherCap.capacity == capacity)
                return label.CompareTo(otherCap.label);

            if (otherCap.capacity.listOrder == capacity.listOrder)
                return capacity.label.CompareTo(otherCap.capacity.label);

            return otherCap.capacity.listOrder < capacity.listOrder ? 1 : -1;
        }
    }

    public enum EUpgradeType
    {
        Offset,
        Factor,
        Exponent,
        ExponentialBase,
        Denominator,
    }
}
