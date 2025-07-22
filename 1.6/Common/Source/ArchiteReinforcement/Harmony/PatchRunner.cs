using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HarmonyLib;

namespace ArchiteReinforcement
{
    [StaticConstructorOnStartup]
    public static class PatchRunner
    {
        static PatchRunner()
        {
            new Harmony("Turnovus.RimWorld.ArchiteReinforcement").PatchAll();
        }
    }
}
