using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Guts;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Hollow Knight Silksong")]
public class Guts : BaseUnityPlugin
{
	internal static Guts Instance;
	internal static ConfigEntry<bool> PluginEnabled;
	internal static ConfigEntry<float> NailScale;

	private void Awake()
	{
		Instance = this;

		PluginEnabled = Config.Bind("General", "Enabled", true);
		NailScale = Config.Bind("General", "Nail Scale", 3f, new ConfigDescription("", new AcceptableValueRange<float>(0, 20)));

		Harmony harmony = new(PluginInfo.PLUGIN_GUID);
		harmony.PatchAll();

		Logger.LogInfo("Plugin loaded and initialized.");
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.SetConfigGroup))]
	private static class HeroController_SetConfigGroupt_Patch
	{
		private static readonly Dictionary<GameObject, Vector3> originalLocalScales = [];

		[HarmonyPostfix]
		private static void Postfix(HeroController __instance, ref HeroController.ConfigGroup configGroup, ref HeroController.ConfigGroup overrideGroup)
		{
			var chargeSlash = configGroup?.ChargeSlash;

			if (chargeSlash == null)
				return;

			if (!PluginEnabled.Value)
			{
				if (originalLocalScales.TryGetValue(chargeSlash, out var originalScale))
					chargeSlash.transform.localScale = originalScale;

				return;
			}

			if (!originalLocalScales.ContainsKey(chargeSlash))
				originalLocalScales[chargeSlash] = chargeSlash.transform.localScale;

			chargeSlash.transform.localScale = originalLocalScales[chargeSlash] * NailScale.Value;

			// Instance.Logger.LogInfo($"ChargeSlash scale: {configGroup.ChargeSlash.transform.localScale}");
		}
	}

	[HarmonyPatch(typeof(NailSlash), nameof(NailSlash.StartSlash))]
	private static class NailSlash_StartSlash_Patch
	{
		private static readonly Dictionary<NailSlash, Vector3> originalScales = [];
		private static readonly Dictionary<NailSlash, Vector3> originalLongNeedleScales = [];

		[HarmonyPrefix]
		private static void Prefix(NailSlash __instance)
		{
			if (!PluginEnabled.Value)
			{
				if (originalScales.ContainsKey(__instance))
					__instance.scale = originalScales[__instance];

				if (originalLongNeedleScales.ContainsKey(__instance))
					__instance.longNeedleScale = originalLongNeedleScales[__instance];

				return;
			}

			if (!originalScales.ContainsKey(__instance))
			{
				originalScales[__instance] = __instance.scale;
				originalLongNeedleScales[__instance] = __instance.longNeedleScale;
			}

			__instance.scale = originalScales[__instance] * NailScale.Value;
			__instance.longNeedleScale = originalLongNeedleScales[__instance] * NailScale.Value;

			// Instance.Logger.LogInfo($"Modified scale: {__instance.scale}, longNeedleScale: {__instance.longNeedleScale}");
		}
	}
}

// Hunter
//   - Sideways: 			  0.90, 1.00, 1.00
//   - Upwards:				  1.00, 0.90, 1.00

// Reaper
//   - Sideways: 			  1.00, 1.00, 1.00
//   - Upwards:				  0.92, 1.00, 1.00
//   - Downwards:			  1.00, 1.00, 0.00
//   - Downwards w/ Longclaw: 1.10, 1.10, 1.00

// Wanderer
//   - Sideways: 			  1.00, 1.00, 1.00
//   - Upwards:				  0.70, -1.15, 1.00
//   - Downwards:			  0.80, 1.00, 1.00

// Beast
//   - Sideways: 			  0.95, 1.00, 1.00
//   - Upwards:				  1.00, 1.00, 1.00
//   - Downwards:			  1.00, 1.00, 0.00
//   - Downwards w/ Longclaw: 1.10, 1.10, 1.00

// Witch
//   - Sideways: 			  0.94, 1.15, 1.00
//   - Sideways w/ Longclaw:  1.05, 1.27, 1.00
//   - Upwards:				  0.90, 1.30, 1.00
//   - Upwards w/ Longclaw:   0.97, 1.40, 1.00
//   - Downwards:			  0.83, 0.90, 0.00
//   - Downwards w/ Longclaw: 0.91, 0.99, 1.00

// Architect
//   - Sideways: 			  0.87, 1.00, 1.00
//   - Upwards:				  0.87, 1.00, 1.00
//   - Upwards w/ Longclaw:	  0.87, 1.00, 1.00
//   - Downwards:			  0.85, 0.85, 1.00

// Shaman
//   - *:		 			  1.00, 1.00, 1.00
