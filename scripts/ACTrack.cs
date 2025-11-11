using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Pfim;
using Pfim.dds;
using KN5;
using Array = Godot.Collections.Array;

[Tool]
public partial class ACTrack : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LoadTrack( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/imola/imola.kn5" );
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process( double delta )
	{
	}

	public void LoadTrack( string filename )
	{
		//try
		{
			if( GetNode( "Track" ) is MeshInstance3D mesh )
			{
				kn5Model model = Kn5Import.readKN5( filename );

				CreateMesh( model,mesh,filename );
			}
		}
		/*
		catch( Exception e )
		{
			Console.WriteLine( e );
			throw;
		}
	*/
	}

	private ArrayMesh CreateMesh( kn5Model model,MeshInstance3D meshInstance,string filename )
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

					if( node.translation.Length > 2 )
					{
						mesh.vertices.Add( new Vector3( node.translation[0] + node.position[index1 * 3],node.translation[1] + node.position[index1 * 3 + 1],node.translation[2] + node.position[index1 * 3 + 2] ) );
						mesh.vertices.Add( new Vector3( node.translation[0] + node.position[index2 * 3],node.translation[1] + node.position[index2 * 3 + 1],node.translation[2] + node.position[index2 * 3 + 2] ) );
						mesh.vertices.Add( new Vector3( node.translation[0] + node.position[index3 * 3],node.translation[1] + node.position[index3 * 3 + 1],node.translation[2] + node.position[index3 * 3 + 2] ) );
					}
					else
					{
						mesh.vertices.Add( new Vector3( node.position[index1 * 3],node.position[index1 * 3 + 1],node.position[index1 * 3 + 2] ) );
						mesh.vertices.Add( new Vector3( node.position[index2 * 3],node.position[index2 * 3 + 1],node.position[index2 * 3 + 2] ) );
						mesh.vertices.Add( new Vector3( node.position[index3 * 3],node.position[index3 * 3 + 1],node.position[index3 * 3 + 2] ) );
					}
					mesh.uvs.Add(new Vector2( node.texture0[index1*2],1-node.texture0[index1*2+1] ));
					mesh.uvs.Add(new Vector2( node.texture0[index2*2],1-node.texture0[index2*2+1] ));
					mesh.uvs.Add(new Vector2( node.texture0[index3*2],1-node.texture0[index3*2+1] ));
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
