class_name Operation
extends Node

func _feed(delta: float, input: Dictionary)-> Dictionary:
	if Input.is_action_pressed("Up"):
		return {"Accelerator":1}
	return {"Accelerator":0}


func _back(delta: float, input: Dictionary)-> Dictionary:
	return input
