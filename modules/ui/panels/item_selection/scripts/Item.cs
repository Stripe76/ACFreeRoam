using Godot;
using System;
using System.IO;
	
public partial class Item : Control
{
	protected ItemData? _item;

	protected string _command = "";
	protected string _listParameter = "";
	
	public Item WithData( ItemData item,string command,string listParameter )
	{
		_item = item;
		_command = command;	
		_listParameter = listParameter;
		
		return this;
	}

	public override void _Ready()
	{
		if( _item != null )
		{
			var title = GetNode<Label>( "%Title" );
			title.Text = _item.Name;

			Button button = GetNode<Button>( "%Load" );
			button.Visible = false;
			button.Pressed += () =>
			{
				GetTree( ).CallGroup( "UI",_command,[_item.ID,_item.Items.Values[0].ID] );
			};
		}
	}

	public void UpdateItemData()
	{
		if( _item != null )
		{
			string imagePath = GetImagePath( );
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

	protected virtual string GetImagePath( )
	{
		if( _item != null )
		{
			string imagePath = Path.Combine( _listParameter,_item.ID,"ui","preview.png" );
			if( _item.Items.Count > 1 && _item.Items.Values[0].ID != "" )
			{
				string variantID = _item.Items.Values[0].ID;
				imagePath = Path.Combine( _listParameter,_item.ID,"ui",variantID,"preview.png" );
			}
			return imagePath;
		}
		return string.Empty;
	}
}
