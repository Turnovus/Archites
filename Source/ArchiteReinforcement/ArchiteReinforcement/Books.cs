using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    public class BookOutcomeProperties_GainArchites : BookOutcomeProperties
    {
        public float bothChance;
        public float capacitySpecializeChance;
        public float mixedBookEffectivenessFactor;

        public float baseArchiteRate;
        public float architeRatePerQuality;
        
        public override Type DoerClass => typeof(BookOutcomeDoer_GainArchites);
    }

    public class BookOutcomeDoer_GainArchites : BookOutcomeDoer
    {
        private float architesPerHour;
        private ArchiteBookType bookType;
        
        BookOutcomeProperties_GainArchites ArchiteProps => props as BookOutcomeProperties_GainArchites;

        public override bool DoesProvidesOutcome(Pawn reader)
        {
            CompArchiteTracker tracker = reader.ArchiteTracker();
            return tracker == null ? false : tracker.HasAnyUpgrades;
        }

        public override void OnBookGenerated(Pawn author = null)
        {
            base.OnBookGenerated(author);

            architesPerHour = ArchiteRateAtQuality(Quality);

            if (Rand.Chance(ArchiteProps.bothChance))
            {
                bookType = ArchiteBookType.Both;
                architesPerHour *= ArchiteProps.mixedBookEffectivenessFactor;
            }
            else
                bookType = Rand.Chance(ArchiteProps.capacitySpecializeChance) ? ArchiteBookType.Capacity : ArchiteBookType.Stat;
        }

        private float ArchiteRateAtQuality(QualityCategory quality)
        {
            return ArchiteProps.baseArchiteRate + ArchiteProps.architeRatePerQuality * (int)quality;
        }

        public override void OnReadingTick(Pawn reader, float factor)
        {
            CompArchiteTracker tracker = reader.ArchiteTracker();
            if (tracker == null || !tracker.HasAnyUpgrades)
                return;

            float upgradeProgress = architesPerHour / GenDate.TicksPerHour;
            upgradeProgress *= factor;

            if (bookType == ArchiteBookType.Both || bookType == ArchiteBookType.Capacity)
                tracker.AddCapacityArchiteProgress(upgradeProgress);
            if (bookType == ArchiteBookType.Both || bookType == ArchiteBookType.Stat)
                tracker.AddStatArchiteProgress(upgradeProgress);
        }

        public override string GetBenefitsString(Pawn reader = null)
        {
            StringBuilder benefits = new StringBuilder();
            string architeRate = architesPerHour.ToString("F1");

            if (bookType == ArchiteBookType.Both || bookType == ArchiteBookType.Capacity)
                benefits.AppendLine("ArchiteReinforcement.ArchitesPerHour.Capacity".Translate(architeRate));
            if (bookType == ArchiteBookType.Both || bookType == ArchiteBookType.Stat)
                benefits.AppendLine("ArchiteReinforcement.ArchitesPerHour.Stat".Translate(architeRate));

            return benefits.ToString();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref architesPerHour, "architesPerHour");
            Scribe_Values.Look(ref bookType, "bookType");
        }

        private enum ArchiteBookType
        {
            Capacity,
            Stat,
            Both,
        }
    }
}
