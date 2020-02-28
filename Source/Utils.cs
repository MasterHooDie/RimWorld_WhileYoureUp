using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace WhileYoureUp
{
	public class Utils
	{
		private static float kMaxExtraDistanceFactor = 1.5f;

		private static float kTooShortBaseDistanceToBother = 3f;

		private static float kDistanceToObjectShortCircuitFactor = 0.25f;

		private static float kDistanceToObjectExtra = 2f;

		private static int MyDistance(IntVec3 a, LocalTargetInfo b, Map m, TraverseParms t)
		{
			if (a.Equals(b.Cell))
			{
				return 0;
			}
			/*  PawnPath pawnPath = m.pathFinder.FindPath(a, b, t, 1); */
			PawnPath pawnPath = m.pathFinder.FindPath(a, b, t, PathEndMode.OnCell);
			if (pawnPath == PawnPath.NotFound)
			{
				return -1;
			}
			int nodesLeftCount = pawnPath.NodesLeftCount;
			pawnPath.Dispose();
			return nodesLeftCount;
		}

		private static float QuickDistance(IntVec3 a, IntVec3 b)
		{
			float arg_1D_0 = (float)(a.x - b.x);
			float num = (float)(a.z - b.z);
			return arg_1D_0 * arg_1D_0 + num * num;
		}

		private static Thing SomeThing(IEnumerable<Thing> searchSet, Predicate<Thing> validator = null)
		{
			if (searchSet == null)
			{
				return null;
			}
			foreach (Thing current in searchSet)
			{
				if (validator(current))
				{
					return current;
				}
			}
			return null;
		}

		public static Job MaybeHaulOtherStuffFirst(Pawn pawn, LocalTargetInfo end)
		{
			if (pawn.WorkTagIsDisabled(WorkTags.ManualDumb) || pawn.WorkTagIsDisabled(WorkTags.Hauling))
			{
				return null;
			}
			TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Deadly, 0, false);
			float baseDistance = (float)Utils.MyDistance(pawn.Position, end, pawn.Map, traverseParms);
			if (baseDistance < (Injector.cpuUsage.Value ? Utils.kTooShortBaseDistanceToBother : (Utils.kTooShortBaseDistanceToBother * 3f)))
			{
				return null;
			}
			float toThingMax = Injector.cpuUsage.Value ? Utils.kDistanceToObjectExtra : (baseDistance * Utils.kDistanceToObjectShortCircuitFactor + Utils.kDistanceToObjectExtra);
			float totalDistanceMax = baseDistance * Utils.kMaxExtraDistanceFactor;
			Predicate<Thing> validator = delegate(Thing t)
			{
				if (ForbidUtility.IsForbidden(t, pawn) || !HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, false) || pawn.carryTracker.MaxStackSpaceEver(t.def) <= 0)
				{
					return false;
				}
				if (StoreUtility.IsInValidStorage(t))
				{
					return false;
				}
				if (Utils.QuickDistance(pawn.Position, t.Position) >= toThingMax * toThingMax)
				{
					return false;
				}
				int num = Utils.MyDistance(pawn.Position, t, pawn.Map, traverseParms);
				if (num < 0 || (float)num > toThingMax)
				{
					return false;
				}
				IntVec3 intVec;
				if (!StoreUtility.TryFindBestBetterStoreCellFor(t, pawn, pawn.Map, StoreUtility.StoragePriorityAtFor(t.Position, t), pawn.Faction, out intVec, true))
				{
					return false;
				}
				int num2 = Utils.MyDistance(t.Position, intVec, pawn.Map, traverseParms);
				if (num2 < 0 || (float)(num + num2) > totalDistanceMax)
				{
					return false;
				}
				int num3 = Utils.MyDistance(intVec, end, pawn.Map, traverseParms);
				return num3 >= 0 && (float)num3 < baseDistance && (float)(num + num2 + num3) < totalDistanceMax;
			};
			Thing thing = Utils.SomeThing(pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling(), validator);
			if (thing != null && HaulAIUtility.PawnCanAutomaticallyHaul(pawn, thing, false))
			{
				return HaulAIUtility.HaulToStorageJob(pawn, thing);
			}
			return null;
		}

		public static string GetStringOrNull(object o)
		{
			if (o == null)
			{
				return "null";
			}
			return o.ToString();
		}
	}
}
