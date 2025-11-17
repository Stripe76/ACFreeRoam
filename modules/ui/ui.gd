extends Control


func _unhandled_input(event: InputEvent) -> void:
	if event.is_action("Pause") and event.is_action_pressed("Pause"):
		get_tree().paused = !get_tree().paused
		self.visible = get_tree().paused
		get_viewport().set_input_as_handled()


func select_panel(panel: String):
	$MainPanel/TracksAndCars.visible = panel == "tracks"
