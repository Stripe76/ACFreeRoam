using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using static BigMap.MeshPointList;

namespace BigMap;

#region Points
public class Point
{
  public double X;
  public double Y;

  public Point()
  {
    
  }
  public Point(double x, double y)
  {
    X = x;
    Y = y;
  }
}

public class Point3D
{
  public double X;
  public double Y;
  public double Z;

  public Point3D( )
  {
  }
  public Point3D( Point3D pt)
  {
    X = pt.X;
    Y = pt.Y;
    Z = pt.Z;
  }
  public Point3D( Point pt )
  {
    X = pt.X;
    Y = pt.Y;
    Z = 0;
  }
  public Point3D( double x,double y,double z )
  {
    X = x;
    Y = y;
    Z = z;
  }
}

public class Point3DList : List<Point3D>
{
  public Point3DList( )
  {
  }
  public Point3DList( int nCount ) : base( nCount )
  {
  }
}

public class Vector
{
  public double X;
  public double Y;

  public Vector()
  {
    
  }
  public Vector(double x, double y)
  {
    X = x;
    Y = y;
  }

  public void Normalize()
  {
    double distance = Math.Sqrt( X * X + Y * Y );

    X = X / distance;
    Y = Y / distance;
  }
}

public class SegmentPoint( Point3D ptPoint )
{
  public Point3D Point = ptPoint;
}

public class SegmentPointList : List<SegmentPoint>
{
}

public class CurvePoint
{
  public Point Point;
  public Vector Vector;

  public CurvePoint( Point ptPoint )
  {
    Point = new( ptPoint.X,ptPoint.Y );
    Vector = Helpers.VectorZero;
  }
  public CurvePoint( Point ptPoint,double direction )
  {
    Point = new ( ptPoint.X,ptPoint.Y );
    Vector = new( Math.Cos( direction ),Math.Sin( direction ) );
  }
  public CurvePoint( Point ptPoint,Vector vVector )
  {
    Point = new( ptPoint.X,ptPoint.Y );
    Vector = vVector;
  }
  public CurvePoint( Point3D ptPoint )
  {
    Point = new( ptPoint.X,ptPoint.Y );
    Vector = Helpers.VectorZero;
  }
}

public class CurvePointList : List<CurvePoint>
{
  public CurvePointList( )
  { 
  }
  public CurvePointList( CurvePointList arPoints,double dWidth ) : base( arPoints.Count )
  {
    foreach( CurvePoint pt in arPoints )
      Add( new CurvePoint( Helpers.Forward( pt.Point,pt.Vector,dWidth ),pt.Vector ) );
  }
  public CurvePointList( CurvePointList arPoints,Direction nDirection ) : base( arPoints.Count )
  {
    if( nDirection == Direction.Forward )
    {
      foreach( CurvePoint pt in arPoints )
        Add( new CurvePoint( pt.Point,pt.Vector ) );
    }
    else if( nDirection == Direction.Backward )
    {
      foreach( CurvePoint pt in arPoints )
        Add( new CurvePoint( pt.Point,Helpers.Backward( pt.Vector ) ) );
    }
    else if( nDirection == Direction.Left )
    {
      foreach( CurvePoint pt in arPoints )
        Add( new CurvePoint( pt.Point,Helpers.Left( pt.Vector ) ) );
    }
    else if( nDirection == Direction.Right )
    {
      foreach( CurvePoint pt in arPoints )
        Add( new CurvePoint( pt.Point,Helpers.Right( pt.Vector ) ) );
    }
  }
  public CurvePointList( CurvePointList arPoints,double nWidth,double dMinDistance,double dMaxDistance ) : this( arPoints,nWidth )
  {
    UpdateDetails( dMinDistance,dMaxDistance );
  }

  private void UpdateDetails( double dMinDetails,double dMaxDetails )
  {
    CurvePointList arRemove = [];

    dMinDetails *= dMinDetails;
    dMaxDetails *= dMaxDetails;

    int nCount = Count;
    for( int c = 0; c < nCount - 1; c++ )
    {
      double dDistance = Helpers.DistanceSqrd( this[c].Point,this[c+1].Point );

      int nHowMany = 0;
      while( dDistance < dMinDetails && (c + nHowMany + 2) < nCount )
      {
        nHowMany++;

        dDistance = Helpers.DistanceSqrd( this[c].Point,this[c + nHowMany + 1].Point );
      }
      if( nHowMany > 0 )
      {
        while( nHowMany-- > 0 )
        {
          RemoveAt( c + 1 );
        }
        nCount = Count;
      }
    }
    nCount = Count;
    for( int i = 0; i < nCount - 1; i++ )
    {
      double dDistance = Helpers.DistanceSqrd( this[i].Point,this[i+1].Point );
      if( dDistance > dMaxDetails )
      {
        CurvePoint ptNewPoint = new CurvePoint( Helpers.Middle( this[i].Point,this[i+1].Point ) );
        this.Insert( i + 1,ptNewPoint );

        i++;
        nCount++;
      }
      else
      {
        if( this[i] == this[i + 1] )
          arRemove.Add( this[i + 1] );
      }
    }
    foreach( CurvePoint pt in arRemove )
      Remove( pt );
  }
}

public class MeshPoint( int id, Point3D point,Vector vVector )
{
  public int ID = id;

  public GeoPoint Point = new( point );
  public UVPoint UVPoint = new( 0,0 );
  public Vector Vector = vVector;

  public int Index { get { return Point.Index; } }
}

public class MeshPointList : List<MeshPoint>
{
  public enum Direction
  {
    Forward,
    Backward,
    Left,
    Right,
  }

  public MeshPointList( )
  { 
  }
  public MeshPointList( Curve cCurve,Direction nDirection,double dMinDistance,double dMaxDistance ) : base( cCurve.Points.Count )
  {
    int nID = 0;
    if( nDirection == Direction.Forward )
    {
      foreach( CurvePoint pt in cCurve.Points )
        Add( new MeshPoint( nID++,new( pt.Point ),pt.Vector ) );
    }
    else if( nDirection == Direction.Backward )
    {
      foreach( CurvePoint pt in cCurve.Points )
        Add( new MeshPoint( nID++,new( pt.Point ),Helpers.Backward( pt.Vector ) ) );
    }
    else if( nDirection == Direction.Left )
    {
      foreach( CurvePoint pt in cCurve.Points )
        Add( new MeshPoint( nID++,new( pt.Point ),Helpers.Left( pt.Vector ) ) );
    }
    else if( nDirection == Direction.Right )
    {
      foreach( CurvePoint pt in cCurve.Points )
        Add( new MeshPoint( nID++,new( pt.Point ),Helpers.Right( pt.Vector ) ) );
    }
    UpdateDetails( dMinDistance,dMaxDistance );
  }

  public MeshPointList( MeshPointList arPoints, double dWidth ) : base( arPoints.Count )
  {
    foreach( MeshPoint pt in arPoints )
      Add( new MeshPoint( pt.ID,Helpers.Forward( pt.Point,pt.Vector,dWidth ),pt.Vector  ) );
  }

