using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace ArchiteReinforcement
{
    public class Building_ArchitePillar : Building
    {
        public const int CollectAtDefault = 10;
        public const int MinThreshold = 1;
        public const int MaxThreshold = 80;

        private Pawn assignedPawn;
        private float capArchites = 0f;
        private float statArchites = 0f;
        private int collectAt = CollectAtDefault;

        public ArchitePillarTuning Tuning => def.GetModExtension<ArchitePillarTuning>();

        public bool CanWithdraw => capArchites >= 1f || statArchites >= 1f;

        public Pawn AssignedPawn => assignedPawn;

        public bool ShowGizmos => Faction == Faction.OfPlayer; // I don't know how the compiler can read this but okay.

        public override void Tick()
        {
            base.Tick();
            if (Rand.MTBEventOccurs(Tuning.architeMtbDays, GenDate.TicksPerDay, 1f))
                AddRandomArchite();
        }

        public override void DrawGUIOverlay()
        {
            if (Find.CameraDriver.CurrentZoom != 0)
                return;
            if (assignedPawn == null || Faction != Faction.OfPlayer)
                return;
            GenMapUI.DrawThingLabel(this, assignedPawn.LabelShort, GenMapUI.DefaultThingLabelColor);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            sb.AppendLineIfNotEmpty();

            sb.AppendLine("ArchiteReinforcement.InspectCapacityCount".Translate(capArchites));
            sb.AppendLine("ArchiteReinforcement.InspectStatCount".Translate(statArchites));

            if (assignedPawn == null)
            {
                sb.Append("ArchiteReinforcement.Pillar.NotAssigned".Translate());
            }
            else
            {
                sb.AppendLine("ArchiteReinforcement.Pillar.Assigned".Translate(assignedPawn.LabelCap));
                sb.Append("ArchiteReinforcement.Pillar.Collecting".Translate(collectAt));
            }

            return sb.ToString();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Unassign(mode == DestroyMode.KillFinalize, true);
            base.DeSpawn(mode);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption option in base.GetFloatMenuOptions(selPawn))
                yield return option;

            string label = CanWithdraw ?
                "ArchiteReinforcement.Pillar.Extract".Translate(LabelCap) :
                "ArchiteReinforcement.Pillar.NoArchites".Translate();

            FloatMenuOption extract = new FloatMenuOption (
                label,
                () =>
                {
                    Job job = JobMaker.MakeJob(MyDefOf.Turn_Job_ExtractPillarArchites, this);
                    selPawn.jobs.TryTakeOrderedJob(job);
                }
            );
            extract.Disabled = !CanWithdraw;
            yield return extract;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref capArchites, "capArchites");
            Scribe_Values.Look(ref statArchites, "statArchites");
            Scribe_Values.Look(ref collectAt, "collectAt");
            Scribe_References.Look(ref assignedPawn, "assignedPawn");
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            // Limit to one pillar at a time because it's easier for me.
            if (!ShowGizmos || Find.Selector.NumSelected != 1)
                yield break;

            Command_Action assign = new Command_Action();
            assign.defaultLabel = "CommandThingSetOwnerLabel".Translate();
            assign.defaultDesc = "ArchiteReinforcement.Pillar.AssignDesc".Translate(Faction.OfPlayer.def.pawnSingular);
            assign.icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner");
            assign.hotKey = KeyBindingDefOf.Misc4;
            assign.action = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Pawn pawn in Map.mapPawns.FreeColonistsAndPrisoners)
                    list.Add(new FloatMenuOption(
                        pawn.LabelShortCap,
                        delegate { Assign(pawn); },
                        pawn,
                        Color.white
                    ));
                if (!list.Any())
                    list.Add(new FloatMenuOption("ArchiteReinforcement.Pillar.NoPawns".Translate(), null));

                Find.WindowStack.Add(new FloatMenu(list));
            };
            yield return assign;

            if (assignedPawn == null)
                yield break;

            Command_Action unassign = new Command_Action();
            unassign.defaultLabel = "ArchiteReinforcement.Pillar.Unassign.Label".Translate();
            unassign.defaultDesc = "ArchiteReinforcement.Pillar.Unassign.Desc".Translate(Faction.OfPlayer.def.pawnSingular);
            unassign.icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner");
            unassign.action = delegate { Unassign(false, false); };
            yield return unassign;

            Command_Action threshold = new Command_Action();
            threshold.defaultLabel = "ArchiteReinforcement.Pillar.SetThreshold.Label".Translate();
            threshold.defaultDesc = "ArchiteReinforcement.Pillar.SetThreshold.Desc".Translate(Faction.OfPlayer.def.pawnSingular);
            threshold.icon = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel");
            threshold.action = delegate
            {
                Dialog_Slider slider = new Dialog_Slider(
                    "ArchiteReinforcement.Pillar.Slider".Translate(),
                    MinThreshold,
                    MaxThreshold,
                    value =>
                    {
                        collectAt = value;
                    },
                    collectAt
                );
                Find.WindowStack.Add(slider);
            };
            yield return threshold;
        }

        public void ClearArchites()
        {
            capArchites = 0;
            statArchites = 0;
        }

        public void AddRandomArchite()
        {
            if (Rand.Chance(Tuning.extraArchiteChance))
            {
                capArchites += 1f;
                statArchites += 1f;
                return;
            }
            if (Rand.Chance(0.5f))
                capArchites += 1f;
            else
                statArchites += 1f;
        }

        public void TransferArchites(CompArchiteTracker tracker)
        {
            tracker.AddCapacityArchiteProgress(capArchites);
            tracker.AddStatArchiteProgress(statArchites);
            ClearArchites();
        }

        public void TransferArchites(Pawn pawn)
        {
            CompArchiteTracker archites = pawn.ArchiteTracker();
            if (archites == null)
                return;
            TransferArchites(archites);
        }

        public void Assign(Pawn pawn)
        {
            Unassign(false, false);
            assignedPawn = pawn;
        }

        public void Unassign(bool destroyed, bool notify)
        {
            if (assignedPawn == null)
                return;

            Pawn pawn = assignedPawn;
            assignedPawn = null;
            collectAt = CollectAtDefault;
            if (!notify)
                return;

            string key = destroyed ?
                "ArchiteReinforcement.Pillar.Destroyed" :
                "ArchiteReinforcement.Pillar.LostAssignment";
            Messages.Message(
                key.Translate(def, pawn),
                new LookTargets(new TargetInfo[2]
                    {
                        this,
                        pawn
                    }
                ),
                MessageTypeDefOf.CautionInput,
                false
            );
        }
    }

    public class ArchitePillarTuning : DefModExtension
    {
#pragma warning disable CS0649
        public float architeMtbDays;
        public float extraArchiteChance;
#pragma warning restore CS0649
    }
}
