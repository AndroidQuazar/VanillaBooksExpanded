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
    public class Book : ThingWithComps
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.stopDraw = false;
            if (!respawningAfterLoad)
            {
                var comp = this.TryGetComp<CompBook>();
                if (!comp.Active)
                {
                    comp.InitializeBook();
                }
            }
        }

        public bool stopDraw = false;
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (!stopDraw)
            {
                base.DrawAt(drawLoc, flip);
            }
        }

        public override void Tick()
        {
            base.Tick();
            //Log.Message(this + " - " + this.GetHashCode(), true);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false, 0))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("CannotUseNoPath"), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            else
            {
                string label = "VBE.ReadBook".Translate();
                Action action = delegate ()
                {
                    Job job = JobMaker.MakeJob(VanillaBooksExpandedDefOf.VBE_ReadBook, null, this);
                    job.count = 1;
                    myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                };
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption
                        (label, action, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn,
                        this, "ReservedBy");
            }
            yield break;
        }


        public bool CanLearnFromBook(Pawn pawn)
        {
            int skillLevel = pawn.skills.GetSkill(BookData.skillToTeach).Level;
            
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
                    return BookData.Awful;
                case QualityCategory.Poor:
                    return BookData.Poor;
                case QualityCategory.Normal:
                    return BookData.Normal;
                case QualityCategory.Good:
                    return BookData.Good;
                case QualityCategory.Excellent:
                    return BookData.Excellent;
                case QualityCategory.Masterwork:
                    return BookData.Masterwork;
                case QualityCategory.Legendary:
                    return BookData.Legendary;
                default:
                    throw new ArgumentException();
            }
        }

        public float GetLearnAmount()
        {
            return BookData.baseGainedXPper1Tick * GetLearnQuality();
        }

        public BookData BookData
        {
            get
            {
                return this.TryGetComp<CompBook>()?.Props.bookData;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.stopDraw, "stopDraw", false);
        }
    }
}