  public MeshPointList( MeshPointList arPoints, Vector yzVector ) : base( arPoints.Count )
  {
    foreach( MeshPoint pt in arPoints )
      Add( new MeshPoint( pt.ID,Helpers.Forward( pt.Point,pt.Vector,yzVector ),pt.Vector ) );
  }
  public MeshPointList( MeshPointList arPoints,Vector yzVector,double dMinDistance,double dMaxDistance ) : this( arPoints,yzVector )
  {
    UpdateDetails( dMinDistance,dMaxDistance );
  }

  public MeshPointList( MeshPointList arPoints, double nWidth,double dMinDistance,double dMaxDistance ) : this( arPoints,nWidth )
  {
    UpdateDetails( dMinDistance,dMaxDistance );
  }

  public int CountPoint( int nStart, int nMax = 1000 )
  {
    if( nStart >= 0 && nStart < Count )
    {
      int nCount = 1;
      int nID = this[nStart++].ID;
      while( nCount < nMax && nStart < Count && this[nStart++].ID == nID )
        nCount++;
      return nCount;
    }
    return 0;
  }

  private void UpdateDetails( double dMinDetails,double dMaxDetails )
  {
    MeshPointList arRemove = [];

    dMinDetails *= dMinDetails;
    dMaxDetails *= dMaxDetails;

    int nCount = Count;
    for( int c = 0; c < nCount - 1; c++ )
    {
      double dDistance = Helpers.DistanceSqrd( this[c].Point,this[c+1].Point );

      int nHowMany = 0;
      while( dDistance < dMinDetails && (c + nHowMany + 2) < nCount )
      {
        nHowMany++;

        dDistance = Helpers.DistanceSqrd( this[c].Point,this[c + nHowMany + 1].Point );
      }
      if( nHowMany > 0 )
      {
        while( nHowMany-- > 0 )
        {
          RemoveAt( c + 1 );
        }
        nCount = Count;
      }
    }
    nCount = Count;
    for( int i = 0; i < nCount - 1; i++ )
    {
      double dDistance = Helpers.DistanceSqrd( this[i].Point,this[i+1].Point );
      if( dDistance > dMaxDetails )
      {
        MeshPoint ptNewPoint = Helpers.Middle( this[i],this[i+1] );
        this.Insert( i + 1,ptNewPoint );

        i++;
        nCount++;
      }
      else
      {
        if( this[i] == this[i + 1] )
          arRemove.Add( this[i + 1] );
      }
    }
    foreach( MeshPoint pt in arRemove )
      Remove( pt );
  }
}

public class MeshPointNullableList : List<MeshPoint?>
{
}

public class GeoPoint
{
  public int Index = -1;

  public double X;
  public double Y;
  public double Z;

  public GeoPoint( double x,double y,double z )
  {
    X = x;
    Y = y;
    Z = z;
  }
  public GeoPoint( Point3D pt )
  {
    X = pt.X;
    Y = pt.Y;
    Z = pt.Z;
  }

  public GeoPoint SetIndex( int nIndex )
  {
    Index = nIndex;

    return this;
  }
}

public class UVPoint( double u, double v )
{
  public int Index = -1;

  public double U = u;
  public double V = v;
}
#endregion

#region Segment
public class Segment : IComparable<Segment>
{
  public enum LerpMode
  {
    None,
    Linear,
    Cubic
  };
  public LerpMode Mode = LerpMode.Cubic;

  public Point3D Point;
  public int CurveIndex;

  public Segment( Point3D point, LerpMode mode )
  {
    Point = point;
    Mode = mode;
  }

  public int CompareTo( Segment? other )
  {
    if( other == null )
      return 1;
    return (int)(Point.X - other.Point.X);
  }
}

public class SegmentList : List<Segment>
{
  public SegmentList( )
  {
  }

  public double GetLength( )
  {
    double dLength = 0;
    for( int i = 0; i < Count - 1; i++ )
      dLength += Helpers.Distance( this[i].Point,this[i + 1].Point );
    return dLength;
  }
}
#endregion

#region Curve
public class Curve
{
  public CurvePointList Points { get; set; }

  public Curve( )
  {
    Points = [];
  }
  public Curve( Curve cCurve,Direction nDirection )
  {
    Points = new CurvePointList( cCurve.Points,nDirection );
  }
  public Curve( Curve cCurve,double nWidth,double dMinDistance,double dMaxDistance )
  {
    Points = new CurvePointList( cCurve.Points,nWidth,dMinDistance,dMaxDistance );
  }

  public Curve( SegmentList arSegments,double dDistance )
  {
    Points = [];

    FromSegments( arSegments,dDistance );
  }

  public Curve Begin( )
  {
    Points.Add( new( new( 0,0 ),0 ) );

    return this;
  }
  public Curve AddPoint( Point pt )
  {
    Points.Add( new CurvePoint( pt,0 ) );

    return this;
  }
  public Curve AddPoint( double dAngleDegrees,double dDistance )
  {
    if( Points.Count > 0 )
      Points.Add( new( Helpers.Forward( Points[^1].Point,Helpers.Radians( dAngleDegrees ),dDistance ),Helpers.Radians( dAngleDegrees ) ) );
    return this;
  }

  public void FromSegments( SegmentList arSegments, double dDistance )
  {
    Points.Clear( );

    for( int n = 0; n < arSegments.Count; n++ )
    {
      Segment sCurrent = arSegments[n];
      if( sCurrent.Mode == Segment.LerpMode.None )
      {
        sCurrent.CurveIndex = Points.Count;

        Points.Add( new CurvePoint( sCurrent.Point ) );
      }
      else if( sCurrent.Mode == Segment.LerpMode.Linear )
      {
        sCurrent.CurveIndex = Points.Count;

        if( n > 0 )
        {
          FromSegmentsLinear( arSegments[n],arSegments[n - 1],dDistance );
        }
        else
        {
          Points.Add( new CurvePoint( sCurrent.Point ) );
        }
      }
      else if( sCurrent.Mode == Segment.LerpMode.Cubic )
      {
        sCurrent.CurveIndex = Points.Count;

        if( n < arSegments.Count-1 )
        {
          SegmentList arInterpolate = [];
          arInterpolate.Add( sCurrent );

          while( ++n < arSegments.Count &&  arSegments[n].Mode == Segment.LerpMode.Cubic )
            arInterpolate.Add( arSegments[n] );
          n--;

          if( arInterpolate.Count > 1 )
            FromSegmentsInterpolate( arInterpolate,dDistance );
          else
            FromSegmentsLinear( arSegments[n],arSegments[n + 1],dDistance );
        }
        else
        {
          Points.Add( new CurvePoint( sCurrent.Point ) );
        }
      }
    }
    UpdateVectors( );
  }

