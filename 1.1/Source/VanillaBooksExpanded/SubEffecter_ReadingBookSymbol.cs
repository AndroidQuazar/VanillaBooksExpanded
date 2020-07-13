using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace VanillaBooksExpanded
{
    public class SubEffecter_BookSymbol : SubEffecter
    {
		private Mote interactMote;

		public SubEffecter_BookSymbol(SubEffecterDef def, Effecter parent) : base(def, parent) { }

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			if (interactMote == null)
			{
				Log.Message("Spawn mote at " + B.Cell);
				interactMote = (Mote)ThingMaker.MakeThing(def.moteDef);
				interactMote.exactPosition = B.Cell.ToVector3Shifted();
				GenSpawn.Spawn(interactMote, B.Cell, A.Map);
			}
		}

		public override void SubCleanup()
		{
			if (interactMote != null && !interactMote.Destroyed)
			{
				interactMote.Destroy();
			}
		}
	}
}
