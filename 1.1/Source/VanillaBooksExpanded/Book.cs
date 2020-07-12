using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VanillaBooksExpanded
{
    public class Book : ThingWithComps
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                var comp = this.TryGetComp<CompBook>();
                comp.InitializeBook();
            }
        }
    }
}

