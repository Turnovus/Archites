using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;
using RimWorld;
using RimWorld.Planet;

namespace ArchiteReinforcement
{
    class SitePartWorker_ArchiteCrucible : SitePartWorker
    {
        public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
        {
            string label = base.GetPostProcessedThreatLabel(site, sitePart);

            if (sitePart.things != null && sitePart.things.Any)
                label = label + ": " + sitePart.things[0].LabelShortCap;
            if (site.HasWorldObjectTimeout)
                label = (label + (" (" + "DurationLeft".Translate(site.WorldObjectTimeoutTicksLeft.ToStringTicksToPeriod()) + ")"));

            return label;
        }
    }
}
