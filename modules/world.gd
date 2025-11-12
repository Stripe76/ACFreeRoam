extends Node3D

@onready var track : Node3D = $"ACTrack"
@onready var vehicle : Node3D = $"Vehicle"

func _ready() -> void:
	var stall : Node3D = track.GetPitStall( 1 )
	vehicle.position = stall.position;
	
	$GenerateInputMap.GenerateMap( "" )
