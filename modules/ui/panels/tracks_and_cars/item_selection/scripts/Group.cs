using Godot;
using System;

public partial class Group : Control
{
	private ItemData? _item = null;
	private PackedScene? _itemScene = null;

	private string _itemCommand = "";
	private string _listParameter = "";

	public Group WithData( ItemData item,PackedScene? itemScene,string itemCommand,string listParameter )
	{
		_item = item;
		_itemScene = itemScene;
		_itemCommand = itemCommand;
		_listParameter = listParameter; 
		
		return this;
	}
	
	public override void _Ready()
	{
		if( _item != null )
		{
			GetNode<Button>( "Title" ).Text = _item.Name;

			var tracks = GetNode<Control>( "%Items" );
			if( tracks != null )
			{
				GetNode<Control>( "Control" ).Visible = false;
				foreach( ItemData item in _item.Items.Values )
				{
					var trackDisplay = _itemScene?.Instantiate<Item>( )?.WithData( item,_itemCommand,_listParameter );
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
				var tracks = GetNode<Control>( "%Items" );
				int c = tracks.GetChildCount( );
				for( int i = 0; i < c; i++ )
					tracks.GetChild<Item>( i ).UpdateItemData( );
			}
		}
	}
}
