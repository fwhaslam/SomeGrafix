// Copyright (c) 2026 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Events;

using static Scripts.Tools.KeyBindings;
using static UnityEditor.PlayerSettings;


/// <summary>
/// Camera Control Goals:
/// 1) Display an X/Y World using camera at a -Z location.
/// 2) Handle both Keyboard/Mouse and Touch input.  Should work for both PC and iPad.
/// 3) Handle both orthographic and perspective
/// 4) Zoom - mouse, key, compass and touch
/// 5) Pan - mouse, key, compass and touch
/// 6) Rotation - mouse, key, compass and touch
/// 7) Transitions are animated ( and can be skipped using 'SkipAnimation' )
/// 
/// Animation Concept:
/// There is some change which requires time to complete.  
/// When a change is requested, the time to complete is reset to a new value ( like half a second ).
/// Multiple changes can leave the camera chasing some view, but still it takes time to complete.
/// Code can override the 'time to complete' to create 'snap changes'. ( eg.  'SkipAnimation()' ).
/// 
/// TODO:
/// Figure out values for DragAdjustFocus.
/// Add rotation for mouse and touch.
/// Test simplified CameraView.Lerp, drop unused easings.
/// Test with unity 5 remote.
/// </summary>
/// 

namespace Scripts.Tools {

    /// <summary>
    /// Properties which define how the camera displays our X/Y world.
    /// </summary>
    public class ViewSettings {

        // are we orthographic or perspective view
        public bool Orthographic { get; set; } = false;

        // will we support rotation in the view
        public bool Rotates { get; set; } = true;

        // limit where the 'focus' can be set.
        public Rect Bounds { get; set; } = new Rect(){ x=-10f, y=-10f, width=20f, height=20f };

        // 'Zoom Near' - absolute distance ( will be made negative internally )
        public float ZMin { get; set; } = 10f;

        // 'Zoom Far' - absolute distance ( will be made negative internally )
        public float ZMax { get; set; } = 100f;

        // Defines rotation for an angled view.  Values from 0f to 90f.
        public System.Func<float,float> Tilt { get; set; } = (z) => {return z; };

    }

    public class CameraView {

        public CameraView( Vector3 pos, Vector3 rot ) {
            Position = pos;
            Rotation = rot;
        }

        // transform.localPosition
        public Vector3 Position { get; set; }

        // transform.eulerAngles
        public Vector3 Rotation { get; set; }

        public void Set( Transform transform ) {
            transform.localPosition = Position;
            transform.eulerAngles = Rotation;
        }

        public static CameraView Lerp( CameraView from, CameraView dest, float delta ) {
            return new CameraView(
                Vector3.Lerp( from.Position, dest.Position, delta ),
                Quaternion.Lerp( Quaternion.Euler(from.Rotation), Quaternion.Euler(dest.Rotation), delta ).eulerAngles );
                //Vector3.Lerp( from.Rotation, dest.Rotation, delta ) );
        }

    }

public class CameraHandler : MonoBehaviour {

    public static readonly UnityEvent<Vector2> moveFocusRequest = new();    // where are we looking in the world

    public static readonly UnityEvent<ViewSettings> viewSettingsRequest = new();    // limits on camera display

    public float animateSeconds = 0.3f;

	// public values
    [Header("Zoom Speed")]
    public float touchZoomRate = 0.03f;
    public float keyboardZoomRate = 4f;
	public float mouseZoomRate = 1f;

    [Header("Pan Limits")]  
    public float keyboardPanRate = 10f;
    public float mousePanRate = 1f;

    [Header("Spin Limits")]
    public float keyboardSpinRate = 40f;
    public float mouseSpinRate = 120f;

    // === private values ===
    internal Camera view;
    internal ViewSettings settings;

    // camera locations for animation
    internal CameraView startView, currentView, goalView;
    internal bool blockInput,isAnimating;
    internal float animateTime;

    // panning :: focus is 2D location in XY map
    internal Vector2 targetFocus;
    internal bool isPanDragging;
    internal Vector3 mouseLastFrame = Vector3.zero;

    // zooming :: zoom is viewing distance
    internal float targetZoom;
    internal float lastPinchMagnitude = 0f;

    // rotation :: spin view around the focus
    internal float targetSpin;
    internal bool isSpinDragging;

    // key bindings
    internal KeyBinding leftKB,rightKB,upKB,downKB,zoomOutKB,zoomInKB,spinLeftKB,spinRightKB;

