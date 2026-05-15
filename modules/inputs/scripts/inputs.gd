class_name Inputs
extends Node

@export var accelerator : float:
	get: return get_accelerator( )
@export var brakes : float:
	get: return get_brakes( )
@export var steering : float:
	get: return get_steering( )

var input

func _ready() -> void:
	input = get_node( "Keyboard" )
	#input = get_node( "Joypad" )


func get_accelerator( )-> float:
	#return input.accelerator
	return $Keyboard.accelerator + $Joypad.accelerator


func get_brakes( )-> float:
	#return input.brakes
	return $Keyboard.brakes + $Joypad.brakes


func get_steering( )-> float:
	#return input.steering
	return $Keyboard.steering + $Joypad.steering 
