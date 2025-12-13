using Godot;
using ACTracks.ACImport;

[Tool]
public partial class ACTrack : Node3D
{
	public override void _Ready()
	{
		if( Engine.IsEditorHint( ) )
		{
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/","imola","" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/ks_vallelunga/ks_vallelunga.kn5" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/monza/monza.kn5" );
			//new ACImportTrack( this ).Load( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/","adc_klutch_kickers_drifters_paradise","" );
		}
	}
	
	public void LoadTrack( string acFolder,string track,string variant )
	{
		new ACImportTrack( this ).Load( acFolder,track,variant );
	}

	public Node3D? GetPitStall( int pit )
	{
		string name = $"AC_PIT_{pit}";

		return GetNodeOrNull( "Placeholders/Pits" )?.GetNodeOrNull( name ) as Node3D;
	}
}
