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
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                this.expireTime = Find.TickManager.TicksAbs + Rand.RangeInclusive(60000, 180000);
                var comp = this.TryGetComp<CompBook>();
                if (!comp.Active)
                {
                    comp.InitializeBook();
                }
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
            else if (GenDate.DaysPassedAt(this.expireTime) >= GenDate.DaysPassedAt(Find.TickManager.TicksAbs))
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
            else if (GenDate.DaysPassedAt(this.expireTime) < GenDate.DaysPassedAt(Find.TickManager.TicksAbs))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("VBE.CantReadNewsPaperExpired"), null,
                MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            yield break;
        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString() + "\n");
            Vector2 vector = Find.WorldGrid.LongLatOf(this.Map.Tile);
            Log.Message(this.expireTime + " - " + Find.TickManager.TicksAbs);
            Log.Message(GenDate.DaysPassedAt(this.expireTime) + " expire day");
            Log.Message(GenDate.DaysPassedAt(Find.TickManager.TicksAbs) + " today day");

            stringBuilder.Append("VBE.NewspaperRelevantUntil".Translate() + GenDate.DateReadoutStringAt((long)this.expireTime, vector));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.stopDraw, "stopDraw", false);
            Scribe_Values.Look<int>(ref this.expireTime, "expireTime", 0);
        }
    }
}

