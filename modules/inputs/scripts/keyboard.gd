extends Node

@export var accelerator : float = 0.0
@export var accelerator_curve : Curve
@export var steering : float = 0.0
@export var steering_curve : Curve

var accelerator_start : float = 0.0
var steering_start : float = 0.0

func _process(delta: float) -> void:
	if Input.is_action_just_pressed("Up") || Input.is_action_just_pressed("Down"):
		accelerator_start = 0.0
	if Input.is_action_pressed("Up"):
		accelerator = accelerator_curve.sample_baked(accelerator_start)
		accelerator_start += delta
	elif Input.is_action_pressed("Down"):
		accelerator = -accelerator_curve.sample_baked(accelerator_start)
		accelerator_start += delta
	else:
		accelerator = 0
	
	if (Input.is_action_just_pressed("Left") or Input.is_action_just_pressed("Right") or 
		Input.is_action_just_released("Left") or Input.is_action_just_released("Right")):
		steering_start = 0.0
	if Input.is_action_pressed("Left"):
		steering = -1 * accelerator_curve.sample_baked(steering_start/2)
		steering_start += delta
	elif Input.is_action_pressed("Right"):
		steering = 1 * accelerator_curve.sample_baked(steering_start/2)
		steering_start += delta
	else:
		steering = 0
