class_name Vehicle
extends VehicleBody3D

@onready var ac_car = $ACCar
@onready var inputs : Inputs = $"%Inputs"
@onready var player : Node3D = $PlayerFlat

var steering_wheel : Node3D

func _process(_delta: float) -> void:
	if steering_wheel:
		steering_wheel.rotation.z = inputs.steering*PI


func _physics_process(_delta: float) -> void:
	var accelerator : float = inputs.accelerator
	#if accelerator > 0:
	self.engine_force = 7500 * accelerator
	#else:
		#self.engine_force = 0
	
	var steer : float = inputs.steering
	self.steering = -steer*0.2


func load_car(ac_folder: String,car: String,skin: String):
	var pos : Vector3 = ac_car.position
	if ac_car:
		remove_child(ac_car)
		ac_car.queue_free()
		ac_car = null
	
	ac_car = ResourceLoader.load("res://modules/cars/ACCar.tscn").instantiate()
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
	
	hide_mesh("ACCar/Dynamics/Cockpits/LowRes")
	hide_mesh("ACCar/Dynamics/Steerings/LowRes")


func hide_mesh(mesh_name: String):
	var mesh = get_node_or_null(mesh_name)
	if mesh: 
		mesh.visible = false
