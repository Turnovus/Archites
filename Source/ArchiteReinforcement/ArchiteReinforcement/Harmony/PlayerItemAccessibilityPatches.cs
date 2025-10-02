using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using RimWorld.Planet;

namespace ArchiteReinforcement
{
    [HarmonyPatch(typeof(PlayerItemAccessibilityUtility))]
    [HarmonyPatch(nameof(PlayerItemAccessibilityUtility.PlayerOrQuestRewardHas))]
    [HarmonyPatch(new Type [] { typeof(ThingDef), typeof(int) })]
    class PlayerItemAccessibilityPatches
    {
        [HarmonyPostfix]
        public static void MaybeCheckForMinifiedBuilding (
            ref bool __result,
            ThingDef thingDef,
            int count
        )
        {
            // HACK: this doesn't take into account installed buildings that the player already
            // has. It will only return true if the player has the required number of installed
            // buildings OR minified buildings. If the player has some combination of minified and
            // installed buildings that add up to match the requirement, it still won't count, so
            // this patch will break for any value of count higher than 1. Fixing this would
            // require a transpiler to extrapolate the temporary variables inside the base method.
            // For now, I've chosen to set a hard limit of 1, since I don't have a need for any
            // higher values. However, the associated methods have been written to account for
            // values higher than 1, in case I ever decide to fix this abomination.
            // I simply wish to stress that I no longer care.
            if (!__result && count == 1 && thingDef.HasModExtension<PlayerPossessionCountsMinified>())
                __result = PlayerOrQuestRewardHasMinified(thingDef.minifiedDef, count);
        }

        private static bool PlayerOrQuestRewardHasMinified(ThingDef thingDef, int count = 1)
        {
            // HACK(?): All of this logic is adapted from
            // PlayerItemAccessibilityUtility.PlayerOrQuestRewardHas, because copy-pasting works,
            // and the only valid alternatives would be:
            // A - Defining a custom MinifiedThing def for the crucible. Won't work because the
            //     vanilla MinifiedThing is hardcoded into a bunch of other stuff.
            // B - Transpiling the original method to account for minified things. That kind of
            //     patch probably isn't possible, and would be incredibly fragile even if I do
            //     manage to make it work.
            // C - Doing some reflection black magic to dynamically copy-paste at runtime. No.

            if (count <= 0)
                return true;

            int found = 0;
            
            foreach (Map map in Find.Maps)
            {
                // HACK: We're specifically targeting MinifiedThing here because we already know
                // that this patch isn't going to be used on anything else. A better solution would
                // search the map's things by Type or something.
                foreach (Thing thing in map.listerThings.ThingsOfDef(ThingDefOf.MinifiedThing))
                {
                    if (IsMinifiedThingOfDef(thing, thingDef))
                    {
                        found += thing.stackCount;
                        if (found >= count)
                            return true;
                    }
                }
            }

            foreach (Caravan caravan in Find.WorldObjects.Caravans)
            {
                if (caravan.IsPlayerControlled)
                {
                    foreach (Thing thing in CaravanInventoryUtility.AllInventoryItems(caravan))
                    {
                        if (IsMinifiedThingOfDef(thing, thingDef))
                        {
                            found += thing.stackCount;
                            if (found >= count)
                                return true;
                        }
                    }
                }
            }

            foreach (Site site in Find.WorldObjects.Sites)
            {
                foreach (SitePart part in site.parts)
                {
                    if (part.things != null)
                    {
                        foreach (Thing thing in part.things)
                        {
                            if (IsMinifiedThingOfDef(thing, thingDef))
                            {
                                found += thing.stackCount;
                                if (found >= count)
                                    return true;
                            }
                        }
                    }
                }

                DefeatAllEnemiesQuestComp component = site.GetComponent<DefeatAllEnemiesQuestComp>();
                if (component != null)
                {
                    foreach(Thing thing in component.rewards)
                    {
                        if (IsMinifiedThingOfDef(thing, thingDef))
                        {
                            found += thing.stackCount;
                            if (found >= count)
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsMinifiedThingOfDef(Thing thing, ThingDef thingDef)
        {
            MinifiedThing minified = thing as MinifiedThing;
            return minified?.InnerThing?.def == thingDef;
        }
    }
}
