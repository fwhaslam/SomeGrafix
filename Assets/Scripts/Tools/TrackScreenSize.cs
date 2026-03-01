// Copyright (c) 2026 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Track the current size of the screen.  When screen size changes, fire an event.
/// This is used by CameraHandler.
/// </summary>
public class TrackScreenSize : MonoBehaviour {

    static public UnityEvent<Vector2> updateScreenSizeEvent = new UnityEvent<Vector2>();

    internal Vector2 screenInPixelSize;

	internal void Awake() {
		Reset();
	}

	internal void OnEnable() {
		Reset();
	}

	internal void Reset() {
		screenInPixelSize = Vector2.zero;
	}


	// Update is called once per frame
	void Update() {

        if (Screen.width!=screenInPixelSize.x || Screen.height!=screenInPixelSize.y) {
            screenInPixelSize = new Vector2(Screen.width,Screen.height);
            updateScreenSizeEvent.Invoke( screenInPixelSize );
		}
        
    }
}

