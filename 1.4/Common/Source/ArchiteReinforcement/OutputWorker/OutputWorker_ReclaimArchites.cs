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

            Log.Message(ingredients.Count().ToString());

            foreach (Thing ingredient in ingredients)
            {
                Log.Message(ingredient.def.defName);
                capacityArchites += CapacityArchitesFrom(ingredient);
                statArchites += StatArchitesFrom(ingredient);
            }
            Log.Message(statArchites.ToString());

            foreach(Thing product in products)
            {
                Log.Message(product.def.defName);
                
                CompUseEffect_ReclaimedArchites comp = product.TryGetComp<CompUseEffect_ReclaimedArchites>();
                Log.Message(comp.ToString());
                if (comp == null)
                    continue;

                comp.capacityArchites = capacityArchites;
                comp.statArchites = statArchites;
                Log.Message(comp.statArchites.ToString());
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
            Log.Message(archites.ToString());
            return archites * randomRange.RandomInRange;
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

            return archites * randomRange.RandomInRange;
        }
    }
}
