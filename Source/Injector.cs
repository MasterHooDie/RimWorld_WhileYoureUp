using HugsLib;
using HugsLib.Settings;
using System;
using Verse;

namespace WhileYoureUp
{
	public class Injector : ModBase
	{
		internal static SettingHandle<bool> rememberPreviousJob;

		internal static SettingHandle<bool> cpuUsage;

		internal static SettingHandle<bool> skipWhenBleeding;

		public override string ModIdentifier
		{
			get
			{
				return "WhileYoureUp";
			}
		}

		public override void DefsLoaded()
		{
			Injector.rememberPreviousJob = base.Settings.GetHandle<bool>("RememberPreviousJob", Translator.Translate("WhileYoureUp.RememberPreviousJob"), Translator.Translate("WhileYoureUp.RememberPreviousJobTip"), true, null, null);
			Injector.cpuUsage = base.Settings.GetHandle<bool>("CpuUsage", Translator.Translate("WhileYoureUp.CpuUsage"), Translator.Translate("WhileYoureUp.CpuUsageTip"), false, null, null);
			Injector.skipWhenBleeding = base.Settings.GetHandle<bool>("SkipWhenBleeding", Translator.Translate("WhileYoureUp.SkipWhenBleeding"), Translator.Translate("WhileYoureUp.SkipWhenBleedingTip"), false, null, null);
		}
	}
}