  private void FromSegmentsLinear( Segment sCurrent,Segment sPrevious,double dDistance )
  {
    Point ptStart = new ( sPrevious.Point.X,sPrevious.Point.Y );
    Point ptSize = new( sCurrent.Point.X - sPrevious.Point.X,sCurrent.Point.Y - sPrevious.Point.Y );

    double dLength = Helpers.Distance( sCurrent.Point,sPrevious.Point );
    int nPointsCount = (int)(dLength / dDistance);
    for( int i = 0; i < nPointsCount; i++ )
    {
      double d = ((double)i)/nPointsCount;

      Points.Add( new CurvePoint( new Point( ptStart.X + ptSize.X * d,ptStart.Y + ptSize.Y * d ) ) );
    }
    //sCurrent.CurveIndex = Points.Count;
  }
  private void FromSegmentsInterpolate( SegmentList arSegments,double dDistance )
  {
    int nIndex = Points.Count;
    double dLength = 0;
    for( int i = 0; i < arSegments.Count - 1; i++ )
    {
      arSegments[i].CurveIndex = nIndex;

      double l = Helpers.Distance( arSegments[i].Point,arSegments[i + 1].Point );

      nIndex += (int)(l / dDistance);
      dLength += l;
    }
    arSegments[^1].CurveIndex = nIndex;

    int nPointsCount = (int)(dLength / dDistance);

    Point3DList? arPoints = Helpers.Interpolate( arSegments,nPointsCount );
    if( arPoints != null )
    {
      for( int i = 0; i < arPoints.Count; i++ )
      {
        Points.Add( new CurvePoint( arPoints[i] ) );
      }
    }
  }

  private void UpdateVectors( )
  {
    int nPointsCount = Points.Count;
    for( int i = 0; i < nPointsCount; i++ )
    {
      if( i < nPointsCount - 1 )
        Points[i].Vector = Helpers.Direction( Points[i].Point,Points[i + 1].Point );
      else if( i > 1 )
        Points[i].Vector = Helpers.Direction( Points[i - 1].Point,Points[i].Point );
    }
  }
}

public class CurveList : List<Curve>
{
}
#endregion

#region Curve shape
public class CurveShape
{
  public SegmentList Segments = [];
  public Curve Curve = new ( );
  public double Detail = 1;

  public CurveShape( )
  {
  }

  public void UpdatePoints( )
  {
    Curve.FromSegments( Segments,Detail );
  }

  public int FindNearestPointIndex( Point ptPoint )
  {
    int nIndex = 0;
    double dMinDistance = double.MaxValue;
    for( int i = 0; i < Curve.Points.Count; i++ )
    {
      double dDistance = Helpers.Distance( ptPoint,Curve.Points[i].Point );
      if( dDistance < dMinDistance )
      {
        nIndex = i;

        dMinDistance = dDistance;
      }
    }
    return nIndex;
  }
}
#endregion

#region Track
public class Track
{
  public SectorList Sectors = [];
  public HeightmapList Heightmaps = [];

  private int m_SectorID = 0;

  public Track( )
  {
  }

  public void UpdatePoints( )
  {
    foreach( Sector sSector in Sectors )
    {
      sSector.UpdatePoints( );
    }
  }

  public void CreateMesh( )
  {
    foreach( Sector sSector in Sectors )
    {
      sSector.CreateMesh( );
    }
  }

  public MeshList GetMeshes( )
  {
    MeshList meshList = [];
    foreach( Sector sSector in Sectors )
    {
      meshList.AddRange( sSector.GetMeshes( ) );
    }
    return meshList;
  }
  public ObjectList GetObjects( )
  {
    ObjectList meshList = [];
    foreach( Sector sSector in Sectors )
    {
      meshList.AddRange( sSector.GetObjects( ) );
    }
    return meshList;
  }
  public MaterialList GetMaterials( )
  {
    MaterialList materialList = [];
    foreach( Sector sSector in Sectors )
      materialList.AddRange( sSector.GetMaterials( ) );
    return materialList;
  }
}
#endregion

#region Sector
public class Sector
{
  public string Name;

  public CurveShape Layout = new();
  public CurveShape Level = new();

  public GroundList LeftGrounds;
  public GroundList RightGrounds;
  public ObjectList Objects = [];

  public MeshList LeftMeshes = [];
  public MeshList RightMeshes = [];

  public Sector( string sName )
  {
    Name = sName;

    LeftGrounds = new GroundList( );
    RightGrounds = new GroundList( );

    Level.Segments.Add( new Segment( new( 0,0,0 ),Segment.LerpMode.Cubic ) );
    Level.Segments.Add( new Segment( new( 1000,0,0 ),Segment.LerpMode.Cubic ) );
  }

  public void UpdatePoints( )
  {
    Layout.UpdatePoints( );
    Level.UpdatePoints( );
  }

  public MeshList GetMeshes( )
  {
    return LeftMeshes;
  }
  public ObjectList GetObjects( )
  {
    /*
    SegmentList arSegment = new SegmentList( );
    arSegment.Add( new( new( 10,0,0 ) ) );
    arSegment.Add( new( new( 0,0,0 ) ) );
    arSegment.Add( new( new( -5,8.6,0 ) ) );
    Curve cCurve = new( arSegment,new (0,0),new(0,1)  );

    Mesh oTree = new( );
    oTree.Vertices.Add( new MeshPointList( cCurve,MeshPointList.Direction.Forward ) );
    oTree.Vertices.Add( new MeshPointList( oTree.Vertices[^1],5 ) );
    oTree.Vertices.Add( new MeshPointList( oTree.Vertices[^1],5 ) );

    Material material = new Material( "tree",@".\textures\tree8C.dds" );
    oTree.Polygons.Add( new MeshPolygon( oTree.Vertices[0][1],
                                         oTree.Vertices[1][1],
                                         oTree.Vertices[1][0],
                                         oTree.Vertices[0][0],
                                         material ) );
    oTree.Polygons.Add( new MeshPolygon( oTree.Vertices[0][1],
                                         oTree.Vertices[1][1],
                                         oTree.Vertices[1][2],
                                         oTree.Vertices[0][2],
                                         material ) );
    materialList.Add( material );

    Object objTree = new( new Point3D( 0,0,5 ));
    objTree.Mesh = oTree;

    meshList.Add( oTree );
    */
    return Objects;
  }
  public MaterialList GetMaterials( )
  {
    MaterialList mMaterials = [];
    foreach( Object oObject in Objects )
    {
      mMaterials.AddRange( oObject.Materials );
    }
    foreach( Ground gGround in LeftGrounds )
    {
      mMaterials.Add( gGround.Material );

      foreach( Ground dDecal in gGround.Decals )
        mMaterials.Add( dDecal.Material );
    }
    return mMaterials;
  }

  public int FindNearestSegmentIndex( Point ptPoint )
  {
    int nMaxIndex = Layout.FindNearestPointIndex( ptPoint );
    for( int i = 1; i < Layout.Segments.Count-1; i++ )
    {
      if( Layout.Segments[i].CurveIndex > nMaxIndex )
        return i - 1;
    }
    return Layout.Segments.Count - 1;
  }

