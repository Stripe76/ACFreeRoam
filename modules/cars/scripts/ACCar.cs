using Godot;
using ACTracks.ACImport;

[Tool]
public partial class ACCar : Node3D
{
	public override void _Ready()
	{
		if( Engine.IsEditorHint( ) )
		{
			//new ACImportCar( this ).Load( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/","abarth500","" );
			//new ACImportCar( this ).Load( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/","ferrari_458","" );
			//new ACImportCar( this ).Load( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/","ks_nissan_gtr","" );
			//new ACImportCar( this ).Load( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/","ks_ford_gt40","" );
		}
	}

	public void LoadCar( string acFolder,string file,string skin )
	{
		new ACImportCar( this ).Load( acFolder,file,skin );
	}
}
