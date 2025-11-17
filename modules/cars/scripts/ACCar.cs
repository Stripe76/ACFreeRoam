using Godot;
using ACTracks.ACImport;

[Tool]
public partial class ACCar : Node3D
{
	public override void _Ready()
	{
		if( Engine.IsEditorHint( ) )
		{
			//new ACImportCar( this ).LoadFile( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/ks_nissan_gtr/nissan_gtr.kn5" );
			//new ACImportCar( this ).LoadFile( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/ks_ford_gt40/ford_gt40.kn5" );
			//new ACImportCar( this ).LoadFile( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/abarth500/abarth500.kn5" );
			//new ACImportCar( this ).LoadFile( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/ferrari_458/ferrari_458.kn5" );
		}
	}

	public void LoadCar( string acFolder,string file,string skin )
	{
		new ACImportCar( this ).Load( acFolder,file,skin );
	}
}
