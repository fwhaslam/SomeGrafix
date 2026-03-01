// Copyright (c) 2026 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


/// <summary>
/// Capture screen clicks, convert to world clicks.
/// </summary>
public class ClickLayerHandler : MonoBehaviour {

    public static UnityEvent<Vector2> inWorldClickEvent = new();

    // Update is called once per frame
    internal void Update() {
        CheckInputs();
    }

     internal void  CheckInputs() {
            
        if (Input.mousePresent) { 

            // left mouse click, and not UI covers click
            if ( Input.GetMouseButtonDown(0) && !IsMouseOverGUI() ) {
//print("Camera.main="+Camera.main);
                var worldPt = Camera.main.ScreenToWorldPoint( Input.mousePosition );
//print("Mouse click in world = "+worldPt);
                inWorldClickEvent.Invoke( worldPt );
		    }

            return;
        }

        // IMPORTANT: unity touch-emulation means we MIGHT trigger both mouse+touch; adding returns
        if (Input.touchSupported && Input.touchCount==1) {

            var touch = Input.GetTouch(0);

            // beging touch and not UI covers click
            if ( touch.phase==TouchPhase.Began && !IsTouchOverGUI(touch) ) {
                var worldPt = Camera.main.ScreenToWorldPoint( touch.position );
//print("Touch click in world = "+worldPt);
                inWorldClickEvent.Invoke( worldPt );
            }
            return;
        }
    }
	
//======================================================================================================================

    /// <summary>
	/// see: https://stackoverflow.com/questions/57010713/unity-ispointerovergameobject-issue
	/// </summary>
	/// <returns></returns>
    public static bool IsMouseOverGUI() {

        // replaces the following
        //return EventSystem.current.IsPointerOverGameObject();
        return IsAnyOverGUI();
    }

    /// <summary>
	/// IsPointerOverGameObject may be buggy for touch.  Some say it only triggers when dragging over GUI.
	/// </summary>
	/// <param name="touch"></param>
	/// <returns></returns>
    public static bool IsTouchOverGUI( Touch touch ) {

		// replaces the following
		//return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
        return IsAnyOverGUI();
	}

    /// <summary>
	/// In iPad emulation, clicks and touches are intermixed.  We need to check both for blocking.
	/// </summary>
	/// <returns></returns>
    public static bool IsAnyOverGUI() {
        if  (EventSystem.current.IsPointerOverGameObject() ) return true;
        if ( Input.touchCount==1 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) ) return true;
        return false;
	}
      
}
