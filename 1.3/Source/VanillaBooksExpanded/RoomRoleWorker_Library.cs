using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaBooksExpanded
{
	public class RoomRoleWorker_Library : RoomRoleWorker
	{
		public override float GetScore(Room room)
		{
			int num = 0;
			foreach (var t in room.ContainedAndAdjacentThings)
            {
				if (t is Book && t.Position.GetFirstBuilding(t.Map) is Building_Storage)
				{
					num++;
				}
			}
			return 3f * (float)num;
		}
	}
}
