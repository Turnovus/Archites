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
    public class Mod_ArchiteReinforcement : Mod
    {
        public const string SettingKey = "ArchiteReinforcement.ModSettings.";
        public const string SettingLabel = ".Label";
        public const string SettingDescription = ".Desc";

        public ModSettings_ArchiteReinforcement Settings =>
            GetSettings<ModSettings_ArchiteReinforcement>();

        public Mod_ArchiteReinforcement(ModContentPack content) : base(content) { }

        public override string SettingsCategory() =>
            "ArchiteReinforcement.ModSettings.SettingsCategory".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            DoCheckboxListing(listing, "EnableArchiteSpawns", ref Settings.enableArchiteSpawns);

            DoLabelListing(listing, "ArchiteBadgeVisibility", true);
            DoCheckboxListing(listing, "DrawBadgeForColonists", ref Settings.drawBadgeForColonists);
            DoCheckboxListing(listing, "DrawBadgeForSlaves", ref Settings.drawBadgeForSlaves);
            DoCheckboxListing(listing, "DrawBadgeForPrisoners", ref Settings.drawBadgeForPrisoners);
            DoCheckboxListing(listing, "DrawBadgeForHostiles", ref Settings.drawBadgeForHostiles);
            DoCheckboxListing(listing, "DrawBadgeForNeutrals", ref Settings.drawBadgeForNeutrals);

            listing.End();
        }

        private static void DoCheckboxListing (
            Listing_Standard list,
            string key,
            ref bool setting,
            bool addTooltip = true
        )
        {
            string label = SettingKey + key + SettingLabel;
            string description = SettingKey + key + SettingDescription;
            list.CheckboxLabeled(label.Translate(), ref setting, addTooltip ? description.Translate() : null);
        }

        private static void DoLabelListing(
            Listing_Standard list,
            string key,
            bool addTooltip = false
        )
        {
            string label = SettingKey + key + SettingLabel;
            string description = SettingKey + key + SettingDescription;
            Text.Font = GameFont.Medium;
            list.Label(label.Translate(), tooltip: addTooltip ? description.Translate() : null);
            Text.Font = GameFont.Small;
        }
    }

    public class ModSettings_ArchiteReinforcement : ModSettings
    {
        public bool enableArchiteSpawns = true;

        public bool drawBadgeForColonists = false;
        public bool drawBadgeForSlaves = false;
        public bool drawBadgeForPrisoners = false;
        public bool drawBadgeForHostiles = true;
        public bool drawBadgeForNeutrals = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableArchiteSpawns, "enableArchiteSpawns");

            Scribe_Values.Look(ref drawBadgeForColonists, "drawBadgeForColonists");
            Scribe_Values.Look(ref drawBadgeForSlaves, "drawBadgeForSlaves");
            Scribe_Values.Look(ref drawBadgeForPrisoners, "drawBadgeForPrisoners");
            Scribe_Values.Look(ref drawBadgeForHostiles, "drawBadgeForHostiles");
            Scribe_Values.Look(ref drawBadgeForNeutrals, "drawBadgeForNeutrals");
        }
    }
}
