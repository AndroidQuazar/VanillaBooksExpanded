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
		private MoteDualAttachedForBook interactMote;

		public SubEffecter_BookSymbol(SubEffecterDef def, Effecter parent) : base(def, parent) { }

		public override void SubEffectTick(TargetInfo A, TargetInfo B)
		{
			if (interactMote == null)
			{
				Pawn pawn = A.Thing as Pawn;
				new LocalTargetInfo();
				interactMote = (MoteDualAttachedForBook)ThingMaker.MakeThing(def.moteDef);
				interactMote.Attach(A, B);
				interactMote.exactPosition = (pawn.Position + pawn.Rotation.FacingCell).ToVector3Shifted();
				GenSpawn.Spawn(interactMote, pawn.Position + pawn.Rotation.FacingCell, pawn.Map);
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
