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
            var bookCandidates = pawn.Map.listerThings.AllThings.Where(x => x is Book && !x.IsForbidden(pawn));
            if (bookCandidates != null && bookCandidates.Any())
            {
                // skillBooks
                var skillBooks = bookCandidates.Where(b => b is SkillBook skillBook
                    && !pawn.skills.GetSkill(skillBook.SkillData.skillToTeach).TotallyDisabled
                    && skillBook.CanLearnFromBook(pawn)
                    && pawn.CanReserveAndReach(skillBook, PathEndMode.Touch, Danger.Deadly));
                if (skillBooks.Any())
                {
                    var book = skillBooks.MaxBy(x => x.TryGetComp<CompQuality>().Quality);
                    Job job = JobMaker.MakeJob(def.jobDef, null, book);
                    job.count = 1;
                    return job;
                }
                if (pawn.needs.joy.CurLevel < 0.6)
                {
                    // newspapers
                    var newspapers = bookCandidates.Where(b => b is Newspaper newspaper && newspaper.IsRelevant && pawn.CanReserveAndReach(newspaper, PathEndMode.Touch, Danger.Deadly));
                    if (newspapers.Any())
                    {
                        var book = newspapers.RandomElement();
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