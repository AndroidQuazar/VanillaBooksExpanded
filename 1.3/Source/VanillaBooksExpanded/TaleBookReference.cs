using RimWorld;
using Verse;

namespace VanillaBooksExpanded
{
	public class TaleBookReference : IExposable
	{
		public Tale tale;

		public int seed;

		public static TaleBookReference Taleless => new TaleBookReference(null);

		public TaleBookReference()
		{
		}

		public TaleBookReference(Tale tale)
		{
			this.tale = tale;
			seed = Rand.Range(0, int.MaxValue);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref seed, "seed", 0);
			Scribe_References.Look(ref tale, "tale");
		}

		public void ReferenceDestroyed()
		{
			if (tale != null)
			{
				tale.Notify_ReferenceDestroyed();
				tale = null;
			}
		}

		public TaggedString GenerateText(TextGenerationPurpose purpose, RulePackDef extraInclude, CompBook compBook)
		{
			return TaleTextGenerator.GenerateTextFromTale(purpose, tale, seed, extraInclude, compBook);
		}

		public override string ToString()
		{
			return "TaleReference(tale=" + ((tale == null) ? "null" : tale.ToString()) + ", seed=" + seed + ")";
		}
	}
}

