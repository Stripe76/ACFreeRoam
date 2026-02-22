extends Node

class_name MenuItem

@export var title : String
@export var command : String
@export var parameter : String

func _ready() -> void:
	title = name

static func create(item_title: String, item_command: String) -> MenuItem:
	var instance = MenuItem.new()
	instance.Title = item_title
	instance.Command = item_command
	return instance
