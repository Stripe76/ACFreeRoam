using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Framework.IniFiles;
using ACTracks.KN5;

using Array = Godot.Collections.Array;

namespace ACTracks.ACImport;

public abstract class ACImport(Node3D self)
{
	private readonly Node3D self = self;

	public abstract void Load( string acFolder,string file,string variant,bool loadTextures = true );
	protected virtual Node3D GetParent( int parentID,string name,Node3D node,Node3D physics,Node3D visual,Node3D dynamics,Node3D placeholders,List<Node3D> nodes )
	{
		if( parentID > 0 )
			return nodes[parentID];
		
		Node3D parent = visual;
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
		return parent;
	}

	protected Node3D AddNode<T>( string name, Node3D parent = null ) where T : Node3D,new( )
	{
		parent ??= self;
		
		var node = new T( )
		{
			Name = name,
		};
		parent.AddChild( node );
		node.Owner = self;
		
		return node;
	}
	protected Node3D GetNode( Node3D parent,string name,bool create = true )
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

	protected static List<Material> CreateMaterials( kn5Model model )
	{
		List<Material> materials = [];
		foreach( var ksMaterial in model.materials )
		{
			//GD.Print($"{ksMaterial.name}: {ksMaterial.shaderProps}");
			var material = new StandardMaterial3D( );
			material.SetName( ksMaterial.name );
			var texName = ksMaterial.txDiffuse;

			if( model.textures.Count > 0 && model.textures.TryGetValue( texName, out kn5Texture? ksTexture ) )
			{
                //GD.Print( $"{ksTexture.name} - {ksTexture.texData.Length}" );
                ksTexture.texData[28] = 0;

				var texImage = new Image( );
				texImage.LoadDdsFromBuffer( ksTexture.texData );
				//texImage.GenerateMipmaps( );
				material.AlbedoTexture = ImageTexture.CreateFromImage( texImage );

				texName = ksMaterial.txNormal;
				if( !string.IsNullOrEmpty( texName ) && model.textures.TryGetValue( texName,out kn5Texture value ) )
				{
					ksTexture = value;
					if( ksTexture.name != "" && ksTexture.texData.Length > 0 )
					{
						//GD.Print( $"{ksTexture.name} - {ksTexture.texData.Length}" );
						ksTexture.texData[28] = 0;

						texImage = new Image( );
						texImage.LoadDdsFromBuffer( ksTexture.texData );
						//texImage.GenerateMipmaps( );

						material.NormalEnabled = true;
						material.NormalTexture = ImageTexture.CreateFromImage( texImage );
					}
				}
			}
			string trans = (texName + " " + ksMaterial.name).ToLower( );
			if( trans.Contains( "tree" ) ||
			    trans.Contains( "people" ) ||
			    trans.Contains( "pine" ) ||
			    trans.Contains( "trasp" ) ||
			    trans.Contains( "glass" ) ||
			    trans.Contains( "vetro" ) ||
			    trans.Contains( "vetri" ) )
				material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
			//if( ksMaterial.ksDiffuse < 1.0 )
			//	material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
			
			materials.Add( material );	
		}
		return materials;
	}
	
