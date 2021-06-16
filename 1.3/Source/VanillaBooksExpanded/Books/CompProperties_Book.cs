using RimWorld;
using Verse;

namespace VanillaBooksExpanded
{
	public class CompProperties_Book : CompProperties
	{
		public RulePackDef nameMaker;

		public RulePackDef descriptionMaker;

		public QualityCategory minQualityForArtistic;

		public EffecterDef readingEffecter;

		public float joyAmountPerTick;

		public int readingTicks;

		public bool destroyAfterReading;

		public bool saveReadingProgress;

		public bool mustBeFullGrave;

		public bool canBeEnjoyedAsArt;

		public string rulesStringName;

		public string rulesStringDescription;

		public SkillData skillData;
		public CompProperties_Book()
		{
			compClass = typeof(CompBook);
		}
	}
}

