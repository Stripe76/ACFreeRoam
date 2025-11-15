using Godot;
using System;

public partial class Nation : Control
{
	// /mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/gui/NationFlags/
	private const string _trackScene = "res://modules/ui/panels/track_selection/track.tscn";

	private TrackNation? _nation = null;

	public Nation WithData( TrackNation nation )
	{
		_nation = nation;
		
		return this;
	}
	
	public override void _Ready()
	{
		if( _nation != null )
		{
			GetNode<Button>( "Title" ).Text = _nation.Name;

			/*
			Image image = new Image();
			image.Load( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/gui/NationFlags/DNK.png" );
			ImageTexture texture = new ImageTexture( );
			texture.SetImage( image );
			GetNode<TextureRect>( "%Flag" ).Texture = texture;  
			*/

			var tracks = GetNode<Control>( "%Tracks" );
			if( tracks != null )
			{
				GetNode<Control>( "Control" ).Visible = false;
				foreach( TrackData track in _nation.Tracks.Values )
				{
					var trackDisplay = GD.Load<PackedScene>( _trackScene ).Instantiate<Track>( )?.WithData( track );
					if( trackDisplay != null )
					{
						tracks.AddChild( trackDisplay );
					}
				}
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_title_toggled( bool toggle )
	{
		var control = GetNode<Control>( "Control" );
		if( control != null )
		{
			control.Visible = toggle;

			if( toggle )
			{
				var tracks = GetNode<Control>( "%Tracks" );
				int c = tracks.GetChildCount( );
				for( int i = 0; i < c; i++ )
					tracks.GetChild<Track>( i ).UpdateTrackData( );
			}
		}
	}
}
