using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using GlobalEnums;
using Mono.Posix;
using UnityEngine;

namespace GunZ;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Hollow Knight Silksong")]
public class GunZ : BaseUnityPlugin
{
	internal static GunZ Instance;
	internal static ConfigEntry<bool> PluginEnabled;

	static float originalDashCooldown;
	static float originalWallclingCooldown;
	static bool originalsStored = false;

	void Awake()
	{
		Instance = this;
		PluginEnabled = Config.Bind("General", "Enabled", true);
		PluginEnabled.SettingChanged += OnPluginEnabledChanged;

		Harmony harmony = new(PluginInfo.PLUGIN_GUID);
		harmony.PatchAll();

		Logger.LogInfo("Plugin loaded and initialized.");
	}

	void OnPluginEnabledChanged(object sender, System.EventArgs e)
	{
		var heroController = HeroController.instance;

		if (heroController == null)
			return;

		if (PluginEnabled.Value)
			ApplyModifications(heroController);
	}

	static void ApplyModifications(HeroController hero)
	{
		if (!originalsStored)
		{
			originalDashCooldown = hero.DASH_COOLDOWN;
			originalWallclingCooldown = hero.WALLCLING_COOLDOWN;
			originalsStored = true;
		}

		hero.DASH_COOLDOWN = 0f;
		hero.WALLCLING_COOLDOWN = 0f;
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
	static class HeroController_Start_Patch
	{
		[HarmonyPostfix]
		static void Postfix(HeroController __instance)
		{
			if (!PluginEnabled.Value) return;
			ApplyModifications(__instance);
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Update))]
	static class HeroController_Update_Patch
	{
		[HarmonyPostfix]
		static void Postfix(HeroController __instance)
		{
			if (!PluginEnabled.Value) return;
			__instance.acceptingInput = true;
			__instance.controlReqlinquished = false;
		}
	}

	[HarmonyPatch]
	static class HeroController_Skips_Patch
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanAttack))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanAttackAction))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanBackDash))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanBind))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanCast))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanDoFSMCancelMove))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanDoFsmMove))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanDoSpecial))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanDownAttack))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanInput))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanNailCharge))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanPlayNeedolin))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanSprint))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanStartWithThrowTool))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanSuperJump))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanTakeControl))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanTryHarpoonDash))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.DashCooldownReady))]
		static bool TruePrefix(HeroController __instance, ref bool __result)
		{
			if (!PluginEnabled.Value) return true;
			__result = true;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.IsAttackLocked))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.IsDashLocked))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.IsInputBlocked))]
		static bool FalsePrefix(HeroController __instance, ref bool __result)
		{
			if (!PluginEnabled.Value) return true;
			__result = false;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.DoSprintSkid))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.RelinquishControl))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.PreventShuttlecock))]
		[HarmonyPatch(typeof(HeroController), nameof(HeroController.StartHarpoonDashCooldown))]
		static bool NoResultPrefix(HeroController __instance) => !PluginEnabled.Value;
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.DidUseAttackTool))]
	private static class HeroController_DidUseAttackTool_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance, ref ToolItemsData.Data toolData)
		{
			if (!PluginEnabled.Value) return;
			Instance.Logger.LogInfo($"DidUseAttackTool called with toolData: {toolData.AmountLeft}");
		}
	}
}

// ToolItem.UsageOptions.ThrowCooldown = 0f;

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetState))]
// static class HeroController_SetState_Patch
// {
// 	[HarmonyPrefix]
// 	static void Prefix(HeroController __instance, ref ActorStates newState)
// 	{
// 		if (PluginEnabled.Value) return;

// 		Instance.Logger.LogInfo($"SetState called: {newState}");
// 	}
// }

// [HarmonyPatch]
// static class HeroController_Skips_Patch_2
// {
// 	// AmbiguousMatchException: Ambiguous match found.
// 	[HarmonyPrefix]
// 	[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanThrowTool))]
// 	static bool CanThrowTool_TruePrefix(HeroController __instance, ref ToolItem tool, ref AttackToolBinding binding, ref bool reportFailure, ref bool __result)
// 	{
// 		if (!PluginEnabled.Value) return true;
// 		__result = true;
// 		return false;
// 	}

// 	[HarmonyPrefix]
// 	[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanFloat))]
// 	static bool CanFloat_TruePrefix(HeroController __instance, ref bool checkControlState, ref bool __result)
// 	{
// 		if (!PluginEnabled.Value) return true;
// 		__result = true;
// 		return false;
// 	}
// }

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.SetToolCooldown))]
// static class HeroController_SetToolCooldown_Patch
// {
// 	[HarmonyPrefix]
// 	static bool Prefix(HeroController __instance, ref float cooldown)
// 	{
// 		Instance.Logger.LogInfo($"SetToolCooldown called with cooldown: {cooldown}");
// 		if (!PluginEnabled.Value) return true;
// 		cooldown = 0f;
// 		return true;
// 	}
// }

// [HarmonyPrefix]
// [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanThrowTool), typeof(ToolItem), typeof(AttackToolBinding), typeof(bool))]
// static bool CanThrowTool_TruePrefix(HeroController __instance, ref ToolItem tool, ref AttackToolBinding binding, ref bool reportFailure, ref bool __result)
// {
// 	if (!PluginEnabled.Value) return true;
// 	__result = true;
// 	return false;
// }

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.ThrowToolCooldownReady))]
// static class HeroController_ThrowToolCooldownReady_Patch
// {
// 	[HarmonyPrefix]
// 	static bool Prefix(HeroController __instance, ref bool __result)
// 	{
// 		Instance.Logger.LogInfo("ThrowToolCooldownReady called");
// 		if (!PluginEnabled.Value) return true;
// 		__result = true;
// 		return false;
// 	}
// }

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanThrowTool), typeof(bool))]
// private static class HeroController_CanThrowTool_Patch_2
// {
// 	[HarmonyPrefix]
// 	static bool Prefix(HeroController __instance, ref bool checkGetWillThrow, ref bool __result)
// 	{
// 		if (!PluginEnabled.Value) return true;
// 		__result = true;
// 		return false;
// 	}
// }

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanThrowTool), typeof(ToolItem), typeof(AttackToolBinding), typeof(bool))]
// private static class HeroController_CanThrowTool_Patch
// {
// 	[HarmonyPrefix]
// 	static bool Prefix(HeroController __instance, ref ToolItem tool, ref AttackToolBinding binding, ref bool reportFailure, ref bool __result)
// 	{
// 		if (!PluginEnabled.Value) return true;
// 		__result = true;
// 		return false;
// 	}
// }
