using Godot;
using ACTracks.ACImport;

[Tool, GlobalClass]
public partial class ACCar : Node3D
{
	[Export]
	public ViewportTexture MirrorTexture = new ViewportTexture( );
	
	public override void _Ready()
	{
		MirrorTexture.SetName( "MirrorTexture" );

		//MirrorTexture.ViewportPath = GetNode( "SubViewport" ).GetPath( );
		
		if( Engine.IsEditorHint( ) )
		{                          
			new ACImportCar( this,MirrorTexture ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/cars/","abarth500","" );
			//new ACImportCar( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/cars/","ferrari_458","" );
			//new ACImportCar( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/cars/","ks_nissan_gtr","" );
			//new ACImportCar( this ).Load( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/cars/","ks_ford_gt40","" );
		}
	}

	public void LoadCar( string acFolder,string file,string skin )
	{
		new ACImportCar( this,MirrorTexture ).Load( acFolder,file,skin );
	}

	public void SetMirrorViewport( SubViewport viewport )
	{
		MirrorTexture.ViewportPath = viewport.GetPath( );
	}
}
