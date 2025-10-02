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
        public override Type DoerClass => typeof(BookOutcomeDoer_GainArchites);
    }

    public class BookOutcomeDoer_GainArchites : BookOutcomeDoer
    {
        private const float BothChance = 0.1f;
        private const float CapacitySpecializeChance = 0.25f;
        private const float MixedBookEffectivenessFactor = 0.75f;

        private const float BaseArchiteRate = 0.8f;
        private const float ArchiteRatePerQuality = 0.3f;

        private float architesPerHour;
        private ArchiteBookType bookType;

        public override bool DoesProvidesOutcome(Pawn reader)
        {
            CompArchiteTracker tracker = reader.ArchiteTracker();
            return tracker == null ? false : tracker.HasAnyUpgrades;
        }

        public override void OnBookGenerated(Pawn author = null)
        {
            base.OnBookGenerated(author);

            architesPerHour = ArchiteRateAtQuality(Quality);

            if (Rand.Chance(BothChance))
            {
                bookType = ArchiteBookType.Both;
                architesPerHour *= MixedBookEffectivenessFactor;
            }
            else
                bookType = Rand.Chance(CapacitySpecializeChance) ? ArchiteBookType.Capacity : ArchiteBookType.Stat;
        }

        private static float ArchiteRateAtQuality(QualityCategory quality)
        {
            return BaseArchiteRate + ArchiteRatePerQuality * (int)quality;
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
