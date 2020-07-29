using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaBooksExpanded
{
    public class JobDriver_ReadBook : JobDriver
    {
        private float totalReadingTicks => 1000;
        private float curReadingTicks = 0;
        private Book book => job.GetTarget(TargetIndex.B).Thing as Book;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(book, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            pawn.CurJob.count = 1;
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return FindSeatsForReading(pawn).FailOnForbidden(TargetIndex.C);

            var toil = new Toil();
            toil.AddPreInitAction(() =>
            {
                pawn.CurJob.targetA = pawn;
                if (pawn.carryTracker.CarriedThing is Book carriedBook)
                {
                    book.stopDraw = true;
                }
                pawn.GainComfortFromCellIfPossible();
                JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob);
            });

            toil.tickAction = () =>
            {
                Pawn actor = pawn;
                if (book is SkillBook skillBook)
                {
                    var compBook = skillBook.TryGetComp<CompBook>();
                    if (compBook != null && compBook.Props.skillData.skillToTeach != null)
                    {
                        var learnValue = skillBook.GetLearnAmount();
                        Log.Message(pawn + " learn " + compBook.Props.skillData.skillToTeach + " ("
                            + learnValue + ") from " + book + " - " + book.TryGetComp<CompQuality>().Quality, true);
                        actor.skills.Learn(compBook.Props.skillData.skillToTeach, learnValue);
                    }
                }
                if (book.Props.joyAmountPerTick > 0)
                {
                    pawn.needs.joy.GainJoy(book.Props.joyAmountPerTick, VBE_DefOf.VBE_Reading);
                }
                curReadingTicks++;
                if (curReadingTicks > totalReadingTicks)
                {
                    if (pawn.carryTracker.CarriedThing is Book carriedBook)
                    {
                        book.stopDraw = false;
                    }
                    ReadyForNextToil();
                }
            };

            toil.AddFinishAction(() =>
            {
                if (pawn.carryTracker.CarriedThing is Book carriedBook)
                {
                    book.stopDraw = false;
                }
                JoyUtility.TryGainRecRoomThought(pawn);
            });
            toil.WithEffect(() => book.Props.readingEffecter, () => TargetA);
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Log.Message("Hauling", true);
                    Thing thing = book;
                    StoragePriority storagePriority = StoreUtility.CurrentStoragePriorityOf(thing);
                    IntVec3 intVec;
                    if (StoreUtility.TryFindBestBetterStoreCellFor(thing, this.pawn, base.Map, storagePriority, this.pawn.Faction, out intVec, true))
                    {
                        this.job.SetTarget(TargetIndex.C, intVec);
                        this.job.SetTarget(TargetIndex.B, thing);
                        this.job.count = thing.stackCount;
                        return;
                    }
                    base.EndJobWith(JobCondition.Incompletable);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        private static Toil FindSeatsForReading(Pawn p)
        {
            foreach (var thing in p.Map?.listerThings?.AllThings?
                    .Where(x => x.def?.building?.isSittable ?? false)?
                    .OrderByDescending(y => y.def.GetStatValueAbstract(StatDefOf.Comfort)).ToList())
            {
                if (p.CanReserve(thing))
                {
                    p.CurJob.targetC = thing;
                    p.Reserve(thing, p.CurJob);
                    var toil = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell);
                    return toil;
                }
            }
            return new Toil();
        }
    }
}