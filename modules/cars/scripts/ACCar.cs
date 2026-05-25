using System.Collections.Generic;
using System.IO;
using ACDBackend;
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
			LoadCar( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/cars/","abarth500","" );
			//LoadCar( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/cars/","ks_ford_gt40","" );
			//LoadCar( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/cars/","ks_nissan_gtr","" );
			//LoadCar( "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/cars/","ferrari_458","" );
		}
	}

	public void LoadCar( string acFolder,string file,string skin )
	{
		var carFolder = Path.Combine( acFolder,file ); 
		var acdData = new ACData( );
		var entries = acdData.GetEntries( carFolder );
		var lods = GetLods( entries );

		if( lods != null && lods.TryGetValue( "Lod0",out var lod ) )
			file = lod;
		else
			file = $"{file.Replace( "ks_","" )}.kn5";
		
		new ACImportCar( this,MirrorTexture ).Load( carFolder,file,skin );
	}

	public Node3D? GetCollider( )
	{
		var physics = FindChild( "Physics" );
		if( physics is not null && physics.GetChildCount( ) > 0 )
			return (Node3D)physics.GetChild( 0 );
		return null;
	}
	public void SetMirrorViewport( SubViewport viewport )
	{
		MirrorTexture.ViewportPath = viewport.GetPath( );
	}

	private SortedList<string,string>? GetLods( ACDFiles acdFiles )
	{
		var lodsFile = acdFiles.GetFile( "lods.ini" );
		if( lodsFile is { IniFile: not null } )
		{
			SortedList<string,string> lods = [];
			for( int i = 0; i < 4; i++ )
			{
				lods.Add( $"Lod{i}",lodsFile.IniFile.GetValue( "FILE",$"LOD_{i}" ) );
			}
			return lods;
		}
		return null;
	}
}
