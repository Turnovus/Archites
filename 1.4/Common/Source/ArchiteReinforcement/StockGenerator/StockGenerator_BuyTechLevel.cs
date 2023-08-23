using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ArchiteReinforcement
{
    class StockGenerator_BuyTechLevel : StockGenerator
    {
        public TechLevel techLevel;

        public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null) =>
            Enumerable.Empty<Thing>();

        public override bool HandlesThingDef(ThingDef thingDef) =>
            thingDef.techLevel == techLevel;
    }
}
