class_name BasePanel extends Control

@export var preview_panel : Control
@export_enum("UI","Weather") var group : String

var dragging : = false
var slider_parent : Control


func _on_slider_changed(value: float,callback: String) -> void:
	get_tree().call_group(group,callback,value)


func _on_slider_drag_started(source: Slider) -> void:
	if preview_panel and not dragging:
		dragging = true;
		
		slider_parent = source.get_parent()
		source.reparent(preview_panel,false)
		
		get_tree().call_group("UI","preview_mode",true)
		
		var click := InputEventMouseButton.new()
		click.set_button_index(MOUSE_BUTTON_LEFT)
		click.position = source.global_position + Vector2((source.value/source.max_value)*source.size.x,source.size.y/2)
		click.set_pressed(true)
		Input.call_deferred("parse_input_event",click)


func _on_slider_drag_ended(_value_changed: bool,source: Slider) -> void:
	dragging = false;
	
	if preview_panel:
		source.reparent(slider_parent,false)
		get_tree().call_group("UI","preview_mode",false)
