using Godot;
using System;
using System.Collections.Generic;
using BigMap;

using Mesh = Godot.Mesh;
using Array = Godot.Collections.Array;

[Tool]
public partial class BMTrack : Node3D
{
	public override void _Ready()
	{
		var track = GenerateTrack( );
		
		GenerateMeshes( track );
	}

	protected Track GenerateTrack()
	{
		CurveShape curveShape = new( );
		curveShape.Segments.Add( new Segment( new Point3D( 0,0,0 ),Segment.LerpMode.Cubic ) );
		curveShape.Segments.Add( new Segment( new Point3D( 0,50,0 ),Segment.LerpMode.Cubic ) );
		curveShape.Segments.Add( new Segment( new Point3D( 50,100,0 ),Segment.LerpMode.Cubic ) );

		Ground ground = new Ground( "",new BigMap.Material( "" ) );
		
		Sector sector = new Sector( "" );
		sector.Layout = curveShape;
		sector.LeftGrounds.Add( ground );
		sector.LeftGrounds.Add( ground );

		Track track = new( );
		track.Sectors.Add( sector );
		
		track.UpdatePoints();
		track.CreateMesh( );

		return track;
	}

	protected void GenerateMeshes( Track track )
	{
		MeshList meshList = track.GetMeshes( );
		MaterialList materialList = track.GetMaterials( );

		int n = 0;
		foreach( BigMap.Material material in materialList )
		{
			if( material is not null && material.Texture is not null )
			{
				material.Index = n++;

				//AddSceneMaterial( material.Name,material.Texture.Filename );
			}
		}
		//MeshList meshList = tTrack.CreateFBXMesh( );

		foreach( BigMap.Mesh mesh in meshList )
		{
			MeshD3D d3dMesh = mesh.CreateD3DMesh( 1 );
			
			MeshInstance3D meshInstance = new MeshInstance3D( )
			{
				Name = "1",
				Mesh = CreateSurface( d3dMesh,new ArrayMesh( ) ) 
			};
			AddChild(meshInstance);
			meshInstance.Owner = this;

			/*
			foreach( Material material in materialList )
				AddMeshMaterial( material.Name );

			AddMesh( "Treeee",
				fbxMesh.Vertices,fbxMesh.Vertices.Length,
				fbxMesh.Indices,fbxMesh.Indices.Length,
				fbxMesh.UVMAPFBXs,fbxMesh.UVMAPFBXs.Length,
				fbxMesh.MaterialIndices,fbxMesh.MaterialIndices.Length );
				*/
		}
	}

	private static ArrayMesh CreateSurface( MeshD3D mesh,ArrayMesh arrayMesh )
	{
		Array array = new Array( );
		array.Resize( (int)Mesh.ArrayType.Max );

		var vertices = new List<Vector3>( );
		foreach( var v in mesh.Vertices )
		{
			vertices.Add( new Vector3( v.x, v.z, v.y ) );
		}
		var indices = new List<int>( );
		foreach( var i in mesh.Indices )
		{
			indices.Add( (int)i );
		}
		array[(int)Mesh.ArrayType.Vertex] = vertices.ToArray( ).AsSpan( );
		array[(int)Mesh.ArrayType.Index] = indices.ToArray( ).AsSpan( );

		//array[(int)Mesh.ArrayType.Normal] = mesh.normals.ToArray( ).AsSpan( );
		//array[(int)Mesh.ArrayType.TexUV] = mesh.uvs.ToArray( ).AsSpan( );

		SurfaceTool surfaceTool = new SurfaceTool( );
		surfaceTool.CreateFromArrays( array );
		surfaceTool.GenerateNormals( false );
		//surfaceTool.GenerateTangents( );

		surfaceTool.Commit( arrayMesh );

		return arrayMesh;
	}
}
