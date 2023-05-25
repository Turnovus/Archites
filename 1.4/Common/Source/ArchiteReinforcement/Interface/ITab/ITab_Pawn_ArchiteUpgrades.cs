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
                FillTab(comp, new Rect(Vector2.zero, size), ref capScrollPosition, ref statScrollPosition);
            }
        }

        private static void FillTab(CompArchiteTracker comp, Rect rect, ref Vector2 capScroll, ref Vector2 statScroll)
        {
            int capCount = comp.capacityUpgrades.Count;
            int statCount = comp.statUpgrades.Count;

            Rect contractedRect = rect.ContractedBy(10f);
            GUI.BeginGroup(contractedRect);

            Rect progressRect = new Rect(0, 0, contractedRect.width, contractedRect.height * 0.20f);

            RectDivider progressLabel = new RectDivider(new Rect(0, 0, contractedRect.width, contractedRect.height * 0.05f), ContextHash);
            Widgets.ListSeparator(ref progressLabel, "ArchiteReinforcement.DivisionProgress".Translate());

            float upgradeVerticalOffset = contractedRect.height * 0.05f;
            float upgradeHeight = progressRect.height - upgradeVerticalOffset;
            float upgradeRowHeight = upgradeHeight / 3f;

            Rect capacityBarRow = new Rect(0, upgradeVerticalOffset, progressRect.width, upgradeRowHeight);
            Rect statBarRow = new Rect(0, upgradeVerticalOffset + (upgradeRowHeight), progressRect.width, upgradeRowHeight);
            Rect fundsRow = new Rect(0, upgradeVerticalOffset + (2 * upgradeRowHeight), progressRect.width, upgradeRowHeight);

            Rect capacityCounter = new Rect(capacityBarRow);
            capacityCounter.width *= 0.40f;
            Rect capacityBar = capacityBarRow.ContractedBy(0f, 0.1f);
            capacityBar.width *= 0.60f;
            capacityBar.x += capacityCounter.width;

            Widgets.Label(capacityCounter, "ArchiteReinforcement.UpgradeProgress".Translate(
                "ArchiteReinforcement.UpgradeTypeCapacity.Cap".Translate(),
                comp.capacityArchiteProgress.ToStringByStyle(ToStringStyle.FloatMaxTwo),
                comp.CapacityUpgradeCost.ToStringByStyle(ToStringStyle.FloatMaxTwo)
           ));
            Widgets.FillableBar(capacityBar, comp.capacityArchiteProgress / comp.CapacityUpgradeCost);

            Rect statCounter = new Rect(statBarRow);
            statCounter.width *= 0.40f;
            Rect statBar = statBarRow.ContractedBy(0f, 0.1f);
            statBar.width *= 0.60f;
            statBar.x += statCounter.width;

            Widgets.Label(statCounter, "ArchiteReinforcement.UpgradeProgress".Translate(
                "ArchiteReinforcement.UpgradeTypeStat.Cap".Translate(),
                comp.statArchiteProgress.ToStringByStyle(ToStringStyle.FloatMaxTwo),
                comp.StatUpgradeCost.ToStringByStyle(ToStringStyle.FloatMaxTwo)
            ));
            Widgets.FillableBar(statBar, comp.statArchiteProgress / comp.StatUpgradeCost);

            string fundsReadout = "ArchiteReinforcement.MenuAvailableArchites".Translate(
                "ArchiteReinforcement.ArchitesTypeQuantity".Translate(
                    "ArchiteReinforcement.UpgradeTypeCapacity.Cap".Translate(),
                    comp.capacityArchitesToSpend.ToStringByStyle(ToStringStyle.FloatMaxTwo)
                ),
                "ArchiteReinforcement.ArchitesTypeQuantity".Translate(
                    "ArchiteReinforcement.UpgradeTypeStat.Cap".Translate(),
                    comp.statArchitesToSpend.ToStringByStyle(ToStringStyle.FloatMaxTwo)
                )
            );
            float textWidth = Text.CalcSize(fundsReadout).x;
            float fundsCenterHorizontal = (fundsRow.width * 0.5f) - (textWidth * 0.5f);
            Rect fundsCenter = new Rect(fundsRow);
            fundsCenter.x += fundsCenterHorizontal;
            fundsCenter.width -= fundsCenterHorizontal;

            Widgets.Label(fundsCenter, fundsReadout);

            RectDivider capLabel = new RectDivider(new Rect(0, contractedRect.height * 0.20f, contractedRect.width, contractedRect.height * 0.05f), ContextHash);
            Widgets.ListSeparator(ref capLabel, "ArchiteReinforcement.DivisionCapacities".Translate());

            Rect capListRect = new Rect(0, contractedRect.height * 0.25f, contractedRect.width, contractedRect.height * 0.35f);
            Rect capScrollContent = new Rect(capListRect);
            capScrollContent.height = capCount * RowHeight;
            capScrollContent.x = 0f;
            capScrollContent.y = 0f;
            capScrollContent.width *= 0.95f;

            Listing_Standard capScrollListing = new Listing_Standard();

            Widgets.BeginScrollView(capListRect, ref capScroll, capScrollContent);
            capScrollListing.Begin(capScrollContent);

            Rect rowRect, rectName, rectEffect, rectCost, rectButton;
            bool alternate = false;

            if (!comp.capacityUpgrades.NullOrEmpty())
            {
                List<CapacityArchiteDef> allCaps = comp.capacityUpgrades.Keys.ToList();
                allCaps.Sort((a, b) => a.CompareTo(b));

                foreach (CapacityArchiteDef cap in allCaps)
                {
                    rowRect = capScrollListing.GetRect(RowHeight);
                    float halfWidth = rowRect.width / 2f;
                    float halfHeight = rowRect.height / 2f;

                    rectName = new Rect(rowRect.x, rowRect.y, halfWidth, halfHeight);
                    rectEffect = new Rect(rowRect.x, rowRect.y + halfHeight, halfWidth, halfHeight);
                    rectCost = new Rect(rowRect.x + halfWidth, rowRect.y, halfWidth, halfHeight);
                    rectButton = new Rect(rowRect.x + halfWidth, rowRect.y + halfHeight, halfWidth, halfHeight);

                    if (alternate)
                        Widgets.DrawBoxSolid(rowRect, highlightColor);

                    int levelInt = comp.capacityUpgrades[cap];
                    string level = levelInt.ToString();
                    string levelReadout = cap.maxUses == null ? level : (string)"ArchiteReinforcement.MenuItemLevelRatio".Translate(level, cap.maxUses.ToString());

                    Widgets.Label(rectName, "ArchiteReinforcement.MenuItemName".Translate(cap.LabelCap, levelReadout));
                    Widgets.Label(rectEffect, cap.ValueModAtLevel(levelInt, cap.capacity.LabelCap));
                    Widgets.Label(rectCost, "ArchiteReinforcement.MenuItemCost".Translate(cap.upgradeValue.ToStringByStyle(ToStringStyle.FloatMaxTwo), "ArchiteReinforcement.UpgradeTypeCapacity".Translate()));
                    if (comp.capacityArchitesToSpend >= cap.upgradeValue)
                    {
                        if (Widgets.ButtonText(rectButton, "ArchiteReinforcement.MenuItemButton".Translate()))
                        {
                            comp.TryBuyUpgrade(cap);
                        }
                    }

                    if (Mouse.IsOver(rowRect))
                    {
                        Widgets.DrawHighlight(rowRect);
                        TooltipHandler.TipRegion(rowRect, cap.DescriptionWithBreakdown());
                    }

                    alternate = !alternate;
                }
            }

            capScrollListing.End();
            Widgets.EndScrollView();

            RectDivider statLabel = new RectDivider(new Rect(0, contractedRect.height * 0.60f, contractedRect.width, contractedRect.height * 0.05f), ContextHash);
            Widgets.ListSeparator(ref statLabel, "ArchiteReinforcement.DivisionStats".Translate());

            Rect statListRect = new Rect(0, contractedRect.height * 0.65f, contractedRect.width, contractedRect.height * 0.35f);
            Rect statScrollContent = new Rect(statListRect);
            statScrollContent.height = statCount * RowHeight;
            statScrollContent.x = 0f;
            statScrollContent.y = 0f;
            statScrollContent.width *= 0.95f;

            Listing_Standard statScrollListing = new Listing_Standard();

            Widgets.BeginScrollView(statListRect, ref statScroll, statScrollContent);
            statScrollListing.Begin(statScrollContent);

            alternate = false;

            if (!comp.statUpgrades.NullOrEmpty())
            {
                List<StatArchiteDef> allStats = comp.statUpgrades.Keys.ToList();
                allStats.Sort((a, b) => a.CompareTo(b));

                foreach (StatArchiteDef stat in allStats)
                {
                    rowRect = statScrollListing.GetRect(RowHeight);
                    float halfWidth = rowRect.width / 2f;
                    float halfHeight = rowRect.height / 2f;

                    rectName = new Rect(rowRect.x, rowRect.y, halfWidth, halfHeight);
                    rectEffect = new Rect(rowRect.x, rowRect.y + halfHeight, halfWidth, halfHeight);
                    rectCost = new Rect(rowRect.x + halfWidth, rowRect.y, halfWidth, halfHeight);
                    rectButton = new Rect(rowRect.x + halfWidth, rowRect.y + halfHeight, halfWidth, halfHeight);

                    if (alternate)
                        Widgets.DrawBoxSolid(rowRect, highlightColor);

                    int levelInt = comp.statUpgrades[stat];
                    string level = levelInt.ToString();
                    string levelReadout = stat.maxUses == null ? level : (string)"ArchiteReinforcement.MenuItemLevelRatio".Translate(level, stat.maxUses.ToString());

                    Widgets.Label(rectName, "ArchiteReinforcement.MenuItemName".Translate(stat.LabelCap, levelReadout));
                    Widgets.Label(rectEffect, stat.ValueModAtLevel(levelInt, stat.stat.LabelCap));
                    Widgets.Label(rectCost, "ArchiteReinforcement.MenuItemCost".Translate(stat.upgradeValue.ToStringByStyle(ToStringStyle.FloatMaxTwo), "ArchiteReinforcement.UpgradeTypeStat".Translate()));
                    if (comp.statArchitesToSpend >= stat.upgradeValue)
                    {
                        if (Widgets.ButtonText(rectButton, "ArchiteReinforcement.MenuItemButton".Translate()))
                        {
                            comp.TryBuyUpgrade(stat);
                        }
                    }

                    if (Mouse.IsOver(rowRect))
                    {
                        Widgets.DrawHighlight(rowRect);
                        TooltipHandler.TipRegion(rowRect, stat.DescriptionWithBreakdown());
                    }

                    alternate = !alternate;
                }
            }

            statScrollListing.End();
            Widgets.EndScrollView();

            GUI.EndGroup();
        }
    }
}
