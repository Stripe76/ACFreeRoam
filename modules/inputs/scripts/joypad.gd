extends Node

@export var accelerator : float = 0.0
@export var steering : float = 0.0

func _process(_delta: float) -> void:
	accelerator = Input.get_action_strength("Accelerator")
	steering = Input.get_axis("SteeringNegative","SteeringPositive")
