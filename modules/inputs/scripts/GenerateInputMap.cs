using Godot;
using System;
using System.Collections.Generic;

public partial class GenerateInputMap : Node
{
	public static void GenerateMap( string iniFile )
	{
		AddAction( "Up",[Key.W,Key.Up] );
		AddAction( "Down",[Key.S,Key.Down] );
		AddAction( "Left",[Key.S,Key.Left] );
		AddAction( "Right",[Key.S,Key.Right] );

		AddAction( "Accelerator",[JoyAxis.TriggerRight] );
		AddAction( "Brakes",[JoyAxis.TriggerLeft] );
		AddActionAxis( "Steering",[JoyAxis.RightX] );
	}

	private static void AddAction( string name,object[] events )
	{
		InputMap.AddAction( name );
		foreach( object e in events )
		{
			if( e is Key k) InputMap.ActionAddEvent( name,new InputEventKey( ) { Keycode = k } );
			if( e is JoyAxis a ) InputMap.ActionAddEvent( name,new InputEventJoypadMotion( ) { Device = -1,Axis = a } );
		}
	}
	private static void AddActionAxis( string name,object[] events )
	{
		InputMap.AddAction( name+"Positive" );
		InputMap.AddAction( name+"Negative" );
		foreach( object e in events )
		{
			if( e is JoyAxis a )
			{
				InputMap.ActionAddEvent( name+"Positive",new InputEventJoypadMotion( ) { Device = -1,Axis = a,AxisValue = 1.0f } );
				InputMap.ActionAddEvent( name+"Negative",new InputEventJoypadMotion( ) { Device = -1,Axis = a,AxisValue = -1.0f } );
			}
		}
	}
}
