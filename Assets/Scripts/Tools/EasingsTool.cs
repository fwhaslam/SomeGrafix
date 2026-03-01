// Copyright (c) 2025 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using Unity.VisualScripting;

using UnityEngine;


/// <summary>
/// See: https://easings.net/
/// </summary>
public class EasingsTool {

	public static float EaseInSine(float x) {
		return 1 - Mathf.Cos((x * Mathf.PI) / 2);
	}

	public static float EaseOutSine(float x) {
		return Mathf.Sin((x * Mathf.PI) / 2);
	}
}