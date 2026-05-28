extends Node3D

@onready var track : Track = %Track
@onready var vehicle : Node3D = %Vehicle
@onready var environment : Node3D = $Environment

var ui : Control
var mouse_forward : SubViewport

var current_pit_stall := 1

var xrInterface : XRInterface

func _ready() -> void:
	xrInterface = XRServer.find_interface("OpenXR")	
	
	if xrInterface and xrInterface.is_initialized():
		vehicle.player = load("res://modules/player/player_vr.tscn").instantiate( )
		
		ui = vehicle.player.ui
		#if vehicle.player.has("mouse_forward"):
		mouse_forward = vehicle.player.get("mouse_forward")
	else:
		vehicle.player = load("res://modules/player/player_flat.tscn").instantiate( )
	
		ui = vehicle.player.ui
	
	if track and true:
		select_car( "abarth500","" );
		#select_track( "imola","" );
		#select_car( "ferrari_458_gt2","" );
	
	var cancel_event = InputEventAction.new()
	cancel_event.action = "Pause"
	cancel_event.pressed = true
	Input.parse_input_event(cancel_event)


func _on_openxr_pose_recentered():
	XRServer.center_on_hmd(XRServer.RESET_BUT_KEEP_TILT, true)


func _input(event: InputEvent) -> void:
	if mouse_forward:
		if event is InputEventMouse:
			mouse_forward.push_input(event)


func _unhandled_input(event: InputEvent) -> void:
	if event.is_action("Pause") and event.is_action_pressed("Pause"):
		get_tree().paused = !get_tree().paused
		ui.visible = get_tree().paused
		
		if ui.visible:
			environment.pause()
		else:
			environment.resume()
		get_viewport().set_input_as_handled()
	
	if event.is_action_released("NextPitStall"):
		current_pit_stall = select_pit_stall(current_pit_stall+1)
	elif event.is_action_released("PrevPitStall"):
		current_pit_stall = select_pit_stall(current_pit_stall-1)
	elif event.is_action_released("ResetVehicle"):
		vehicle.reset(vehicle.position+Vector3(0,1.5,0),Vector3.ZERO)


func select_track(track_id: String,variant_id: String):
	if track:
		print(track_id," ",variant_id)
		var ac_folder = "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/tracks/"
		track.load_track( ac_folder,track_id,variant_id );
		
		current_pit_stall = select_pit_stall(current_pit_stall)


func select_car(car_id: String,skin_id: String):
	if vehicle:
		var ac_folder = "/mnt/data/Steam_Linux/steamapps/common/assettocorsa/content/cars/"
		vehicle.load_car( ac_folder,car_id,skin_id );


func select_pit_stall(pit_stall: int) -> int:
	var stall : Node3D = track.get_pit_stall(pit_stall)
	if not stall:
		pit_stall = 0
		stall = track.get_pit_stall(pit_stall)
	if stall and vehicle:
		print(pit_stall)
		vehicle.reset(stall.position+Vector3(0,0.5,0),stall.rotation)
	return pit_stall
