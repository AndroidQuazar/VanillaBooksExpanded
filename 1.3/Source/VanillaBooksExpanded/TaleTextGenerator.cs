using System.Linq;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace VanillaBooksExpanded
{
	public static class TaleTextGenerator
	{
		private const float TalelessChanceWithTales = 0.2f;

		public static TaggedString GenerateTextFromTale(TextGenerationPurpose purpose, Tale tale, 
			int seed, RulePackDef extraInclude, CompBook compBook)
		{
			Rand.PushState();
			Rand.Seed = seed;
			string rootKeyword = null;
			GrammarRequest request = default(GrammarRequest);
			request.Includes.Add(extraInclude);

			switch (purpose)
			{
				case TextGenerationPurpose.ArtDescription:
					rootKeyword = compBook.Props.nameMaker.RulesImmediate
						.Where(x => x.keyword != null && x.keyword.Length > 0).RandomElement().keyword;
					//Log.Message("rootKeyword for description: " + rootKeyword);
					if (tale != null && !Rand.Chance(0.2f))
					{
						request.Includes.Add(RulePackDefOf.ArtDescriptionRoot_HasTale);
						request.IncludesBare.AddRange(tale.GetTextGenerationIncludes());
						request.Rules.AddRange(tale.GetTextGenerationRules());
					}
					else
					{
						request.Includes.Add(RulePackDefOf.ArtDescriptionRoot_Taleless);
						request.Includes.Add(RulePackDefOf.TalelessImages);
					}
					request.Includes.Add(RulePackDefOf.ArtDescriptionUtility_Global);
					break;
				case TextGenerationPurpose.ArtName:
					rootKeyword = compBook.Props.descriptionMaker.RulesImmediate
						.Where(x => x.keyword != null && x.keyword.Length > 0).RandomElement().keyword;
					//Log.Message("rootKeyword for name: " + rootKeyword);
					if (tale != null)
					{
						request.IncludesBare.AddRange(tale.GetTextGenerationIncludes());
						request.Rules.AddRange(tale.GetTextGenerationRules());
					}
					break;
			}
			string str = GrammarResolver.Resolve(rootKeyword, request, (tale != null) ? tale.def.defName : "null_tale");
			Rand.PopState();
			return str;
		}
	}
}

