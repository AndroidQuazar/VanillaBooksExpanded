using RimWorld;
using Verse;

namespace VanillaBooksExpanded
{
	public class CompProperties_Book : CompProperties
	{
		public RulePackDef nameMaker;

		public RulePackDef descriptionMaker;

		public QualityCategory minQualityForArtistic;

		public bool mustBeFullGrave;

		public bool canBeEnjoyedAsArt;

		public string rulesStringName;

		public string rulesStringDescription;

		public BookData bookData;
		public CompProperties_Book()
		{
			compClass = typeof(CompBook);
		}
	}
}

