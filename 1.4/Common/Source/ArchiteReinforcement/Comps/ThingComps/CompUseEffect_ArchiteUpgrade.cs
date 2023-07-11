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
                string fallback = "ArchiteReinforcement.UpgradeNameFallback".Translate();
                if (Props.upgrade is StatArchiteDef stat)
                    return stat.statLabelOverride ?? stat.stat?.label ?? fallback;
                if (Props.upgrade is CapacityArchiteDef cap)
                    return cap.capacity.label ?? fallback;
                return fallback;
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
                fullString = Props.upgrade.effectDescriptionOverride.CapitalizeFirst() + " " + UpgradeName;
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

        public override string GetDescriptionPart() =>
            Props?.upgrade?.DescriptionWithBreakdown() ?? string.Empty;
            
    }

    public class CompProperties_UseEffectArchiteUpgrade : CompProperties_UseEffect
    {
        public ArchiteDef upgrade;

        public CompProperties_UseEffectArchiteUpgrade() =>
            compClass = typeof(CompUseEffect_ArchiteUpgrade);
    }
}
