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
        public bool stopDraw = false;

        public int curReadingTicks;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.stopDraw = false;
        }

        public CompProperties_Book Props => this.TryGetComp<CompBook>()?.Props;

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (!stopDraw)
            {
                base.DrawAt(drawLoc, flip);
            }
        }

        public SkillData SkillData
        {
            get
            {
                return this.TryGetComp<CompBook>()?.Props.skillData;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.stopDraw, "stopDraw", false);
            Scribe_Values.Look<int>(ref this.curReadingTicks, "curReadingTicks", 0);

        }
    }
}

