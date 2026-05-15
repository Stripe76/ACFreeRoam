extends Node

@export var accelerator : float = 0.0
@export var brakes : float = 0.0
@export var steering : float = 0.0

func _process(_delta: float) -> void:
	accelerator = Input.get_action_strength("Accelerator")
	brakes = Input.get_action_strength("Brakes")
	steering = Input.get_axis("SteeringPositive","SteeringNegative")
