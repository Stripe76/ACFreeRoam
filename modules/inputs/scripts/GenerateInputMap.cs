using Godot;

public partial class GenerateInputMap : Node
{
	public override void _Ready( )
	{
		GenerateMap( "" );
		
		QueueFree(  );
	}
	
	public static void GenerateMap( string iniFile )
	{
		AddAction( "Pause",[Key.Escape] );

		AddAction( "Up",[Key.W,Key.Up] );
		AddAction( "Down",[Key.S,Key.Down] );
		AddAction( "Left",[Key.A,Key.Left] );
		AddAction( "Right",[Key.D,Key.Right] );
		
		AddAction( "CycleCamera",[Key.F1] );
		AddAction( "NextPitStall",[Key.F5] );
		AddAction( "PrevPitStall",[Key.F6] );
		AddAction( "ResetVehicle",[Key.F7] );

		AddAction( "Accelerator",[JoyAxis.TriggerRight] );
		AddAction( "Brakes",[JoyAxis.TriggerLeft] );
		AddAction( "Clutch",[JoyButton.A] );
		AddActionAxis( "Steering",[JoyAxis.RightX] );

		AddAction( "Handbrake",[JoyButton.B] );
		AddAction( "ShiftUp",[Key.Pageup,JoyButton.RightShoulder] );
		AddAction( "ShiftDown",[Key.Pagedown,JoyButton.LeftShoulder] );
	}

	private static void AddAction( string name,object[] events )
	{
		InputMap.AddAction( name );
		foreach( object e in events )
		{
			if( e is Key k) InputMap.ActionAddEvent( name,new InputEventKey( ) { Keycode = k } );
			if( e is JoyButton b) InputMap.ActionAddEvent( name,new InputEventJoypadButton( ) { ButtonIndex = b } );
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