  public void CreateMesh( )
  {
    LeftMeshes.Clear( );

    if( LeftGrounds.Count > 0 && Layout.Segments.Count > 1 )
    {
      Mesh mNewMesh = new( );
      mNewMesh.Vertices.Add( new MeshPointList( Layout.Curve,MeshPointList.Direction.Left,LeftGrounds[0].MinDetail,LeftGrounds[0].MaxDetail ) );

      foreach( Ground gGround in LeftGrounds )
      {
        int nStart = mNewMesh.Vertices.Count-1;

        mNewMesh.AddCurve( gGround.Curve,gGround.Width,gGround.MinDetail,gGround.MaxDetail );
        mNewMesh.CreateMesh( nStart,mNewMesh.Vertices.Count - 1,gGround );
      }
      LeftMeshes.Add( mNewMesh );
    }
  }
}

public class SectorList : BindingList<Sector>
{
  public SectorList( )
  {
  }
}
#endregion

#region Ground
public class Ground : IComparable<Ground>
{
  public Material Material;
  public Curve Curve;

  public string Name;

  public int Order;
  public int Level;

  public double Width = 10f;
  public double Start = 0f;
  public double End = 1f;

  public double MinDetail = 1f;
  public double MaxDetail = 2f;

  public double Height;

  public GroundList Decals { get; set; }

  public SegmentList WidthSegments = [];

  public Point3DList Widths = [];

  public Point3DList PointsA { get; set; } = [];
  public Point3DList PointsB { get; set; } = [];

  #region Compare
  public int CompareTo( Ground? other )
  {
    if( other == null )
      return -1;
    /*
    if( Level == 0 )
    { 
      if( other.Level == 0 )
        return 0;
      return -1;
    }
    */
    if( other.Level == Level )
      return Order - other.Order;
    return other.Level - Level;
  }
  #endregion

  public Ground( string sName, Material material )
  {
    Name = sName;
    Material = material;

    Curve cCurve = new Curve( );
    cCurve.Begin( ).AddPoint( 0,5 );

    Curve = cCurve;

    Decals = new GroundList( );
  }

  public Segment? FindSegment( Point3D pt,double dMaxDistance )
  {
    Segment? sSegment = null;

    double dMin = dMaxDistance;
    foreach( Segment s in WidthSegments )
    {
      double d = Helpers.Distance2D( pt,s.Point );
      if( d < dMin )
      {
        sSegment = s;

        dMin = d;
      }
    }
    return sSegment;
  }

  public bool Contains( double d )
  {
    return Start <= d && End >= d;
  }

  public void UpdateWidths( )
  {
    WidthSegments.Sort( );

    Widths.Clear( );
    Point3DList? arPoints = Helpers.Interpolate( WidthSegments,200 );
    if( arPoints != null )
    {
      foreach( Point3D pt in arPoints )
        Widths.Add( pt );
      //Widths.Add( pt.Y );
    }
  }

  public double GetWidth( double dPos )
  {
    if( Widths.Count > 0 )
      return Width + Widths[(int)((Widths.Count-1) * dPos)].Y;
    return Width;
  }
}

public class GroundList : BindingList<Ground>
{
  public GroundList( )
  {
  }
}
#endregion

#region Mesh
public class Mesh
{
  public List<MeshPointList> Vertices { get; set; } = [];
  public MeshPolygonList Polygons { get; set; } = [];

