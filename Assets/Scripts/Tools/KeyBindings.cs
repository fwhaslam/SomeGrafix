// Copyright (c) 2024 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using System.Linq;

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Organize and manage key controls.
/// 
/// NOTE: this script runs first in the Edit/ProjectSettings/ScriptExecutionOrder ( priority = -20 )
/// 
/// </summary>

namespace Scripts.Tools {

public class KeyBindings {

	public class KeyBinding {

		internal KeyBinding( string name, KeyCode one, KeyCode two, string info ) {
			this.Name = name;
			this.Info = info;
			this.First = one;
			this.Second = two;
		}

		public KeyBinding( KeyBinding src ){
			Reset(src);
		}

		public void Reset(KeyBinding src) {
			this.Name = src.Name;
			this.Info = src.Info;
			this.First = src.First;
			this.Second = src.Second;
		}

		public KeyBinding Copy(){
			return new KeyBinding( this );
		}

		public string Name { get; set; }

		public string Info { get; set; }

		public KeyCode First { get; set; }

		public KeyCode Second { get; set; }

		public bool BindingActive(){
			if (First!=KeyCode.None && Input.GetKey(First)) return true;
			if (Second!=KeyCode.None && Input.GetKey(Second)) return true;
			return false;
		}

	}

	internal static List<KeyBinding> DEFAULT_BINDINGS = new List<KeyBinding>(){

		new KeyBinding( "GameMenu", KeyCode.Escape, KeyCode.Tilde,
			"Open the Game Menu."),

		new KeyBinding( "NextTurn", KeyCode.Return, KeyCode.None,
			"Advance to the next turn."),

		// view functions
		new KeyBinding( "ZoomIn", KeyCode.Plus, KeyCode.Equals,
			"Enlarge map tiles."),

		new KeyBinding( "ZoomOut", KeyCode.Minus, KeyCode.Underscore,
			"Shrink map tiles."),

		new KeyBinding( "ViewLeft", KeyCode.A, KeyCode.LeftArrow,
			"Shift the map view to the left."),

		new KeyBinding( "ViewRight", KeyCode.D, KeyCode.RightArrow,
			"Shift the map view to the right."),

		new KeyBinding( "ViewUp", KeyCode.W, KeyCode.UpArrow,
			"Shift the map view to the top."),

		new KeyBinding( "ViewDown", KeyCode.S, KeyCode.DownArrow,
			"Shift the map view to the bottom."),

		new KeyBinding( "SpinLeft", KeyCode.Q, KeyCode.Comma,
			"Rotate the map Counter Clockwise."),

		new KeyBinding( "SpinRight", KeyCode.E, KeyCode.Period,
			"Rotate the map Clockwise."),

		// unit management
		new KeyBinding( "Undo", KeyCode.U, KeyCode.Backspace,
			"Undo the last unit action.  The undo stack goes back to the start of the turn.  "+
			"To go before that that you need to load a previous turn."),

		new KeyBinding( "Next", KeyCode.N, KeyCode.PageDown,
			"Jump to next active unit."),

		new KeyBinding( "Last", KeyCode.B, KeyCode.PageUp,
			"Jump to previous active unit."),

		new KeyBinding( "Sleep", KeyCode.Z, KeyCode.Space,
			"Put unit to sleep for one turn.  Unit may repair if it did not move nor attack."),

		new KeyBinding( "Repair", KeyCode.Alpha0, KeyCode.RightParen,
			"Fully repair unit.  Costs 30 fortune."),

		new KeyBinding( "Writs", KeyCode.Slash, KeyCode.Pipe,
			"Open the Writs dialog to apply bonuses to your fleet."),

	};

//======================================================================================================================

	/// <summary>
	/// Replace the Generic AsDictionary() from original code.
	/// </summary>
	/// <param name="list"></param>
	/// <returns></returns>
	internal static IDictionary<string,KeyBinding> AsDictionary( IList<KeyBinding> list) {
		var map = new Dictionary<string,KeyBinding>();
		foreach( var item in list ) map[item.Name] = item;
		return map;
	}

	/// <summary>
	/// It is important that these objects do not change.
	/// Their values may change due to player preference or reset to default,
	///		but the objects themselves are the same, acting as wrappers for the value.
	/// </summary>
	internal static IList<KeyBinding> PREFERRED_BINDINGS = 
		DEFAULT_BINDINGS.Select( (kf) => kf.Copy() ).ToList();

	public static IDictionary<string,KeyBinding> PREFERRED_BY_NAME =
		//System.Linq.ToDictionary();
		 AsDictionary( PREFERRED_BINDINGS );

	public static ISet<string> UnusedBindings = 
		new HashSet<string>( PREFERRED_BY_NAME.Keys );

//======================================================================================================================

	public KeyBindings(){
		LoadPreferredSettings();
		var unusedNames = string.Join( ",", UnusedBindings );
		Debug.Log(">>>>>>>>>> Unused Key Bindings["+UnusedBindings.Count+"] ::: "+unusedNames);
	}


	/// <summary>
	/// Need to implement 'load and save' feature.
	/// </summary>
	public void LoadPreferredSettings(){

	}

	public void SavePreferredSettings(){

	}
	
	public void ResetPreferredSettings(){
		foreach ( var kbind in DEFAULT_BINDINGS ) {
			PREFERRED_BY_NAME[kbind.Name].Reset( kbind );
		}
	}

	public static KeyBinding Register( string name ) {
		UnusedBindings.Remove( name );
		return Get(name);
	}

	internal static KeyBinding Get(string name) {
		return PREFERRED_BY_NAME[name];
	}

	public void SetFirstKeyCode( string name, KeyCode newCode ){
		RemoveKeyCode( newCode );
		Get(name).First = newCode;
	}

	public void SetSecondKeyCode( string name, KeyCode newCode ){
		RemoveKeyCode( newCode );
		Get(name).Second = newCode;
	}

	/// <summary>
	/// Before setting a new keycode, we have to remove the code from
	///		any previous KeyboardFunction.
	/// </summary>
	/// <param name="oldCode"></param>
	internal void RemoveKeyCode ( KeyCode oldCode ) {

		foreach ( var kbind in PREFERRED_BINDINGS ) {

			if (kbind.First == oldCode ) kbind.First = KeyCode.None;
			if (kbind.Second == oldCode ) kbind.Second = KeyCode.None;
		}
	}
}

}