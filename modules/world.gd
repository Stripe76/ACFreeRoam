extends Node3D

@onready var ui : Control = $"UI"
@onready var track : Track = $"%Track"
@onready var vehicle : Vehicle = $"%Vehicle"

var xrInterface : XRInterface

func _ready() -> void:
	$GenerateInputMap.GenerateMap( "" )

	if not Engine.is_editor_hint() and false:
		xrInterface = XRServer.find_interface("OpenXR")	
		
		if xrInterface and xrInterface.is_initialized():
			DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_DISABLED)
			get_viewport().use_xr = true
			#get_viewport().vrs_mode = Viewport.VRS_XR
			#get_viewport().size = Vector2(1024,768);
			#XRServer.world_scale = 1.0
			xrInterface.pose_recentered.connect(_on_openxr_pose_recentered)
			
			XRServer.center_on_hmd(XRServer.RESET_BUT_KEEP_TILT,true)
	
	if track and true:
		select_track( "imola","" );
		#select_car( "abarth500","" );
		#
		#var stall : Node3D = track.get_pit_stall( 1 )
		#if stall:
			#vehicle.position = stall.position + Vector3(0,1,0);
	
	#vehicle.load_car( "/mnt/data/Steam_Windows/steamapps/common/assettocorsa/content/cars/abarth500/abarth500.kn5" )
	
	var cancel_event = InputEventAction.new()
	cancel_event.action = "Pause"
	cancel_event.pressed = true
	Input.parse_input_event(cancel_event)


func _on_openxr_pose_recentered():
	XRServer.center_on_hmd(XRServer.RESET_BUT_KEEP_TILT, true)


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
