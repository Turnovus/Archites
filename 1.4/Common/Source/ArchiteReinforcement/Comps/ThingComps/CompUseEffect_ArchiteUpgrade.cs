using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public class CompUseEffect_ArchiteUpgrade : CompUseEffect
    {
        private List<ArchiteDef> excludedDefs;

        public CompProperties_UseEffectArchiteUpgrade Props =>
            (CompProperties_UseEffectArchiteUpgrade)props;

        public string UpgradeName
        {
            get
            {
                if (Props.upgrade is StatArchiteDef stat)
                    return stat.stat.label;
                if (Props.upgrade is CapacityArchiteDef cap)
                    return cap.capacity.label;
                return "ArchiteReinforcement.UpgradeNameFallback".Translate();
            }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();

            if (Props.upgrade.exclusionTags.NullOrEmpty())
                return;

            excludedDefs = new List<ArchiteDef>();

            foreach (string tag in Props.upgrade.exclusionTags)
            {
                if (!DefPatcher.exclusionGroups.ContainsKey(tag) || DefPatcher.exclusionGroups[tag].NullOrEmpty())
                    continue;

                foreach (ArchiteDef archite in DefPatcher.exclusionGroups[tag])
                    if (!excludedDefs.Contains(archite))
                        excludedDefs.Add(archite);
            }
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            CompArchiteTracker comp = usedBy.TryGetComp<CompArchiteTracker>();
            if (comp == null)
                return;

            comp.DoUpgrade(Props.upgrade);
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            CompArchiteTracker comp = p.TryGetComp<CompArchiteTracker>();

            if (comp == null)
            {
                failReason = "ArchiteReinforcement.UseFailReasonNoComp".Translate();
                return false;
            }

            return comp.CanAcceptUpgrade(Props.upgrade, out failReason);
        }

        public override string CompInspectStringExtra()
        {
            if (Props.upgrade == null)
                return string.Empty;

            string fullString;

            if (!Props.upgrade.effectDescriptionOverride.NullOrEmpty())
                fullString = Props.upgrade.effectDescriptionOverride + " " + UpgradeName;
            else
                fullString = "ArchiteReinforcement.WillUpgrade".Translate(UpgradeName);

            if (excludedDefs.NullOrEmpty())
                return fullString;

            fullString += "\n";
            fullString += "ArchiteReinforcement.CannotUseWith".Translate();

            foreach (ArchiteDef excluded in excludedDefs)
            {
                if (excluded == Props.upgrade)
                    continue;

                fullString += "\n";
                fullString += "ArchiteReinforcement.IncompatibilityItem".Translate(excluded.LabelCap, excluded.NameOfThingToUpgrade);
            }

            return fullString;
        }

        public override string GetDescriptionPart()
        {
            string fullString = string.Empty;
            fullString += Props.upgrade.effectDescriptionOverride.NullOrEmpty() ?
                (string)"ArchiteReinforcement.WillUpgrade".Translate(UpgradeName) :
                Props.upgrade.effectDescriptionOverride + " " + UpgradeName;

            fullString += "\n\n";
            fullString += "ArchiteReinforcement.PerLevelHeader".Translate();

            int iterations = 10;
            if (Props.upgrade.maxUses != null)
                iterations = Math.Min(10, (int)Props.upgrade.maxUses);

            for (int i = 1; i <= iterations; i++)
            {
                fullString += "\n";
                fullString += "ArchiteReinforcement.PerLevelItem".Translate(i.ToString(), Props.upgrade.ValueReadoutAtLevel(i));
            }

            if (Props.upgrade.maxUses == null || iterations < Props.upgrade.maxUses)
                fullString += "\n" + "ArchiteReinforcement.PerLevelItem.EtCetera".Translate();

            fullString += "\n\n";
            fullString += "ArchiteReinforcement.DescriptionMaxLevel".Translate(
                Props.upgrade.maxUses == null ?
                    (string)"ArchiteReinforcement.DescriptionMaxLevel.Unlimited".Translate() :
                    Props.upgrade.maxUses.ToString()
            );

            return fullString;
        }
    }

    public class CompProperties_UseEffectArchiteUpgrade : CompProperties_UseEffect
    {
        public ArchiteDef upgrade;

        public CompProperties_UseEffectArchiteUpgrade() =>
            compClass = typeof(CompUseEffect_ArchiteUpgrade);
    }
}
