﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace ArchiteReinforcement
{
    class Dialog_ViewArchites : Window
    {
        private Pawn targetPawn;
        bool canBuyUpgrades;
        private Vector2 capacityScrollPosition = Vector2.zero;
        private Vector2 statScrollPosition = Vector2.zero;


        public override Vector2 InitialSize => new Vector2(736f, 700f); // TODO: Auto-scaling

        public Dialog_ViewArchites(Pawn target, bool canBuyUpgrades)
        {
            targetPawn = target;
            this.canBuyUpgrades = canBuyUpgrades;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            doCloseX = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            CompArchiteTracker tracker = targetPawn.ArchiteTracker();
            if (tracker == null)
                Close(false);

            ArchiteUpgradesWindowDrawer.FillRect(
                tracker,
                inRect,
                ref capacityScrollPosition,
                ref statScrollPosition,
                canBuyUpgrades
            );
        }
    }
}
