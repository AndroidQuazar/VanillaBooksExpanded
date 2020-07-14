using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaBooksExpanded
{
	public class ITab_Book : ITab
	{
		private static string cachedImageDescription;

		private static CompBook cachedImageSource;

		private static TaleBookReference cachedTaleRef;

		private static readonly Vector2 WinSize = new Vector2(400f, 300f);

		private CompBook SelectedCompBook
		{
			get
			{
				Thing thing = Find.Selector.SingleSelectedThing;
				MinifiedThing minifiedThing = thing as MinifiedThing;
				if (minifiedThing != null)
				{
					thing = minifiedThing.InnerThing;
				}
				return thing?.TryGetComp<CompBook>();
			}
		}

		public override bool IsVisible
		{
			get
			{
				if (SelectedCompBook != null)
				{
					return SelectedCompBook.Active;
				}
				return false;
			}
		}
		public ITab_Book()
		{
			size = WinSize;
			labelKey = "VBE.Book";
			tutorTag = "Art";
		}

		protected override void FillTab()
		{
			Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
			Text.Font = GameFont.Medium;
			Widgets.Label(rect, SelectedCompBook.Title);
			if (cachedImageSource != SelectedCompBook || cachedTaleRef != SelectedCompBook.TaleRef)
			{
				cachedImageDescription = SelectedCompBook.GenerateImageDescription();
				cachedImageSource = SelectedCompBook;
				cachedTaleRef = SelectedCompBook.TaleRef;
			}
			Rect rect2 = rect;
			rect2.yMin += 35f;
			Text.Font = GameFont.Small;
			Widgets.Label(rect2, cachedImageDescription);
		}
	}
}

