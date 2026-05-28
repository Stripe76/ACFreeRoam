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
#if DEBUG
	private static List<string> _done_shaders = ["ksTree","ksPerPixel","ksPerPixelReflection","ksPerPixelMultiMap_NMDetail","ksPerPixelMultiMap","ksMultilayer_fresnel_nm",
		"ksMultilayer","ksMultilayer_objsp","ksFlags","ksPerPixelAT","ksGrass","ksPerPixelAlpha","ksPerPixelMultiMapSimpleRefl","ksWindscreen","ksPerPixelNM","ksTyres",
		"ksBrakeDisc","ksPerPixelMultiMap_damage_dirt","ksPerPixelAT_NM","ksBrokenGlass","ksPerPixelNM_UVMult","ksSkinnedMesh","GL"];
#endif
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

	protected Node3D AddNode<T>( string name, Node3D? parent = null ) where T : Node3D,new( )
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

	protected static List<Material> CreateMaterials( kn5Model model,ViewportTexture? mirror = null,Debug? debug = null)
	{
		List<Material> materials = [];
		SortedList<string,Image> images = [];
		
		foreach( var ksMaterial in model.materials )
		{
			var material = new ShaderMaterial( );
			material.SetName( $"{ksMaterial.name} ({ksMaterial.shader})" );
			material.SetShader( GetShader( ksMaterial ) );

			//var texName = ksMaterial.txDiffuse;
			foreach( var textureName in ksMaterial.textures.Keys )
			{
				string texValue = ksMaterial.textures[textureName];
				if( texValue.EndsWith( "mirror.dds",StringComparison.InvariantCultureIgnoreCase ) && mirror != null )
				{
					material.SetShaderParameter( textureName,mirror );
				}
				else
				{
					if( model.textures.TryGetValue( texValue,out kn5Texture? ksTexture ) )
					{
						if( !images.TryGetValue( texValue,out var texImage ) )
						{
							texImage = new Image( );
							if( texValue.EndsWith( ".dds",StringComparison.InvariantCultureIgnoreCase ) )
							{
								//GD.Print( $"{ksTexture.name} - {ksTexture.texData.Length}" );
								ksTexture.texData[28] = 0;

								texImage.LoadDdsFromBuffer( ksTexture.texData );
								if( texImage.IsCompressed( ) && textureName == "txDiffuse" )
								{
									//GD.Print( $"Mipmaps: {ksTexture.name}" );
									texImage.Decompress( );
									texImage.ResizeToPo2( false,Image.Interpolation.Trilinear );
									texImage.GenerateMipmaps( );
									texImage.Compress( Image.CompressMode.S3Tc );
								}
							}
							else if( texValue.EndsWith( ".png" ) )
							{
								GD.Print( $"Loading PNG: {texValue}" );
								texImage.LoadPngFromBuffer( ksTexture.texData );
								
								if( textureName == "txDiffuse" )
								{
									texImage.ResizeToPo2( false,Image.Interpolation.Trilinear );
									texImage.GenerateMipmaps( );
								}
							}
							else
							{
								GD.PushError( "Unsupported image file format: ",texValue );
							}
							images.Add( texValue,texImage );
						}
						material.SetShaderParameter( textureName,ImageTexture.CreateFromImage( texImage ) );
					}
				}
			}
			foreach( var param in ksMaterial.parameters.Keys )
			{
				if( param == "multA" && ksMaterial.parameters.Keys.Contains( "multA_B" ) && ksMaterial.parameters.Keys.Contains( "multA_C" ) )
				{
					float valueB = ksMaterial.parameters["multA_B"];
					float valueC = ksMaterial.parameters["multA_C"];

					material.SetShaderParameter( "multA",new Vector2( valueB,valueC ) );
				}
				else
				{
					float value = ksMaterial.parameters[param];
				
					material.SetShaderParameter( param,value );
				}
			}
			materials.Add( material );	
#if DEBUG
			if( debug != null )
			{
				//GD.Print(material.GetName(  ));
				var debugData = new Godot.Collections.Dictionary( );
				debugData["Name"] = ksMaterial.name;
				debugData["Shader"] = ksMaterial.shader;
				debugData["Params"] = ksMaterial.shaderProps;
				debugData["Material"] = material;
				debug.Shaders.Add( debugData );
			}
#endif
		}
		return materials;
	}

	private static Shader GetShader(kn5Material material)
	{
#if DEBUG
		/*
		if( !_done_shaders.Contains( material.shader ) )
		{
			_done_shaders.Add( material.shader );
		}
		*/
#endif
		switch( material.shader )
		{
			case "":
			case "GL":
			case "ksTreeAT":
			case "ksGrassAT":
			case "ksPerPixel":
			case "ksPerPixelAB":
			case "ksPerPixelAlpha":
			case "ksWindscreenAB":
			case "ksBrokenGlassAB":
			case "ksPerPixelAT":
			case "ksPerPixelAT_NM":
			case "ksPerPixelReflection":
			case "ksPerPixelReflectionAB":
			case "ksPerPixelMultiMap_NMDetail":
			case "ksPerPixelMultiMap_AT_NMDetail":
			case "ksPerPixelMultiMap":
			case "ksPerPixelMultiMap_AT":
			case "ksPerPixelMultiMap_damage":
			case "ksPerPixelSimpleRefl":
			case "ksPerPixelSimpleReflAB":
			case "ksMultilayer":
			case "ksMultilayer_objsp":
			case "ksMultilayer_fresnel_nm":
			case "ksFlags":
			case "ksPerPixelMultiMapSimpleRefl":
			case "ksPerPixelNM":
			case "ksPerPixelNMAB":
			case "ksTyres":
			case "ksBrakeDisc":
			case "ksPerPixelMultiMap_damage_dirt":
			case "ksPerPixelNM_UVMult":
			case "ksPerPixelNM_UVMultAB":
			case "ksSkinnedMesh":
			case "ksSkinnedMeshAB":
				return GD.Load<Shader>( $"res://shaders/kunos/{material.shader}.gdshader" );
			default:
				GD.PushError( $"-- Missing shader: {material.shader}" );
				break;
		}
		return GD.Load<Shader>( "res://shaders/base.gdshader" );
	}
	
	protected void CreateMeshes( kn5Model model,List<Material> materials,Node3D physics,Node3D visual,Node3D dynamics,Node3D markers,bool isCollider,bool recenterMeshes )
	{
		List<Node3D> nodes = [];
		foreach( kn5Node ksNode in model.nodes )
		{
			int materialID = ksNode.materialID;

			bool isPhysics = IsPhysicsNode( ksNode ) || isCollider;
			bool isVisual = materialID >= 0 && materialID < materials.Count && !isCollider;

			if( isPhysics && isVisual &&
			    (ksNode.name.Contains( "WALL" ) ||
			     model.materials[materialID].name == "wall" ||
			     model.materials[materialID].name == "barr" ||
			     model.materials[materialID].name == "barriers" ||
			     model.materials[materialID].name == "physics" ||
			     model.materials[materialID].name == "PHYSICS" ||
			     model.materials[materialID].name == "marshall" ||
			     model.materials[materialID].name == "marshalls" ) )
				isVisual = false;

			if( isVisual && ( model.materials[materialID].name == "top-ext" ||
							  model.materials[materialID].name == "barriers" ||
							  model.materials[materialID].name == "PHYSICS" ) )
				isVisual = false;
			
			if( isVisual && model.materials[materialID].shader == "ksGrassAT" )
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
				AddToParent( ksNode.parentID,ksNode.name,node3D,physics,visual,dynamics,markers,nodes );
			}
			else
			{
				if( ksNode.indices is { Length: > 0 } )
				{
					ACMesh mesh = new ACMesh( ksNode.name );
					AABBTracker aabb = new AABBTracker( );
					for( int i = 0; i < ksNode.indices.Length; i += 3 )
					{
						int index1 = ksNode.indices[i];
						int index2 = ksNode.indices[i + 2];
						int index3 = ksNode.indices[i + 1];

						mesh.vertices.Add( ksNode.position[index1] );
						mesh.vertices.Add( ksNode.position[index2] );
						mesh.vertices.Add( ksNode.position[index3] );

						aabb.AddVertex( ksNode.position[index1] );
						aabb.AddVertex( ksNode.position[index2] );
						aabb.AddVertex( ksNode.position[index3] );

						mesh.normals.Add( ksNode.normal[index1] );
						mesh.normals.Add( ksNode.normal[index2] );
						mesh.normals.Add( ksNode.normal[index3] );

						mesh.uvs.Add( ksNode.texture0[index1] );
						mesh.uvs.Add( ksNode.texture0[index2] );
						mesh.uvs.Add( ksNode.texture0[index3] );
					}
					Vector3 center = new Vector3( );
					if( recenterMeshes )
					{
						center = aabb.GetCenter( );
						for( int i = 0; i < mesh.vertices.Count; i++ )
						{
							mesh.vertices[i] = mesh.vertices[i] - center;
						}
					}
					MeshInstance3D meshInstance = new MeshInstance3D( )
					{
						Name = ksNode.name + $"_({ksNode.abyte},{ksNode.bbyte},{ksNode.cbyte},{ksNode.dbyte})",
						Mesh = CreateSurface( mesh,new ArrayMesh( ) ),
						//Position = center,
						Position = center + ksNode.translation,
						Rotation = ksNode.rotation,
						//Scale = ksNode.scaling
						//Layers = (recenterMeshes ? (uint)0b00000_00000_00000_00001 : (uint)0b00000_00000_00001_00001),
					};
					//meshInstance.Set( "resource_local_to_scene",true );
					
					if( isVisual )
					{
						((ArrayMesh)meshInstance.Mesh).SurfaceSetName( 0,$"{materialID}: {model.materials[materialID].name} ({model.materials[materialID].shader})" );
						meshInstance.Mesh.SurfaceSetMaterial( 0,materials[materialID] );
					}
					else
					{
						meshInstance.Visible = false;
					}
					if( isPhysics && ksNode.indices.Length > 0 )
					{
						//kn5Material ksMaterial = model.materials[ksNode.materialID];
						//GD.Print($"{ksMaterial.name}: {ksMaterial.shaderProps}");
						//GD.Print(ksNode.);

						CollisionShape3D collision = new CollisionShape3D( );
						collision.Name = ksNode.name + $"_({ksNode.abyte},{ksNode.bbyte},{ksNode.cbyte},{ksNode.dbyte})";
						collision.Position = center;
						if( isCollider )
							collision.SetShape( meshInstance.Mesh.CreateConvexShape( ) );
						else
							collision.SetShape( meshInstance.Mesh.CreateTrimeshShape( ) );
							
						physics.AddChild( collision );
						collision.Owner = self;
					}
					AddToParent( ksNode.parentID,ksNode.name,meshInstance,physics,visual,dynamics,markers,nodes );
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
	
	private void AddToParent( int parentID,string name,Node3D node,Node3D physics,Node3D visual,Node3D dynamics,Node3D markers,List<Node3D> nodes )
	{
		Node3D parent = GetParent( parentID,name,node,physics,visual,dynamics,markers,nodes );

		parent.AddChild( node );
		node.Owner = self;
				
		nodes.Add( node );
	}

	private static ArrayMesh CreateSurface( ACMesh mesh,ArrayMesh arrayMesh )
	{
		Array array = new Array( );
		array.Resize( (int)Mesh.ArrayType.Max );

		array[(int)Mesh.ArrayType.Vertex] = mesh.vertices.ToArray( ).AsSpan( );
		array[(int)Mesh.ArrayType.Normal] = mesh.normals.ToArray( ).AsSpan( );
		array[(int)Mesh.ArrayType.TexUV] = mesh.uvs.ToArray( ).AsSpan( );

		SurfaceTool surfaceTool = new SurfaceTool( );
		surfaceTool.CreateFromArrays( array );
		//surfaceTool.GenerateNormals( false );
		surfaceTool.GenerateTangents( );

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
		var markers = AddNode<Node3D>( "Markers" );

		physics.Visible = false;
		markers.Visible = false;
		dynamics.Visible = false;
		
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
						materials = CreateMaterials( model,null,debug );
					}
					CreateMeshes( model,materials,physics,visuals,dynamics,markers,false,true );
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

public class ACImportCar( Node3D self,ViewportTexture mirror ) : ACImport( self )
{
	public override void Load( string carFolder,string carModelFile,string skinID,bool loadTextures = true )
	{
#if DEBUG
		Debug debug = new Debug( );
		self.AddChild( debug );
		debug.Name = "Debug";
		debug.Owner = self;
#endif
		var physics = AddNode<Node3D>( "Physics" );
		var visuals = AddNode<Node3D>( "Visuals" );
		var dynamics = AddNode<Node3D>( "Dynamics" );
		var markers = AddNode<Node3D>( "Markers" );
		
		List<Material> materials = [];
		try
		{
			string modelFile = Path.Combine( carFolder,carModelFile );
			kn5Model? model = Kn5Import.readKN5( modelFile );
			if( model != null )
			{
				if( loadTextures )
					materials = CreateMaterials( model,mirror,debug );

				CreateMeshes( model,materials,physics,visuals,dynamics,markers,false,false );
			}
			string colliderFile = Path.Combine( carFolder,"collider.kn5" );
			kn5Model? collider = Kn5Import.readKN5( colliderFile );
			if( collider != null )
			{
				CreateMeshes( collider,materials,physics,visuals,dynamics,markers,true,false );
			}
		}
		catch( Exception e )
		{
			GD.PushError( e );
			throw;
		}
	}

	protected override Node3D GetParent( int parentID,string name,Node3D node,Node3D physics,Node3D visual,Node3D dynamics,Node3D placeholders,List<Node3D> nodes )
	{
		Node3D parent = base.GetParent( parentID,name,node,physics,visual,dynamics,placeholders,nodes );
		if( parentID > 0 && !name.StartsWith( "STEER_" ) && !name.StartsWith( "DAMAGE_" ) )
		{
			return parent;
		}
		else
		{
			node.Position += parent.Position;
		}
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


internal class AABBTracker( )
{
	public Vector3 Position = new Vector3(float.MaxValue,float.MaxValue,float.MaxValue);
	public Vector3 End = new Vector3(float.MinValue,float.MinValue,float.MinValue);

	public Vector3 GetCenter( )
	{
		return (Position + End) / 2.0f;
	}

	public void AddVertex( Vector3 v )
	{
		if( v.X < Position.X )
			Position.X = v.X;
		if( v.X > End.X )
			End.X = v.X;

		if( v.Y < Position.Y )
			Position.Y = v.Y;
		if( v.Y > End.Y )
			End.Y = v.Y;

		if( v.Z < Position.Z )
			Position.Z = v.Z;
		if( v.Z > End.Z )
			End.Z = v.Z;
	}
}