  public void CreateMesh( int nStart,int nEnd,Ground gGround )
  {
    /*
     * Crea i quads/tris della mesh
     * Utilizza gli ID per capire quali devono essere collegati
     * Se gli ID combaciano e ce n'è uno solo, crea un quad con quello successivo
     * Se gli ID non combaciano scorre creando tris fino ad allinearli
     * 
     */
    for( int m = nStart + 1; m <= nEnd; m++ )
    {
      MeshPointList arPoints = Vertices[m-1];
      MeshPointList arNewPoints = Vertices[m];

      int nj = arPoints.Count;
      int ni = arNewPoints.Count;

      int j = 0;
      for( int i = 0; i < ni - 1 && j < nj - 1; i++, j++ )
      {
        int nA = arPoints.CountPoint( j );
        int nB = arNewPoints.CountPoint( i );

        if( nA == 1 && nA == nB )
        {
          if( arPoints[j + 1].ID == arNewPoints[i + 1].ID )
          {
            Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j + 1],arPoints[j],gGround.Height,gGround.Material ) );
          }
          else
          {
            if( i + 2 < ni && j + 2 < nj )
            {
              if( arPoints[j + 1].ID < arNewPoints[i + 1].ID )
              {
                int nNext = (arNewPoints[i + 1].ID + arNewPoints[i + 2].ID) / 2;

                Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j + 1],arPoints[j],gGround.Height,gGround.Material ) );

                while( arPoints[++j].ID < nNext )
                {
                  Polygons.Add( new MeshPolygon( arNewPoints[i + 1],arPoints[j + 1],arPoints[j],gGround.Height,gGround.Material ) );
                }
                j--;
              }
              else if( arNewPoints[i + 1].ID < arPoints[j + 1].ID )
              {
                int nNext = (arPoints[j + 1].ID + arPoints[j + 2].ID) / 2;

                Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j + 1],arPoints[j],gGround.Height,gGround.Material ) );

                while( arNewPoints[++i].ID < nNext )
                {
                  Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j + 1],gGround.Height,gGround.Material ) );
                }
                i--;
              }
            }
            else
            {
              if( arPoints[j + 1].ID < arNewPoints[i + 1].ID )
              {
                Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j],gGround.Height,gGround.Material ) );

                while( arPoints[++j].ID < arNewPoints[i + 1].ID )
                {
                  Polygons.Add( new MeshPolygon( arNewPoints[i + 1],arPoints[j],arPoints[j - 1],gGround.Height,gGround.Material ) );
                }
                j--;
              }
              else if( arNewPoints[i + 1].ID < arPoints[j + 1].ID )
              {
                Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j],gGround.Height,gGround.Material ) );

                while( arNewPoints[++i].ID < arPoints[j + 1].ID )
                {
                  Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j],gGround.Height,gGround.Material ) );
                }
                i--;
              }
            }
          }
        }
        else
        {
          if( nA == 1 )
          {
            Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j + 1],arPoints[j],gGround.Height,gGround.Material ) );

            while( arNewPoints[++i].ID < arPoints[j + 1].ID )
            {
              Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j + 1],gGround.Height,gGround.Material ) );
            }
            i--;
          }
          else
          {
            // TODO: aggiungere quando sia sotto che sopra ci sono puù ID consecutivi uguali.
            if( nA == nB )
            {
              while( arNewPoints[i++].ID == arPoints[j++].ID && i < ni - 1 && j < nj - 1 )
              {
                Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j + 1],arPoints[j],gGround.Height,gGround.Material ) );
              }
            }
            else
            {
              // Da controllare fatta senza testare il caso specifico
              Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j + 1],arPoints[j],gGround.Height,gGround.Material ) );

              while( arNewPoints[++i].ID == arPoints[++j].ID && i < ni - 1 && j < nj - 1 )
              {
                Polygons.Add( new MeshPolygon( arNewPoints[i],arNewPoints[i + 1],arPoints[j + 1],gGround.Height,gGround.Material ) );
              }
              i--;
              j--;
            }
          }
        }
      }
      // TODO: aggiungere le chiusure se i<ni or j < nj
    }
  }
  public void CreateMesh( Heightmap hMap,double dWidth,double dHeight,double dLowHeight,double dHighHeight )
  {
    if( hMap.Curves.Count > 0 )
    {
      int nXWidth = hMap.Curves.Count;
      int nYWidth = hMap.Curves[0].Points.Count;

      for( int x = 0; x < nXWidth - 1; x++ )
      {
        CurvePointList arPoints = hMap.Curves[x].Points;
        CurvePointList arNewPoints = hMap.Curves[x+1].Points;

        Vector v = new ();

        for( int y = 0; y < nYWidth - 1; y++ )
        {
          Material m = new ( "" );
          /*
          m.Color = new SolidColorBrush( Color.FromRgb( (byte)(((double)x / nXWidth) * 255),(byte)(((double)y / nYWidth) * 255),0 ) );
          Point3D a = new Point3D( ((double)x/nXWidth)*dWidth,
                                   arPoints[y].Point.X*dWidth,
                                   dLowHeight + arPoints[y].Point.Y*dHighHeight );

          Point3D b = new Point3D( ((double)x/nXWidth)*dWidth,
                                   arPoints[y+1].Point.X*dWidth,
                                   dLowHeight + arPoints[y+1].Point.Y*dHighHeight );

          Point3D c = new Point3D( ((double)x/nXWidth)*dWidth,
                                   arNewPoints[y+1].Point.X*dWidth,
                                   dLowHeight + arNewPoints[y+1].Point.Y*dHighHeight );

          Point3D d = new Point3D( ((double)x/nXWidth)*dWidth,
                                   arNewPoints[y].Point.X*dWidth,
                                   dLowHeight + arNewPoints[y].Point.Y*dHighHeight );
          */
          Point3D a = new Point3D( ((double)x/nXWidth)*dWidth,
                                   ((double)y/nYWidth)*dHeight,
                                   dLowHeight + arPoints[y].Point.Y*dHighHeight );

          Point3D b = new Point3D( ((double)(x+1)/nXWidth)*dWidth,
                                   ((double)y/nYWidth)*dHeight,
                                   dLowHeight + arNewPoints[y].Point.Y*dHighHeight );

          Point3D c = new Point3D( ((double)(x+1)/nXWidth)*dWidth,
                                   ((double)(y+1)/nYWidth)*dHeight,
                                   dLowHeight + arNewPoints[y+1].Point.Y*dHighHeight );

          Point3D d = new Point3D( ((double)x/nXWidth)*dWidth,
                                   ((double)(y+1)/nYWidth)*dHeight,
                                   dLowHeight + arPoints[y+1].Point.Y*dHighHeight );

          Polygons.Add( new MeshPolygon( new( 0,a,v ),new( 0,d,v ),new( 0,c,v ),new( 0,b,v ),0,m ) );
        }
      }
    }
  }

  public MeshFBX CreateFBXMesh( double dScaling = 1.0f )
  {
    MeshPointList FBXVertices = [];

    int n = 0;
    foreach( MeshPolygon meshPoly in Polygons )
    {
      n = meshPoly.AddToIndices( FBXVertices,n );
    }
    VERTEXFBX[] Vertices3D = new VERTEXFBX[FBXVertices.Count];
    for( int i = 0; i < Vertices3D.Length; i++ )
    {
      Vertices3D[i].x = (float)(FBXVertices[i].Point.X * dScaling);
      Vertices3D[i].y = -(float)(FBXVertices[i].Point.Y * dScaling);
      Vertices3D[i].z = (float)(FBXVertices[i].Point.Z * dScaling);
    }
    List<int> intIndices = new( Polygons.Count*3 );
    List<UVMAPFBX> UVMAPFBX = new( Polygons.Count*3 );
    List<int> matIndices = new( Polygons.Count );

    foreach( MeshPolygon meshPoly in Polygons )
    {
      intIndices.Add( meshPoly.P1.Index );
      intIndices.Add( meshPoly.P2.Index );

      UVMAPFBX.Add( new UVMAPFBX( (float)meshPoly.UV1.X,(float)meshPoly.UV1.Y ) );
      UVMAPFBX.Add( new UVMAPFBX( (float)meshPoly.UV2.X,(float)meshPoly.UV2.Y ) );

      if( meshPoly.P4 is not null )
      {
        intIndices.Add( meshPoly.P3.Index );
        intIndices.Add( -(meshPoly.P4.Index + 1) );

        UVMAPFBX.Add( new UVMAPFBX( (float)meshPoly.UV3.X,(float)meshPoly.UV3.Y ) );
        UVMAPFBX.Add( new UVMAPFBX( (float)meshPoly.UV4.X,(float)meshPoly.UV4.Y ) );
      }
      else
      {
        intIndices.Add( -(meshPoly.P3.Index + 1) );

        UVMAPFBX.Add( new UVMAPFBX( (float)meshPoly.UV3.X,(float)meshPoly.UV3.Y ) );
      }
      matIndices.Add( meshPoly.Material.Index );
    }
    return new MeshFBX( Vertices3D,[.. intIndices],[.. UVMAPFBX],[.. matIndices] );
  }
  public MeshD3D CreateD3DMesh( double dScaling = 1.0f )
    {
      MeshPointList d3dVertices = [];

      int n = 0;
      foreach( MeshPolygon meshPoly in Polygons )
        n = meshPoly.AddToIndices( d3dVertices,n );

      var Vertices3D = new VERTEXD3D[d3dVertices.Count];
      for( int i = 0; i < Vertices3D.Length; i++ )
      {
        Vertices3D[i].x = (float)(d3dVertices[i].Point.X * dScaling);
        Vertices3D[i].y = (float)(d3dVertices[i].Point.Y * dScaling);
        Vertices3D[i].z = (float)(d3dVertices[i].Point.Z * dScaling);
      }
      List<uint> intIndices = new( Polygons.Count*3 );
      List<UVMAPD3D> UVMAPFBX = new( Polygons.Count*3 );
      List<ushort> matIndices = new( Polygons.Count );

      foreach( MeshPolygon meshPoly in Polygons )
      {
        intIndices.Add( (uint)meshPoly.P3.Index );
        intIndices.Add( (uint)meshPoly.P2.Index );

        //UVMAPFBX.Add( new UVMAPD3D( (float)meshPoly.UV1.X,(float)meshPoly.UV1.Y ) );
        //UVMAPFBX.Add( new UVMAPD3D( (float)meshPoly.UV2.X,(float)meshPoly.UV2.Y ) );

        if( meshPoly.P4 is not null )
        {
          intIndices.Add( (uint)meshPoly.P1.Index );

          intIndices.Add( (uint)meshPoly.P3.Index );
          intIndices.Add( (uint)meshPoly.P1.Index );
          intIndices.Add( (uint)meshPoly.P4.Index );

          //UVMAPFBX.Add( new UVMAPD3D( (float)meshPoly.UV3.X,(float)meshPoly.UV3.Y ) );
          //UVMAPFBX.Add( new UVMAPD3D( (float)meshPoly.UV4.X,(float)meshPoly.UV4.Y ) );
        }
        else
        {
          intIndices.Add( (uint)meshPoly.P1.Index );

          //UVMAPFBX.Add( new UVMAPD3D( (float)meshPoly.UV3.X,(float)meshPoly.UV3.Y ) );
        }
        matIndices.Add( (ushort)meshPoly.Material.Index );
      }
      return new MeshD3D( Vertices3D,[.. intIndices],[.. UVMAPFBX],[.. matIndices] );
    }


  public void AddCurve( Curve cCurve,double dWidth,double dMinDetail,double dMaxDetail )
  {
    if( Vertices.Count > 0 && cCurve.Points.Count > 0 )
    {
      for( int i = 1; i < cCurve.Points.Count; i++ )
      {
        Vector vVector = new ( dWidth + cCurve.Points[i].Point.X - cCurve.Points[i-1].Point.X,
                               cCurve.Points[i].Point.Y - cCurve.Points[i-1].Point.Y );

        Vertices.Add( new( Vertices[^1],vVector,dMinDetail,dMaxDetail ) );
      }
    }
  }
}

