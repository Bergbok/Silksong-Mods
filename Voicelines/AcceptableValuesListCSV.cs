using System;
using System.Linq;
using BepInEx.Configuration;

namespace Voicelines;

public class AcceptableCSV<T>(params T[] acceptableValues) : AcceptableValueList<T>(acceptableValues) where T : IEquatable<T>
{
	public override bool IsValid(object value)
	{
		if (value == null)
			return false;

		string stringValue = value.ToString();

		if (string.IsNullOrWhiteSpace(stringValue))
			return false;

		var parts = stringValue.Split(',')
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrEmpty(s))
			.ToArray();

		if (parts.Length == 0)
			return false;

		return true;
	}

	public override object Clamp(object value)
	{
		if (IsValid(value))
			return value;

		return "None";
	}
}
