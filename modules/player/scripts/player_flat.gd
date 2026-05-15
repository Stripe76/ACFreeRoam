class_name PlayerFlat extends Node3D

@onready var _cameras := [
	$Cockpit,
	$FollowNear,
	$FollowFar,
	$LeftSide
]
var _current_camera := 0


func set_cockpit_position(camera: Vector3):
	print(set_cockpit_position)
	$Cockpit.position = camera


func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("CycleCamera"):
		_current_camera += 1
		_current_camera = wrap(_current_camera,0,_cameras.size())
		_cameras[_current_camera].current = true
