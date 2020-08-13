using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaBooksExpanded
{
    public class Newspaper : Book
    {
        public int expireTime = 0;
        public int expireTimeAbs = 0;

        public bool initialized = false;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && !this.initialized)
            {
                var randValue = Rand.RangeInclusive(60000, 180000);
                randValue = Rand.RangeInclusive(60, 180);

                this.expireTime = Find.TickManager.TicksGame + randValue;
                this.expireTimeAbs = Find.TickManager.TicksAbs + randValue;

                //Log.Message(this + " is created");
                var comp = this.TryGetComp<CompBook>();
                if (!comp.Active)
                {
                    comp.InitializeBook();
                }
                this.initialized = true;
            }
        }
        public bool IsRelevant => GenDate.DaysPassedAt(this.expireTime) >= GenDate.DaysPassed;
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false, 0))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("CannotUseNoPath"), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            else if (IsRelevant)
            {
                string label = "VBE.ReadNewsPaper".Translate();
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
            else if (!IsRelevant)
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("VBE.CantReadNewsPaperExpired"), null,
                MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            yield break;
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                if (!IsRelevant && this.Map != null)
                {
                    FilthMaker.TryMakeFilth(this.Position, this.Map, VBE_DefOf.VBE_Filth_Newspaper);
                    this.Destroy(DestroyMode.Vanish);
                }
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString() + "\n");
            Vector2 vector = Find.WorldGrid.LongLatOf(this.Map.Tile);
            stringBuilder.Append("VBE.NewspaperRelevantUntil".Translate() + GenDate.DateReadoutStringAt((long)this.expireTimeAbs, vector));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.initialized, "initialized", false);
            Scribe_Values.Look<bool>(ref this.stopDraw, "stopDraw", false);
            Scribe_Values.Look<int>(ref this.expireTime, "expireTime", 0);
            Scribe_Values.Look<int>(ref this.expireTimeAbs, "expireTimeAbs", 0);
        }
    }
}

