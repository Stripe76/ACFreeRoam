using Godot;
using System;

public partial class World : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Node3D stall = ((ACTrack)GetNode( "ACTrack" )).GetPitStall( 1 );
		if( stall != null )
		{
			((Node3D)GetNode( "Vehicle" )).Position = stall.Position;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
