using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.BaseGen;

namespace ArchiteReinforcement
{
    class GenStep_ArchiteCrucible : GenStep_Scatterer
    {
        private const int Size = 8;

        public override int SeedPart => 1207689346;

        protected override bool CanScatterAt(IntVec3 location, Map map)
        {
            if (!base.CanScatterAt(location, map) || !location.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) || !map.reachability.CanReachMapEdge(location, TraverseParms.For(TraverseMode.PassDoors)))
                return false;
            foreach (IntVec3 cell in CellRect.CenteredOn(location, Size, Size))
            {
                if (!cell.InBounds(map) || cell.GetEdifice(map) != null)
                    return false;
            }
            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            Faction ancients = Find.FactionManager.OfAncientsHostile;
            CellRect rect = CellRect.CenteredOn(loc, Size, Size).ClipInsideMap(map);

            ResolveParams resolver1 = new ResolveParams();
            resolver1.rect = rect;
            resolver1.faction = ancients;
            BaseGen.globalSettings.map = map;
            BaseGen.symbolStack.Push("ancientRuins", resolver1);
            BaseGen.Generate();

            MapGenerator.SetVar<CellRect>("RectOfInterest", rect);
        }
    }
}
