extends Node3D

@onready var ui : Control = $"UI"
@onready var track : Track = $"Game/Track"
@onready var vehicle : Vehicle = $"Game/Vehicle"

func _ready() -> void:
	$GenerateInputMap.GenerateMap( "" )
	
	#if track and false:
		#track.load_track( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/imola/imola.kn5" );
		#
		#var stall : Node3D = track.get_pit_stall( 1 )
		#if stall:
			#vehicle.position = stall.position + Vector3(0,1,0);
	
	#vehicle.load_car( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/abarth500/abarth500.kn5" )
	
	var cancel_event = InputEventAction.new()
	cancel_event.action = "Pause"
	cancel_event.pressed = true
	Input.parse_input_event(cancel_event)


func _unhandled_input(event: InputEvent) -> void:
	if event.is_action("Pause") and event.is_action_pressed("Pause"):
		get_tree().paused = !get_tree().paused
		ui.visible = get_tree().paused
		get_viewport().set_input_as_handled()


func select_track(track_id: String,variant_id: String):
	if track:
		var ac_folder = "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/tracks/"
		track.load_track( ac_folder,track_id,variant_id );
		
		var stall : Node3D = track.get_pit_stall( 1 )
		if stall:
			vehicle.position = stall.position + Vector3(0,1,0);
			vehicle.rotation = stall.rotation


func select_car(car_id: String,skin_id: String):
	if vehicle:
		var ac_folder = "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/"
		vehicle.load_car( ac_folder,car_id,skin_id );
