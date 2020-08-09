using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaBooksExpanded
{
    public class SkillBook : Book
    {
        public bool initialized = false;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && !initialized)
            {
                //Log.Message(this + " is created");
                var comp = this.TryGetComp<CompBook>();
                if (!comp.Active)
                {
                    comp.InitializeBook();
                }
                initialized = true;
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
            else if (this.CanLearnFromBook(myPawn))
            {
                string label = "VBE.ReadBook".Translate();
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
            else if (!this.CanLearnFromBook(myPawn))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("VBE.CantReadSkillBookTooSimple"), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            yield break;
        }


        public bool CanLearnFromBook(Pawn pawn)
        {
            int skillLevel = pawn.skills.GetSkill(SkillData.skillToTeach).Level;
            
            switch (this.TryGetComp<CompQuality>().Quality)
            {
                case QualityCategory.Awful:
                    return skillLevel < 4;
                case QualityCategory.Poor:
                    return skillLevel < 6;
                case QualityCategory.Normal:
                    return skillLevel < 8;
                case QualityCategory.Good:
                    return skillLevel < 10;
                case QualityCategory.Excellent:
                    return skillLevel < 14;
                case QualityCategory.Masterwork:
                    return skillLevel < 18;
                case QualityCategory.Legendary:
                    return skillLevel < 20;
                default:
                    throw new ArgumentException();
            }
        }

        public float GetLearnQuality()
        {
            switch (this.TryGetComp<CompQuality>().Quality)
            {
                case QualityCategory.Awful:
                    return SkillData.Awful;
                case QualityCategory.Poor:
                    return SkillData.Poor;
                case QualityCategory.Normal:
                    return SkillData.Normal;
                case QualityCategory.Good:
                    return SkillData.Good;
                case QualityCategory.Excellent:
                    return SkillData.Excellent;
                case QualityCategory.Masterwork:
                    return SkillData.Masterwork;
                case QualityCategory.Legendary:
                    return SkillData.Legendary;
                default:
                    throw new ArgumentException();
            }
        }

        public float GetLearnAmount()
        {
            return SkillData.baseGainedXPper1Tick * GetLearnQuality();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.stopDraw, "stopDraw", false);
            Scribe_Values.Look<bool>(ref this.initialized, "initialized", false);
        }
    }
}

