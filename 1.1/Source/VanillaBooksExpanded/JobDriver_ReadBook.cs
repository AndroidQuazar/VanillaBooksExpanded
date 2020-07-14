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
                var compBook = book.TryGetComp<CompBook>();
                if (compBook != null && compBook.Props.bookData.skillToTeach != null)
                {
                    var learnValue = book.GetLearnAmount();
                    Log.Message(pawn + " learn " + compBook.Props.bookData.skillToTeach + " (" 
                        + learnValue + ") from " + book + " - " + book.TryGetComp<CompQuality>().Quality);
                    actor.skills.Learn(compBook.Props.bookData.skillToTeach, learnValue);
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
            toil.WithEffect(() => book.BookData.readingEffecter, () => TargetA);
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }

        private static Toil FindSeatsForReading(Pawn p)
        {
            foreach (var thing in p.Map.listerThings.AllThings.Where(x => x.def?.building?.isSittable ?? false)
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
            p.CurJob.targetB = null;
            return new Toil();
        }
    }
}