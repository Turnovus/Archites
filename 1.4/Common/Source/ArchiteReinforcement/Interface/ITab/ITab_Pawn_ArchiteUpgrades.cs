using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace ArchiteReinforcement
{
    class ITab_Pawn_ArchiteUpgrades : ITab
    {
        private Vector2 capScrollPosition = Vector2.zero;
        private Vector2 statScrollPosition = Vector2.zero;
        private static readonly Vector2 windowSize = new Vector2(600f, 500f);
        private const int ContextHash = 230506240;
        private const float RowHeight = 46f;
        private static readonly Color highlightColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        public ITab_Pawn_ArchiteUpgrades()
        {
            labelKey = "ArchiteReinforcement.ITab_Pawn_ArchiteUpgrades";
        }

        public override bool IsVisible
        {
            get
            {
                if (SelPawn == null)
                    return false;

                CompArchiteTracker comp = SelPawn.ArchiteTracker();
                if (comp == null)
                    return false;

                return !(comp.statUpgrades.NullOrEmpty() && comp.capacityUpgrades.NullOrEmpty()) ||
                    comp.capacityArchiteProgress > 0 ||
                    comp.statArchiteProgress > 0 ||
                    comp.capacityArchitesToSpend > 0 ||
                    comp.statArchitesToSpend > 0;
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();

            capScrollPosition = Vector2.zero;
            statScrollPosition = Vector2.zero;
        }

        protected override void FillTab()
        {
            if (SelPawn == null)
                return;

            CompArchiteTracker comp = SelPawn.ArchiteTracker();
            if (comp == null)
                return;

            size = windowSize;
            size.y = PaneTopY - 30f;

            using (TextBlock.Default())
            {
                ArchiteUpgradesWindowDrawer.FillRect(comp,
                    new Rect(Vector2.zero, size),
                    ref capScrollPosition,
                    ref statScrollPosition,
                    PlayerCanBuyArchitesFor(SelPawn)
                );
            }
        }

        // FIXME: Migrate to CompArchiteTracker
        public static bool PlayerCanBuyArchitesFor(Pawn pawn) =>
            pawn.Faction == Faction.OfPlayer || pawn.IsSlaveOfColony;
    }
}
