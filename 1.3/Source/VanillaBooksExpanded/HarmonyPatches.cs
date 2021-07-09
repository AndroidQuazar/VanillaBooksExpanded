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
                        Log.Message("Patching " + method);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
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
                    Log.Message("part.parms.threatPoints: " + part.parms.threatPoints);
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

    //[HarmonyPatch(typeof(QuestNode_Root_RelicHunt))]
    //[HarmonyPatch("RunInt")]
    //public static class Patch_RunInt
    //{
	//	private const int RelicsInfoRequiredCount = 5;
    //
	//	private const int MinIntervalTicks = 300000;
    //
	//	private const int MaxIntervalTicks = 600000;
    //
	//	private const int MinDistanceFromColony = 2;
    //
	//	private const int MaxDistanceFromColony = 10;
    //
	//	private const int SecurityWakeupDelayTicks = 180000;
    //
	//	private const int SecurityWakupDelayCriticalTicks = 2500;
    //
	//	private static readonly SimpleCurve ExteriorThreatPointsOverPoints = new SimpleCurve
	//	{
	//		new CurvePoint(0f, 500f),
	//		new CurvePoint(500f, 500f),
	//		new CurvePoint(10000f, 10000f)
	//	};
    //
	//	private static readonly SimpleCurve InteriorThreatPointsOverPoints = new SimpleCurve
	//	{
	//		new CurvePoint(0f, 300f),
	//		new CurvePoint(300f, 300f),
	//		new CurvePoint(10000f, 5000f)
	//	};
    //
    //    public static bool Prefix()
    //    {
    //        Log.Message(" - Prefix - if (!ModLister.CheckIdeology(\"Relic hunt rescue\")) - 1", true);
    //        if (!ModLister.CheckIdeology("Relic hunt rescue"))
    //        {
    //            Log.Message(" - Prefix - return true; - 2", true);
    //            return true;
    //        }
    //        Quest quest = QuestGen.quest;
    //        Log.Message(" - Prefix - Slate slate = QuestGen.slate; - 4", true);
    //        Slate slate = QuestGen.slate;
    //        Log.Message(" - Prefix - Map map = QuestGen_Get.GetMap(); - 5", true);
    //        Map map = QuestGen_Get.GetMap();
    //        Log.Message(" - Prefix - float num = slate.Get(\"points\", 0f); - 6", true);
    //        float num = slate.Get("points", 0f);
    //        Log.Message(" - Prefix - Ideo primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo; - 7", true);
    //        Ideo primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
    //        Log.Message(" - Prefix - TryGetRandomPlayerRelic(out var relic); - 8", true);
    //        TryGetRandomPlayerRelic(out var relic);
    //        Log.Message(" - Prefix - TryFindSiteTile(out var tile); - 9", true);
    //        TryFindSiteTile(out var tile);
    //        Log.Message(" - Prefix - string text = QuestGen.GenerateNewSignal(\"SubquestsCompleted\"); - 10", true);
    //        string text = QuestGen.GenerateNewSignal("SubquestsCompleted");
    //        Log.Message(" - Prefix - string awakenSecurityThreatsSignal = QuestGen.GenerateNewSignal(\"AwakenSecurityThreats\"); - 11", true);
    //        string awakenSecurityThreatsSignal = QuestGen.GenerateNewSignal("AwakenSecurityThreats");
    //        Log.Message(" - Prefix - QuestGen.GenerateNewSignal(\"PostMapAdded\"); - 12", true);
    //        QuestGen.GenerateNewSignal("PostMapAdded");
    //        Log.Message(" - Prefix - string text2 = QuestGen.GenerateNewSignal(\"RelicLostFromMap\"); - 13", true);
    //        string text2 = QuestGen.GenerateNewSignal("RelicLostFromMap");
    //        Log.Message(" - Prefix - bool allowViolentQuests = Find.Storyteller.difficulty.allowViolentQuests; - 14", true);
    //        bool allowViolentQuests = Find.Storyteller.difficulty.allowViolentQuests;
    //        Log.Message(" - Prefix - QuestPart_SubquestGenerator_RelicHunt questPart_SubquestGenerator_RelicHunt = new QuestPart_SubquestGenerator_RelicHunt(); - 15", true);
    //        QuestPart_SubquestGenerator_RelicHunt questPart_SubquestGenerator_RelicHunt = new QuestPart_SubquestGenerator_RelicHunt();
    //        Log.Message(" - Prefix - questPart_SubquestGenerator_RelicHunt.inSignalEnable = QuestGen.slate.Get<string>(\"inSignal\"); - 16", true);
    //        questPart_SubquestGenerator_RelicHunt.inSignalEnable = QuestGen.slate.Get<string>("inSignal");
    //        Log.Message(" - Prefix - questPart_SubquestGenerator_RelicHunt.interval = new IntRange(300000, 600000); - 17", true);
    //        questPart_SubquestGenerator_RelicHunt.interval = new IntRange(300000, 600000);
    //        Log.Message(" - Prefix - questPart_SubquestGenerator_RelicHunt.relic = relic; - 18", true);
    //        questPart_SubquestGenerator_RelicHunt.relic = relic;
    //        Log.Message(" - Prefix - questPart_SubquestGenerator_RelicHunt.relicSlateName = \"relic\"; - 19", true);
    //        questPart_SubquestGenerator_RelicHunt.relicSlateName = "relic";
    //        Log.Message(" - Prefix - questPart_SubquestGenerator_RelicHunt.useMapParentThreatPoints = map.Parent; - 20", true);
    //        questPart_SubquestGenerator_RelicHunt.useMapParentThreatPoints = map.Parent;
    //        Log.Message(" - Prefix - questPart_SubquestGenerator_RelicHunt.expiryInfoPartKey = \"RelicInfoFound\"; - 21", true);
    //        questPart_SubquestGenerator_RelicHunt.expiryInfoPartKey = "RelicInfoFound";
    //        Log.Message(" - Prefix - questPart_SubquestGenerator_RelicHunt.maxSuccessfulSubquests = 5; - 22", true);
    //        questPart_SubquestGenerator_RelicHunt.maxSuccessfulSubquests = 5;
    //        Log.Message(" - Prefix - questPart_SubquestGenerator_RelicHunt.subquestDefs.AddRange(GetAllSubquests(QuestGen.Root)); - 23", true);
    //        questPart_SubquestGenerator_RelicHunt.subquestDefs.AddRange(GetAllSubquests(QuestGen.Root));
    //        Log.Message(" - Prefix - questPart_SubquestGenerator_RelicHunt.outSignalsCompleted.Add(text); - 24", true);
    //        questPart_SubquestGenerator_RelicHunt.outSignalsCompleted.Add(text);
    //        Log.Message(" - Prefix - quest.AddPart(questPart_SubquestGenerator_RelicHunt); - 25", true);
    //        quest.AddPart(questPart_SubquestGenerator_RelicHunt);
    //        Log.Message(" - Prefix - QuestGenUtility.RunAdjustPointsForDistantFight(); - 26", true);
    //        QuestGenUtility.RunAdjustPointsForDistantFight();
    //        Log.Message(" - Prefix - num = slate.Get(\"points\", 0f); - 27", true);
    //        num = slate.Get("points", 0f);
    //        Log.Message(" - Prefix - Thing thing = relic.GenerateRelic(); - 28", true);
    //        Thing thing = relic.GenerateRelic();
    //        Log.Message(" - Prefix - QuestGen_Signal.SignalPass(inSignal: QuestGenUtility.HardcodedSignalWithQuestID(\"relicThing.StartedExtractingFromContainer\"), quest: quest, action: null, outSignal: awakenSecurityThreatsSignal); - 29", true);
    //        QuestGen_Signal.SignalPass(inSignal: QuestGenUtility.HardcodedSignalWithQuestID("relicThing.StartedExtractingFromContainer"), quest: quest, action: null, outSignal: awakenSecurityThreatsSignal);
    //        Reward_Items item = new Reward_Items
    //        {
    //            items = { thing }
    //        };
    //        Log.Message(" - Prefix - QuestPart_Choice questPart_Choice = quest.RewardChoice(); - 31", true);
    //        QuestPart_Choice questPart_Choice = quest.RewardChoice();
    //        QuestPart_Choice.Choice item2 = new QuestPart_Choice.Choice
    //        {
    //            rewards = { (Reward)item }
    //        };
    //        Log.Message(" - Prefix - questPart_Choice.choices.Add(item2); - 33", true);
    //        questPart_Choice.choices.Add(item2);
    //        Log.Message(" - Prefix - float num2 = (allowViolentQuests ? num : 0f); - 34", true);
    //        float num2 = (allowViolentQuests ? num : 0f);
    //        SitePartParams sitePartParams = new SitePartParams
    //        {
    //            points = num2,
    //            relicThing = thing,
    //            triggerSecuritySignal = awakenSecurityThreatsSignal,
    //            relicLostSignal = text2
    //        };
    //        Log.Message(" - Prefix - if (num2 > 0f) - 36", true);
    //        if (num2 > 0f)
    //        {
    //            Log.Message(" - Prefix - sitePartParams.exteriorThreatPoints = ExteriorThreatPointsOverPoints.Evaluate(num2); - 37", true);
    //            sitePartParams.exteriorThreatPoints = ExteriorThreatPointsOverPoints.Evaluate(num2);
    //            Log.Message(" - Prefix - sitePartParams.interiorThreatPoints = InteriorThreatPointsOverPoints.Evaluate(num2); - 38", true);
    //            sitePartParams.interiorThreatPoints = InteriorThreatPointsOverPoints.Evaluate(num2);
    //        }
    //        Site site = QuestGen_Sites.GenerateSite(Gen.YieldSingle(new SitePartDefWithParams(SitePartDefOf.AncientAltar, sitePartParams)), tile, Faction.OfAncientsHostile);
    //        Log.Message(" - Prefix - quest.SpawnWorldObject(site, null, text); - 40", true);
    //        quest.SpawnWorldObject(site, null, text);
    //        Log.Message(" - Prefix - TaggedString taggedString = \"LetterTextRelicFoundLocation\".Translate(relic.Label); - 41", true);
    //        TaggedString taggedString = "LetterTextRelicFoundLocation".Translate(relic.Label);
    //        Log.Message(" - Prefix - if (allowViolentQuests) - 42", true);
    //        if (allowViolentQuests)
    //        {
    //            taggedString += "" + "LetterTextRelicFoundSecurityThreats".Translate(180000.ToStringTicksToPeriodVague());
    //                    }
    //        quest.Letter(LetterDefOf.RelicHuntInstallationFound, text, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, label: "LetterLabelRelicFound".Translate(relic.Label), text: taggedString, lookTargets: Gen.YieldSingle(site));
    //        Log.Message(" - Prefix - quest.DescriptionPart(\"RelicHuntFindRelicSite\".Translate(thing.Label), quest.AddedSignal, text, QuestPart.SignalListenMode.OngoingOrNotYetAccepted); - 44", true);
    //        quest.DescriptionPart("RelicHuntFindRelicSite".Translate(thing.Label), quest.AddedSignal, text, QuestPart.SignalListenMode.OngoingOrNotYetAccepted);
    //        Log.Message(" - Prefix - if (allowViolentQuests) - 45", true);
    //        if (allowViolentQuests)
    //        {
    //            Log.Message(" - Prefix - quest.DescriptionPart(\"RelicHuntFoundRelicSite\".Translate(), text); - 46", true);
    //            quest.DescriptionPart("RelicHuntFoundRelicSite".Translate(), text);
    //            QuestPart_Delay part = new QuestPart_Delay
    //            {
    //                delayTicks = 180000,
    //                alertLabel = "AncientAltarThreatsWaking".Translate(),
    //                alertExplanation = "AncientAltarThreatsWakingDesc".Translate(),
    //                ticksLeftAlertCritical = 2500,
    //                inSignalEnable = text,
    //                alertCulprits =
    //                                    {
    //                                            (GlobalTargetInfo)thing,
    //                                            (GlobalTargetInfo)site
    //                                    },
    //                isBad = true,
    //                outSignalsCompleted = { awakenSecurityThreatsSignal }
    //            };
    //            Log.Message(" - Prefix - quest.AddPart(part); - 48", true);
    //            quest.AddPart(part);
    //            Log.Message(" - Prefix - string text4 = QuestGen.GenerateNewSignal(\"ReTriggerSecurityThreats\"); - 49", true);
    //            string text4 = QuestGen.GenerateNewSignal("ReTriggerSecurityThreats");
    //            QuestPart_PassWhileActive part2 = new QuestPart_PassWhileActive
    //            {
    //                inSignalEnable = awakenSecurityThreatsSignal,
    //                inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated"),
    //                outSignal = text4
    //            };
    //            Log.Message(" - Prefix - quest.AddPart(part2); - 51", true);
    //            quest.AddPart(part2);
    //            quest.SignalPass(delegate
    //            {
    //                Log.Message(" - Prefix - quest.SignalPass(null, null, awakenSecurityThreatsSignal); - 52", true);
    //                quest.SignalPass(null, null, awakenSecurityThreatsSignal);
    //                Log.Message(" - Prefix - quest.Message(\"MessageAncientAltarThreatsAlerted\".Translate(), MessageTypeDefOf.NegativeEvent); - 53", true);
    //                quest.Message("MessageAncientAltarThreatsAlerted".Translate(), MessageTypeDefOf.NegativeEvent);
    //                Log.Message(" - Prefix - }, text4); - 54", true);
    //            }, text4);
    //            quest.AnyHostileThreatToPlayer(site, countDormantPawns: true, delegate
    //            {
    //                Log.Message(" - Prefix - quest.Message(\"MessageAncientAltarThreatsWokenUp\".Translate(), MessageTypeDefOf.NegativeEvent); - 55", true);
    //                quest.Message("MessageAncientAltarThreatsWokenUp".Translate(), MessageTypeDefOf.NegativeEvent);
    //                Log.Message(" - Prefix - }, null, awakenSecurityThreatsSignal); - 56", true);
    //            }, null, awakenSecurityThreatsSignal);
    //        }
    //        else
    //        {
    //            Log.Message(" - Prefix - quest.DescriptionPart(\"RelicHuntFoundRelicSitePeaceful\".Translate(), text); - 57", true);
    //            quest.DescriptionPart("RelicHuntFoundRelicSitePeaceful".Translate(), text);
    //        }
    //        quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("relicThing.Destroyed"), QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
    //        Log.Message(" - Prefix - quest.End(QuestEndOutcome.Success, 0, null, text2, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true); - 59", true);
    //        quest.End(QuestEndOutcome.Success, 0, null, text2, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
    //        Log.Message(" - Prefix - slate.Set(\"ideo\", primaryIdeo); - 60", true);
    //        slate.Set("ideo", primaryIdeo);
    //        Log.Message(" - Prefix - slate.Set(\"relic\", relic); - 61", true);
    //        slate.Set("relic", relic);
    //        Log.Message(" - Prefix - slate.Set(\"relicThing\", thing); - 62", true);
    //        slate.Set("relicThing", thing);
    //        Log.Message(" - Prefix - slate.Set(\"site\", site); - 63", true);
    //        slate.Set("site", site);
    //        Log.Message(" - Prefix - return false; - 64", true);
    //        return false;
    //    }
    //
	//	private static bool TryFindSiteTile(out int tile, bool exitOnFirstTileFound = false)
	//	{
	//		return TileFinder.TryFindNewSiteTile(out tile, 2, 10, allowCaravans: false, TileFinderMode.Near, -1, exitOnFirstTileFound);
	//	}
    //
	//	private static bool TryGetRandomPlayerRelic(out Precept_Relic relic)
	//	{
	//		return (from p in Faction.OfPlayer.ideos.PrimaryIdeo.GetAllPreceptsOfType<Precept_Relic>()
	//				where p.CanGenerateRelic
	//				select p).TryRandomElement(out relic);
	//	}
    //
	//	private static IEnumerable<QuestScriptDef> GetAllSubquests(QuestScriptDef parent)
	//	{
	//		return DefDatabase<QuestScriptDef>.AllDefs.Where((QuestScriptDef q) => q.epicParent == parent);
	//	}
    //
	//	private static bool TestRunInt(Slate slate)
	//	{
	//		if (TryGetRandomPlayerRelic(out var _) && TryFindSiteTile(out var _, exitOnFirstTileFound: true))
	//		{
	//			return GetAllSubquests(QuestGen.Root).Any();
	//		}
	//		return false;
	//	}
	//}
}


