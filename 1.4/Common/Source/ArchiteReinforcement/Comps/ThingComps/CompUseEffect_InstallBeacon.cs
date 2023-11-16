using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    class CompUseEffect_InstallBeacon : CompUseEffect
    {
        public CompProperties_UseEffectInstallBeacon Props =>
            (CompProperties_UseEffectInstallBeacon)props;

        public override void DoEffect(Pawn user)
        {
            BodyPartRecord torso = CorePart(user);
            if (torso == null)
                return;

            Hediff implant = GetExistingImplant(user);
            if (implant == null)
            {
                user.health.AddHediff(Props.hediffDef, torso);
                return;
            }
            if (implant is Hediff_Level leveled)
                leveled.ChangeLevel(1);
        }

        public Hediff GetExistingImplant(Pawn pawn) =>
            pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            if ((!p.IsFreeColonist || p.HasExtraHomeFaction()) && !Props.allowNonColonists)
            {
                failReason = "InstallImplantNotAllowedForNonColonists".Translate();
                return false;
            }
            Hediff existing = GetExistingImplant(p);
            if (existing != null && existing.Severity >= Props.maxSeverity)
            {
                failReason = "InstallImplantAlreadyMaxLevel".Translate();
                return false;
            }
            if (p.ArchiteTracker() == null)
            {
                failReason = "ArchiteReinforcement.UseFailReasonNoComp".Translate();
                return false;
            }
            failReason = null;
            return true;
        }

        public BodyPartRecord CorePart(Pawn pawn) => pawn.RaceProps.body.corePart;
    }

    public class CompProperties_UseEffectInstallBeacon : CompProperties_Usable
    {
        public HediffDef hediffDef;
        public bool allowNonColonists;
        public float maxSeverity = float.MaxValue;

        public CompProperties_UseEffectInstallBeacon() =>
            compClass = typeof(CompUseEffect_InstallBeacon);
    }

    public class CompUsableImplantBeacon : CompUsable
    {
        protected override string FloatMenuOptionLabel(Pawn pawn)
        {
            CompUseEffect_InstallBeacon comp = parent.TryGetComp<CompUseEffect_InstallBeacon>();
            return comp != null && comp.GetExistingImplant(pawn) is Hediff_Level existingImplant && (existingImplant.level < existingImplant.def.maxSeverity) ?
                (string)"UpgradeImplant".Translate(existingImplant.def.label, existingImplant.level + 1) :
                base.FloatMenuOptionLabel(pawn);
        }
    }
}
