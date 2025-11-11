using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Pfim;
using Pfim.dds;
using ACTracks.KN5;
using ACTracks.scripts.ACImport;
using Array = Godot.Collections.Array;

[Tool]
public partial class ACTrack : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		new ACImport( this ).LoadTrack( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/imola/imola.kn5" );

		//LoadTrack( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/imola/imola.kn5" );
		//LoadTrack( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/monza/monza.kn5" );
	}
}
