using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaBooksExpanded
{
    public class JoyGiver_ReadBook : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            Log.Message("JoyGiver_ReadBook TryGiveJob");
            var bookCandidates = pawn.Map.listerThings.AllThings.Where(x => x is Book bookCandidate
                    && !pawn.skills.GetSkill(bookCandidate.BookData.skillToTeach).TotallyDisabled
                    && bookCandidate.CanLearnFromBook(pawn)).ToList();

            if (bookCandidates != null && bookCandidates.Count > 0)
            {
                var book = bookCandidates.MaxBy(x => x.TryGetComp<CompQuality>().Quality);
                Log.Message(pawn + " got " + book + " with quality " + book?.TryGetComp<CompQuality>().Quality);
                Job job = JobMaker.MakeJob(def.jobDef, book);
                job.count = 1;
                return job;
            }
            return null;
        }
    }
}