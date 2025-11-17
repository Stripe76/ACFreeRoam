using System.IO;

namespace ACTracks.modules.ui.panels.car_selection.scripts;

public partial class CarItem : Item
{
	protected override string GetImagePath()
	{
		if( _item != null )
		{
			string imagePath = Path.Combine( _listParameter,_item.ID,"skins",_item.Items.Values[0].ID,"preview.jpg" );

			return imagePath;
		}
		return string.Empty;
	}
}