using BepInEx;
using BepInEx.Configuration;
using GlobalEnums;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace Voicelines;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Hollow Knight Silksong")]
public class Voicelines : BaseUnityPlugin
{
	internal static Voicelines Instance;
	internal static ConfigEntry<bool> PluginEnabled;
	internal static ConfigEntry<float> AudioVolume;
	internal static ConfigEntry<string> AttackSound;
	internal static ConfigEntry<string> BindSound;
	internal static ConfigEntry<string> WardingBellHitSound;
	internal static ConfigEntry<string> CrossStitchSound;
	internal static ConfigEntry<string> DashAttackSound;
	internal static ConfigEntry<string> DeathSound;
	internal static ConfigEntry<string> FaydownCloakSound;
	internal static ConfigEntry<string> SwiftStepSound;
	internal static ConfigEntry<string> DrifersCloakSound;
	internal static ConfigEntry<string> HurtSound;
	internal static ConfigEntry<string> JumpSound;
	internal static ConfigEntry<string> ClawlineSound;
	internal static ConfigEntry<string> LavaBellHitSound;
	internal static ConfigEntry<string> NailArtSound;
	internal static ConfigEntry<string> NeedolinSound;
	internal static ConfigEntry<string> PaleNailsSound;
	internal static ConfigEntry<string> RingTauntSound;
	internal static ConfigEntry<string> RuneRageSound;
	internal static ConfigEntry<string> SharpdartSound;
	internal static ConfigEntry<string> SilkspearSound;
	internal static ConfigEntry<string> SilkSoarSound;
	internal static ConfigEntry<string> TauntSound;
	internal static ConfigEntry<string> ThreadStormSound;

	private AudioSource audioSource;
	private List<string> audioList = new List<string>();
	private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