public class MeshList : List<Mesh>
{
}

public class MeshPolygon
{
  public MeshPoint P1 { get; set; }
  public MeshPoint P2 { get; set; }
  public MeshPoint P3 { get; set; }
  public MeshPoint? P4 { get; set; }

  public Point UV1 { get; set; } = new Point( 0,0 );
  public Point UV2 { get; set; } = new Point( 0,1 );
  public Point UV3 { get; set; } = new Point( .5,1 );
  public Point UV4 { get; set; } = new Point( .5,0 );

  public Material Material { get; set; }

  public MeshPolygon( MeshPoint p1,MeshPoint p2,MeshPoint p3,MeshPoint? p4,double dHeight,Material material )
  {
    P1 = p1;
    P2 = p2;
    P3 = p3;
    P4 = p4;

    AddHeight( dHeight );

    Material = material;
  }
  public MeshPolygon( MeshPoint p1,MeshPoint p2,MeshPoint p3,double dHeight,Material material ) : this( p1,p2,p3,null,dHeight,material )
  {
  }

  public void AddHeight( double dHeight )
  {
    P1.Point.Z += dHeight;
    P2.Point.Z += dHeight;
    P3.Point.Z += dHeight;
    if( P4 is not null )
      P4.Point.Z += dHeight;
  }
  public int AddToIndices( MeshPointList arIndices,int nIndex )
  {
    if( P1.Point.Index < 0 )
    {
      P1.Point.Index = nIndex++;

      arIndices.Add( P1 );
    }
    if( P2.Point.Index < 0 )
    {
      P2.Point.Index = nIndex++;

      arIndices.Add( P2 );
    }
    if( P3.Point.Index < 0 )
    {
      P3.Point.Index = nIndex++;

      arIndices.Add( P3 );
    }
    if( P4 is not null && P4.Point.Index < 0 )
    {
      P4.Point.Index = nIndex++;

      arIndices.Add( P4 );
    }
    return nIndex;
  }
}

public class MeshPolygonList : List<MeshPolygon>
{
}
#endregion

#region Material
public class Material
{
  public string Name;

  public int Index = -1;

  public Texture? Texture = null;

  public Material( string name )
  {
    Name = name;
  }
  public Material( string name,string textureFile )
  {
    Name = name;
    Texture = new Texture( textureFile );
  }
}

public class MaterialList : List<Material>
{
}
#endregion

#region Texture
public class Texture
{
  public string Filename;

  public Texture( string sFile )
  {
    Filename = sFile;
  }
}
#endregion

#region Heightmap
public class Heightmap( )
{
  public Mesh Mesh = new ( );
  public CurveList Curves = [];

  public void GenerateHeightmap( )
  {
    /*
    if( Image.UI is not null && Image.UI.BitmapData is not null )
    {
      BitmapData bd = new ( Image.UI.BitmapData );

      int nWidth = bd.Width;
      int nHeight = bd.Height;
      for( int x = 0; x < nWidth; x++)
      {
        Curve curve = new Curve( );
        for( int y = 0; y < nHeight; y++ )
        { 
          ushort p = bd.GetPixelBW( x,y );

          curve.Points.Add( new CurvePoint( new Point( (float)x/nWidth,p/65535.0f ) ) );
        }
        Curves.Add( curve );
      }
    }
  */
  }
  public void GenerateMesh( double dWidth, double dHeight, double dLowHeight, double dHighHeight )
  {
    Mesh.CreateMesh( this,dWidth,dHeight,dLowHeight,dHighHeight );
  }
}

public class HeightmapList : BindingList<Heightmap>
{

}
#endregion

#region Object
public class Object( Point3D origin )
{
  public Point3D Point { get; set; } = origin;

  public MaterialList Materials { get; set; } = [];
  public Mesh? Mesh { get; set; }
}

public class ObjectList : List<Object>
{

}
#endregion

#region FBX
public struct VERTEXFBX( float ix,float iy,float iz )
{
  public float x = ix;
  public float y = iy;
  public float z = iz;
}
public struct UVMAPFBX( float x,float y )
{
  public float x = x;
  public float y = y;
}

public class MeshFBX( VERTEXFBX[] vertices,int[] indices,UVMAPFBX[] uv,int[] mats )
{
  public int[] Indices = indices;
  public int[] MaterialIndices = mats;

  public VERTEXFBX[] Vertices = vertices;
  public UVMAPFBX[] UVMAPFBXs = uv;
}
#endregion

#region D3D
public struct VERTEXD3D( float ix,float iy,float iz,float ir,float ig,float ib,float ia )
{
  public float x = ix;
  public float y = iy;
  public float z = iz;
}
public struct UVMAPD3D( float x,float y )
{
  public float x = x;
  public float y = y;
}

public class MeshD3D( VERTEXD3D[] vertices,uint[] indices,UVMAPD3D[] uv,int[] mats )
{
  public uint[] Indices = indices;
  public int[] MaterialIndices = mats;
  public VERTEXD3D[] Vertices = vertices;
  public UVMAPD3D[] UVMAPFBXs = uv;
}
#endregion

#region Helpers
public static class Helpers
{
  public static Vector VectorZero;

  /*
   public static double Direction( Point3D a,Point3D b )
   {
     return -Math.Atan2(b.Y - a.Y, b.X - a.X);
     //return AngleFromXY( b.Y - a.Y,a.X - b.X );
   }
   */
  public static Vector Direction( Point a,Point b )
  {
    Vector v = new( b.X - a.X,b.Y - a.Y ); 
    v.Normalize();
    return v;
  }
  public static Vector Direction( Point3D a,Point3D b )
  {
    Vector v = new( b.X-a.X,b.Y-a.Y ); 
    v.Normalize();
    return v;
  }

  public static Vector Backward( Vector v )
  {
    return new( -v.X,-v.Y );
  }
  public static Vector Left( Vector v )
  {
    return new(-v.Y,v.X);
  }
  public static Vector Right( Vector v )
  {
    return new(-v.Y,v.X);
  }
  public static Vector Middle( Vector a,Vector b )
  {
    return new( (a.X + b.X) / 2,(a.Y + b.Y) / 2 );
  }

