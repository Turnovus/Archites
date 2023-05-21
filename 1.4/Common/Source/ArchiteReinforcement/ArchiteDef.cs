using System;
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
        public float baseOffset = 0f;
        public float marketValuePerLevel = 0f;
        public List<string> exclusionTags = new List<string>();
        public float upgradeValue = 1f;
        public string effectDescriptionOverride = null;

        public virtual bool IsImplied => false;

        public abstract string NameOfThingToUpgrade { get; }

        public void ModifyValueAtLevel(ref float value, int level)
        {
            if (level <= 0)
                return;

            if (maxUses != null)
                level = Math.Min(level, (int)maxUses);

            float mod = baseOffset + (effectPerLevel * level);

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
            if (maxUses != null)
                level = Math.Min(level, (int)maxUses);

            float mod = baseOffset + (effectPerLevel * level);

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

            float mod = baseOffset + (effectPerLevel * level);

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
            if (IsImplied && !other.IsImplied)
                return 1;

            if (other.IsImplied && !IsImplied)
                return -1;

            if (other.upgradeValue == upgradeValue)
                return 0;

            return upgradeValue > other.upgradeValue ? -1 : 1;
        }
    }
    
    public class StatArchiteDef : ArchiteDef
    {
        public StatDef stat;

        public override bool IsImplied => stat == null;

        public override string NameOfThingToUpgrade => stat?.LabelCap ?? "ArchiteReinforcement.UpgradeNameFallback".Translate();

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

        public override string NameOfThingToUpgrade => capacity.LabelCap;

        public override int CompareTo(ArchiteDef other)
        {
            int n = base.CompareTo(other);
            if (n != 0)
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
