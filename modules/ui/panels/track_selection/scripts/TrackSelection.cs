using Godot;
using System;
using System.Collections.Generic;
using ACLibrary.Tracks;

public partial class TrackSelection : Control
{
	[Export]
	public int ColumnsNumber = 3;

	private const string _nationScene = "res://modules/ui/panels/track_selection/nation.tscn";

	private readonly TrackNationList _tracksList = [];

	public override void _Ready()
	{
		Control? nations = GetNode<Control>( "%Nations" );
		if( nations != null )
		{
			LoadTracksList( _tracksList,"/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/" );

			int rows = _tracksList.Values.Count / ColumnsNumber;
			for( int i = 0; i < ColumnsNumber; i++ )
			{
				VBoxContainer container = new VBoxContainer( );
				for( int j = 0; j < rows; j++ )
				{
					int index = i * rows + j;
					if( index < _tracksList.Values.Count )
					{
						var nationDisplay = GD.Load<PackedScene>( _nationScene ).Instantiate<Nation>( ).WithData( _tracksList.Values[index] );
						container.AddChild( nationDisplay );
					}
				}
				nations.AddChild( container );
			}
			
			foreach( TrackNation nation in _tracksList.Values )
			{
				/*
				var nationDisplay = GD.Load<PackedScene>( "res://modules/ui/panels/track_selection/nation.tscn" ).Instantiate<Nation>( ).WithData( nation );

				nations.AddChild( nationDisplay );
				*/
				/*
				Button button = new Button( )
				{
					Text = nation.Name,
					CustomMinimumSize = new Vector2( 200,50 )
				};
				button.Pressed += () =>
				{
					GetTree( ).CallGroup( "UI","select_track_nation",[nation.ID] );
				};
				nations.AddChild( button );
				*/
			}
		}
	}

	private void LoadTracksList(TrackNationList nationList,string folder)
	{
		List<TrackInfo> tracks = ACLibrary.Tracks.TrackInfo.GetTracksInfos( folder,null );
		foreach( TrackInfo newTrack in tracks )
		{
			if( !nationList.TryGetValue( newTrack.Nation,out var nation ) )
			{
				nationList.Add( newTrack.Nation,nation = new TrackNation( newTrack.Nation,newTrack.Nation ) );
			}
			if( !nation.Tracks.TryGetValue( newTrack.TrackID,out var track ) )
			{
				nation.Tracks.Add( newTrack.TrackID,track = new TrackData( newTrack.TrackID,newTrack.TrackName ) );
			}
			if( !track.Variants.ContainsKey( newTrack.VariantID ) )
			{
				track.Variants.Add( newTrack.VariantID,new TrackVariant( newTrack.VariantID,newTrack.VariantName ) );
			}
		}
	}

	/*
	public void select_track( string trackID )
	{
		Image image = new Image();
		image.Load( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/" + trackID + "/ui/preview.png" );
		
		ImageTexture texture = new ImageTexture();
		texture.SetImage( image );

		TextureRect? textureRect = GetNode<TextureRect>( "%TrackImage" );
		if( textureRect != null )
		{
			textureRect.Texture = texture;
		}
	}
	/*
	public void select_track_nation( string nationID )
	{
		if( _tracksList.TryGetValue( nationID,out TrackNation nation ) )
		{
			Control? trackList = GetNode<Control>( "%TracksList" );
			if( trackList != null )
			{
				while( trackList.GetChildCount(  ) > 0 )
					trackList.RemoveChild( trackList.GetChild( 0 ) );
				
				foreach( TrackData track in nation.Tracks.Values )
				{
					Button button = new Button( )
					{
						Text = track.ID,
						CustomMinimumSize = new Vector2( 200,50 )
					};
					button.Pressed += () =>
					{
						GetTree( ).CallGroup( "UI","select_track",[track.ID] );
					};
					trackList.AddChild( button );
				}
			}
		}
	}
	*/
}

public class TrackNation( string nationID,string name )
{
	public string ID = nationID;
	public string Name = name;

	public readonly TrackDataList Tracks = [];
}

public class TrackNationList : SortedList<string,TrackNation>
{
}

public class TrackData( string trackID,string name )
{
	public string ID = trackID;
	public string Name  = name;

	public TrackVariantList Variants = [];
}

public class TrackDataList : SortedList<string,TrackData>
{
}

public class TrackVariant( string variantID,string name )
{
	public string ID  = variantID;
	public string Name = name;
}

public class TrackVariantList : SortedList<string,TrackVariant>
{
}