	internal void Awake() {

//Debug.Log("CameraHandler.Awake!("+GetHashCode()+")");

 	    view = gameObject.GetComponent<Camera>();
        UpdateViewSettings( new ViewSettings() );

        // nothing currently moving
        isAnimating = false;
        isPanDragging = false;

        // nothing currently blocked
        blockInput = false;

        leftKB = KeyBindings.Get("ViewLeft");
        rightKB = KeyBindings.Get("ViewRight");
        upKB = KeyBindings.Get("ViewUp");
        downKB = KeyBindings.Get("ViewDown");
        zoomOutKB = KeyBindings.Get("ZoomOut");
        zoomInKB = KeyBindings.Get("ZoomIn");
        spinLeftKB = KeyBindings.Get("SpinLeft");
        spinRightKB = KeyBindings.Get("SpinRight");
	}
    
	internal void OnEnable() {
        CameraHandler.moveFocusRequest.AddListener( UpdateViewFocus );
        CameraHandler.viewSettingsRequest.AddListener( UpdateViewSettings );
    }

    internal void OnDisable() {
        CameraHandler.moveFocusRequest.RemoveListener( UpdateViewFocus );
        CameraHandler.viewSettingsRequest.RemoveListener( UpdateViewSettings );
    }

    /// <summary>
    /// Prevent camera motion input for certain activites ( switching screens and the like )
    /// </summary>
    /// <param name="value"></param>
    public void SetBlockInput( bool value ) {
        blockInput = value;
	}

    /// <summary>
    /// Calculate the GoalView based on targetFocus and targetZoom.
    /// Where do we need the camera to sit in order to focus on a particular point,
    /// with a particular rotation, and a particular tilt ( view angle ).
    /// </summary>
    /// <param name="target"></param>
    /// <returns>Tuple( position, rotation )</returns>
    internal CameraView CalculateGoalView() {

        // rotate around focus, then apply tilt
        var tilt = settings.Tilt( targetZoom );
        var quat = Quaternion.Euler( 0f, 0f, targetSpin ) * Quaternion.Euler( -tilt, 0f, 0f );

        // up :: rotate the view vector ( camera to surface )
        var offset = quat * new Vector3( 0f, 0f, -targetZoom );
        var pos = new Vector3( targetFocus.x+offset.x, targetFocus.y+offset.y, -targetZoom );

        return new CameraView(pos,quat.eulerAngles);
    }

//======================================================================================================================
//  Update / Input Events
//======================================================================================================================

	/// <summary>
	/// Animate camera move using keys, pinch or drag.
	/// </summary>
	internal void Update() {

        AnimateView();

        CheckForZoomInput();
        CheckForPanInput();
        CheckForSpinInput();
            
        mouseLastFrame = Input.mousePosition;

    }

    void AnimateView() {

        if (isAnimating) {

            animateTime += Time.deltaTime;
            float delta = animateTime / animateSeconds;

            if (delta>=1f) {
                currentView = goalView;
                isAnimating = false;
                animateTime = 0f;
            }
            else {
                float eased = EaseOutSine( delta );
                currentView = CameraView.Lerp( startView, goalView, eased );
            }
            
            currentView.Set( transform );
        }
        
	}
            
    internal void RestartAnimation(){
        animateTime = 0f;
        isAnimating = true;
        startView = currentView;
    }

    public void SkipAnimation(){
        isAnimating = false;
            isPanDragging = isSpinDragging = false;
        currentView = goalView;
        currentView.Set( transform );
    }

    /// <summary>
    /// Copied from EasingTools
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
	internal static float EaseOutSine(float x) {
		return Mathf.Sin((x * Mathf.PI) / 2);
	}


//======================================================================================================================

