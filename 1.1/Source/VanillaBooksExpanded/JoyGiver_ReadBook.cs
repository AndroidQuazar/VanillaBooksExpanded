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
            //Log.Message("JoyGiver_ReadBook TryGiveJob");
            var bookCandidates = pawn.Map.listerThings.AllThings.Where(x => x is Book && !x.IsForbidden(pawn));
            if (bookCandidates != null && bookCandidates.Count() > 0)
            {
                // skillBooks
                var skillBooks = bookCandidates.Where(b => b is SkillBook skillBook
                    && pawn.CanReserveAndReach(skillBook, PathEndMode.Touch, Danger.Deadly)
                    && !pawn.skills.GetSkill(skillBook.SkillData.skillToTeach).TotallyDisabled
                    && skillBook.CanLearnFromBook(pawn));
                if (skillBooks.Count() > 0)
                {
                    var book = skillBooks.MaxBy(x => x.TryGetComp<CompQuality>().Quality);
                    //Log.Message(pawn + " got " + book + " with quality " + book?.TryGetComp<CompQuality>().Quality);
                    Job job = JobMaker.MakeJob(def.jobDef, null, book);
                    job.count = 1;
                    return job;
                }
                if (pawn.needs.joy.CurLevel < 0.6)
                {
                    // newspapers
                    var newspapers = bookCandidates.Where(b => b is Newspaper newspaper && pawn.CanReserveAndReach(newspaper, PathEndMode.Touch, Danger.Deadly) 
                        && newspaper.IsRelevant);
                    if (newspapers.Count() > 0)
                    {
                        var book = newspapers.RandomElement();
                        //Log.Message(pawn + " got " + book);
                        Job job = JobMaker.MakeJob(def.jobDef, null, book);
                        job.count = 1;
                        return job;
                    }
                }
            }
            return null;
        }
    }
}