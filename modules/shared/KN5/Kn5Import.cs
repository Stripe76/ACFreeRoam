using Godot;
using System;
using System.IO;
using System.Collections.Generic;

namespace ACTracks.KN5
{
  public class kn5Model
  {
    public int version;

    public string modelDir;
    public string modelName;

    public readonly List<kn5Node> nodes = [];
    public readonly List<kn5Material> materials = [];
    public readonly SortedList<string,kn5Texture> textures = new ();
  }

  public class kn5Material
  {
    public string name = "Default";
    public string shader = "";
    
    public float ksAmbient = 0.6f;
    public float ksDiffuse = 0.6f;
    public float ksSpecular = 0.9f;
    public float ksSpecularEXP = 1.0f;
    public float diffuseMult = 1.0f;
    public float normalMult = 1.0f;
    public float useDetail = 0.0f;
    public float detailUVMultiplier = 1.0f;

    public string txDiffuse;
    public string txNormal;
    public string txDetail;

    public string shaderProps = "";
  }

  public class kn5Texture
  {
    public string name;
    
    public float UVScaling = 1.0f;
    
    public byte[] texData;
  }
  
  public class kn5Node
  {
    public int parentID = -1;
    public int materialID = -1;

    public int type = 1;
    public string name = "Default";

    public float[,] tmatrix = new float[4, 4] { { 1.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 1.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 1.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 1.0f } };
    public float[,] hmatrix = new float[4, 4] { { 1.0f, 0.0f, 0.0f, 0.0f }, { 0.0f, 1.0f, 0.0f, 0.0f }, { 0.0f, 0.0f, 1.0f, 0.0f }, { 0.0f, 0.0f, 0.0f, 1.0f } };

    public Vector3 translation = Vector3.Zero;
    public Vector3 rotation = Vector3.Zero;
    public Vector3 scaling = Vector3.Zero;

    public int vertexCount;
    
    public Vector3[] position;
    public Vector3[] normal;
    public Vector2[] texture0;

    public ushort[] indices;
  }

  public class Kn5Import
  {
    public static kn5Model? readKN5( string kn5File )
    {
      using( BinaryReader binStream = new BinaryReader( File.OpenRead( kn5File ) ) )
      {
        string magicNumber = ReadStr(binStream, 6);
        if( magicNumber == "sc6969" )
        {
          kn5Model newModel = new kn5Model();
          newModel.modelDir = Path.GetDirectoryName( kn5File );
          newModel.modelName = Path.GetFileNameWithoutExtension( kn5File );

          newModel.version = binStream.ReadInt32( );
          if( newModel.version > 5 )
          { int unknownNo = binStream.ReadInt32(); } //673425

          #region extract textures
          int texCount = binStream.ReadInt32();
          for( int t = 0; t < texCount; t++ )
          {
            int texType = binStream.ReadInt32();
            string texName = ReadStr(binStream, binStream.ReadInt32());
            int texSize = binStream.ReadInt32();
            byte[] texBuffer = binStream.ReadBytes(texSize);

            kn5Texture tex = new kn5Texture( )
            {
              name = texName,
              texData = texBuffer
            };
            newModel.textures[texName] = tex;
            /*
            using( BinaryWriter texWriter = new BinaryWriter( File.Create( Path.Join( newModel.modelDir,"textures",texName ) ) ) )
            {
              texWriter.Write( texBuffer );
            }
            */
          }
          #endregion

          #region read materials
          int matCount = binStream.ReadInt32();
          for( int m = 0; m < matCount; m++ )
          {
            kn5Material newMaterial = new kn5Material();
            newMaterial.name = ReadStr( binStream,binStream.ReadInt32( ) );
            newMaterial.shader = ReadStr( binStream,binStream.ReadInt32( ) );
            
            short ashort = binStream.ReadInt16();
            if( newModel.version > 4 )
            { int azero = binStream.ReadInt32(); }

            int propCount = binStream.ReadInt32();
            for( int p = 0; p < propCount; p++ )
            {
              string propName = ReadStr( binStream,binStream.ReadInt32( ) );
              float propValue = binStream.ReadSingle( );
              newMaterial.shaderProps += propName + " = " + propValue.ToString( ) + "; ";

              switch( propName )
              {
                case "ksAmbient":
                  newMaterial.ksAmbient = propValue;
                  break;
                case "ksDiffuse":
                  newMaterial.ksDiffuse = propValue;
                  break;
                case "ksSpecular":
                  newMaterial.ksSpecular = propValue;
                  break;
                case "ksSpecularEXP":
                  newMaterial.ksSpecularEXP = propValue;
                  break;
                case "diffuseMult":
                  newMaterial.diffuseMult = propValue;
                  break;
                case "normalMult":
                  newMaterial.normalMult = propValue;
                  break;
                case "useDetail":
                  newMaterial.useDetail = propValue;
                  break;
                case "detailUVMultiplier":
                  newMaterial.detailUVMultiplier = propValue;
                  break;
              }
              binStream.BaseStream.Position += 36;
            }

            int textures = binStream.ReadInt32();
            for( int t = 0; t < textures; t++ )
            {
              string sampleName = ReadStr(binStream, binStream.ReadInt32());
              int sampleSlot = binStream.ReadInt32();
              string texName = ReadStr(binStream, binStream.ReadInt32());

              newMaterial.shaderProps += sampleName + " = " + texName + "&cr;&lf;";

              switch( sampleName )
              {
                case "txDiffuse":
                  newMaterial.txDiffuse = texName;
                  break;
                case "txNormal":
                  newMaterial.txNormal = texName;
                  break;
                case "txDetail":
                  newMaterial.txDetail = texName;
                  break;
              }
            }
            newModel.materials.Add( newMaterial );
          }
          #endregion

          (long start,long children) = readNodes( binStream,newModel.nodes,-1 ); //recursive

          return newModel;
        }
        else
        {
          Console.WriteLine( "Unknown file type." );
          return null;
        }
      }
    }

