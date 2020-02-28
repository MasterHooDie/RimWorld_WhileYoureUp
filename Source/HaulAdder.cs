using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace WhileYoureUp
{
	[HarmonyPatch(typeof(Pawn_JobTracker), "DetermineNextJob")]
	public static class HaulAdder
	{
		private static Dictionary<Pawn, Job> overriddenJobs = new Dictionary<Pawn, Job>();

		private static LocalTargetInfo GetFirstTarget(Job job, TargetIndex index)
		{
			if (!GenList.NullOrEmpty<LocalTargetInfo>(job.GetTargetQueue(index)))
			{
				return job.GetTargetQueue(index)[0];
			}
			return job.GetTarget(index);
		}

		[HarmonyPostfix]
		public static void MyDetermineNextJob(Pawn_JobTracker __instance, ref ThinkResult __result)
		{
			Pawn value = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
			if (Injector.skipWhenBleeding.Value && value.health.hediffSet.BleedRateTotal > 0f)
			{
				return;
			}
			if (!value.IsColonistPlayerControlled)
			{
				return;
			}
			if (__result.Job == null)
			{
				return;
			}
			Job job = __result.Job;
			Job job2 = null;
			if (Injector.rememberPreviousJob.Value)
			{
				Dictionary<Pawn, Job> obj = HaulAdder.overriddenJobs;
				lock (obj)
				{
					if (HaulAdder.overriddenJobs.ContainsKey(value))
					{
						Job job3 = HaulAdder.overriddenJobs[value];
						if (job3 != null && job3 == job)
						{
							HaulAdder.overriddenJobs.Remove(value);
							return;
						}
					}
				}
			}
			if (job.def == JobDefOf.DoBill)
			{
				LocalTargetInfo firstTarget = HaulAdder.GetFirstTarget(job, TargetIndex.B);
				if (firstTarget != null)
				{
					job2 = Utils.MaybeHaulOtherStuffFirst(value, firstTarget);
				}
				else
				{
					job2 = Utils.MaybeHaulOtherStuffFirst(value, HaulAdder.GetFirstTarget(job, TargetIndex.A));
				}
			}
			else if (job.def == JobDefOf.Clean || job.def == JobDefOf.ClearSnow || job.def == JobDefOf.Flick || job.def == JobDefOf.Research || job.def == JobDefOf.Wear || job.def == JobDefOf.LayDown || job.def == JobDefOf.RemoveFloor || job.def == JobDefOf.SmoothFloor || job.def == JobDefOf.Mine || job.def == JobDefOf.CutPlant || job.def == JobDefOf.Sow || job.def == JobDefOf.HaulToCell || job.def == JobDefOf.HaulToContainer || job.def == JobDefOf.Slaughter || job.def == JobDefOf.Milk || job.def == JobDefOf.Shear || job.def == JobDefOf.Tame || job.def == JobDefOf.Train || job.def == JobDefOf.Repair || job.def == JobDefOf.Deconstruct || job.def == JobDefOf.Uninstall || job.def == JobDefOf.PlaceNoCostFrame || job.def == JobDefOf.FinishFrame || job.def == JobDefOf.OperateDeepDrill || job.def == JobDefOf.Open || job.def == JobDefOf.PrisonerAttemptRecruit || job.def == JobDefOf.Equip || job.def == JobDefOf.Ingest || job.def == JobDefOf.PlaceNoCostFrame || job.def == JobDefOf.LayDown)
			{
				job2 = Utils.MaybeHaulOtherStuffFirst(value, HaulAdder.GetFirstTarget(job, TargetIndex.A));
			}
			else if (job.def == JobDefOf.Harvest || job.def == JobDefOf.HarvestDesignated)
			{
				if (job2 == null)
				{
					job2 = Utils.MaybeHaulOtherStuffFirst(value, HaulAdder.GetFirstTarget(job, TargetIndex.A));
				}
			}
			else if (job.def == JobDefOf.Ingest)
			{
				LocalTargetInfo firstTarget2 = HaulAdder.GetFirstTarget(job, TargetIndex.A);
				if (value.inventory == null || !value.inventory.Contains(firstTarget2.Thing))
				{
					job2 = Utils.MaybeHaulOtherStuffFirst(value, firstTarget2);
				}
			}
			else if (job.def == JobDefOf.BuildRoof || job.def == JobDefOf.RemoveRoof || job.def == JobDefOf.Refuel || job.def == JobDefOf.FillFermentingBarrel || job.def == JobDefOf.FixBrokenDownBuilding)
			{
				job2 = Utils.MaybeHaulOtherStuffFirst(value, HaulAdder.GetFirstTarget(job, TargetIndex.B));
			}
			if (job2 != null)
			{
				if (Injector.rememberPreviousJob.Value)
				{
					Dictionary<Pawn, Job> obj = HaulAdder.overriddenJobs;
					lock (obj)
					{
						HaulAdder.overriddenJobs[value] = job;
					}
					__instance.jobQueue.EnqueueFirst(__result.Job, null);
				}
				__result = new ThinkResult(job2, __result.SourceNode, __result.Tag, false);
			}
		}
	}
}
