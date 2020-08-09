using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
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

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && !this.initialized)
            {
                //Log.Message(this + " is created", true);
                this.initialized = true;
                this.questToUnlock = DefDatabase<QuestScriptDef>.AllDefs.Where(q => q.IsRootAny && this.HasMapNode(q.root)).RandomElement();
            }
        }

        public bool HasMapNode(QuestNode node)
        {
            if (node is QuestNode_GenerateSite || node is QuestNode_GenerateWorldObject || node is QuestNode_GetSiteTile)
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

        public void UnlockQuest(Pawn pawn)
        {
            if (this.questToUnlock != null)
            {
                Slate slate = new Slate();
                slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(pawn.Map));
                if (this.questToUnlock == QuestScriptDefOf.LongRangeMineralScannerLump)
                {
                    slate.Set("targetMineable", ThingDefOf.MineableGold);
                    slate.Set("worker", PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
                }
                Quest quest = QuestGen.Generate(this.questToUnlock, slate);
                Find.SignalManager.RegisterReceiver(quest);
                List<QuestPart> partsListForReading = quest.PartsListForReading;
                for (int i = 0; i < partsListForReading.Count; i++)
                {
                    partsListForReading[i].PostQuestAdded();
                }
                quest.Initiate();
                this.used = true;
                Find.LetterStack.ReceiveLetter("VBE.LocationsOpened".Translate(), "VBE.LocationsOpenedDesc".Translate(), LetterDefOf.NeutralEvent,
                    quest.QuestLookTargets.Where(t => t.IsWorldTarget || t.IsMapTarget).FirstOrDefault());
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false, 0))
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

