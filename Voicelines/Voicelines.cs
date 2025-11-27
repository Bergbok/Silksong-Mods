using BepInEx;
using BepInEx.Configuration;
using GlobalEnums;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
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
	internal static ConfigEntry<bool> ReplaceGameSounds;
	internal static ConfigEntry<bool> UseGameVolume;
	internal static ConfigEntry<float> AudioVolume;
	internal static ConfigEntry<float> TriggerChance;
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

	private AudioSource playerAudioSource;
	private readonly List<string> audioList = [];
	private readonly Dictionary<string, AudioClip> audioClips = [];
	private class BlockEntry
	{
		public string ID;
		public string Pattern;
		public float ExpiryTime;
	}

	private static readonly List<BlockEntry> ClipsToBlock = new();

	private void Awake()
	{
		Instance = this;

		LoadAudioClips();

		var soundList = new[] { "None", "Random" }.Concat(audioList).ToArray();
		var acceptableValues = new AcceptableCSV<string>(soundList);

		PluginEnabled 		= Config.Bind("General", 		"Enabled", 			   true, 	 new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 5 }));
		ReplaceGameSounds 	= Config.Bind("General", 		"Replace Game Sounds", true, 	 new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 4 }));
		UseGameVolume 		= Config.Bind("General", 		"Use Game Volume", 	   true, 	 new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 3 }));
		AudioVolume 		= Config.Bind("General", 		"Custom Volume", 	   0.125f,   new ConfigDescription("", new AcceptableValueRange<float>(0, 1), new ConfigurationManagerAttributes { Order = 2, ShowRangeAsPercent = true }));
		TriggerChance 		= Config.Bind("General", 		"Trigger Chance", 	   1f,   	 new ConfigDescription("", new AcceptableValueRange<float>(0, 1), new ConfigurationManagerAttributes { Order = 1, ShowRangeAsPercent = true }));
		AttackSound 		= Config.Bind("Sound Bindings", "Attack", 			   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		BindSound 			= Config.Bind("Sound Bindings", "Bind", 			   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		ClawlineSound 		= Config.Bind("Sound Bindings", "Clawline", 		   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		CrossStitchSound 	= Config.Bind("Sound Bindings", "Cross Stitch", 	   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		DashAttackSound 	= Config.Bind("Sound Bindings", "Dash Attack", 		   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		DeathSound 			= Config.Bind("Sound Bindings", "Death", 			   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		DrifersCloakSound 	= Config.Bind("Sound Bindings", "Drifters Cloak", 	   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		FaydownCloakSound 	= Config.Bind("Sound Bindings", "Faydown Cloak", 	   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		HurtSound 			= Config.Bind("Sound Bindings", "Hurt", 			   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		JumpSound 			= Config.Bind("Sound Bindings", "Jump", 			   "Jump",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		LavaBellHitSound 	= Config.Bind("Sound Bindings", "Lava Bell Hit", 	   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		NailArtSound 		= Config.Bind("Sound Bindings", "Nail Art", 		   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		NeedolinSound 		= Config.Bind("Sound Bindings", "Needolin", 		   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		PaleNailsSound 		= Config.Bind("Sound Bindings", "Pale Nails", 		   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		RingTauntSound 		= Config.Bind("Sound Bindings", "Ring Taunt", 		   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		RuneRageSound 		= Config.Bind("Sound Bindings", "Rune Rage", 		   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		SharpdartSound 		= Config.Bind("Sound Bindings", "Sharpdart", 		   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		SilkSoarSound 		= Config.Bind("Sound Bindings", "Silk Soar", 		   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		SilkspearSound 		= Config.Bind("Sound Bindings", "Silkspear", 		   "ADEENO", new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		SwiftStepSound 		= Config.Bind("Sound Bindings", "Swift Step", 		   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		TauntSound 			= Config.Bind("Sound Bindings", "Taunt", 			   "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		ThreadStormSound 	= Config.Bind("Sound Bindings", "Thread Storm", 	   "SHAW",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));
		WardingBellHitSound = Config.Bind("Sound Bindings", "Warding Bell Hit",    "None",   new ConfigDescription("", acceptableValues, new ConfigurationManagerAttributes { CustomDrawer = (entry) => MultiselectDrawer.Draw(entry, soundList) }));

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

	public static void PlayAudio(string action, string clipName, string blockPattern = "")
	{
		if (!PluginEnabled.Value || clipName == "None" || Instance == null)
			return;

		if (Random.value > TriggerChance.Value) {
			Instance.Logger.LogInfo($"Skipping sound for action: {action} due to trigger chance");
			return;
		}

		var clipNames = clipName.Split(',')
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrEmpty(s))
			.ToArray();

		if (clipNames.Length == 0)
			return;

		string selectedClip;
		if (clipNames.Contains("Random"))
		{
			var availableSounds = Instance.audioList.Where(sound => sound != "None").ToArray();
			if (availableSounds.Length > 0)
			{
				selectedClip = availableSounds[Random.Range(0, availableSounds.Length)];
			}
			else
			{
				Instance.Logger.LogError("No sounds available for random selection");
				return;
			}
		}
		else if (clipNames.Length == 1)
		{
			selectedClip = clipNames[0];
		}
		else
		{
			selectedClip = clipNames[Random.Range(0, clipNames.Length)];
		}

		Instance.Logger.LogInfo($"Playing sound: {selectedClip} for action: {action}");

		if (Instance.audioClips.TryGetValue(selectedClip, out AudioClip clip))
		{
			if (!string.IsNullOrEmpty(blockPattern) && ReplaceGameSounds.Value)
			{
				var blockID = System.Guid.NewGuid().ToString();
				var entry = new BlockEntry
				{
					ID = blockID,
					Pattern = blockPattern,
					ExpiryTime = Time.time + clip.length + 0.2f
				};
				ClipsToBlock.Add(entry);
				// Instance.Logger.LogInfo($"Added block pattern: {blockPattern} (expires in {clip.length + 0.2f}s)");
				Instance.StartCoroutine(CleanupBlockEntry(blockID, clip.length + 0.2f));
			}

			if (Instance.playerAudioSource != null)
			{
				if (UseGameVolume.Value)
				{
					Instance.playerAudioSource.PlayOneShot(clip);
				}
				else
				{
					Instance.playerAudioSource.PlayOneShot(clip, AudioVolume.Value);
				}
			}
		}
		else
		{
			Instance.Logger.LogError($"Could not play {selectedClip} - clip not available");
		}
	}

	private static IEnumerator CleanupBlockEntry(string blockID, float delay)
	{
		yield return new WaitForSeconds(delay);

		var entry = ClipsToBlock.FirstOrDefault(e => e.ID == blockID);
		if (entry != null)
		{
			// Instance.Logger.LogInfo($"Auto-removing expired block pattern: {entry.Pattern}");
			ClipsToBlock.Remove(entry);
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Attack))]
	private static class HeroController_Attack_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance, ref AttackDirection attackDir)
		{
			PlayAudio("Attack", AttackSound.Value, "^a");
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Die))]
	private static class HeroController_Die_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance, ref bool nonLethal, ref bool frostDeath)
		{
			PlayAudio("Death", DeathSound.Value, "^hero_damage");
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.DoDoubleJump))]
	private static class HeroController_DoDoubleJump_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance)
		{
			PlayAudio("Faydown Cloak", FaydownCloakSound.Value, "^g");
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.HeroDamaged))]
	private static class HeroController_HeroDamaged_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance)
		{
			PlayAudio("Hurt", HurtSound.Value, "^hero_damage");
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.HeroDash))]
	private static class HeroController_HeroDash_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance, ref bool startAlreadyDashing)
		{
			PlayAudio("Swift Step", SwiftStepSound.Value, "^g");
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.OnHeroJumped))]
	private static class HeroController_OnHeroJumped_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance)
		{
			PlayAudio("Jump", JumpSound.Value, "^g");
		}
	}

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
	private static class HeroController_Start_Patch
	{
		[HarmonyPostfix]
		private static void Postfix(HeroController __instance)
		{
			Instance.playerAudioSource = __instance.GetComponent<AudioSource>();

			if (Instance.playerAudioSource == null)
			{
				Instance.Logger.LogError("Could not find player AudioSource, audio will not play.");
			}

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
	private static class HeroController_UseLavaBell_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(HeroController __instance)
		{
			PlayAudio("Lava Bell Hit", LavaBellHitSound.Value);
		}
	}

	[HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.SendEvent))]
	private static class PlayMakerFSM_SendEvent_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(PlayMakerFSM __instance, ref string eventName)
		{
			// Instance.Logger.LogInfo($"{__instance.FsmName} received event: {eventName}");

			switch (__instance.FsmName)
			{
				case "Control":
					if (eventName == "HIT")
						PlayAudio("Warding Bell Hit", WardingBellHitSound.Value, "hornet_warding_bell_bind_break");
					break;
				case "Harpoon Dash":
					if (eventName == "DO MOVE")
						PlayAudio("Clawline", ClawlineSound.Value, "^a|^g");
					break;
				case "Silk Specials":
					switch (eventName)
					{
						case "NEEDLE THROW":
							PlayAudio("Silkspear", SilkspearSound.Value, "^Hornet_attack_large");
							break;
						case "PARRY":
							PlayAudio("Cross Stitch", CrossStitchSound.Value, "^a");
							break;
						case "SILK CHARGE":
							PlayAudio("Sharpdart", SharpdartSound.Value, "^Hornet_attack_large");
							break;
						case "SILK BOMB":
							PlayAudio("Rune Rage", RuneRageSound.Value, "^Hornet_attack_large");
							break;
						case "BOSS NEEDLE":
							PlayAudio("Pale Nails", PaleNailsSound.Value);
							break;
						case "THREAD SPHERE":
							PlayAudio("Thread Storm", ThreadStormSound.Value, "^Hornet_attack_large");
							break;
					}
					break;
				case "Superjump":
					if (eventName == "DO MOVE")
						PlayAudio("Silk Soar", SilkSoarSound.Value, "hornet_superjump_pt_1_into_position");
					break;
				case "Umbrella Float":
					if (eventName == "FLOAT" && __instance.ActiveStateName != "Cooldown")
						PlayAudio("Drifter's Cloak", DrifersCloakSound.Value, "hornet_umbrella_open");
					break;
			}
		}
	}

	[HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
	private static class PlayMakerFSM_Start_Patch
	{
		[HarmonyPostfix]
		public static void Postfix(PlayMakerFSM __instance)
		{
			if (__instance is { name: "Hero_Hornet(Clone)", FsmName: "Silk Specials" })
			{
				Fsm silkSpecials = __instance.Fsm;
				Fsm? needolinFsm = silkSpecials.GetAction<RunFSM>("Needolin Sub", 2)?.fsmTemplateControl.RunFsm;

				// hooking into Check Benched / Start Needolin Benched / Play Needolin Benched doesn't work
				needolinFsm.GetState("Start Needolin").AddLambdaMethod(_ =>
					PlayAudio("Needolin", NeedolinSound.Value, "^g")
				);
			}
		}
	}

	[HarmonyPatch(typeof(RandomAudioClipTable), nameof(RandomAudioClipTable.CanPlay))]
	private static class RandomAudioClipTable_CanPlay_Patch
	{
		[HarmonyPrefix]
		private static void Prefix(RandomAudioClipTable __instance)
		{
			// Attack Heavy Hornet Voice, Attack Needle Art Hornet Voice, Attack Normal Hornet Voice, Bind Hornet Voice, Death Hornet Voice, Frost Damage Hornet Voice, Grunt Hornet Voice, Hazard Damage Hornet Voice, Hornet_attack_large, hornet_footstep_bell, hornet_footstep_bone, hornet_footstep_grass, hornet_footstep_hard, hornet_footstep_metal, hornet_footstep_metal_thin, hornet_footstep_moss, hornet_footstep_sand, hornet_footstep_snow, hornet_footstep_wet_metal, hornet_footstep_wet_wood, hornet_footstep_wood, hornet_needolin_small_move, Hornet_poshanka, hornet_projectile_twang Quick Sling, Hornet_roar_lock_grunt, hornet_run_start, hornet_run_start Cloakless, hornet_silk_sprint_overlay, Hornet_small_wake_sharp, hornet_taunt_beast, hornet_tool_piton, hornet_weak_exert_short, hornet_weak_exert_standard, hornet_wound_heavy, Power Up Hornet Voice, Quick Wound Hornet Voice, Skill A Hornet Voice, Skill H Hornet Voice, Small Wake Hornet Voice, Talk Hornet Voice, Taunt Hornet Voice, WakeFromDream Hornet Voice, Wallrun Grunt Hornet Voice, Wound Hornet Voice
			// Instance.Logger.LogInfo($"RandomAudioClipTable Name: {__instance.name}");

			switch (__instance.name)
			{
				case "Attack Heavy Hornet Voice":
					PlayAudio("Dash Attack", DashAttackSound.Value, "^a");
					break;
				case "Attack Needle Art Hornet Voice":
					PlayAudio("Nail Art", NailArtSound.Value, "^a");
					break;
				case "Bind Hornet Voice":
					PlayAudio("Bind", BindSound.Value, "^bind|^hornet_bind");
					break;
				case "Hornet_poshanka":
					PlayAudio("Ring Taunt", RingTauntSound.Value, "^Hornet_poshanka");
					break;
				case "Taunt Hornet Voice":
					PlayAudio("Taunt", TauntSound.Value, "^t");
					break;
				default:
					break;
			}
		}
	}

	[HarmonyPatch(typeof(AudioSource), nameof(AudioSource.PlayOneShot), typeof(AudioClip), typeof(float))]
	private static class AudioSource_PlayOneShot_Patch
	{
		private static bool Prefix(AudioSource __instance, AudioClip clip)
		{
			string sourcename = __instance.GetName();

			if (!(sourcename == "Audio Player Actor(Clone)"
				|| sourcename == "Audio Player Actor 2D(Clone)"
				|| sourcename.EndsWith("Voice")))
				return true;

			if (!PluginEnabled.Value || !ReplaceGameSounds.Value || ClipsToBlock.Count == 0)
				return true;

			ClipsToBlock.RemoveAll(entry => Time.time > entry.ExpiryTime);

			var matchingEntries = ClipsToBlock.Where(entry =>
				System.Text.RegularExpressions.Regex.IsMatch(clip.name, entry.Pattern)
			).ToList();

			if (matchingEntries.Any())
			{
				Instance.Logger.LogInfo($"Suppressing clip: {clip.name} (matched patterns: {string.Join(", ", matchingEntries.Select(e => e.Pattern))})");

				// Remove the matched entries from the block list
				foreach (var entry in matchingEntries)
				{
					ClipsToBlock.Remove(entry);
				}

				return false;
			}

			return true;
		}
	}
}
