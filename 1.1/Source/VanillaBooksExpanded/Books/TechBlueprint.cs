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
    public class TechBlueprint : Book
    {
        public bool used = false;

        public ResearchProjectDef researchProject;

        public bool initialized = false;

        public override string Label
        {
            get
            {
                if (this.researchProject != null)
                {
                    return base.Label + " (" + this.researchProject.LabelCap + ")";
                }
                return base.Label;
            }
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && !initialized)
            {
                this.used = false;
                this.researchProject = DefDatabase<ResearchProjectDef>.AllDefs.RandomElement();
                //Log.Message(this + " is created");
                initialized = true;
            }
        }

        public void UnlockResearch(Pawn pawn)
        {
            Find.ResearchManager.FinishProject(this.researchProject, false, pawn);
            Find.LetterStack.ReceiveLetter("ResearchFinished".Translate(this.researchProject.LabelCap),
                this.researchProject.description, LetterDefOf.PositiveEvent, pawn);
            this.used = true;
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false, 0))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("CannotUseNoPath"), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            else if (!this.used && !this.researchProject.IsFinished && this.researchProject.PrerequisitesCompleted)
            {
                string label = "VBE.ReadBlueprint".Translate();
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
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("VBE.CantReadBlueprintUsed"), null,
                MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            else if (this.researchProject.IsFinished)
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("VBE.CantReadBlueprintAlreadyResearched"), null,
                MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            else if (!this.researchProject.PrerequisitesCompleted)
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("VBE.CantReadBlueprintTooAdvanced"), null,
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
            Scribe_Defs.Look<ResearchProjectDef>(ref this.researchProject, "researchProject");
        }
    }
}

