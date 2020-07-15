using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace VanillaBooksExpanded
{

    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("Vanilla.BookExpanded").PatchAll();
        }
    }

    [HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("PostProcessProduct")]
    public static class Patch_PostProcessProduct
    {
        [HarmonyPostfix]
        public static void Postfix(ref Thing product, RecipeDef recipeDef, Pawn worker)
        {
            if (product is Book book)
            {
                book.TryGetComp<CompBook>().JustCreatedBy(worker);
            }
        }
    }
}


