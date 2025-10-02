using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace ArchiteReinforcement
{
    [HarmonyPatch(typeof(TransferableUIUtility))]
    [HarmonyPatch(nameof(TransferableUIUtility.DrawTransferableInfo))]
    public static class DrawTransferableInfo_Patch
    {
        public static float ButtonSize = 24f;

        [HarmonyPostfix]
        public static void DrawArchiteUpgradeButton(Transferable trad, Rect idRect)
        {
            if (!trad.IsThing)
                return;

            if (!(trad.AnyThing is Pawn pawn))
                return;

            CompArchiteTracker tracker = pawn.ArchiteTracker();
            if (tracker == null || !tracker.CanOpenMenu)
                return;

            Rect buttonRect = new Rect(idRect.xMax - ButtonSize, 0f, ButtonSize, ButtonSize);
            ArchiteUpgradesWindowDrawer.UpgradeViewerButton(buttonRect, pawn, "ArchiteReinforcement.UpgradeButtonTip.CreationMenu");
        }
    }
}
