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
#pragma warning disable CS0649
        public TechLevel techLevel;
#pragma warning restore CS0649

        public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null) =>
            Enumerable.Empty<Thing>();

        public override bool HandlesThingDef(ThingDef thingDef) =>
            thingDef.techLevel == techLevel;
    }
}
