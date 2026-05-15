@abstract class_name Vehicle extends RigidBody3D

@onready var inputs : Inputs = %Inputs

var ac_car : Node3D
var player : Node3D

var steering_wheel : Node3D

var fl_wheel
var fr_wheel
var rl_wheel
var rr_wheel

var spin_path : String
var fixed_path : String

func _ready() -> void:
	player = load("res://modules/player/player_flat.tscn").instantiate()
	player.name = "PlayerFlat";
	
	add_child(player)


func _process(_delta: float) -> void:
	if steering_wheel:
		steering_wheel.rotation.z = -inputs.steering*PI


func load_car(ac_folder: String,car: String,skin: String):
	print(car," - ",skin)
	
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
			print("Set player position")
			player.set_cockpit_position(steering_wheel.position + Vector3( 0,0.25,-0.3 ))
			#player.position = pos + steering_wheel.position + Vector3( 0,0.3,-0.3 )
		steering_wheel = steering_wheel.get_node("STEER_HR")
	
	set_node_position(fl_wheel,"ACCar/Dynamics/Wheels/LeftFront")
	set_node_position(fr_wheel,"ACCar/Dynamics/Wheels/RightFront")
	set_node_position(rl_wheel,"ACCar/Dynamics/Wheels/LeftRear")
	set_node_position(rr_wheel,"ACCar/Dynamics/Wheels/RightRear")
	
	hide_mesh("ACCar/Dynamics/Wheels/LeftFront/WHEEL_LF/RIM_BLUR_LF")
	hide_mesh("ACCar/Dynamics/Wheels/RightFront/WHEEL_RF/RIM_BLUR_RF")
	hide_mesh("ACCar/Dynamics/Wheels/LeftRear/WHEEL_LR/RIM_BLUR_LR")
	hide_mesh("ACCar/Dynamics/Wheels/RightRear/WHEEL_RR/RIM_BLUR_RR")
	
	reparent_wheel(fl_wheel.get_node(spin_path),fl_wheel.get_node(fixed_path),"ACCar/Dynamics/Wheels/LeftFront","LF")
	reparent_wheel(fr_wheel.get_node(spin_path),fr_wheel.get_node(fixed_path),"ACCar/Dynamics/Wheels/RightFront","RF")
	reparent_wheel(rl_wheel.get_node(spin_path),rl_wheel.get_node(fixed_path),"ACCar/Dynamics/Wheels/LeftRear","LR")
	reparent_wheel(rr_wheel.get_node(spin_path),rr_wheel.get_node(fixed_path),"ACCar/Dynamics/Wheels/RightRear","RR")
	
	hide_mesh("ACCar/Dynamics/Cockpits/LowRes")
	hide_mesh("ACCar/Dynamics/Steerings/LowRes")
	
	#ac_car.position = ac_car.position + Vector3(0,-0.25,0)


func set_node_position(node: Node3D,mesh_name: String):
	if node:
		var mesh : Node3D = get_node_or_null(mesh_name)
		if mesh:
			node.position = mesh.position


func reparent_wheel(spin: Node3D,fixed: Node3D,mesh_name: String,_wheel: String):
	if spin and fixed:
		for n in spin.get_children():
			spin.remove_child(n)
			n.free()
		for n in fixed.get_children():
			fixed.remove_child(n)
			n.free()
		
		var mesh : Node3D = get_node_or_null(mesh_name)
		if mesh:
			for n in mesh.get_children():
				if n.name.begins_with("WHEEL"):
					n.reparent(spin)
				else:
					n.reparent(fixed)


func reparent_mesh(node: Node3D,mesh_name: String):
	if node:
		var mesh : Node3D = get_node_or_null(mesh_name)
		if mesh:
			mesh.reparent(node)


func hide_mesh(mesh_name: String):
	var mesh = get_node_or_null(mesh_name)
	if mesh: 
		mesh.visible = false