    private static (long start,long children) readNodes( BinaryReader modelStream,List<kn5Node> nodeList,int parentID )
    {
      long start = modelStream.BaseStream.Position;

      kn5Node newNode = new ()
      {
        parentID = parentID,
        type = modelStream.ReadInt32( ),
        name = ReadStr( modelStream,modelStream.ReadInt32( ) )
      };
      long children = modelStream.BaseStream.Position;

      int childrenCount = modelStream.ReadInt32();
      byte abyte = modelStream.ReadByte();

      switch( newNode.type )
      {
        #region dummy node
        case 1: //dummy
        {
          newNode.tmatrix[0,0] = modelStream.ReadSingle( );
          newNode.tmatrix[0,1] = modelStream.ReadSingle( );
          newNode.tmatrix[0,2] = modelStream.ReadSingle( );
          newNode.tmatrix[0,3] = modelStream.ReadSingle( );
          newNode.tmatrix[1,0] = modelStream.ReadSingle( );
          newNode.tmatrix[1,1] = modelStream.ReadSingle( );
          newNode.tmatrix[1,2] = modelStream.ReadSingle( );
          newNode.tmatrix[1,3] = modelStream.ReadSingle( );
          newNode.tmatrix[2,0] = modelStream.ReadSingle( );
          newNode.tmatrix[2,1] = modelStream.ReadSingle( );
          newNode.tmatrix[2,2] = modelStream.ReadSingle( );
          newNode.tmatrix[2,3] = modelStream.ReadSingle( );
          newNode.tmatrix[3,0] = modelStream.ReadSingle( );
          newNode.tmatrix[3,1] = modelStream.ReadSingle( );
          newNode.tmatrix[3,2] = modelStream.ReadSingle( );
          newNode.tmatrix[3,3] = modelStream.ReadSingle( );

          newNode.translation = new Vector3( newNode.tmatrix[3,0],newNode.tmatrix[3,1],newNode.tmatrix[3,2] );
          newNode.rotation = MatrixToEuler( newNode.tmatrix );
          newNode.scaling = ScaleFromMatrix( newNode.tmatrix );

          break;
        }
        #endregion
        #region mesh node
        case 2: //mesh
        {
          byte bbyte = modelStream.ReadByte();
          byte cbyte = modelStream.ReadByte();
          byte dbyte = modelStream.ReadByte();

          newNode.vertexCount = modelStream.ReadInt32( );
          newNode.position = new Vector3[newNode.vertexCount];
          newNode.normal = new Vector3[newNode.vertexCount];
          newNode.texture0 = new Vector2[newNode.vertexCount];

          for( int v = 0; v < newNode.vertexCount; v++ )
          {
            newNode.position[v] = new Vector3( modelStream.ReadSingle( ),modelStream.ReadSingle( ),modelStream.ReadSingle( ) );
            newNode.normal[v] = new Vector3( modelStream.ReadSingle( ),modelStream.ReadSingle( ),modelStream.ReadSingle( ) );
            newNode.texture0[v] = new Vector2( modelStream.ReadSingle( ),modelStream.ReadSingle( ) );

            modelStream.BaseStream.Position += 12; //tangents
          }

          int indexCount = modelStream.ReadInt32();
          newNode.indices = new ushort[indexCount];
          for( int i = 0; i < indexCount; i++ )
          {
            newNode.indices[i] = modelStream.ReadUInt16( );
          }

          newNode.materialID = modelStream.ReadInt32( );
          modelStream.BaseStream.Position += 29;

          break;
        }
        #endregion
        #region animated mesh
        case 3: //animated mesh
        {
          byte bbyte = modelStream.ReadByte();
          byte cbyte = modelStream.ReadByte();
          byte dbyte = modelStream.ReadByte();

          int boneCount = modelStream.ReadInt32();
          for( int b = 0; b < boneCount; b++ )
          {
            string boneName = ReadStr(modelStream, modelStream.ReadInt32());
            modelStream.BaseStream.Position += 64; //transformation matrix
          }

          newNode.vertexCount = modelStream.ReadInt32( );
          newNode.position = new Vector3[newNode.vertexCount];
          newNode.normal = new Vector3[newNode.vertexCount];
          newNode.texture0 = new Vector2[newNode.vertexCount];

          for( int v = 0; v < newNode.vertexCount; v++ )
          {
            newNode.position[v] = new Vector3( modelStream.ReadSingle( ),modelStream.ReadSingle( ),modelStream.ReadSingle( ) );
            newNode.normal[v] = new Vector3( modelStream.ReadSingle( ),modelStream.ReadSingle( ),modelStream.ReadSingle( ) );
            newNode.texture0[v] = new Vector2( modelStream.ReadSingle( ),modelStream.ReadSingle( ) );

            modelStream.BaseStream.Position += 44; //tangents & weights
          }
          int indexCount = modelStream.ReadInt32();
          newNode.indices = new ushort[indexCount];
          for( int i = 0; i < indexCount; i++ )
          {
            newNode.indices[i] = modelStream.ReadUInt16( );
          }
          newNode.materialID = modelStream.ReadInt32( );
          modelStream.BaseStream.Position += 12;

          break;
        }
        #endregion
      }

      if( parentID < 0 )
      {
        newNode.hmatrix = newNode.tmatrix;
      }
      else
      {
        newNode.hmatrix = matrixMult( newNode.hmatrix,nodeList[parentID].hmatrix );
      }

      nodeList.Add( newNode );
      int currentID = nodeList.IndexOf(newNode);

      for( int c = 0; c < childrenCount; c++ )
      {
        readNodes( modelStream,nodeList,currentID );
      }
      return (start,children);
    }

