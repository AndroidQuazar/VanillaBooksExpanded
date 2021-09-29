using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace VanillaBooksExpanded
{

    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("Vanilla.BookExpanded");
            harmony.PatchAll();
            foreach (Type type in typeof(SitePartWorker).InstantiableDescendantsAndSelf())
            {
                var method = AccessTools.Method(type, "Notify_GeneratedByQuestGen");
                if (method != null)
                {
                    try
                    {
                        harmony.Patch(method, new HarmonyMethod(AccessTools.Method(typeof(HarmonyPatches), "Notify_GeneratedByQuestGenPrefix")));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                foreach (var def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (typeof(Book).IsAssignableFrom(def.thingClass))
                    {
                        def.stackLimit = 1;
                    }
                }
            });
        }

        public static float? CustomParmsPoints;
        private static void Notify_GeneratedByQuestGenPrefix(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
        {
            if (CustomParmsPoints.HasValue)
            {
                if (part.site.Faction != null)
                {
                    part.parms.threatPoints = Mathf.Max(CustomParmsPoints.Value * 0.5f, part.site.Faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
                    part.parms.points = part.parms.threatPoints;
                }
            }
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

    [HarmonyPatch(typeof(StockGeneratorUtility))]
    [HarmonyPatch("TryMakeForStock")]
    public static class Patch_TryMakeForStock
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, ThingDef thingDef, int count, Faction faction)
        {
            foreach (var t in __result)
            {
                if (t is Book && t.stackCount > 1)
                {
                    for (var i = 0; i < t.stackCount; i++)
                    {
                        yield return StockGeneratorUtility.TryMakeForStockSingle(thingDef, 1, faction);
                    }
                }
                else
                {
                    yield return t;
                }
            }
        }
    }
}


