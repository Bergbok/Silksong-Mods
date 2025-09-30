using BepInEx;
using BepInEx.Configuration;
using GlobalEnums;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using InControl;
using Mono.Posix;

namespace Scuttlebraced;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Hollow Knight Silksong")]
public class Scuttlebraced : BaseUnityPlugin
{
	internal static Scuttlebraced Instance;
	internal static ConfigEntry<bool> PluginEnabled;

	private void Awake()
	{
		Instance = this;

		PluginEnabled = Config.Bind("General", "Enabled", true);

		Harmony harmony = new(PluginInfo.PLUGIN_GUID);
		harmony.PatchAll();

		Logger.LogInfo("Plugin loaded and initialized.");
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Update))]
	private static class HeroController_Update_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(HeroController __instance)
		{
			if (!PluginEnabled.Value) return;

			__instance.toolEventTarget.SendEventSafe("TAKE CONTROL");
			__instance.toolEventTarget.SendEvent("SCUTTLE");
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.CanDash))]
	private static class HeroController_CanDash_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(HeroController __instance, ref bool __result)
		{
			if (!PluginEnabled.Value) return;
			__result = true;
		}
	}

	[HarmonyPatch(typeof(OneAxisInputControl), nameof(OneAxisInputControl.IsPressed), MethodType.Getter)]
	private static class OneAxisInputControl_IsPressed_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(OneAxisInputControl __instance, ref bool __result)
		{
			if (!PluginEnabled.Value) return;

			if (__instance is PlayerAction playerAction && playerAction.Name == "Dash")
				__result = true;
		}
	}
}

// failed experiments:

// [HarmonyPatch(typeof(ToolItemManager), nameof(ToolItemManager.SetActiveState))]
// private static class ToolItemManager_SetActiveState_Patch
// {
// 	[HarmonyPostfix]
// 	private static void Postfix(ToolItemManager __instance, ref ToolsActiveStates value)
// 	{
// 		Instance.Logger.LogInfo($"ToolItemManager.SetActiveState: {__instance.name} {value}");
// 	}
// }

// [HarmonyPatch(typeof(Gameplay), nameof(Gameplay.ScuttleCharmTool), MethodType.Getter)]
// private static class Gameplay_ScuttleCharmTool_Patch
// {
// 	[HarmonyPostfix]
// 	private static void Postfix(Gameplay __instance, ref ToolItem __result)
// 	{
// 		Instance.Logger.LogInfo($"Checked usage of {__instance.name}, ThrowCooldown: {__result.Usage.ThrowCooldown}, __result: {__result}");
// 	}
// }

// [HarmonyPatch(typeof(ToolItem), nameof(ToolItem.Usage), MethodType.Getter)]
// private static class ToolItem_Usage_Patch
// {
// 	[HarmonyPostfix]
// 	private static void Postfix(ToolItem __instance, ref ToolItem.UsageOptions __result)
// 	{
// 		Instance.Logger.LogInfo($"Checked usage of {__instance.name}, ThrowCooldown: {__result.ThrowCooldown}, __result: {__result}");
// 	}
// }

// [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.SendEvent))]
// private static class PlayMakerFSM_SendEvent_Patch
// {
// 	[HarmonyPrefix]
// 	private static bool Prefix(PlayMakerFSM __instance, ref string eventName)
// 	{
// 		if (!PluginEnabled.Value)
// 			return true;

// 		// Instance.Logger.LogInfo($"PlayMakerFSM.SendEvent: {__instance.FsmName} - {eventName}");

// 		if (__instance.FsmName == "Sprint" && eventName == "CANCEL SPRINT")
// 			return false;

// 		return true;
// 	}
// }

// leaves hornet uncontrollable
// [HarmonyPatch(typeof(HeroController), nameof(HeroController.CancelBackDash))]
// private static class HeroController_CancelBackDash_Patch
// {
// 	[HarmonyPrefix]
// 	private static bool Prefix(HeroController __instance)
// 	{
// 		Instance.Logger.LogInfo($"Skipped CancelBackDash");
// 		return false;
// 	}
// }

// [HarmonyPatch(typeof(HeroControllerStates), nameof(HeroControllerStates.SetState))]
// private static class HHeroControllerStates_SetState_Patch
// {
// 	[HarmonyPostfix]
// 	private static void Postfix(HeroControllerStates __instance, ref string stateName, ref bool value)
// 	{
// 		if (stateName == "isInCancelableFSMMove")
// 			value = true;
// 	}
// }

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanDash))]
// private static class HeroController_CanDash_Patch
// {
// 	[HarmonyPostfix]
// 	private static void Postfix(ToolItem __instance, ref bool __result)
// 	{
// 		Instance.Logger.LogInfo($"HeroController.CanDash = {__result}");
// 		__result = true;
// 	}
// }

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanWallScramble))]
// private static class HeroController_CanWallScramble_Patch
// {
// 	[HarmonyPostfix]
// 	private static void Postfix(ToolItem __instance, ref bool __result)
// 	{
// 		Instance.Logger.LogInfo($"HeroController.CanWallScramble = {__result}");
// 		__result = true;
// 	}
// }

