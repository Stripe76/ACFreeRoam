extends VehicleBody3D

@onready var inputs = $"../Inputs"

func _physics_process(_delta: float) -> void:
	var accelerator : float = inputs.accelerator
	if accelerator > 0:
		self.engine_force = 5000 * accelerator
	else:
		self.engine_force = 0
	
	var steer : float = inputs.steering
	self.steering = -steer*0.2
