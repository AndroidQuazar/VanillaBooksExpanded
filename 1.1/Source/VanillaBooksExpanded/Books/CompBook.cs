using RimWorld;
using Verse;

namespace VanillaBooksExpanded
{
	public class CompBook : ThingComp
	{
		private TaggedString authorNameInt = null;

		private TaggedString titleInt = null;

		private TaleBookReference taleRef;

		public TaggedString AuthorName
		{
			get
			{
				if (authorNameInt.NullOrEmpty())
				{
					return "UnknownLower".Translate().CapitalizeFirst();
				}
				return authorNameInt.Resolve();
			}
		}

		public string Title
		{
			get
			{
				if (titleInt.NullOrEmpty())
				{
					Log.Error("CompArt got title but it wasn't configured.");
					titleInt = "Error";
				}
				return titleInt;
			}
		}

		public TaleBookReference TaleRef => taleRef;

		public bool Active => taleRef != null;

		public CompProperties_Book Props => (CompProperties_Book)props;

		public void InitializeBook()
		{
			//Log.Message("Initialize book");
			if (taleRef != null)
			{
				taleRef.ReferenceDestroyed();
				taleRef = null;
			}
			taleRef = TaleBookReference.Taleless;
			//Log.Message("Generating title");
			titleInt = GenerateTitle();
			//Log.Message("Title: " + titleInt);
		}

		public void JustCreatedBy(Pawn pawn)
		{
			authorNameInt = pawn.NameFullColored;
		}

		public void Clear()
		{
			authorNameInt = null;
			titleInt = null;
			if (taleRef != null)
			{
				taleRef.ReferenceDestroyed();
				taleRef = null;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref authorNameInt, "authorName", null);
			Scribe_Values.Look(ref titleInt, "title", null);
			Scribe_Deep.Look(ref taleRef, "taleRef");
		}

		public override string CompInspectStringExtra()
		{
			if (!Active)
			{
				return null;
			}
			return (string)("Author".Translate() + ": " + AuthorName) + ("\n" + "Title".Translate() + ": " + Title);
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			if (taleRef != null)
			{
				taleRef.ReferenceDestroyed();
				taleRef = null;
			}
		}

		public override string GetDescriptionPart()
		{
			if (!Active)
			{
				return null;
			}
			return string.Concat(string.Concat("" + Title, "\n\n") + GenerateImageDescription(), "\n\n") + ("Author".Translate() + ": " + AuthorName);
		}

		public override bool AllowStackWith(Thing other)
		{
			if (Active)
			{
				return false;
			}
			return true;
		}

		public TaggedString GenerateImageDescription()
		{
			return taleRef.GenerateText(TextGenerationPurpose.ArtDescription, Props.descriptionMaker, this);
		}

		private string GenerateTitle()
		{
			
			return GenText.CapitalizeAsTitle(taleRef.GenerateText(TextGenerationPurpose.ArtName, Props.nameMaker, this));
		}
	}
}

