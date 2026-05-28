class_name PlayerVR extends Node3D

@onready var ui := $SubViewport/UI
@onready var mouse_forward := $SubViewport

var xrInterface : XRInterface

func _ready() -> void:
	if not Engine.is_editor_hint():
		xrInterface = XRServer.find_interface("OpenXR")	
		
		if xrInterface and xrInterface.is_initialized():
			DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_DISABLED)
			get_viewport().use_xr = true
			get_viewport().vrs_mode = Viewport.VRS_XR
			xrInterface.pose_recentered.connect(_on_openxr_pose_recentered)
			
			XRServer.center_on_hmd(XRServer.RESET_BUT_KEEP_TILT,true)


func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		print(event.screen_relative)

func _on_openxr_pose_recentered():
	XRServer.center_on_hmd(XRServer.RESET_BUT_KEEP_TILT, true)


func _on_ui_visibility_changed() -> void:
	$XROrigin3D/UILayer.visible = ui.visible


func set_cockpit_position(camera: Vector3):
	$XROrigin3D.position = camera + Vector3(0,.2,0)
