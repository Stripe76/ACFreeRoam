using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Pfim;
using Pfim.dds;
using ACTracks.KN5;
using Array = Godot.Collections.Array;

[Tool]
public partial class ACTrack : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadTrack( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/imola/imola.kn5" );
		//LoadTrack( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/monza/monza.kn5" );
		//LoadTrack( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/abarth500/abarth500.kn5" );
		//LoadTrack( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/ferrari_458/ferrari_458.kn5" );
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process( double delta )
	{
	}

	public void LoadTrack( string filename )
	{
		//var physics = AddNode( "Physics" );
		var physics = new StaticBody3D( )
		{
			Name = "Physics",
		};
		AddChild( physics );
		physics.Owner = this;
		
		var visuals = AddNode( "Visuals" );
		var dynamics = AddNode( "Dynamics" );

		var placeholders = AddNode( "Placeholders" );
		
		try
		{
			kn5Model model = Kn5Import.readKN5( filename );

			List<Material> materials = CreateMaterials( model );

			CreateMeshes( model,materials,physics,visuals,dynamics,placeholders );

			//CreateMesh( model,mesh,filename );
		}
		catch( Exception e )
		{
			Console.WriteLine( e );
			throw;
		}
	}

	private Node3D AddNode( string name, Node3D parent = null )
	{
		parent ??= this;
		
		var node = new Node3D( )
		{
			Name = name,
		};
		parent.AddChild( node );
		node.Owner = this;
		
		return node;
	}
	private Node3D GetNode( Node3D parent,string name,bool create = true )
	{
		Node3D node = (Node3D)parent.GetNode( name );
		if( node == null )
		{
			node = new Node3D( )
			{
				Name = name
			};
			parent.AddChild( node );
			node.Owner = this;
		}
		return node;
	}

	private void CreateMeshes( kn5Model model,List<Material> materials,Node3D physics,Node3D visual,Node3D dynamics,Node3D placeholders )
	{
		List<Node3D> nodes = [];
		foreach( kn5Node ksNode in model.nodes )
		{
			int materialID = ksNode.materialID;

			bool isPhysics = IsPhysicsNode( ksNode );
			bool isVisual = materialID >= 0 && materialID < materials.Count; 
			
			if( ksNode.type == 1 )
			{
				Node3D node3D = new Node3D( )
				{
					Name = ksNode.name,
					Position = ksNode.translation,
					Rotation = ksNode.rotation,
					Scale = ksNode.scaling
				};
				AddToParent( ksNode.parentID,ksNode.name,node3D,physics,visual,dynamics,placeholders,nodes );
			}
			else
			{
				if( ksNode.indices is { Length: > 0 } )
				{
					ACMesh mesh = new ACMesh( ksNode.name );
					for( int i = 0; i < ksNode.indices.Length; i += 3 )
					{
						int index1 = ksNode.indices[i];
						int index2 = ksNode.indices[i + 2];
						int index3 = ksNode.indices[i + 1];

						mesh.vertices.Add( ksNode.position[index1] );
						mesh.vertices.Add( ksNode.position[index2] );
						mesh.vertices.Add( ksNode.position[index3] );

						//mesh.normals.Add( new Vector3( node.normal[index * 3],1-node.normal[index * 3 + 1],node.normal[index * 3 + 2] ) );

						mesh.uvs.Add( ksNode.texture0[index1] );
						mesh.uvs.Add( ksNode.texture0[index2] );
						mesh.uvs.Add( ksNode.texture0[index3] );
					}
					MeshInstance3D meshInstance = new MeshInstance3D( )
					{
						Name = ksNode.name,
						Mesh = CreateSurface( mesh,new ArrayMesh( ) ) 
					};
					if( isVisual )
					{
						meshInstance.Mesh.SurfaceSetMaterial( 0,materials[ksNode.materialID] );
					}
					if( isPhysics )
					{
						//Console.WriteLine(ksNode.name);
						meshInstance.CreateTrimeshCollision(  );

						if( meshInstance.GetChildCount( ) > 0 )
						{
							StaticBody3D staticBody = (StaticBody3D)meshInstance.GetChild( 0 );
							if( staticBody.GetChildCount( ) > 0 )
							{
								CollisionShape3D collision = (CollisionShape3D)staticBody.GetChild( 0 );
								collision.Reparent( physics );
								collision.Owner = this;
							}
							meshInstance.RemoveChild( staticBody );
							staticBody.QueueFree( );
						}
						/*
						MeshInstance3D physicsInstance = new MeshInstance3D( )
						{
							Name = ksNode.name,
							Mesh = CreateSurface( mesh,new ArrayMesh( ) )
						};
						physics.AddChild( physicsInstance );
						physicsInstance.Owner = this;
					*/
					}
					AddToParent( ksNode.parentID,ksNode.name,meshInstance,physics,visual,dynamics,placeholders,nodes );
				}
			}
		}
	}

	private static bool IsPhysicsNode( kn5Node ksNode )
	{
		if( (char.IsDigit( ksNode.name[0] ) && ksNode.name[0] != '0') ||
		    (ksNode.name[0] == '0' && char.IsDigit( ksNode.name[1] ) && ksNode.name[1] != '0') )
		{
			return true;
		}
		return false;
	}
	
	private void AddToParent( int parentID,string name,Node3D node,Node3D physics,Node3D visual,Node3D dynamics,Node3D placeholders,List<Node3D> nodes )
	{
		Node3D parent = visual;
		if( parentID > 0 )
		{
			parent = nodes[parentID];
		}
		else
		{
			if( name.StartsWith( "AC_" ) )
			{
				parent = placeholders;

				if( name.StartsWith( "AC_CREW_" ) )
				{
					parent = dynamics;
					parent = GetNode( parent,"Crews" );
				}
				else if( name.StartsWith( "AC_START_" ) )
					parent = GetNode( parent,"Grids" );
				else if( name.StartsWith( "AC_PIT_" ) )
					parent = GetNode( parent,"Pits" );
				else if( name.StartsWith( "AC_TIME_" ) || name.StartsWith( "AC_HOTLAP_" ))
					parent = GetNode( parent,"Timings" );
				else if( name.StartsWith( "AC_AUDIO_" ) )
					parent = GetNode( parent,"Audios" );
			}
		}
		parent.AddChild( node );
		node.Owner = this;
				
		nodes.Add( node );
	}
	
	private ArrayMesh CreateSurface( ACMesh mesh,ArrayMesh arrayMesh )
	{
		Array array = new Array( );
		array.Resize( (int)Mesh.ArrayType.Max );

		array[(int)Mesh.ArrayType.Vertex] = mesh.vertices.ToArray( ).AsSpan( );
		//array[(int)Mesh.ArrayType.Normal] = mesh.normals.ToArray( ).AsSpan( );
		array[(int)Mesh.ArrayType.TexUV] = mesh.uvs.ToArray( ).AsSpan( );

		SurfaceTool surfaceTool = new SurfaceTool( );
		surfaceTool.CreateFromArrays( array );
		surfaceTool.GenerateNormals( false );
		//surfaceTool.GenerateTangents( );

		surfaceTool.Commit( arrayMesh );

		return arrayMesh;
	}

	private static List<Material> CreateMaterials( kn5Model model )
	{
		List<Material> materials = [];
		foreach( var ksMaterial in model.materials )
		{
			var material = new StandardMaterial3D( );
			var texName = ksMaterial.txDiffuse;
			kn5Texture ksTexture = model.textures[texName];
				
			//Console.WriteLine( $"{ksTexture.name} - {ksTexture.texData.Length}" );
			
			var texImage = new Image( );
			ksTexture.texData[28] = 0;
			texImage.LoadDdsFromBuffer( ksTexture.texData );
			//texImage.GenerateMipmaps( );

			material.AlbedoTexture = ImageTexture.CreateFromImage( texImage );
			if( texName.Contains( "tree" ) || texName.Contains( "people" ) || texName.Contains( "pine" ) )
				material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
			
			materials.Add( material );	
		}
		return materials;
	}

	private ArrayMesh CreateMes_( kn5Model model,MeshInstance3D meshInstance,string filename )
	{
		var meshes = new SortedList<int,ACMesh>( );

		foreach( kn5Node node in model.nodes )
		{
			if( (char.IsDigit( node.name[0] ) && node.name[0] != '0') ||
			    (node.name[0] == 0 && char.IsDigit( node.name[1] ) && node.name[1] != '0') )
			{
				GD.Print( $"Skipped {node.name}" );
				continue;
			}
			int materialID = node.materialID;
			//if( materialID < 0 )
			//	continue;
			GD.Print( $"Rotation {node.name}: {node.rotation[0]},{node.rotation[1]},{node.rotation[2]}" );
			
			if( !meshes.ContainsKey( node.materialID ) )
				meshes[node.materialID] = new ACMesh( node.materialID >= 0 ? model.materials[node.materialID].name : "" );

			if( node.indices != null )
			{
				ACMesh mesh = meshes[node.materialID];
				for( int i = 0; i < node.indices.Length; i += 3 )
				{
					//mesh.vertices.Add(new Vector3( node.position[i], node.position[i+1], node.position[i+2] ) );
					int index1 = node.indices[i];
					int index2 = node.indices[i+2];
					int index3 = node.indices[i+1];

					mesh.vertices.Add( node.position[index1] );
					mesh.vertices.Add( node.position[index2] );
					mesh.vertices.Add( node.position[index3] );

					mesh.uvs.Add(node.texture0[index1]);
					mesh.uvs.Add(node.texture0[index2]);
					mesh.uvs.Add(node.texture0[index3]);
					//mesh.normals.Add( new Vector3( node.normal[index * 3],1-node.normal[index * 3 + 1],node.normal[index * 3 + 2] ) );
				}
			}
		}
		ArrayMesh arrayMesh = new ArrayMesh( );
		SurfaceTool surfaceTool = new SurfaceTool( );
		foreach( ACMesh mesh in meshes.Values )
		{
			//GD.Print( mesh.name );
			if(mesh.name.StartsWith( "tree" ) || mesh.vertices.Count <= 0 )
				continue;
			
			Array array = new Array( );
			array.Resize( (int)Mesh.ArrayType.Max );

			array[(int)Mesh.ArrayType.Vertex] = mesh.vertices.ToArray( ).AsSpan( );
			//array[(int)Mesh.ArrayType.Normal] = mesh.normals.ToArray( ).AsSpan( );
			array[(int)Mesh.ArrayType.TexUV] = mesh.uvs.ToArray( ).AsSpan( );

			surfaceTool.CreateFromArrays( array );
			surfaceTool.GenerateNormals( false );
			//surfaceTool.GenerateTangents( );

			surfaceTool.Commit( arrayMesh );
		}
		meshInstance.Mesh = arrayMesh;
		
		/*
		foreach( var tex in model.textures )
			GD.Print(tex.name );
			*/

		//foreach( var m in meshes.Keys )
		int me = 0;
		foreach( ACMesh mesh in meshes.Values )
		{
			int m = meshes.Keys[me];
			if( m >= 0 )
			{
				var ksMaterial = model.materials[m];
				//GD.Print( ksMaterial.txDiffuse );

				var material = new StandardMaterial3D( );
				var texName = ksMaterial.txDiffuse;
				kn5Texture ksTexture = model.textures[texName];
				
				//GD.Print( $"{ksTexture.name} - {ksTexture.texData.Length}" );
				var texImage = new Image( );
				ksTexture.texData[28] = 0;
				texImage.LoadDdsFromBuffer( ksTexture.texData );
				//using MemoryStream memoryStream = new MemoryStream( ksTexture.texData );
				//var texImage = LoadDDS( memoryStream,Path.GetDirectoryName( filename ) );
				//var texImage = new Image( );

				if( false )
				{
					var dict = texImage.Data;

					var w = (int)dict["width"];
					var h = (int)dict["height"];

					for( int x = 0; x < w; x++ )
					{
						for( int y = 0; y < h; y++ )
						{
							Color c = texImage.GetPixel( x,y );
							(c.R,c.B) = (c.B,c.R);
							texImage.SetPixel( x,y,c );
						}
					}
				}
				//texImage.GenerateMipmaps( );

				var imageTexture = ImageTexture.CreateFromImage( texImage );
				
				material.AlbedoTexture = imageTexture;
				
				arrayMesh.SurfaceSetName( me,ksMaterial.name );
				arrayMesh.SurfaceSetMaterial( me,material );

				me++;
			}
		}
		return arrayMesh;
	}

	private Image LoadDDS( Stream stream,string folder )
	{
		using( var image = Pfimage.FromStream( stream ) )
		{
			ImageFormat format;

			// Convert from Pfim's backend agnostic image format into GDI+'s image format
			switch( image.Format )
			{
				case ImageFormat.Rgba32:
					format = ImageFormat.Rgba32;
					break;
				default:
					// see the sample for more details
					throw new NotImplementedException( );
			}

			// Pin pfim's data array so that it doesn't get reaped by GC, unnecessary
			// in this snippet but useful technique if the data was going to be used in
			// control like a picture box
			//var handle = GCHandle.Alloc( image.Data,GCHandleType.Pinned );
			try
			{
				//GD.Print( image.Format );
				//byte[] data = new byte[(int)image.Width * (int)image.Height * 4];
				byte[] data = image.Data.Take( (int)image.Width * (int)image.Height * 4 ).ToArray();
				return Image.CreateFromData( (int)image.Width,(int)image.Height,false,Image.Format.Rgba8,data );
			}
			finally
			{
//				handle.Free();
			}
		}
	}
}

internal struct ACMesh
{
	public string name;
	
	public List<Vector3> vertices = [];
	public List<Vector3> normals = [];
	public List<Vector2> uvs = [];
	
	public ACMesh(string name)
	{
		this.name = name;
	}
}