    void CheckForPanInput() {

        // animation can prevent screen panning
        if (blockInput) {
            isPanDragging = false;
            if (Input.mousePresent) {
                mouseLastFrame = Input.mousePosition;
			}
            return;
		}
        
        // Keyboard
        if ( leftKB.BindingActive() ) {
            KeyboardAdjustFocus( Vector2.left );
            return;
		}
        if ( rightKB.BindingActive() ) {
            KeyboardAdjustFocus( Vector2.right );
            return;
		}
        if ( upKB.BindingActive() ) {
           KeyboardAdjustFocus( Vector2.up );
            return;
		}
        if (downKB.BindingActive() ) {
            KeyboardAdjustFocus( Vector2.down );
            return;
		}
        
        // Mouse 
		if (Input.mousePresent) {

            // animation blocks pan,  UI blocks click, mouse button is up
            if (ClickLayerHandler.IsMouseOverGUI() || Input.GetMouseButtonUp(0) ) {
                isPanDragging = false;
            }
            // do some drag panning
            else if ( Input.GetMouseButtonDown(0) ) {
Debug.Log("Mouse(0) START Pan Dragging");
                isPanDragging = true;
            }
            else if (isPanDragging) {
 Debug.Log("Mouse(0) is Pan Dragging");
               Vector2 delta = Input.mousePosition - mouseLastFrame;
                DragAdjustFocus( delta );
                //Vector3 worldPt = new( Screen.width/2f - delta.x, Screen.height/2f - delta.y, 1f );
                //SnapPanning( view.ScreenToWorldPoint( worldPt ) );
            }

            return;
		}

        // IMPORTANT: unity touch-emulation means we MIGHT trigger both mouse+touch; adding returns Touch Screen
        if (Input.touchSupported && Input.touchCount==1) {

            Touch touch = Input.GetTouch(0);

            // UI blocks touch, touch ended/cancelled
            if ( ClickLayerHandler.IsTouchOverGUI(touch) || touch.phase==TouchPhase.Ended || touch.phase==TouchPhase.Canceled ) {
                isPanDragging = false;
			}
            else if ( touch.phase==TouchPhase.Began ) {
                isPanDragging = true;
			}
            else if ( isPanDragging && touch.phase==TouchPhase.Moved ) {
Debug.Log("Touch is Pan Dragging");
                // good enough.  Perfect in game and simulator modes.
                //      Near perfect for remote with identical screen resolution.
                //      slow/fast when screen horizontal does not match remote.
                Vector2 delta = touch.deltaPosition; 
                DragAdjustFocus( delta );
                //Vector3 screenPt = new( Screen.width/2f - delta.x, Screen.height/2f - delta.y, 1f );
                //SnapPanning( view.ScreenToWorldPoint( screenPt ) );
			}

            return;
		}
	}

	void CheckForZoomInput() {

        if (blockInput) return;

        // Keyboard
        if ( zoomInKB.BindingActive() ) {
             KeyboardAdjustZoom( +1 );
		}

        if ( zoomOutKB.BindingActive() ) {
            KeyboardAdjustZoom( -1 );
		}

        // Mouse Wheel
        if (Input.mousePresent) {
            Vector2 wheel = Input.mouseScrollDelta;
            if (wheel.x!=0 || wheel.y!=0) {
                WheelAdjustZoom( wheel.y );
            }
            return;
		}

        // Touch Screen Pinch :: two finger gesture
        if (Input.touchCount == 2) {

            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            var start = (touchZero.phase==TouchPhase.Began || touchOne.phase==TouchPhase.Began );
            var active = (touchZero.phase==TouchPhase.Moved || touchOne.phase==TouchPhase.Moved );

            // Find magnitude of the vector (the distance) between touches each frame.
            float pinchMagnitude = (touchZero.position - touchOne.position).magnitude;
            if (start) {
                lastPinchMagnitude = pinchMagnitude;
			}
            else if ( active ) {
Debug.Log("Touch is Zoom Pinching");
                float pinchDiff = pinchMagnitude - lastPinchMagnitude;
                lastPinchMagnitude = pinchMagnitude;
                TouchAdjustZoom( pinchDiff );
            }

            return;
        }

	}

    internal void CheckForSpinInput(){

        if (blockInput) return;

        // Keyboard
        if ( spinLeftKB.BindingActive() ) {
             KeyboardAdjustSpin( +1 );
		}

        if ( spinRightKB.BindingActive() ) {
            KeyboardAdjustSpin( -1 );
		}

        // Mouse 
		if (Input.mousePresent) {

            // animation blocks spin,  UI blocks click, third mouse button is up
            if (ClickLayerHandler.IsMouseOverGUI() || Input.GetMouseButtonUp(2) ) {
                isSpinDragging = false;
            }
            // do some spin dragging
            else if ( Input.GetMouseButtonDown(2) ) {
Debug.Log("Mouse(2) START Spin Dragging  X1="+Input.mousePosition+" X2="+mouseLastFrame);
                isSpinDragging = true;
            }
            else if (isSpinDragging) {
 Debug.Log("Mouse(2) is Spin Dragging  X1="+Input.mousePosition+" X2="+mouseLastFrame);
                float delta = Input.mousePosition.x - mouseLastFrame.x;
                DragAdjustSpin( delta );
                //Vector3 worldPt = new( Screen.width/2f - delta.x, Screen.height/2f - delta.y, 1f );
                //SnapPanning( view.ScreenToWorldPoint( worldPt ) );
            }

            return;
		}

    }

//======================================================================================================================

