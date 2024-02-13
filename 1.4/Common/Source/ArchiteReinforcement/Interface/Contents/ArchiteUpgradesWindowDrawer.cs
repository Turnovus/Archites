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
    public static class ArchiteUpgradesWindowDrawer
    {
        public const float InnerMargin = 10f;
        public const float ProgressRectFillRatio = 0.20f;

        public const float ProgressRectHeaderFillRatio = 0.25f;
        public const float ProgressRectFooterFillRatio = 0.25f;
        public const float ProgressBarLabelWidthRatio = 0.40f;
        public const float ProgressBarVerticalMargin = 0.1f;

        public const float ListHeaderFillRatio = 0.12f;
        public const float UpgradeRowHeight = 46f;
        public const float ListScrollContentWidthRatio = 0.95f;

        private static readonly Color HighlightColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        private const int ContextHash = 402101046;

        public static void FillRect(
            CompArchiteTracker tracker,
            Rect rect,
            ref Vector2 capacityScrollPosition,
            ref Vector2 StatScrollPosition,
            bool allowPurchases
        )
        {
            Rect mainRect = rect.ContractedBy(InnerMargin);
            GUI.BeginGroup(mainRect);

            float mainRectWidth = mainRect.width;
            float mainRectHeight = mainRect.height;

            float progressRectHeight = mainRectHeight * ProgressRectFillRatio;
            Rect progressRect = new Rect(0f, 0f, mainRectWidth, progressRectHeight);
            FillProgressRect(tracker, progressRect);

            float listRectHeight = (mainRectHeight - progressRectHeight) * 0.5f;

            Rect capacityUpgradeRect = new Rect(0, progressRectHeight, mainRectWidth, listRectHeight);
            FillCapacityUpgradeList(tracker, capacityUpgradeRect, allowPurchases, ref capacityScrollPosition);

            Rect statUpgradeRect = new Rect(0, progressRectHeight + listRectHeight, mainRectWidth, listRectHeight);
            FillStatUpgradeList(tracker, statUpgradeRect, allowPurchases, ref StatScrollPosition);

            GUI.EndGroup(); // mainRect
        }

        private static void FillProgressRect(CompArchiteTracker tracker, Rect rect)
        {
            float width = rect.width;
            float height = rect.height;

            float headerHeight = height * ProgressRectHeaderFillRatio;
            float footerHeight = height * ProgressRectFooterFillRatio;
            float barHeight = (height - footerHeight - headerHeight) * 0.5f;

            Rect labelRect = new Rect(0, 0, width, headerHeight);
            DrawLabel(rect, "ArchiteReinforcement.DivisionProgress".Translate());

            Rect capacityProgressBar = new Rect(0, headerHeight, width, barHeight);
            FillCapacityBar(tracker, capacityProgressBar);

            Rect statProgressBar = new Rect(0, headerHeight + barHeight, width, barHeight);
            FillStatBar(tracker, statProgressBar);

            Rect footerRect = new Rect(0, headerHeight + barHeight*2f, width, footerHeight);
            DrawProgressFooter(footerRect, tracker.capacityArchitesToSpend, tracker.statArchitesToSpend);
        }

        private static void DrawLabel(Rect rect, string label)
        {
            RectDivider labelDivider = new RectDivider(rect, ContextHash);

            Widgets.ListSeparator(
                ref labelDivider,
                label
            );
        }

        private static void FillCapacityBar(CompArchiteTracker tracker, Rect rect)
        {
            DrawUpgradeProgressBar(
                rect,
                "ArchiteReinforcement.UpgradeTypeCapacity.Cap".Translate(),
                tracker.capacityArchiteProgress,
                tracker.CapacityUpgradeCost
            );
        }

        private static void FillStatBar(CompArchiteTracker tracker, Rect rect)
        {
            DrawUpgradeProgressBar(
                rect,
                "ArchiteReinforcement.UpgradeTypeStat.Cap".Translate(),
                tracker.statArchiteProgress,
                tracker.StatUpgradeCost
            );
        }

        private static void DrawUpgradeProgressBar(
            Rect rect,
            string upgradeLabel,
            float progress,
            float target
        )
        {
            Rect labelRect = new Rect(rect);
            labelRect.width *= ProgressBarLabelWidthRatio;
            Widgets.Label(labelRect, "ArchiteReinforcement.UpgradeProgress".Translate(
                upgradeLabel,
                progress.ToStringByStyle(ToStringStyle.FloatMaxTwo),
                target.ToStringByStyle(ToStringStyle.FloatMaxTwo)
            ));

            Rect barRect = rect.ContractedBy(0f, ProgressBarVerticalMargin);
            barRect.width -= labelRect.width;
            barRect.x += labelRect.width;
            Widgets.FillableBar(barRect, progress / target);
        }

        private static void DrawProgressFooter(Rect rect, float capacityPoints, float statPoints)
        {
            string fundsReadout = "ArchiteReinforcement.MenuAvailableArchites".Translate(
                "ArchiteReinforcement.ArchitesTypeQuantity".Translate(
                    "ArchiteReinforcement.UpgradeTypeCapacity.Cap".Translate(),
                    capacityPoints.ToStringByStyle(ToStringStyle.FloatMaxTwo)
                ),
                "ArchiteReinforcement.ArchitesTypeQuantity".Translate(
                    "ArchiteReinforcement.UpgradeTypeStat.Cap".Translate(),
                    statPoints.ToStringByStyle(ToStringStyle.FloatMaxTwo)
                )
            );
            float textWidth = Text.CalcSize(fundsReadout).x;
            float footerCenterHorizontal = 0.5f * (rect.width - textWidth);

            Rect labelRect = rect.ContractedBy(footerCenterHorizontal, 0f);
            Widgets.Label(labelRect, fundsReadout);
        }

        private static void FillCapacityUpgradeList(
            CompArchiteTracker tracker,
            Rect rect,
            bool allowPurchases,
            ref Vector2 scrollPosition
        )
        {
            FillUpgradeList(
                rect,
                tracker,
                "ArchiteReinforcement.DivisionCapacities".Translate(),
                GenerifyUpgradeCollection(tracker.capacityUpgrades),
                allowPurchases,
                tracker.capacityArchitesToSpend,
                ref scrollPosition
            );
        }

        private static void FillStatUpgradeList(
            CompArchiteTracker tracker,
            Rect rect,
            bool allowPurchases,
            ref Vector2 scrollPosition
        )
        {
            FillUpgradeList(
                rect,
                tracker,
                "ArchiteReinforcement.DivisionStats".Translate(),
                GenerifyUpgradeCollection(tracker.statUpgrades),
                allowPurchases,
                tracker.statArchitesToSpend,
                ref scrollPosition
            );
        }

        private static Dictionary<ArchiteDef, int> GenerifyUpgradeCollection(Dictionary<CapacityArchiteDef, int> collection)
        {
            Dictionary<ArchiteDef, int> generic = new Dictionary<ArchiteDef, int>();
            foreach (CapacityArchiteDef key in collection.Keys)
                generic[key] = collection[key];
            return generic;
        }

        private static Dictionary<ArchiteDef, int> GenerifyUpgradeCollection(Dictionary<StatArchiteDef, int> collection)
        {
            Dictionary<ArchiteDef, int> generic = new Dictionary<ArchiteDef, int>();
            foreach(StatArchiteDef key in collection.Keys)
                generic[key] = collection[key];
            return generic;
        }

        private static void FillUpgradeList(
            Rect rect,
            CompArchiteTracker tracker,
            string label,
            Dictionary<ArchiteDef, int> upgrades,
            bool allowPurchases,
            float budget,
            ref Vector2 scrollPosition
        )
        {
            float labelHeight = rect.height * ListHeaderFillRatio;
            Rect labelRect = new Rect(rect);
            labelRect.height = labelHeight;
            DrawLabel(labelRect, label);

            if (upgrades.NullOrEmpty())
                return; // TODO: "No Upgrades" text

            Rect listRect = new Rect(rect);
            listRect.height -= labelHeight;
            listRect.y += labelHeight;

            float upgradeCount = upgrades.Count;
            Rect scrollContent = new Rect(listRect);
            scrollContent.height = upgradeCount * UpgradeRowHeight;
            scrollContent.position = Vector2.zero;
            scrollContent.width *= ListScrollContentWidthRatio;

            Listing_Standard scrollListing = new Listing_Standard();
            Widgets.BeginScrollView(listRect, ref scrollPosition, scrollContent);
            scrollListing.Begin(scrollContent);

            bool alternate = false;

            List<ArchiteDef> upgradeKeysSorted = upgrades.Keys.ToList();
            upgradeKeysSorted.Sort((a, b) => a.CompareTo(b));

            foreach (ArchiteDef upgrade in upgradeKeysSorted)
            {
                DrawUpgradeListing(
                    scrollListing,
                    tracker,
                    upgrade,
                    upgrades[upgrade],
                    allowPurchases,
                    budget,
                    alternate
                );
                alternate = !alternate;
            }

            scrollListing.End();
            Widgets.EndScrollView();
        }

        private static void DrawUpgradeListing(
            Listing_Standard list,
            CompArchiteTracker tracker,
            ArchiteDef upgrade,
            int level,
            bool allowPurchases,
            float budget,
            bool highlight
        )
        {
            Rect rect = list.GetRect(UpgradeRowHeight);

            if (highlight)
                Widgets.DrawBoxSolid(rect, HighlightColor);

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, upgrade.DescriptionWithBreakdown());
            }

            float halfWidth = rect.width * 0.5f;
            float halfHeight = rect.height * 0.5f;

            Rect nameRect = new Rect(rect.x, rect.y, halfWidth, halfHeight);
            string levelLabel = level.ToString();
            string levelReadout = upgrade.maxUses == null ?
                levelLabel :
                (string)"ArchiteReinforcement.MenuItemLevelRatio".Translate(level, upgrade.maxUses.ToString());
            Widgets.Label(nameRect, "ArchiteReinforcement.MenuItemName".Translate(upgrade.LabelCap, levelReadout));

            Rect effectRect = new Rect(rect.x, rect.y + halfHeight, halfWidth, halfHeight);
            Widgets.Label(effectRect, upgrade.ValueModAtLevel(level, upgrade.NameOfThingToUpgrade));

            if (!allowPurchases)
                return;

            Rect costRect = new Rect(rect.x + halfWidth, rect.y, halfWidth, halfHeight);
            Widgets.Label(costRect, "ArchiteReinforcement.MenuItemCost".Translate(
                upgrade.upgradeValue.ToStringByStyle(ToStringStyle.FloatMaxTwo),
                "ArchiteReinforcement.UpgradeTypeCapacity".Translate()
            ));

            Rect buttonRect = new Rect(rect.x + halfWidth, rect.y + halfHeight, halfWidth, halfHeight);
            HandleUpgradeButton(buttonRect, tracker, upgrade, level, budget);
        }

        private static void HandleUpgradeButton(
            Rect rect,
            CompArchiteTracker tracker,
            ArchiteDef upgrade,
            int level,
            float budget
        )
        {
            if (budget < upgrade.upgradeValue)
                return;
            if (upgrade.maxUses != null && upgrade.maxUses == level)
                return;

            if (Widgets.ButtonText(rect, "ArchiteReinforcement.MenuItemButton".Translate()))
                tracker.TryBuyUpgrade(upgrade);
        }
    }
}