  public static Point Forward( Point pt,double dDirection,double dLength )
  {
    double x = (float)(pt.X + Math.Cos( dDirection ) * dLength);
    double y = (float)(pt.Y + Math.Sin( dDirection ) * dLength);

    return new Point( x,y );
  }
  public static Point Forward( Point pt,Vector vVector,double dLength )
  {
    double x = (float)(pt.X + vVector.X * dLength);
    double y = (float)(pt.Y + vVector.Y * dLength);

    return new Point( x,y );
  }

  public static Point3D Forward( GeoPoint pt,Vector vVector,double dLength )
  {
    double x = (float)(pt.X + vVector.X * dLength);
    double y = (float)(pt.Y + vVector.Y * dLength);

    return new Point3D( x,y,pt.Z );
  }
  public static Point3D Forward( GeoPoint pt,Vector forwardVector,Vector moveVector )
  {
    double x = (float)(pt.X + forwardVector.X * moveVector.X );
    double y = (float)(pt.Y + forwardVector.Y * moveVector.X );
    double z = pt.Z + moveVector.Y;

    return new Point3D( x,y,z );
  }
  public static Point3D Forward( Point3D pt,double dDirection,double dLength )
  {
    double x = (float)(pt.X + Math.Cos( dDirection ) * dLength);
    double y = (float)(pt.Y + Math.Sin( dDirection ) * dLength);

    return new Point3D( x,y,pt.Z );
  }

  public static Point Middle( Point a,Point b )
  {
    return new Point( a.X + (b.X - a.X) / 2,a.Y + (b.Y - a.Y) / 2 );
  }
  public static Point3D Middle( GeoPoint a,GeoPoint b )
  {
    return new Point3D( a.X + (b.X - a.X) / 2,a.Y + (b.Y - a.Y) / 2,a.Z + (b.Z - a.Z) / 2 );
  }
  public static MeshPoint Middle( MeshPoint a,MeshPoint b )
  {
    return new MeshPoint( (a.ID + b.ID) / 2,Middle( a.Point,b.Point ),Middle( a.Vector,b.Vector ) );
  }

  /*
  public static Point3D Forward( Point3D pt,double dDirection,double dLength )
  {
    double x = (float)(pt.X + Math.Cos( -dDirection ) * dLength);
    double y = (float)(pt.Y - Math.Sin( -dDirection ) * dLength);

    return new Point3D( x,y,pt.Z );
  }
  public static Point3D ForwardXY( Point3D pt,Vector vVector,double dLength )
  {
    double x = (float)(pt.X + vVector.X * dLength);
    double y = (float)(pt.Y + vVector.Y * dLength);

    return new Point3D( x,y,pt.Z );
  }

  public static Point3D Left( Point3D pt,double dDirection, double dLength )
  {
    double x = (float)(pt.X + Math.Cos( -dDirection + (Math.PI / 2) ) * dLength);
    double y = (float)(pt.Y - Math.Sin( -dDirection + (Math.PI / 2) ) * dLength);

    return new Point3D( x,y,pt.Z );
  }
  public static Point3D Right( Point3D pt,double dDirection,double dLength )
  {
    double x = (float)(pt.X + Math.Cos( -dDirection - (Math.PI / 2) ) * dLength);
    double y = (float)(pt.Y - Math.Sin( -dDirection - (Math.PI / 2) ) * dLength);

    return new Point3D( x,y,pt.Z );
  }

  public static Point3D Forward( GeoPoint pt,double dDirection,double dLength )
  {
    double x = (float)(pt.X + Math.Cos( -dDirection ) * dLength);
    double y = (float)(pt.Y - Math.Sin( -dDirection ) * dLength);

    return new Point3D( x,y,pt.Z );
  }
  public static Point3D ForwardXY( GeoPoint pt,Vector vVector,double dLength )
  {
    double x = (float)(pt.X + vVector.X * dLength);
    double y = (float)(pt.Y + vVector.Y * dLength);

    return new Point3D( x,y,pt.Z );
  }
  public static Point3D Left( GeoPoint pt,double dDirection,double dLength )
  {
    double x = (float)(pt.X + Math.Cos( -dDirection + (Math.PI / 2) ) * dLength);
    double y = (float)(pt.Y - Math.Sin( -dDirection + (Math.PI / 2) ) * dLength);

    return new Point3D( x,y,pt.Z );
  }
  public static Point3D Right( GeoPoint pt,double dDirection,double dLength )
  {
    double x = (float)(pt.X + Math.Cos( -dDirection - (Math.PI / 2) ) * dLength);
    double y = (float)(pt.Y - Math.Sin( -dDirection - (Math.PI / 2) ) * dLength);

    return new Point3D( x,y,pt.Z );
  }

  public static Point3D Middle( Point3D a,Point3D b )
  {
    return new Point3D( a.X + (b.X-a.X)/2,a.Y + (b.Y - a.Y) / 2,a.Z + (b.Z - a.Z) / 2 );
  }
  public static Point3D Middle( GeoPoint a,GeoPoint b )
  {
    return new Point3D( a.X + (b.X - a.X) / 2,a.Y + (b.Y - a.Y) / 2,a.Z + (b.Z - a.Z) / 2 );
  }
  public static MeshPoint Middle( MeshPoint a,MeshPoint b )
  {
    return new MeshPoint( (a.ID + b.ID) / 2,Middle( a.Point,b.Point ),Middle( a.XYVector,b.XYVector ),Middle( a.YZVector,b.YZVector ) );
  }
  */
  /*
  public static double Direction( GeoPoint a,GeoPoint b )
  {
    return Math.Atan2( b.Y - a.Y,b.X - a.X );
    //return AngleFromXY( b.Y - a.Y,a.X - b.X );
  }
  public static double Direction( double aX, double aY, double bX, double bY )
  {
    return Math.Atan2( bY - aY,bX - aX );
    //return AngleFromXY( aY - bY,bX - aX );
  }
  */
  /*
      public static Vector Direction( Point3D a,Point3D b )
      {
        Vector v = new Vector( b.X - a.X,b.Y - a.Y );
        v.Normalize( );
        return v;
      }
      public static Vector Direction( GeoPoint a,GeoPoint b )
      {
        Vector v = new Vector( b.X - a.X,b.Y - a.Y );
        v.Normalize( );
        return v;
      }
      public static Vector Direction( double aX,double aY,double bX,double bY )
      {
        Vector v = new Vector( bX - aX,bY - aY );
        v.Normalize( );
        return v;
      }
  */
  public static double Distance( Point a,Point b )
  {
    double deltaX = b.X - a.X;
    double deltaY = b.Y - a.Y;

    return Math.Sqrt( deltaX * deltaX + deltaY * deltaY );
  }
  public static double Distance( Point a,Point3D b )
  {
    double deltaX = b.X - a.X;
    double deltaY = b.Y - a.Y;

    return Math.Sqrt( deltaX * deltaX + deltaY * deltaY );
  }
  public static double Distance( Point3D a,Point3D b )
  {
    double deltaX = b.X - a.X;
    double deltaY = b.Y - a.Y;
    double deltaZ = b.Z - a.Z;

    return Math.Sqrt( deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ );
  }
  public static double Distance2D( Point3D a,Point3D b )
  {
    double deltaX = b.X - a.X;
    double deltaY = b.Y - a.Y;

    return Math.Sqrt( deltaX * deltaX + deltaY * deltaY );
  }

