using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CF;

namespace ArchiteReinforcement
{
    public abstract class OutputWorker_ReclaimArchites : OutputWorker
    {
        public static readonly FloatRange randomRange = new FloatRange(0.8f, 1.2f);

        public abstract float CapacityArchitesFrom(Thing thing);
        public abstract float StatArchitesFrom(Thing thing);

        public override IEnumerable<Thing> PostCraft(
            IEnumerable<Thing> products,
            RecipeDef recipeDef,
            Pawn worker,
            IEnumerable<Thing> ingredients,
            IBillGiver billGiver,
            Precept_ThingStyle precept = null,
            ThingStyleDef style = null,
            int? overrideGraphicIndex = null
        )
        {
            float capacityArchites = 0f;
            float statArchites = 0f;

            foreach (Thing ingredient in ingredients)
            {
                capacityArchites += CapacityArchitesFrom(ingredient);
                statArchites += StatArchitesFrom(ingredient);
            }

            foreach(Thing product in products)
            {
                
                CompUseEffect_ReclaimedArchites comp = product.TryGetComp<CompUseEffect_ReclaimedArchites>();
                if (comp == null)
                    continue;

                comp.capacityArchites = capacityArchites;
                comp.statArchites = statArchites;
            }

            return null;
        }
    }

    public class OutputWorker_ReclaimArchitesFromUpgrade : OutputWorker_ReclaimArchites
    {
        public ArchiteDef ThingUpgrades(Thing thing)
        {
            return thing.TryGetComp<CompUseEffect_ArchiteUpgrade>()?.Props.upgrade;
        }

        public override float CapacityArchitesFrom(Thing thing)
        {
            ArchiteDef upgrade = ThingUpgrades(thing);
            if (upgrade == null)
                return 0f;

            float archites = upgrade.upgradeValue;
            archites *= CompArchiteTracker.StartCapCost;
            if (!(upgrade is CapacityArchiteDef))
                archites *= 0.15f;

            archites = archites * randomRange.RandomInRange;

            return archites - (archites % 0.1f);
        }

        public override float StatArchitesFrom(Thing thing)
        {
            ArchiteDef upgrade = ThingUpgrades(thing);
            if (upgrade == null)
                return 0f;

            float archites = upgrade.upgradeValue;
            archites *= CompArchiteTracker.StartStatCost;
            if (!(upgrade is StatArchiteDef))
                archites *= 0.15f;

            archites = archites * randomRange.RandomInRange;

            return archites - (archites % 0.1f);
        }
    }

    public class OutputWorker_ReclaimArchitesFromValue : OutputWorker_ReclaimArchites
    {
        public static readonly FloatRange ValueReturnRange = new FloatRange(0.6f, 0.85f);

        public static float BaseArchitesPerMarketValue
        {
            get
            {
                float marketValue = MyDefOf.Turn_ArchiteReinforcement_Chamber
                    .GetStatValueAbstract(StatDefOf.MarketValue);

                CompProperties_UseEffectArchiteProgress props =
                    MyDefOf.Turn_ArchiteReinforcement_Chamber
                        .GetCompProperties<CompProperties_UseEffectArchiteProgress>();

                return (props?.defaultStoredArchites ?? 1f) / marketValue;
            }
        }

        public static float AnyArchitesFrom(Thing thing)
        {
            float archites = thing.GetStatValue(StatDefOf.MarketValue)
                * BaseArchitesPerMarketValue
                * ValueReturnRange.RandomInRange;

            return archites - (archites % 0.1f);
        }

        public override float CapacityArchitesFrom(Thing thing)
            => AnyArchitesFrom(thing);

        public override float StatArchitesFrom(Thing thing)
            => AnyArchitesFrom(thing);
    }

    public class OutputWorker_ReclaimArchitesFromCorpse : OutputWorker_ReclaimArchites
    {
        public static readonly FloatRange ReturnRange = new FloatRange(0.35f, 0.5f);

        public override float CapacityArchitesFrom(Thing thing)
        {
            if (!(thing is Corpse corpse))
                return 0f;

            CompArchiteTracker tracker = corpse.ArchiteTracker();
            if (tracker == null)
                return 0f;

            return GetNumReclaimedArchites(tracker.TotalCapacityArchiteUpgradeValue);
        }

        public override float StatArchitesFrom(Thing thing)
        {
            if (!(thing is Corpse corpse))
                return 0f;

            CompArchiteTracker tracker = corpse.ArchiteTracker();
            if (tracker == null)
                return 0f;

            return GetNumReclaimedArchites(tracker.TotalStatArchiteUpgradeValue);
        }

        private static float GetNumReclaimedArchites(float originalArchiteCount)
        {
            float random = ReturnRange.RandomInRange;
            float value = originalArchiteCount * random;
            return value - (value % 0.1f);
        }
    }
}
