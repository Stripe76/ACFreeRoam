using Godot;
using System;
using System.Collections.Generic;
using ACLibrary.Cars;

public partial class CarSelection : ItemSelection
{
	protected override void LoadItemsList( ItemDataList itemsList,string folder )
	{
		List<CarInfo> cars = ACLibrary.Cars.CarInfo.GetCarsInfos( folder,null );
		foreach( CarInfo newCar in cars )
		{
			if( !itemsList.TryGetValue( newCar.Brand,out var brand ) )
			{
				itemsList.Add( newCar.Brand,brand = new ItemData( newCar.Brand,newCar.Brand ) );
			}
			if( !brand.Items.TryGetValue( newCar.CarID,out var car ) )
			{
				brand.Items.Add( newCar.CarID,car = new ItemData( newCar.CarID,newCar.Model ) );
			}
			if( newCar.Skins.Count > 0 )
			{
				foreach( var skin in newCar.Skins )
				{
					car.Items.Add( skin.Name,new ItemData( skin.Name,skin.Title ) );
				}
			}
			else
			{
				
			}
		}

	}
}