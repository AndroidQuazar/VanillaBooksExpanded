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
        private int curReadingTicks = 0;
        private Book book => job.GetTarget(TargetIndex.B).Thing as Book;
        private int totalReadingTicks => book.Props.readingTicks > 0 ? book.Props.readingTicks : 1000;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(book, job, errorOnFailed: errorOnFailed);
        }

        public static void TryGainLibraryRoomThought(Pawn pawn)
        {
            Room room = pawn.GetRoom();
            if (room != null && room.Role == VBE_DefOf.VBE_Library)
            {
                int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
                if (pawn.needs.mood != null && VBE_DefOf.VBE_JoyActivityInImpressiveLibraryRoom.stages[scoreStageIndex] != null)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(VBE_DefOf.VBE_JoyActivityInImpressiveLibraryRoom, scoreStageIndex));
                }
            }
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
                if (book.Props.saveReadingProgress)
                {
                    curReadingTicks = book.curReadingTicks;
                }
            });

            toil.tickAction = () =>
            {
                Pawn actor = pawn;
                if (TargetC.HasThing)
                {
                    actor.Rotation = TargetC.Thing.Rotation;
                }
                if (book is SkillBook skillBook)
                {
                    if (skillBook.CanLearnFromBook(pawn))
                    {
                        var compBook = skillBook.TryGetComp<CompBook>();
                        if (compBook != null && compBook.Props.skillData.skillToTeach != null)
                        {
                            var learnValue = skillBook.GetLearnAmount();
                            //Log.Message(pawn + " learn " + compBook.Props.skillData.skillToTeach + " ("
                            //    + learnValue + ") from " + book + " - " + book.TryGetComp<CompQuality>().Quality, true);
                            actor.skills.Learn(compBook.Props.skillData.skillToTeach, learnValue);
                        }
                    }
                    else
                    {
                        if (pawn.carryTracker.CarriedThing is Book carriedBook)
                        {
                            book.stopDraw = false;
                        }
                        ReadyForNextToil();
                    }
                }
                if (book.Props.joyAmountPerTick > 0)
                {
                    pawn.needs.joy.GainJoy(book.Props.joyAmountPerTick, VBE_DefOf.VBE_Reading);
                }
                curReadingTicks++;
                book.curReadingTicks = curReadingTicks;
                if (curReadingTicks > totalReadingTicks)
                {
                    if (pawn.carryTracker.CarriedThing is Book carriedBook)
                    {
                        book.stopDraw = false;
                    }
                    ReadyForNextToil();
                }
            };
            toil.handlingFacing = true;
            toil.WithEffect(() => book.Props.readingEffecter, () => TargetA);
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            ToilEffects.WithProgressBar(toil, TargetIndex.B, () => (float)this.curReadingTicks / (float)this.totalReadingTicks, false, -0.5f);
            yield return toil;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    if (pawn.carryTracker.CarriedThing is Book carriedBook)
                    {
                        book.stopDraw = false;
                    }

                    if (book is TechBlueprint techBlueprint)
                    {
                        techBlueprint.UnlockResearch(pawn);
                    }
                    else if (book is MapItem mapItem)
                    {
                        mapItem.UnlockQuest(pawn);
                    }
                    if (pawn.GetRoom()?.Role == VBE_DefOf.VBE_Library)
                    {
                        TryGainLibraryRoomThought(pawn);
                    }
                    else
                    {
                        JoyUtility.TryGainRecRoomThought(pawn);
                    }

                    if (book.Props.destroyAfterReading)
                    {
                        book.Destroy(DestroyMode.Vanish);
                    }
                    else
                    {
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
                    }
                    base.EndJobWith(JobCondition.Succeeded);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        public List<RoomRoleDef> allowedRooms = new List<RoomRoleDef>
        {
            RoomRoleDefOf.Bedroom,
            RoomRoleDefOf.RecRoom,
            RoomRoleDefOf.DiningRoom,
            VBE_DefOf.VBE_Library
        };
        private Toil FindSeatsForReading(Pawn p)
        {
            try
            {
                var chairCandidates = p.Map?.listerThings?.AllThings?.Where(x => x.def?.building?.isSittable ?? false);
                var bestChairs = new Dictionary<float, List<Thing>>();
                foreach (var chair in chairCandidates)
                {
                    var score = chair.def?.GetStatValueAbstract(StatDefOf.Comfort);
                    if (score.HasValue && IntVec3Utility.DistanceTo(book.Position, chair.Position) < 60 && !chair.IsForbidden(p))
                    {
                        var role = chair?.GetRoom()?.Role;
                        if (role != null && allowedRooms.Contains(role) && !(chair is Building_Throne))
                        {
                            var building = (chair.Position + chair.Rotation.FacingCell).GetFirstBuilding(p.Map);
                            if (building != null && building.def.IsWorkTable) continue;

                            if (bestChairs.ContainsKey(score.Value))
                            {
                                bestChairs[score.Value].Add(chair);
                            }
                            else
                            {
                                bestChairs[score.Value] = new List<Thing> { chair };
                            }
                        }
                    }
                }

                while (bestChairs.Count > 0)
                {
                    var key = bestChairs.MaxBy(x => x.Key).Key;
                    foreach (var thing in bestChairs[key].OrderBy(y => IntVec3Utility.DistanceTo(book.Position, y.Position)))
                    {
                        if (p.CanReserveAndReach(thing, PathEndMode.OnCell, Danger.Deadly) && !thing.IsForbidden(p))
                        {
                            p.CurJob.targetC = thing;
                            p.Reserve(thing, p.CurJob);
                            var toil = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell);
                            return toil;
                        }
                    }
                    bestChairs.Remove(key);
                }

                foreach (var thing in chairCandidates.OrderByDescending(y => y.def?.GetStatValueAbstract(StatDefOf.Comfort)))
                {
                    if (p.CanReserveAndReach(thing, PathEndMode.OnCell, Danger.Deadly) && !thing.IsForbidden(p))
                    {
                        p.CurJob.targetC = thing;
                        p.Reserve(thing, p.CurJob);
                        var toil = Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell);
                        return toil;
                    }
                }
            }
            catch { };
            return new Toil();
        }
    }
}