   internal void SetTargetZoom( float zoom ) {

        targetZoom = Mathf.Clamp( zoom, settings.ZMin, settings.ZMax);

        goalView = CalculateGoalView();
        RestartAnimation();
    }

   internal void SetTargetSpin( float spin ) {

        // we expect that negatives remain above -360f
        targetSpin = (spin + 360f ) % 360f;

        goalView = CalculateGoalView();
        RestartAnimation();
    }

   internal void SetTargetFocus( Vector2 focus ) {
			
        targetFocus = new Vector3(
            Mathf.Clamp( focus.x, settings.Bounds.xMin, settings.Bounds.xMax),
            Mathf.Clamp( focus.y, settings.Bounds.yMin, settings.Bounds.yMax ) );

        goalView = CalculateGoalView();
        RestartAnimation();
    }

   internal void AdjustTargetZoom( float zoomDelta ) {
        SetTargetZoom( targetZoom + zoomDelta );
    }

   internal void AdjustTargetSpin( float spinDelta ) {
        SetTargetSpin( targetSpin + spinDelta );
	}

   internal void AdjustTargetFocus( Vector2 focusDelta ) {

        var spin = Quaternion.Euler( 0f, 0f, targetSpin ) * focusDelta;
        SetTargetFocus( targetFocus + (Vector2)spin );
	}

//======================================================================================================================

    internal void TouchAdjustZoom( float delta ) {
        AdjustTargetZoom( -delta * touchZoomRate ); 
    }

    internal void WheelAdjustZoom( float delta ) {
        AdjustTargetZoom( -delta * mouseZoomRate );
    }

    internal void KeyboardAdjustZoom( float delta ) {
        AdjustTargetZoom( -delta * keyboardZoomRate * Time.deltaTime );
    }

    internal void KeyboardAdjustSpin( float delta ) {
        AdjustTargetSpin( -delta * keyboardSpinRate * Time.deltaTime );
    }

    internal void DragAdjustSpin( float delta ) {
            AdjustTargetSpin( delta * mouseSpinRate / Screen.width );
    }

    internal void KeyboardAdjustFocus( Vector2 delta ) {
        AdjustTargetFocus( delta * keyboardPanRate * Time.deltaTime );
    }

    internal void DragAdjustFocus( Vector2 delta ) {
        AdjustTargetFocus( -delta * mousePanRate * targetZoom / Screen.width );
    }

//======================================================================================================================
//      Changes to View
//======================================================================================================================

    /// <summary>
    /// Sometimes other systems want to control the POV ( like tutorials ).
    /// </summary>
    /// <param name=""></param>
    internal void UpdateViewFocus( Vector2 focus ) {
        SetTargetFocus( focus );
    }

    /// <summary>
    /// Defines the map display parameters.  Size of board in units, and scaling for tiles.
    /// This is only called with the game shifts display content ( new missions and so on ).
    /// </summary>
    /// <param name="boundsInUnits"></param>
    /// <param name="tileScaleShift"></param>
    internal void UpdateViewSettings(ViewSettings newSettings) {

        Debug.Log("HandleDisplayContextChanged.start");

        // zoom values must be absolute
        newSettings.ZMin = Mathf.Abs( newSettings.ZMin );
        newSettings.ZMax = Mathf.Abs( newSettings.ZMax );

        // start using new settings
        settings = newSettings;

        // switch camera if necessary
        view.orthographic = settings.Orthographic;

        // focus is in center ( offset south a little), zoom is halfway, and skip animation
        SetTargetSpin( 0f );
        SetTargetZoom( (settings.ZMin+settings.ZMax) / 2f );
        SetTargetFocus( new Vector2( 
            settings.Bounds.x + settings.Bounds.width/2f, 
            settings.Bounds.y + settings.Bounds.height/2.3f
         ) );
         SkipAnimation();

        Debug.Log("CameraHandler.HandleViewSettings.end");
    }

}

}