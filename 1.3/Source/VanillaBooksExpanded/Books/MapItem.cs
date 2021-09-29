using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaBooksExpanded
{
    public class MapItem : Book
    {
        public bool used = false;

        public bool initialized = false;

        public QuestScriptDef questToUnlock;

        private static HashSet<string> bannedQuestDefs = new HashSet<string>
        {
            "OpportunitySite_PeaceTalks",
            "EndGame_ShipEscape",
        };

        public override void PostMake()
        {
            base.PostMake();
            if (!this.initialized)
            {
                this.initialized = true;
                this.questToUnlock = GetRandomQuestDef();
            }
        }

        private bool MapCheck(QuestNode node)
        {
            if (ModsConfig.IdeologyActive)
            {
                if (node is QuestNode_Root_WorkSite || node is QuestNode_Root_Hack_AncientComplex || node is QuestNode_Root_Hack_Spacedrone
                    || node is QuestNode_Root_Hack_WorshippedTerminal || node is QuestNode_Root_Loot_AncientComplex || node is QuestNode_Root_Mission_AncientComplex
                     || node is QuestNode_Root_RelicHunt || node is QuestNode_Root_ReliquaryPilgrims)
                {
                    return true;
                }
            }
            if (ModsConfig.RoyaltyActive)
            {
                if (node is QuestNode_Root_Mission_BanditCamp)
                {
                    return true;
                }
            }
            if (node is QuestNode_GenerateSite || node is QuestNode_GenerateWorldObject || node is QuestNode_GetSiteTile)
            {
                return true;
            }
            return false;
        }
        public bool HasMapNode(QuestNode node)
        {
            if (MapCheck(node))
            {
                return true;
            }
            else if (node is QuestNode_RandomNode randomNode)
            {
                foreach (var node2 in randomNode.nodes)
                {
                    if (HasMapNode(node2))
                    {
                        return true;
                    }
                }
            }
            else if (node is QuestNode_Sequence sequence)
            {
                foreach (var node3 in sequence.nodes)
                {
                    if (HasMapNode(node3))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private QuestScriptDef GetRandomQuestDef()
        {
            return DefDatabase<QuestScriptDef>.AllDefs.Where(q => !bannedQuestDefs.Contains(q.defName) && q.IsRootAny && this.HasMapNode(q.root)).RandomElement();
        }
        private bool TryGetRandomPlayerRelic(out Precept_Relic relic)
        {
            return (from p in Faction.OfPlayer.ideos.PrimaryIdeo.GetAllPreceptsOfType<Precept_Relic>()
                    where p.CanGenerateRelic
                    select p).TryRandomElement(out relic);
        }
        public void UnlockQuest(Pawn pawn)
        {
            int attempts = 0;
            while (this.questToUnlock != null && attempts < 100)
            {
                attempts++;
                Slate slate = new Slate();
                HarmonyPatches.CustomParmsPoints = StorytellerUtility.DefaultThreatPointsNow(pawn.Map);
                slate.Set("points", HarmonyPatches.CustomParmsPoints.Value);
                slate.Set("population", PawnsFinder.AllMaps_FreeColonists.Count);
                slate.Set("colonistsSingularOrPlural", 1);
                slate.Set("passengersSingularOrPlural", 1);
                
                if (this.questToUnlock == QuestScriptDefOf.LongRangeMineralScannerLump)
                {
                    var mineables = DefDatabase<ThingDef>.AllDefs.Where(x => x.building?.mineableThing != null).ToList();
                    var mineableRock = mineables.RandomElementByWeight(x => x.building.mineableScatterCommonality);
                    var mineableThing = mineableRock.building.mineableThing;

                    slate.Set("targetMineableThing", mineableThing);
                    slate.Set("targetMineable", mineableRock);
                    slate.Set("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
                }
                if (ModsConfig.IdeologyActive)
                {
                    if (!TryGetRandomPlayerRelic(out var relic))
                    {
                        relic = Faction.OfPlayer.ideos.PrimaryIdeo.GetAllPreceptsOfType<Precept_Relic>().RandomElement();
                    }
                    slate.Set("ideo", Faction.OfPlayer.ideos.PrimaryIdeo);
                    slate.Set("relic", relic);
                    slate.Set("relicThing", relic.GenerateRelic());
                }

                if (this.questToUnlock.CanRun(slate))
                {
                    Quest quest = QuestGen.Generate(this.questToUnlock, slate);
                    Find.SignalManager.RegisterReceiver(quest);
                    List<QuestPart> partsListForReading = quest.PartsListForReading;

                    for (int i = 0; i < partsListForReading.Count; i++)
                    {
                        partsListForReading[i].PostQuestAdded();
                    }
                    quest.Initiate();

                    GlobalTargetInfo questTarget = GlobalTargetInfo.Invalid;
                    for (int i = 0; i < partsListForReading.Count; i++)
                    {
                        if (partsListForReading[i] is QuestPart_SpawnWorldObject part)
                        {
                            part.Notify_QuestSignalReceived(new Signal(part.inSignal));
                            questTarget = part.QuestLookTargets.FirstOrDefault();
                        }
                    }
                    this.used = true;
                    if (!questTarget.IsValid)
                    {
                        quest.QuestLookTargets.Where(t => t.IsWorldTarget || t.IsMapTarget).FirstOrDefault();
                    }
                    Find.LetterStack.ReceiveLetter("VBE.LocationsOpened".Translate(), "VBE.LocationsOpenedDesc".Translate(), LetterDefOf.NeutralEvent, questTarget);
                    this.questToUnlock = null;
                    break;
                }
                else
                {
                    this.questToUnlock = GetRandomQuestDef();
                }
                HarmonyPatches.CustomParmsPoints = null;
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("CannotUseNoPath"), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            else if (!this.used)
            {
                string label = "VBE.ReadMap".Translate();
                Action action = delegate ()
                {
                    Job job = JobMaker.MakeJob(VBE_DefOf.VBE_ReadBook, null, this);
                    job.count = 1;
                    myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                };
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption
                        (label, action, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn,
                        this, "ReservedBy");
            }
            else if (this.used)
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("VBE.CantReadMapUsed"), null,
                MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            yield break;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.stopDraw, "stopDraw", false);
            Scribe_Values.Look<bool>(ref this.used, "used", false);
            Scribe_Values.Look<bool>(ref this.initialized, "initialized", false);
            Scribe_Defs.Look<QuestScriptDef>(ref this.questToUnlock, "questToUnlock");

        }
    }
}

