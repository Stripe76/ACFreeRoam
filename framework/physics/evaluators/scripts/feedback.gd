class_name Feedback
extends Node


func _physics_process(delta: float) -> void:
	var input : Dictionary = {}
	feed(delta,input)
	
	input = {}
	back(delta,input)


func feed(delta: float,input: Dictionary)-> Dictionary:
	for f in get_children():
		input = f.feed(delta,input)
	return input


func back(delta: float,input: Dictionary):
	for f in get_children():
		input = f.back(delta,input)
	return input