	protected void CreateMeshes( kn5Model model,List<Material> materials,Node3D physics,Node3D visual,Node3D dynamics,Node3D placeholders )
	{
		List<Node3D> nodes = [];
		foreach( kn5Node ksNode in model.nodes )
		{
			int materialID = ksNode.materialID;

			bool isPhysics = IsPhysicsNode( ksNode );
			bool isVisual = materialID >= 0 && materialID < materials.Count;
			
			if( isPhysics && ( ksNode.name.Contains( "WALL" ) || ksNode.name.Contains( "TARMAC" ) ) )
				isVisual = false;
			
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
						((ArrayMesh)meshInstance.Mesh).SurfaceSetName( 0,model.materials[ksNode.materialID].name );
						meshInstance.Mesh.SurfaceSetMaterial( 0,materials[ksNode.materialID] );
					}
					else
					{
						meshInstance.Visible = false;
					}
					if( isPhysics )
					{
						//kn5Material ksMaterial = model.materials[ksNode.materialID];
						//GD.Print($"{ksMaterial.name}: {ksMaterial.shaderProps}");
						//GD.Print(ksNode.);

						CollisionShape3D collision = new CollisionShape3D( );
						collision.Name = ksNode.name;
						collision.SetShape( meshInstance.Mesh.CreateTrimeshShape( ) );
							
						physics.AddChild( collision );
						collision.Owner = self;
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
		Node3D parent = GetParent( parentID,name,node,physics,visual,dynamics,placeholders,nodes );

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
}

public class ACImportTrack(Node3D self) : ACImport( self )
{
	public override void Load( string acFolder,string track,string variant,bool loadTextures = true )
	{
#if DEBUG
		Debug debug = new Debug( );
		self.AddChild( debug );
		debug.Name = "Debug";
		debug.Owner = self;
#endif
		var physics = AddNode<StaticBody3D>( "Physics" );
		var visuals = AddNode<Node3D>( "Visuals" );
		var dynamics = AddNode<Node3D>( "Dynamics" );
		var placeholders = AddNode<Node3D>( "Placeholders" );

		
		List<Material> materials = [];
		try
		{
			var files = GetModelFiles( acFolder,track,variant );
			foreach( KeyValuePair<string,Vector3> file in files )
			{
				kn5Model? model = Kn5Import.readKN5( Path.Combine( acFolder,track,file.Key ) );
				if( model != null )
				{
					if( loadTextures )
					{
						materials = CreateMaterials( model );
#if DEBUG
						foreach( var material in materials )
						{
							//GD.Print(material.GetName(  ));
							debug.Shaders.Add( material );
						}
#endif
					}
					CreateMeshes( model,materials,physics,visuals,dynamics,placeholders );
				}
			}
		}
		catch( Exception e )
		{
			GD.Print( e );
			throw;
		}
	}

	private List<KeyValuePair<string,Vector3>> GetModelFiles( string acFolder,string track,string variant )
	{
		List<KeyValuePair<string,Vector3>> files = [];
				
		string modelsFile = Path.Combine( acFolder,track,"models.ini" );
		if( variant != "" )
			modelsFile = Path.Combine( acFolder,track,$"models_{variant}.ini" );
		if( File.Exists( modelsFile ) )
		{
			IniFile iniFile = new IniFile( modelsFile );
			for( int i = 0; i < 256; i++ )
			{
				string modelFile = iniFile.GetValue( "FILE",$"MODEL_{i}" );
				if( modelFile != string.Empty )
				{
					//GD.Print( modelFile );
					if( !iniFile.GetValue( "POSITION",$"MODEL_{i}" ).Equals( "0,0,0" ) )
						GD.PushWarning( $"{track}: model file '{modelFile}' position not 0,0,0" );
					files.Add( new KeyValuePair<string,Vector3>( modelFile,Vector3.Zero ) );
				}
			}
		}
		else
		{
			files.Add( new KeyValuePair<string,Vector3>( track+".kn5",Vector3.Zero ) );
		}
		return files;
	}
}

public class ACImportCar( Node3D self ) : ACImport( self )
{
	public override void Load( string acFolder,string carID,string skinID,bool loadTextures=true )
	{
		var physics = AddNode<StaticBody3D>( "Physics" );
		var visuals = AddNode<Node3D>( "Visuals" );
		var dynamics = AddNode<Node3D>( "Dynamics" );
		var placeholders = AddNode<Node3D>( "Placeholders" );
		
		try
		{
			string modelFile = Path.Combine( acFolder,carID,$"{carID.Replace( "ks_","" )}.kn5" );
			kn5Model model = Kn5Import.readKN5( modelFile );

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

	protected override Node3D GetParent( int parentID,string name,Node3D node,Node3D physics,Node3D visual,Node3D dynamics,Node3D placeholders,List<Node3D> nodes )
	{
		Node3D parent = base.GetParent( parentID,name,node,physics,visual,dynamics,placeholders,nodes );
		if( name.EndsWith( "_LIGHT" ) )
		{
			parent = dynamics;
			parent = GetNode( parent,"Lights" );
		}
		else if( name.EndsWith( "WHEEL_LF" ) ||
		         name.EndsWith( "WHEEL_RF" ) ||
		         name.EndsWith( "WHEEL_LR" ) ||
		         name.EndsWith( "WHEEL_RR" ) ||
		         name.EndsWith( "SUSP_LF" ) ||
		         name.EndsWith( "SUSP_RF" ) ||
		         name.EndsWith( "SUSP_LR" ) ||
		         name.EndsWith( "SUSP_RR" ) ||
		         name.StartsWith( "DISC_" ) )
		{
			parent = dynamics;
			parent = GetNode( parent,"Wheels" );

			if( name.EndsWith( "_LF" ) )
				parent = GetNode( parent,"LeftFront" );
			else if( name.EndsWith( "_RF" ) )
				parent = GetNode( parent,"RightFront" );
			else if( name.EndsWith( "_LR" ) )
				parent = GetNode( parent,"LeftRear" );
			else if( name.EndsWith( "_RR" ) )
				parent = GetNode( parent,"RightRear" );

			parent.Position = node.Position;
			node.Position -= parent.Position;
		}
		else if( name.StartsWith( "STEER_" ) )
		{
			parent = dynamics;
			parent = GetNode( parent,"Steerings" );

			if( name.EndsWith( "_LR" ) )
				parent = GetNode( parent,"LowRes" );
			else if( name.EndsWith( "_HR" ) )
				parent = GetNode( parent,"HighRes" );

			parent.Position = node.Position;
			node.Position -= parent.Position;
		}
		else if( name.StartsWith( "COCKPIT_" ) )
		{
			parent = dynamics;
			parent = GetNode( parent,"Cockpits" );

			if( name.EndsWith( "_LR" ) )
				parent = GetNode( parent,"LowRes" );
			else if( name.EndsWith( "_HR" ) )
				parent = GetNode( parent,"HighRes" );
		}
		else if( name.StartsWith( "DOOR_" ) || name.StartsWith( "INT_DOOR_" ) )
		{
			parent = dynamics;
			parent = GetNode( parent,"Doors" );

			if( name.EndsWith( "_R" ) || name.StartsWith( "DOOR_R" ) )
			{
				parent = GetNode( parent,"Right" );
				
				parent.Position = node.Position;
				node.Position -= parent.Position;
			}
			else if( name.EndsWith( "_L" ) || name.StartsWith( "DOOR_L" ) )
			{
				parent = GetNode( parent,"Left" );
				
				parent.Position = node.Position;
				node.Position -= parent.Position;
			}
		}
		else if( name.StartsWith( "WIPER_" ) )
		{
			parent = dynamics;
			parent = GetNode( parent,"Wipers" );
		}
		else if( name.EndsWith( "_BUMPER" ) )
		{
			parent = dynamics;
			parent = GetNode( parent,"Bumpers" );
		}
		else if( name.StartsWith( "DAMAGE_" ) )
		{
			parent = dynamics;
			parent = GetNode( parent,"Damages" );

			parent.Visible = false;
		}
		else if( name.StartsWith( "EXHAUST" ) )
		{
			parent = placeholders;
			parent = GetNode( parent,"Exhausts" );
		}
		else if( name.StartsWith( "FLYCAM_" ) )
		{
			parent = placeholders;
			parent = GetNode( parent,"Cameras" );
		}
		return parent;
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
