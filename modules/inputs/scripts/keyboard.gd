extends Node

@export var accelerator : float = 0.0
@export var accelerator_curve : Curve
@export var steering : float = 0.0

var accelerator_start : float = 0.0

func _process(delta: float) -> void:
	if Input.is_action_just_pressed("Up"):
		accelerator_start = 0.0
	if Input.is_action_pressed("Up"):
		accelerator = accelerator_curve.sample_baked(accelerator_start)
		accelerator_start += delta
	else:
		accelerator = 0
	
	if Input.is_action_pressed("Left"):
		steering = -1
	elif Input.is_action_pressed("Right"):
		steering = 1
	else:
		steering = 0
