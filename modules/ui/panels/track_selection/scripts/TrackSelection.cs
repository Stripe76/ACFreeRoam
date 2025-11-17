using Godot;
using System;
using System.Collections.Generic;
using ACLibrary.Tracks;

public partial class TrackSelection : ItemSelection
{
	protected override void LoadItemsList( ItemDataList itemsList,string folder )
	{
		List<TrackInfo> tracks = ACLibrary.Tracks.TrackInfo.GetTracksInfos( folder,null );
		foreach( TrackInfo newTrack in tracks )
		{
			if( !itemsList.TryGetValue( newTrack.Nation,out var nation ) )
			{
				itemsList.Add( newTrack.Nation,nation = new ItemData( newTrack.Nation,newTrack.Nation ) );
			}
			if( !nation.Items.TryGetValue( newTrack.TrackID,out var track ) )
			{
				nation.Items.Add( newTrack.TrackID,track = new ItemData( newTrack.TrackID,newTrack.TrackName ) );
			}
			if( !track.Items.ContainsKey( newTrack.VariantID ) )
			{
				track.Items.Add( newTrack.VariantID,new ItemData( newTrack.VariantID,newTrack.VariantName ) );
			}
		}

	}
}