	private void Awake()
	{
		Instance = this;

		LoadAudioClips();

		var soundList = new AcceptableValueList<string>(new[] { "None", "Random" }.Concat(audioList).ToArray());

		PluginEnabled = 	  Config.Bind("General", 		"Enabled",			 true, 	  new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
		AudioVolume = 		  Config.Bind("General", 		"Volume", 			0.125f,   new ConfigDescription("", new AcceptableValueRange<float>(0, 1), new ConfigurationManagerAttributes { Order = 1 }));
		AttackSound = 		  Config.Bind("Sound Bindings", "Attack", 			"None",   new ConfigDescription("", soundList));
		BindSound = 		  Config.Bind("Sound Bindings", "Bind", 			"None",   new ConfigDescription("", soundList));
		ClawlineSound = 	  Config.Bind("Sound Bindings", "Clawline", 		"None",   new ConfigDescription("", soundList));
		CrossStitchSound = 	  Config.Bind("Sound Bindings", "Cross Stitch", 	"None",   new ConfigDescription("", soundList));
		DashAttackSound = 	  Config.Bind("Sound Bindings", "Dash Attack", 		"None",   new ConfigDescription("", soundList));
		DeathSound = 		  Config.Bind("Sound Bindings", "Death", 			"None",   new ConfigDescription("", soundList));
		DrifersCloakSound =   Config.Bind("Sound Bindings", "Drifters Cloak", 	"None",   new ConfigDescription("", soundList));
		FaydownCloakSound =   Config.Bind("Sound Bindings", "Faydown Cloak", 	"None",   new ConfigDescription("", soundList));
		HurtSound = 		  Config.Bind("Sound Bindings", "Hurt", 			"None",   new ConfigDescription("", soundList));
		JumpSound = 		  Config.Bind("Sound Bindings", "Jump", 			"Jump",   new ConfigDescription("", soundList));
		LavaBellHitSound = 	  Config.Bind("Sound Bindings", "Lava Bell Hit", 	"None",   new ConfigDescription("", soundList));
		NailArtSound = 		  Config.Bind("Sound Bindings", "Nail Art", 		"None",   new ConfigDescription("", soundList));
		NeedolinSound = 	  Config.Bind("Sound Bindings", "Needolin", 		"None",   new ConfigDescription("", soundList));
		PaleNailsSound = 	  Config.Bind("Sound Bindings", "Pail Nails", 		"None",   new ConfigDescription("", soundList));
		RingTauntSound = 	  Config.Bind("Sound Bindings", "Ring Taunt", 		"None",   new ConfigDescription("", soundList));
		RuneRageSound = 	  Config.Bind("Sound Bindings", "Rune Rage", 		"None",   new ConfigDescription("", soundList));
		SharpdartSound = 	  Config.Bind("Sound Bindings", "Sharpdart", 		"None",   new ConfigDescription("", soundList));
		SilkSoarSound = 	  Config.Bind("Sound Bindings", "Silk Soar", 		"None",   new ConfigDescription("", soundList));
		SilkspearSound = 	  Config.Bind("Sound Bindings", "Silkspear", 		"ADEENO", new ConfigDescription("", soundList));
		SwiftStepSound = 	  Config.Bind("Sound Bindings", "Swift Step", 		"None",   new ConfigDescription("", soundList));
		TauntSound = 		  Config.Bind("Sound Bindings", "Taunt", 			"None",   new ConfigDescription("", soundList));
		ThreadStormSound = 	  Config.Bind("Sound Bindings", "Thread Storm", 	"SHAW",   new ConfigDescription("", soundList));
		WardingBellHitSound = Config.Bind("Sound Bindings", "Warding Bell Hit", "None",   new ConfigDescription("", soundList));

		GameObject audioObj = new("VoicelinesAudio");
		DontDestroyOnLoad(audioObj);
		audioSource = audioObj.AddComponent<AudioSource>();
		audioSource.volume = AudioVolume.Value;

		Harmony harmony = new(PluginInfo.PLUGIN_GUID);
		harmony.PatchAll();

		Logger.LogInfo("Plugin loaded and initialized.");
	}

	private void LoadAudioClips()
	{
		string sfxPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SFX");

		if (!Directory.Exists(sfxPath))
		{
			Logger.LogError($"SFX directory not found at: {sfxPath}");
			return;
		}

		foreach (string file in Directory.GetFiles(sfxPath, "*"))
		{
			string clipName = Path.GetFileNameWithoutExtension(file);
			StartCoroutine(LoadAudioClip(file, clipName));
			audioList.Add(clipName);
		}
	}

	private IEnumerator LoadAudioClip(string filePath, string clipName)
	{
		string url = "file:///" + filePath.Replace("\\", "/").Replace(" ", "%20");
		Logger.LogInfo($"Loading audio: {clipName} from {url}");

		var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
		yield return www.SendWebRequest();

		if (www.result == UnityWebRequest.Result.Success)
		{
			AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
			clip.name = clipName;
			audioClips.Add(clipName, clip);
			Logger.LogInfo($"Successfully loaded audio clip: {clipName}");
		}
		else
		{
			Logger.LogError($"Failed to load audio clip {clipName}: {www.error}");
		}
	}

	public static void PlayAudio(string action, string clipName)
	{
		if (!PluginEnabled.Value || clipName == "None" || Instance == null)
			return;

		if (clipName == "Random")
		{
			var availableSounds = Instance.audioList.Where(sound => sound != "None").ToArray();

			if (availableSounds.Length > 0)
			{
				clipName = availableSounds[Random.Range(0, availableSounds.Length)];
			}
			else
			{
				Instance.Logger.LogError("No sounds available for random selection");
				return;
			}
		}

		Instance.Logger.LogInfo($"Playing sound: {clipName} for action: {action}");

		if (Instance.audioClips.TryGetValue(clipName, out AudioClip clip))
		{
			Instance.audioSource.volume = AudioVolume.Value;
			Instance.audioSource.PlayOneShot(clip);
		}
		else
		{
			Instance.Logger.LogError($"Could not play {clipName} - clip or audio source not available");
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Attack))]
	private static class Attack_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance, ref AttackDirection attackDir)
		{
			PlayAudio("Attack", AttackSound.Value);
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Die))]
	private static class Die_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance, ref bool nonLethal, ref bool frostDeath)
		{
			PlayAudio("Death", DeathSound.Value);
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.DoDoubleJump))]
	private static class DoDoubleJump_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance)
		{
			PlayAudio("Faydown Cloak", FaydownCloakSound.Value);
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.HeroDamaged))]
	private static class HeroDamaged_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance)
		{
			PlayAudio("Hurt", HurtSound.Value);
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.HeroDash))]
	private static class HeroDash_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance, ref bool startAlreadyDashing)
		{
			PlayAudio("Dash", SwiftStepSound.Value);
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.OnHeroJumped))]
	private static class OnHeroJumped_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance)
		{
			PlayAudio("Jump", JumpSound.Value);
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.RingTaunted))]
	private static class RingTaunted_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance)
		{
			PlayAudio("RingTaunt", RingTauntSound.Value);
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.SilkTaunted))]
	private static class SilkTaunted_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance)
		{
			PlayAudio("Taunt", TauntSound.Value);
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
	private static class Start_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(HeroController __instance)
		{
			__instance.StartCoroutine(SetupFsmMonitors(__instance));
		}

		private static IEnumerator SetupFsmMonitors(HeroController hornet)
		{
			yield return null;

			SendEvent_Patch.RegisterFsmMonitor(hornet.bellBindFSM, "bellBindFSM");
			SendEvent_Patch.RegisterFsmMonitor(hornet.harpoonDashFSM, "harpoonDashFSM");
			SendEvent_Patch.RegisterFsmMonitor(hornet.silkSpecialFSM, "silkSpecialFSM");
			SendEvent_Patch.RegisterFsmMonitor(hornet.umbrellaFSM, "umbrellaFSM");

			// foreach (var state in hornet.spellControl.FsmStates)
			// {
			// 	Instance.Logger.LogInfo($"spellControl State: {state.Name}");
			// }
			// foreach (var evt in hornet.spellControl.FsmEvents)
			// {
			// 	Instance.Logger.LogInfo($"spellControl Event: {evt.Name}");
			// }
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.UseLavaBell))]
	private static class UseLavaBell_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance)
		{
			PlayAudio("Lava Bell Hit", LavaBellHitSound.Value);
		}
	}

	[HarmonyPatch(typeof(HeroPerformanceRegion), nameof(HeroPerformanceRegion.SetIsPerforming))]
	private static class SetIsPerforming_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(HeroPerformanceRegion __instance, ref bool value)
		{
			if (value)
				PlayAudio("Needolin", NeedolinSound.Value);
		}
	}

	[HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.SendEvent))]
	private static class SendEvent_Patch
	{
		private static readonly Dictionary<PlayMakerFSM, string> monitoredFsms = new Dictionary<PlayMakerFSM, string>();

		public static void RegisterFsmMonitor(PlayMakerFSM fsm, string fsmName)
		{
			if (fsm != null && !monitoredFsms.ContainsKey(fsm))
				monitoredFsms[fsm] = fsmName;
		}

		[HarmonyPrefix]
		private static void Prefix(PlayMakerFSM __instance, ref string eventName)
		{
			if (monitoredFsms.TryGetValue(__instance, out string fsmName))
			{
				// Instance.Logger.LogInfo($"{fsmName} received event: {eventName}");

				switch (fsmName)
				{
					case "bellBindFSM":
						switch (eventName)
						{
							case "HIT":
								PlayAudio("Warding Bell Hit", WardingBellHitSound.Value);
								break;
						}
						break;
					case "harpoonDashFSM":
						switch (eventName)
						{
							case "DO MOVE":
								PlayAudio("Clawline", ClawlineSound.Value);
								break;
						}
						break;
					case "silkSpecialFSM":
						switch (eventName)
						{
							case "NEEDLE THROW":
								PlayAudio("Silkspear", SilkspearSound.Value);
								break;
							case "PARRY":
								PlayAudio("Cross Stitch", CrossStitchSound.Value);
								break;
							case "SILK CHARGE":
								PlayAudio("Sharpdart", SharpdartSound.Value);
								break;
							case "SILK BOMB":
								PlayAudio("Rune Rage", RuneRageSound.Value);
								break;
							case "BOSS NEEDLE":
								PlayAudio("Pale Nails", PaleNailsSound.Value);
								break;
							case "THREAD SPHERE":
								PlayAudio("Thread Storm", ThreadStormSound.Value);
								break;
						}
						break;
					case "umbrellaFSM":
						switch (eventName)
						{
							case "FLOAT":
								if (__instance.ActiveStateName != "Cooldown")
									PlayAudio("Drifer's Cloak", DrifersCloakSound.Value);
								break;
						}
						break;
				}
			}
		}
	}

	// Prevents overlapping voice lines by preventing originals from playing & plays audio for events I couldn't find hooks for.
	// Environmental damage currently not handled, this approach does not allow distinguishing between different damage sources.
	// Alternatively patch SelectRandomClip and set __instance.volumeMin = 0f; __instance.volumeMax = 0f;
	[HarmonyPatch(typeof(RandomAudioClipTable), nameof(RandomAudioClipTable.CanPlay))]
	private static class CanPlay_Patch
	{
		[HarmonyPrefix]
		private static bool Prefix(RandomAudioClipTable __instance)
		{
			// Attack Heavy Hornet Voice, Attack Needle Art Hornet Voice, Attack Normal Hornet Voice, Bind Hornet Voice, Death Hornet Voice, Frost Damage Hornet Voice, Grunt Hornet Voice, Hazard Damage Hornet Voice, Hornet_attack_large, hornet_footstep_bell, hornet_footstep_bone, hornet_footstep_grass, hornet_footstep_hard, hornet_footstep_metal, hornet_footstep_metal_thin, hornet_footstep_moss, hornet_footstep_sand, hornet_footstep_snow, hornet_footstep_wet_metal, hornet_footstep_wet_wood, hornet_footstep_wood, hornet_needolin_small_move, Hornet_poshanka, hornet_projectile_twang Quick Sling, Hornet_roar_lock_grunt, hornet_run_start, hornet_run_start Cloakless, hornet_silk_sprint_overlay, Hornet_small_wake_sharp, hornet_taunt_beast, hornet_tool_piton, hornet_weak_exert_short, hornet_weak_exert_standard, hornet_wound_heavy, Power Up Hornet Voice, Quick Wound Hornet Voice, Skill A Hornet Voice, Skill H Hornet Voice, Small Wake Hornet Voice, Talk Hornet Voice, Taunt Hornet Voice, WakeFromDream Hornet Voice, Wallrun Grunt Hornet Voice, Wound Hornet Voice
			// Instance.Logger.LogInfo($"RandomAudioClipTable Name: {__instance.name}");

			if (!PluginEnabled.Value)
				return true;

			switch (__instance.name)
			{
				case "Attack Heavy Hornet Voice" when DashAttackSound.Value != "None":
					PlayAudio("Dash Attack", DashAttackSound.Value);
					return false;
				case "Attack Needle Art Hornet Voice" when NailArtSound.Value != "None":
					PlayAudio("Nail Art", NailArtSound.Value);
					return false;
				case "Bind Hornet Voice" when BindSound.Value != "None":
					PlayAudio("Nail Art", BindSound.Value);
					return false;
				case "Attack Normal Hornet Voice" when AttackSound.Value != "None":
				case "Grunt Hornet Voice" when JumpSound.Value != "None":
				case "Hornet_poshanka" when RingTauntSound.Value != "None":
				case "Taunt Hornet Voice" when TauntSound.Value != "None":
				case "Wound Hornet Voice" when HurtSound.Value != "None":
					return false;
				default:
					return true;
			}
		}
	}
}
