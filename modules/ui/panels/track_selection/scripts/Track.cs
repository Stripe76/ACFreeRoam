using Godot;
using System;
using System.IO;

public partial class Track : Control
{
	private string _ACTracksFolder = "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/";
	
	private TrackData? _track;
	
	public Track WithData( TrackData track )
	{
		_track = track;
		
		return this;
	}

	public override void _Ready()
	{
		if( _track != null )
		{
			var title = GetNode<Label>( "%Title" );
			title.Text = _track.ID;

			Button button = GetNode<Button>( "%Load" );
			button.Pressed += () =>
			{
				GetTree( ).CallGroup( "UI","select_track",[_track.ID,_track.Variants.Values[0].ID] );
			};
		}
	}

	public void UpdateTrackData()
	{
		if( _track != null )
		{
			string imagePath = Path.Combine( _ACTracksFolder,_track.ID,"ui","preview.png" );
			if( _track.Variants.Count > 1 && _track.Variants.Values[0].ID != "" )
			{
				string variantID = _track.Variants.Values[0].ID;
				imagePath = Path.Combine( _ACTracksFolder,_track.ID,"ui",variantID,"preview.png" );
			}
			if( File.Exists( imagePath ) )
			{
				ImageTexture texture = new ImageTexture( );
				texture.SetImage( Image.LoadFromFile( imagePath ) );
					
				GetNode<TextureRect>( "%Image" ).SetTexture( texture );
			}
		}
	}

	public void _on_focus_entered()
	{
		Button button = GetNode<Button>( "%Load" );
		button.Visible = true;
	}
	public void _on_focus_exited()
	{
		Button button = GetNode<Button>( "%Load" );
		button.Visible = false;
	}
}
