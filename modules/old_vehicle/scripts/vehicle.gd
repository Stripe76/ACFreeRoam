#class_name Vehicle
extends RigidBody3D

@onready var ac_car = $ACCar
@onready var inputs : Inputs = %Inputs
@onready var player : Node3D = $PlayerFlat

var steering_wheel : Node3D


func _process(_delta: float) -> void:
	if steering_wheel:
		steering_wheel.rotation.z = inputs.steering*PI


func _physics_process(delta: float) -> void:
	var accelerator : float = inputs.accelerator
	var steer : float = inputs.steering
	#var clutch : float = Input.get_action_strength("Clutch")
	
	_car_physics_process(delta)


func _car_physics_process(delta: float):
	pass


func load_car(ac_folder: String,car: String,skin: String):
	var pos := Vector3()
	if ac_car:
		pos = ac_car.position
		remove_child(ac_car)
		ac_car.queue_free()
		ac_car = null
	
	ac_car = load("res://modules/cars/ACCar.tscn").instantiate()
	ac_car.name = "ACCar"
	ac_car.LoadCar( ac_folder,car,skin )
	ac_car.position = pos
	add_child(ac_car)
	ac_car.owner = self
	
	steering_wheel = get_node_or_null("ACCar/Dynamics/Steerings/HighRes")
	if steering_wheel:
		if player:
			player.position = pos + steering_wheel.position + Vector3( 0,0.3,-0.3 )
		steering_wheel = steering_wheel.get_node("STEER_HR")
	
	#set_node_position(fl_wheel,"ACCar/Dynamics/Wheels/LeftFront")
	#set_node_position(fr_wheel,"ACCar/Dynamics/Wheels/RightFront")
	#set_node_position(rl_wheel,"ACCar/Dynamics/Wheels/LeftRear")
	#set_node_position(rr_wheel,"ACCar/Dynamics/Wheels/RightRear")
	
	hide_mesh("ACCar/Dynamics/Wheels/LeftFront/WHEEL_LF/RIM_BLUR_LF")
	hide_mesh("ACCar/Dynamics/Wheels/RightFront/WHEEL_RF/RIM_BLUR_RF")
	hide_mesh("ACCar/Dynamics/Wheels/LeftRear/WHEEL_LR/RIM_BLUR_LR")
	hide_mesh("ACCar/Dynamics/Wheels/RightRear/WHEEL_RR/RIM_BLUR_RR")
	
	#reparent_mesh(fl_wheel.get_node("hub/spin"),"ACCar/Dynamics/Wheels/LeftFront")
	#reparent_mesh(fr_wheel.get_node("hub/spin"),"ACCar/Dynamics/Wheels/RightFront")
	#reparent_mesh(rl_wheel.get_node("hub/spin"),"ACCar/Dynamics/Wheels/LeftRear")
	#reparent_mesh(rr_wheel.get_node("hub/spin"),"ACCar/Dynamics/Wheels/RightRear")
	
	hide_mesh("ACCar/Dynamics/Cockpits/LowRes")
	hide_mesh("ACCar/Dynamics/Steerings/LowRes")
	
	ac_car.position = ac_car.position + Vector3(0,-0.25,0)


func set_node_position(node: Node3D,mesh_name: String):
	if node:
		var mesh : Node3D = get_node_or_null(mesh_name)
		if mesh:
			node.position = mesh.position


func reparent_mesh(node: Node3D,mesh_name: String):
	if node:
		var mesh : Node3D = get_node_or_null(mesh_name)
		if mesh:
			mesh.reparent(node)


func hide_mesh(mesh_name: String):
	var mesh = get_node_or_null(mesh_name)
	if mesh: 
		mesh.visible = false