    private static float[,] matrixMult( float[,] ma,float[,] mb )
    {
      float[,] mm = new float[4, 4];

      for( int i = 0; i < 4; i++ )
      {
        for( int j = 0; j < 4; j++ )
        {
          mm[i,j] = ma[i,0] * mb[0,j] + ma[i,1] * mb[1,j] + ma[i,2] * mb[2,j] + ma[i,3] * mb[3,j];
        }
      }

      /*
      mm[0, 0] = ma00*mb00 + ma01*mb10 + ma02*mb20 + ma03*mb30
      mm[0, 1] = ma00*mb01 + ma01*mb11 + ma02*mb21 + ma03*mb31
      mm[0, 2] = ma00*mb02 + ma01*mb12 + ma02*mb22 + ma03*mb32
      mm[0, 3] = ma00*mb03 + ma01*mb13 + ma02*mb23 + ma03*mb33

      mm[1, 1] = ma10*mb00 + ma11*mb10 + ma12*mb20 + ma13*mb30
      mm[1, 1] = ma10*mb01 + ma11*mb11 + ma12*mb21 + ma13*mb31
      mm[1, 2] = ma10*mb02 + ma11*mb12 + ma12*mb22 + ma13*mb32
      mm[1, 3] = ma10*mb03 + ma11*mb13 + ma12*mb23 + ma13*mb33

      mm[2, 0] = ma20*mb00 + ma21*mb10 + ma22*mb20 + ma23*mb30
      mm[2, 1] = ma20*mb01 + ma21*mb11 + ma22*mb21 + ma23*mb31
      mm[2, 2] = ma20*mb02 + ma21*mb12 + ma22*mb22 + ma23*mb32
      mm[2, 3] = ma20*mb03 + ma21*mb13 + ma22*mb23 + ma23*mb33

      mm[3, 0] = ma30*mb00 + ma31*mb10 + ma32*mb20 + ma33*mb30
      mm[3, 1] = ma30*mb01 + ma31*mb11 + ma32*mb21 + ma33*mb31
      mm[3, 2] = ma30*mb02 + ma31*mb12 + ma32*mb22 + ma33*mb32
      mm[3, 3] = ma30*mb03 + ma31*mb13 + ma32*mb23 + ma33*mb33*/

      return mm;
    }

