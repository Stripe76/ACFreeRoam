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

namespace ACTracks.scripts.ACImport;

public class ACImport(Node3D self)
{
	private readonly Node3D self = self;
	
	public void LoadCar( string filename,bool loadTextures=true )
	{
		//var physics = AddNode( "Physics" );
		var physics = new StaticBody3D( )
		{
			Name = "Physics",
		};
		self.AddChild( physics );
		physics.Owner = self;
		
		var visuals = AddNode( "Visuals" );
		var dynamics = AddNode( "Dynamics" );

		try
		{
			kn5Model model = Kn5Import.readKN5( filename );

			List<Material> materials = [];
			if( loadTextures )
				materials = CreateMaterials( model );

			CreateMeshes( model,materials,physics,visuals,dynamics,null );
		}
		catch( Exception e )
		{
			GD.Print( e );
			throw;
		}
	}
	public void LoadTrack( string filename,bool loadTextures=true )
	{
		//var physics = AddNode( "Physics" );
		var physics = new StaticBody3D( )
		{
			Name = "Physics",
		};
		self.AddChild( physics );
		physics.Owner = self;
		
		var visuals = AddNode( "Visuals" );
		var dynamics = AddNode( "Dynamics" );

		var placeholders = AddNode( "Placeholders" );
		
		try
		{
			kn5Model model = Kn5Import.readKN5( filename );

			List<Material> materials = [];
			if( loadTextures )
				materials = CreateMaterials( model );

			CreateMeshes( model,materials,physics,visuals,dynamics,placeholders );
		}
		catch( Exception e )
		{
			GD.Print( e );
			throw;
		}
	}

	private Node3D AddNode( string name, Node3D parent = null )
	{
		parent ??= self;
		
		var node = new Node3D( )
		{
			Name = name,
		};
		parent.AddChild( node );
		node.Owner = self;
		
		return node;
	}
	private Node3D GetNode( Node3D parent,string name,bool create = true )
	{
		Node3D node = (Node3D)parent.GetNodeOrNull( name );
		if( node == null )
		{
			node = new Node3D( )
			{
				Name = name
			};
			parent.AddChild( node );
			node.Owner = self;
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
						//GD.Print(ksNode.name);
						meshInstance.CreateTrimeshCollision(  );

						if( meshInstance.GetChildCount( ) > 0 )
						{
							StaticBody3D staticBody = (StaticBody3D)meshInstance.GetChild( 0 );
							if( staticBody.GetChildCount( ) > 0 )
							{
								CollisionShape3D collision = (CollisionShape3D)staticBody.GetChild( 0 );
								staticBody.RemoveChild( collision );
								
								physics.AddChild( collision );
								collision.Owner = self;
							}
							meshInstance.RemoveChild( staticBody );
							staticBody.QueueFree( );
						}
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
		node.Owner = self;
				
		nodes.Add( node );
	}
	
	private static ArrayMesh CreateSurface( ACMesh mesh,ArrayMesh arrayMesh )
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
				
			//GD.Print( $"{ksTexture.name} - {ksTexture.texData.Length}" );
			
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
