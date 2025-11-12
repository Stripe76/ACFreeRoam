using Godot;
using System;
using ACTracks.scripts.ACImport;

[Tool]
public partial class ACCar : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		new ACImportCar( this ).LoadFile( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/abarth500/abarth500.kn5" );
		//new ACImportCar( this ).LoadFile( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/ferrari_458/ferrari_458.kn5" );
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
