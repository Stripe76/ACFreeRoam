using Godot;
using System;
using System.Collections.Generic;
using ACLibrary.Tracks;

public abstract partial class ItemSelection : Control
{
	[ExportGroup("Data")]
	[Export] public PackedScene? ItemScene;
	[Export] public PackedScene? GroupScene;

	[Export] public string ItemCommand = "select_track";
	[Export] public string ListParameter = "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/";

	private int _columnsNumber = 0;

	private readonly ItemDataList _itemsList = [];

	public override void _Ready()
	{
		//Control? groups = GetNode<Control>( "%Groups" );
		Control? groups = GetNode<Control>( "." );
		if( groups != null )
		{
			LoadItemsList( _itemsList,ListParameter );
			if( _columnsNumber != 0 )
				FillColumns( groups,_columnsNumber );

			Resized += () =>
			{
				int newColumns = (int)(Size.X / 250);
				if( newColumns != _columnsNumber )
					FillColumns( groups,_columnsNumber = newColumns );
			};
		}
	}

	private void FillColumns( Control groups,int colsNumber )
	{
		if( colsNumber == 0 )
			return;
		
		List<Control> nodes = [];
		while( GetChildCount(  ) > 0 )
		{
			VBoxContainer child = GetChild<VBoxContainer>( 0 );
			while( child.GetChildCount( ) > 0 )
			{
				var n = child.GetChild<Control>( 0 );
				nodes.Add( n );
				child.RemoveChild( n );
			}
			RemoveChild( child );
			child.QueueFree(  );
		}
		if( nodes.Count == 0 && GroupScene != null )
		{
			int count = _itemsList.Values.Count;
			for( int i = 0; i < count; i++ )
				nodes.Add( GroupScene.Instantiate<Group>( ).WithData( _itemsList.Values[i],ItemScene,ItemCommand,ListParameter  ) );
		}
		int rows = nodes.Count / colsNumber + 1;
		for( int i = 0; i < colsNumber; i++ )
		{
			VBoxContainer container = new VBoxContainer( );
			container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			
			for( int j = 0; j < rows; j++ )
			{
				int index = i * rows + j;
				if( index < nodes.Count )
				{
					//var groupDisplay = GroupScene.Instantiate<Group>( ).WithData( _itemsList.Values[index],ItemScene );
					container.AddChild( nodes[index] );
				}
			}
			groups.AddChild( container );
		}
	}

	protected abstract void LoadItemsList( ItemDataList itemsList,string folder );
}

public class ItemData( string dataID,string name )
{
	public string ID = dataID;
	public string Name = name;

	public readonly ItemDataList Items = [];
}

public class ItemDataList : SortedList<string,ItemData>
{
}
