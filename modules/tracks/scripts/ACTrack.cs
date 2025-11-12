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
		new ACImport( this ).LoadFile( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/imola/imola.kn5" );

		//LoadTrack( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/imola/imola.kn5" );
		//LoadTrack( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/monza/monza.kn5" );
	}

	public Node3D GetPitStall( int pit )
	{
		string name = $"AC_PIT_{pit}";

		return GetNodeOrNull( "Placeholders/Pits" )?.GetNodeOrNull( name ) as Node3D;
	}
}