    private static Vector3 MatrixToEuler( float[,] transf )
    {
      double heading = 0;
      double attitude = 0;
      double bank = 0;
      //original code by Martin John Baker for right-handed coordinate system
      /*if (transf[0, 1] > 0.998)
      { // singularity at north pole
          heading = Math.Atan2(transf[0, 2], transf[2, 2]);
          attitude = Math.PI / 2;
          bank = 0;
      }
      if (transf[0, 1] < -0.998)
      { // singularity at south pole
          heading = Math.Atan2(transf[0, 2], transf[2, 2]);
          attitude = -Math.PI / 2;
          bank = 0;
      }

      heading = Math.Atan2(-transf[2, 0], transf[0, 0]);
      bank = Math.Atan2(-transf[1, 2], transf[1, 1]);
      attitude = Math.Asin(transf[1, 0]);*/

      //left handed
      if( transf[0,1] > 0.998 )
      { // singularity at north pole
        heading = Math.Atan2( -transf[1,0],transf[1,1] );
        attitude = -Math.PI / 2;
        bank = 0;
      }
      else if( transf[0,1] < -0.998 )
      { // singularity at south pole
        heading = Math.Atan2( -transf[1,0],transf[1,1] );
        attitude = Math.PI / 2;
        bank = 0;
      }
      else
      {
        heading = Math.Atan2( transf[0,1],transf[0,0] );
        bank = Math.Atan2( transf[1,2],transf[2,2] );
        attitude = Math.Asin( -transf[0,2] );
      }


      //alternative code by Mike Day, Insomniac Games
      /*bank = Math.Atan2(transf[1, 2], transf[2, 2]);

      double c2 = Math.Sqrt(transf[0, 0] * transf[0, 0] + transf[0, 1] * transf[0, 1]);
      attitude = Math.Atan2(-transf[0, 2], c2);

      double s1 = Math.Sin(bank);
      double c1 = Math.Cos(bank);
      heading = Math.Atan2(s1 * transf[2, 0] - c1 * transf[1, 0], c1 * transf[1, 1] - s1 * transf[2, 1]);*/

      /*
      attitude *= 180 / Math.PI;
      heading *= 180 / Math.PI;
      bank *= 180 / Math.PI;
      */

      return new Vector3( (float)bank,(float)attitude,(float)heading );
    }

    private static Vector3 ScaleFromMatrix( float[,] transf )
    {
      double scaleX = Math.Sqrt(transf[0, 0] * transf[0, 0] + transf[1, 0] * transf[1, 0] + transf[2, 0] * transf[2, 0]);
      double scaleY = Math.Sqrt(transf[0, 1] * transf[0, 1] + transf[1, 1] * transf[1, 1] + transf[2, 1] * transf[2, 1]);
      double scaleZ = Math.Sqrt(transf[0, 2] * transf[0, 2] + transf[1, 2] * transf[1, 2] + transf[2, 2] * transf[2, 2]);

      return new Vector3( (float)scaleX,(float)scaleY,(float)scaleZ );
    }

    private static string ReadStr( BinaryReader str,int len )
    {
      //int len = str.ReadInt32();
      byte[] stringData = new byte[len];
      str.Read( stringData,0,len );
      var result = System.Text.Encoding.UTF8.GetString(stringData);
      return result;
    }

    public static void writeNode( BinaryWriter modelStream,kn5Node newNode )
    {
      modelStream.Write( newNode.type );
      //modelStream.Write( newNode.name.Length );
      modelStream.Write( newNode.name );

      modelStream.Write( 0 );
      modelStream.Write( (byte)0 );

      switch( newNode.type )
      {
        #region dummy node
        case 1: //dummy
        {
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );
          modelStream.Write( (Single)0 );

          break;
        }
        #endregion
      }
    }
  }
}