// [HarmonyPatch(typeof(HeroAnimationController), nameof(HeroAnimationController.Update))]
// private static class HeroAnimationController_Update_Patch
// {
// 	[HarmonyPostfix]
// 	private static void Postfix(HeroAnimationController __instance)
// 	{
//
// 	}
// }

// [HarmonyPatch(typeof(ToolItem), nameof(ToolItem.IsEquipped), MethodType.Getter)]
// private static class ToolItem_IsEquipped_Patch
// {
// 	[HarmonyPostfix]
// 	private static void Postfix(HeroAnimationController __instance, ref bool __result)
// 	{
// 		Instance.Logger.LogInfo($"Checked if {__instance.name} is equipped ({__result}).");
// 	}
// }

// [HarmonyPatch(typeof(GameManager), nameof(GameManager.ContinueGame))]
// private static class ContinueGame_Patch
// {
// 	[HarmonyPrefix]
// 	private static void Prefix(GameManager __instance)
// 	{
// 		Instance.Logger.LogInfo("ContinueGame called");
// 	}
// }

// [HarmonyPatch(typeof(GameManager), nameof(GameManager.Start))]
// private static class GameManager_Start_Patch
// {
// 	[HarmonyPrefix]
// 	private static void Prefix(GameManager __instance)
// 	{
// 		Instance.Logger.LogInfo("GameManager Start called");
// 	}
// }

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
// private static class HeroController_Start_Patch
// {
// 	[HarmonyPrefix]
// 	private static void Prefix(HeroController __instance)
// 	{
// 		Instance.Logger.LogInfo("HeroController Start called");
// 	}
// }

// [HarmonyPatch(typeof(HeroControllerConfig), nameof(HeroControllerConfig.OnUpdatedVariable))]
// private static class HeroControllerConfig_OnUpdatedVariable_Patch
// {
// 	[HarmonyPostfix]
// 	private static void Postfix(HeroControllerConfig __instance, ref string variableName)
// 	{
// 		Instance.Logger.LogInfo($"HeroControllerConfig: Updated variable {variableName}");
// 	}
// }

// [HarmonyPatch(typeof(InputHandler), nameof(InputHandler.GetWasButtonPressedQueued))]
// private static class InputHandler_GetWasButtonPressedQueuedPatch
// {
// 	[HarmonyPostfix]
// 	private static void Postfix(InputHandler __instance, ref HeroActionButton heroAction, ref bool consume)
// 	{
// 		if (!PluginEnabled.Value) return;

// 		Instance.Logger.LogInfo($"GetWasButtonPressedQueued: {heroAction}, consume: {consume}");
// 	}
// }

// [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.SetState))]
// private static class PlayMakerFSM_SetState_Patch
// {
// 	[HarmonyPrefix]
// 	private static void Prefix(PlayMakerFSM __instance, ref string stateName)
// 	{
// 		Instance.Logger.LogInfo($"PlayMakerFSM.SetState: {__instance.FsmName} - {stateName}");
// 	}
// }

// [HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.ChangeState))]
// private static class PlayMakerFSM_ChangeState_Patch
// {
// 	[HarmonyPrefix]
// 	private static void Prefix(PlayMakerFSM __instance, ref FsmEvent fsmEvent)
// 	{
// 		Instance.Logger.LogInfo($"PlayMakerFSM.ChangeState: {__instance.FsmName} - {fsmEvent}");
// 	}
// }

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.LookForQueueInput))]
// private static class HeroController_LookForQueueInput_Patch
// {
// 	[HarmonyPrefix]
// 	private static void Prefix(HeroController __instance)
// 	{
//
// 	}
// }

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.CancelDash))]
// private static class HeroController_CancelDash_Patch
// {
// 	[HarmonyPrefix]
// 	private static bool Prefix(HeroController __instance, ref bool sendSprintEvent)
// 	{
// 		if (!PluginEnabled.Value) return true;
// 		Instance.Logger.LogInfo($"Skipped CancelDash" + (sendSprintEvent ? " with sprint event" : ""));
// 		return false;
// 	}
// }

// [HarmonyPatch(typeof(HeroController), nameof(HeroController.StopDashEffect))]
// private static class HeroController_StopDashEffecth_Patch
// {
// 	[HarmonyPrefix]
// 	private static bool Prefix(HeroController __instance, ref bool sendSprintEvent)
// 	{
// 		if (!PluginEnabled.Value) return true;
// 		Instance.Logger.LogInfo($"Skipped StopDashEffect");
// 		return false;
// 	}
// }
