using Godot;
using ACTracks.ACImport;

[Tool]
public partial class ACTrack : Node3D
{
	public override void _Ready()
	{
		if( Engine.IsEditorHint( ) )
		{
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","monaco","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","drift","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","ks_zandvoort","" );
			new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","spa","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","ks_red_bull_ring","layout_gp" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","trento-bondone","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","magione","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","ks_vallelunga","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","mugello","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","monza","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","imola","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","la_canyons","freeroam" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/ks_vallelunga/ks_vallelunga.kn5" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","adc_klutch_kickers_drifters_paradise","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/","ks_nordschleife","nordschleife" );
		}
	}
	
	public void LoadTrack( string acFolder,string track,string variant )
	{
		new ACImportTrack( this ).Load( acFolder,track,variant );
	}

	public Node3D? GetPitStall( int pit )
	{
		string name = $"AC_PIT_{pit}";

		return GetNodeOrNull( "Markers/Pits" )?.GetNodeOrNull( name ) as Node3D;
	}
}
