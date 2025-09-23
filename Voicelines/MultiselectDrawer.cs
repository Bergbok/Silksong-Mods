using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voicelines;

public static class MultiselectDrawer
{
	private static readonly Dictionary<string, Dictionary<string, bool>> multiSelectionStates = [];
	private static readonly Dictionary<string, Vector2> scrollPositions = [];
	private static string? currentlyExpanded = null;

	public static void Draw(ConfigEntryBase entry, string[] soundOptions)
	{
		string settingKey = entry.Definition.Key;

		if (!multiSelectionStates.ContainsKey(settingKey))
		{
			multiSelectionStates[settingKey] = [];
			scrollPositions[settingKey] = Vector2.zero;

			foreach (var sound in soundOptions)
			{
				multiSelectionStates[settingKey][sound] = false;
			}
		}

		var currentValues = entry.BoxedValue.ToString()
			.Split(',')
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrEmpty(s))
			.ToArray();

		GUILayout.BeginVertical();

		var displayValue = string.Join(", ", currentValues);
		var buttonStyle = currentlyExpanded == settingKey ? GUI.skin.textArea : GUI.skin.button;

		if (GUILayout.Button(displayValue, buttonStyle, GUILayout.MaxWidth(300), GUILayout.ExpandWidth(true)))
		{
			if (currentlyExpanded == settingKey)
			{
				currentlyExpanded = null;
			}
			else
			{
				currentlyExpanded = settingKey;

				foreach (var sound in soundOptions)
				{
					multiSelectionStates[settingKey][sound] = currentValues.Contains(sound);
				}
			}
		}

		if (currentlyExpanded == settingKey)
		{
			int itemCount = soundOptions.Length;
			float itemHeight = 20f;
			float contentHeight = soundOptions.Length * itemHeight;
			float viewHeight = 150f;

			scrollPositions[settingKey] = GUILayout.BeginScrollView(
				scrollPositions[settingKey],
				false,
				contentHeight > viewHeight,
				GUI.skin.horizontalScrollbar,
				GUI.skin.verticalScrollbar,
				GUI.skin.box,
				GUILayout.Height(viewHeight),
				GUILayout.MaxWidth(300),
				GUILayout.ExpandWidth(true)
			);

			foreach (var sound in soundOptions)
			{
				bool currentState = multiSelectionStates[settingKey][sound];
				bool newState = GUILayout.Toggle(currentState, sound, GUILayout.ExpandWidth(true));

				if (newState != currentState)
				{
					multiSelectionStates[settingKey][sound] = newState;

					if (newState && sound == "None")
					{
						// When selecting None, uncheck everything else
						foreach (var otherSound in soundOptions.Where(s => s != "None"))
						{
							multiSelectionStates[settingKey][otherSound] = false;
						}
					}
					else if (newState && sound != "None")
					{
						// When selecting any non-None sound, uncheck None
						multiSelectionStates[settingKey]["None"] = false;
					}

					var selectedSounds = multiSelectionStates[settingKey]
						.Where(kvp => kvp.Value)
						.Select(kvp => kvp.Key)
						.ToArray();

					string newValue = selectedSounds.Length == 0 ? "None" : string.Join(", ", selectedSounds);
					entry.BoxedValue = newValue;
				}
			}

			GUILayout.EndScrollView();

			if (GUILayout.Button("Clear All", GUILayout.MaxWidth(300), GUILayout.ExpandWidth(true)))
			{
				foreach (var sound in soundOptions)
				{
					multiSelectionStates[settingKey][sound] = false;
				}
				multiSelectionStates[settingKey]["None"] = true;
				entry.BoxedValue = "None";
			}
		}

		GUILayout.EndVertical();
	}
}