  public static double Distance( GeoPoint a,GeoPoint b )
  {
    double deltaX = b.X - a.X;
    double deltaY = b.Y - a.Y;
    double deltaZ = b.Z - a.Z;

    return Math.Sqrt( deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ );
  }
  public static double Distance2D( GeoPoint a,GeoPoint b )
  {
    double deltaX = b.X - a.X;
    double deltaY = b.Y - a.Y;

    return Math.Sqrt( deltaX * deltaX + deltaY * deltaY );
  }

  public static double DistanceSqrd( Point a,Point b )
  {
    double deltaX = b.X - a.X;
    double deltaY = b.Y - a.Y;

    return deltaX * deltaX + deltaY * deltaY;
  }
  public static double DistanceSqrd( Point3D a,Point3D b )
  {
    double deltaX = b.X - a.X;
    double deltaY = b.Y - a.Y;
    double deltaZ = b.Z - a.Z;

    return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
  }
  public static double DistanceSqrd( GeoPoint a,GeoPoint b )
  {
    double deltaX = b.X - a.X;
    double deltaY = b.Y - a.Y;
    double deltaZ = b.Z - a.Z;

    return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
  }

  public static double Degrees( double Radians )
  {
    return Radians * (180 / Math.PI);
  }
  public static double Radians( double Degrees )
  {
    return Degrees * (Math.PI / 180);
  }
  public static double GetPositiveAngleDifference( double a,double b )
  {
    if( (a > Math.PI / 2 && b < -Math.PI / 2) || (a < -Math.PI / 2 && b > Math.PI / 2) )
      return Math.Abs( a ) - Math.Abs( b );
    return a - b;
  }

  public static Point3DList? Interpolate( SegmentList arSegments, int nTotalPoints )
  {
    if( arSegments.Count > 1 )
    {
      int pointCount = arSegments.Count;

      double[]? xs1 = new double[pointCount];
      double[]? ys1 = new double[pointCount];

      int i = 0;
      foreach( Segment s in arSegments )
      {
        xs1[i] = s.Point.X;
        ys1[i] = s.Point.Y;

        i++;
      }
      // Use cubic interpolation to smooth the original data
      int nPointsCount = nTotalPoints;
      (double[] xs2, double[] ys2) = Helpers.InterpolateXY( xs1,ys1,nPointsCount );

      Point3DList arPoints = [];
      for( i = 0; i < nPointsCount; i++ )
        arPoints.Add( new Point3D( xs2[i],ys2[i],0 ) );
      return arPoints;
    }
    return null;
  }

  // Cubic interpolation
  public static (double[] xs, double[] ys) InterpolateXY( double[] xs,double[] ys,int count )
  {
    if( xs is null || ys is null || xs.Length != ys.Length )
      throw new ArgumentException( $"{nameof( xs )} and {nameof( ys )} must have same length" );

    int inputPointCount = xs.Length;
    double[] inputDistances = new double[inputPointCount];
    for( int i = 1; i < inputPointCount; i++ )
    {
      double dx = xs[i] - xs[i - 1];
      double dy = ys[i] - ys[i - 1];
      double distance = Math.Sqrt(dx * dx + dy * dy);
      inputDistances[i] = inputDistances[i - 1] + distance;
    }

    double meanDistance = inputDistances.Last() / (count - 1);
    double[] evenDistances = Enumerable.Range(0, count).Select(x => x * meanDistance).ToArray();
    double[] xsOut = Interpolate(inputDistances, xs, evenDistances);
    double[] ysOut = Interpolate(inputDistances, ys, evenDistances);

    return (xsOut, ysOut);
  }
  private static double[] Interpolate( double[] xOrig,double[] yOrig,double[] xInterp )
  {
    (double[] a, double[] b) = FitMatrix( xOrig,yOrig );

    double[] yInterp = new double[xInterp.Length];
    for( int i = 0; i < yInterp.Length; i++ )
    {
      int j;
      for( j = 0; j < xOrig.Length - 2; j++ )
        if( xInterp[i] <= xOrig[j + 1] )
          break;

      double dx = xOrig[j + 1] - xOrig[j];
      double t = (xInterp[i] - xOrig[j]) / dx;
      double y = (1 - t) * yOrig[j] + t * yOrig[j + 1] +
              t * (1 - t) * (a[j] * (1 - t) + b[j] * t);
      yInterp[i] = y;
    }

    return yInterp;
  }
  private static (double[] a, double[] b) FitMatrix( double[] x,double[] y )
  {
    int n = x.Length;
    double[] a = new double[n - 1];
    double[] b = new double[n - 1];
    double[] r = new double[n];
    double[] A = new double[n];
    double[] B = new double[n];
    double[] C = new double[n];

    double dx1, dx2, dy1, dy2;

    dx1 = x[1] - x[0];
    C[0] = 1.0f / dx1;
    B[0] = 2.0f * C[0];
    r[0] = 3 * (y[1] - y[0]) / (dx1 * dx1);

    for( int i = 1; i < n - 1; i++ )
    {
      dx1 = x[i] - x[i - 1];
      dx2 = x[i + 1] - x[i];
      A[i] = 1.0f / dx1;
      C[i] = 1.0f / dx2;
      B[i] = 2.0f * (A[i] + C[i]);
      dy1 = y[i] - y[i - 1];
      dy2 = y[i + 1] - y[i];
      r[i] = 3 * (dy1 / (dx1 * dx1) + dy2 / (dx2 * dx2));
    }

    dx1 = x[n - 1] - x[n - 2];
    dy1 = y[n - 1] - y[n - 2];
    A[n - 1] = 1.0f / dx1;
    B[n - 1] = 2.0f * A[n - 1];
    r[n - 1] = 3 * (dy1 / (dx1 * dx1));

    double[] cPrime = new double[n];
    cPrime[0] = C[0] / B[0];
    for( int i = 1; i < n; i++ )
      cPrime[i] = C[i] / (B[i] - cPrime[i - 1] * A[i]);

    double[] dPrime = new double[n];
    dPrime[0] = r[0] / B[0];
    for( int i = 1; i < n; i++ )
      dPrime[i] = (r[i] - dPrime[i - 1] * A[i]) / (B[i] - cPrime[i - 1] * A[i]);

    double[] k = new double[n];
    k[n - 1] = dPrime[n - 1];
    for( int i = n - 2; i >= 0; i-- )
      k[i] = dPrime[i] - cPrime[i] * k[i + 1];

    for( int i = 1; i < n; i++ )
    {
      dx1 = x[i] - x[i - 1];
      dy1 = y[i] - y[i - 1];
      a[i - 1] = k[i - 1] * dx1 - dy1;
      b[i - 1] = -k[i] * dx1 + dy1;
    }

    return (a, b);
  }
}
#endregion
